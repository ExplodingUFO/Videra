using System.Numerics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Videra.Core.Exceptions;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;

namespace Videra.Core.IO;

/// <summary>
/// Static utility for loading 3D model files (glTF, GLB, OBJ) into <see cref="Object3D"/> instances
/// ready for GPU rendering. Handles format detection, scene graph traversal, vertex transformations,
/// and GPU resource allocation.
/// </summary>
public static partial class ModelImporter
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".gltf", ".glb", ".obj"
    };

    /// <summary>
    /// Gets the file glob patterns supported by this importer.
    /// Returns <c>"*.gltf"</c>, <c>"*.glb"</c>, and <c>"*.obj"</c>.
    /// </summary>
    public static string[] SupportedFormats => new[]
        { "*.gltf", "*.glb", "*.obj" };

    /// <summary>
    /// Imports a 3D model from disk into a backend-neutral asset payload.
    /// Supports glTF (<c>.gltf</c>/<c>.glb</c>) and Wavefront OBJ (<c>.obj</c>) formats.
    /// The returned asset contains CPU-side mesh data only; GPU upload is deferred to
    /// <see cref="SceneUploadCoordinator"/>.
    /// </summary>
    /// <param name="filePath">Absolute or relative path to the model file.</param>
    /// <param name="logger">
    /// Optional logger for diagnostic output. When <c>null</c>, a no-op logger is used.
    /// </param>
    /// <returns>A backend-neutral imported scene asset containing mesh data and identity.</returns>
    /// <exception cref="InvalidModelInputException">
    /// Thrown when the file path is invalid, the format is unsupported, or the file does not exist.
    /// </exception>
    /// <exception cref="Exception">
    /// Re-thrown from the underlying parser when the model file is corrupt or cannot be read.
    /// </exception>
    public static ImportedSceneAsset Import(string filePath, ILogger? logger = null)
    {
        var log = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger("ModelImporter");

        try
        {
            ValidateFilePath(filePath);

            Log.Loading(log, filePath);

            var ext = Path.GetExtension(filePath).ToLowerInvariant();

            MeshData meshData = ext switch
            {
                ".gltf" or ".glb" => LoadWithSharpGLTF(filePath, log),
                ".obj" => LoadSimpleObj(filePath),
                _ => throw new InvalidModelInputException(
                    $"Format {ext} is not supported. Supported formats: {string.Join(", ", AllowedExtensions)}",
                    "LoadModel",
                    new Dictionary<string, string?> { ["Extension"] = ext })
            };

            Log.Loaded(log, meshData.Vertices.Length, meshData.Indices.Length);
            Log.LoadSucceeded(log, filePath);

            var payload = MeshPayload.FromMesh(meshData, cloneArrays: false);
            return new ImportedSceneAsset(
                filePath,
                Path.GetFileName(filePath),
                meshData)
            {
                Payload = payload,
                Metrics = SceneAssetMetrics.FromPayload(payload)
            };
        }
        catch (VideraException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.LoadFailed(log, filePath, ex.Message, ex);
            throw;
        }
    }

    /// <summary>
    /// Loads a 3D model from disk and initializes it for rendering immediately.
    /// </summary>
    public static Object3D Load(string filePath, IResourceFactory factory, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        var log = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger("ModelImporter");

        try
        {
            var asset = Import(filePath, log);
            Log.InitializingGpuResources(log);
            return SceneUploadCoordinator.Upload(asset, factory, log);
        }
        catch (VideraException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.LoadFailed(log, filePath, ex.Message, ex);
            throw;
        }
    }

    private static void ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new InvalidModelInputException(
                "File path must not be null or empty.",
                "LoadModel",
                new Dictionary<string, string?> { ["FilePath"] = filePath ?? "(null)" });
        }

        var fullPath = Path.GetFullPath(filePath);

        if (Directory.Exists(fullPath))
        {
            throw new InvalidModelInputException(
                $"Path is a directory, not a file: {fullPath}",
                "LoadModel",
                new Dictionary<string, string?> { ["NormalizedPath"] = fullPath });
        }

        var ext = Path.GetExtension(fullPath).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            throw new InvalidModelInputException(
                $"File extension '{ext}' is not supported. Supported formats: {string.Join(", ", AllowedExtensions)}",
                "LoadModel",
                new Dictionary<string, string?> { ["Extension"] = ext, ["NormalizedPath"] = fullPath });
        }

        if (!File.Exists(fullPath))
        {
            throw new InvalidModelInputException(
                $"File not found: {fullPath}",
                "LoadModel",
                new Dictionary<string, string?> { ["NormalizedPath"] = fullPath });
        }
    }

    private static MeshData LoadWithSharpGLTF(string filePath, ILogger logger)
    {
        var model = LoadModelRootWithFallback(filePath, logger);

        var allVertices = new List<VertexPositionNormalColor>();
        var allIndices = new List<uint>();
        uint indexOffset = 0;

        // 遍历场景中的所有节点，应用节点变换
        var defaultScene = model.DefaultScene ?? model.LogicalScenes.FirstOrDefault();
        if (defaultScene != null)
        {
            foreach (var node in defaultScene.VisualChildren)
            {
                ProcessNodeRecursive(node, Matrix4x4.Identity, allVertices, allIndices, ref indexOffset, logger);
            }
        }
        else
        {
            // 回退：如果没有场景，直接处理 mesh（不带变换）
            foreach (var mesh in model.LogicalMeshes)
            {
                ProcessMesh(mesh, Matrix4x4.Identity, allVertices, allIndices, ref indexOffset, logger);
            }
        }

        Log.SharpGltfTotals(logger, allVertices.Count, allIndices.Count);

        return new MeshData
        {
            Vertices = allVertices.ToArray(),
            Indices = allIndices.ToArray(),
            Topology = MeshTopology.Triangles
        };
    }

    private static ModelRoot LoadModelRootWithFallback(string filePath, ILogger logger)
    {
        try
        {
            return ModelRoot.Load(filePath, new ReadSettings { Validation = ValidationMode.Strict });
        }
        catch (DataException ex)
        {
            Log.StrictValidationFailed(logger, filePath, ex);

            return ModelRoot.Load(filePath, new ReadSettings { Validation = ValidationMode.TryFix });
        }
    }

    private static void ProcessNodeRecursive(
        SharpGLTF.Schema2.Node node,
        Matrix4x4 parentTransform,
        List<VertexPositionNormalColor> allVertices,
        List<uint> allIndices,
        ref uint indexOffset,
        ILogger logger)
    {
        // 计算当前节点的世界变换
        var localTransform = node.LocalMatrix;
        var worldTransform = localTransform * parentTransform;

        // 如果节点有 mesh，处理它
        if (node.Mesh != null)
        {
            Log.ProcessingNode(logger, node.Name, node.Mesh.Name);
            ProcessMesh(node.Mesh, worldTransform, allVertices, allIndices, ref indexOffset, logger);
        }

        // 递归处理子节点
        foreach (var child in node.VisualChildren)
        {
            ProcessNodeRecursive(child, worldTransform, allVertices, allIndices, ref indexOffset, logger);
        }
    }

    private static void ProcessMesh(
        SharpGLTF.Schema2.Mesh mesh,
        Matrix4x4 transform,
        List<VertexPositionNormalColor> allVertices,
        List<uint> allIndices,
        ref uint indexOffset,
        ILogger logger)
    {
        // 计算法线变换矩阵（逆转置）
        Matrix4x4.Invert(transform, out var inverseTransform);
        var normalTransform = Matrix4x4.Transpose(inverseTransform);

        foreach (var primitive in mesh.Primitives)
        {
            // 获取顶点数据
            var positions = primitive.GetVertexAccessor("POSITION")?.AsVector3Array();
            var normals = primitive.GetVertexAccessor("NORMAL")?.AsVector3Array();
            var colors = primitive.GetVertexAccessor("COLOR_0")?.AsVector4Array();

            if (positions == null)
            {
                Log.SkippingPrimitiveWithoutPositions(logger);
                continue;
            }

            // 获取材质颜色
            var material = primitive.Material;
            var baseColor = material?.FindChannel("BaseColor")?.Parameter ?? Vector4.One;
            var defaultColor = new RgbaFloat(baseColor.X, baseColor.Y, baseColor.Z, baseColor.W);

            // 处理顶点
            for (int i = 0; i < positions.Count; i++)
            {
                // 应用变换到位置
                var pos = Vector3.Transform(positions[i], transform);

                // 应用法线变换
                var normal = normals != null && i < normals.Count
                    ? Vector3.Normalize(Vector3.TransformNormal(normals[i], normalTransform))
                    : Vector3.UnitY;

                var color = colors != null && i < colors.Count
                    ? new RgbaFloat(colors[i].X, colors[i].Y, colors[i].Z, colors[i].W)
                    : defaultColor;

                allVertices.Add(new VertexPositionNormalColor(pos, normal, color));
            }

            // 处理索引
            var indices = primitive.GetIndices();
            foreach (var idx in indices)
            {
                if (indexOffset + idx > uint.MaxValue)
                {
                    Log.IndexOverflow(logger);
                    break;
                }
                allIndices.Add(indexOffset + (uint)idx);
            }

            indexOffset += (uint)positions.Count;
            Log.ProcessedVertices(logger, positions.Count, indexOffset);
        }
    }

    // 简单的 OBJ 加载器（备用）
    private static MeshData LoadSimpleObj(string path)
    {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var finalVertices = new List<VertexPositionNormalColor>();
        var finalIndices = new List<uint>();

        foreach (var line in File.ReadLines(path))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            switch (parts[0])
            {
                case "v" when parts.Length >= 4:
                    vertices.Add(new Vector3(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)));
                    break;

                case "vn" when parts.Length >= 4:
                    normals.Add(new Vector3(
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture),
                        float.Parse(parts[3], CultureInfo.InvariantCulture)));
                    break;

                case "f" when parts.Length >= 4:
                    for (int i = 1; i <= 3; i++)
                    {
                        var indices = parts[i].Split('/');
                        if (!int.TryParse(indices[0], out var vIdxRaw))
                            continue;
                        var vIdx = vIdxRaw > 0 ? vIdxRaw - 1 : vIdxRaw + vertices.Count;
                        if (vIdx < 0 || vIdx >= vertices.Count)
                            continue;

                        var nIdx = 0;
                        if (indices.Length > 2 && !string.IsNullOrEmpty(indices[2]) && int.TryParse(indices[2], out var nIdxRaw))
                        {
                            nIdx = nIdxRaw > 0 ? nIdxRaw - 1 : nIdxRaw + normals.Count;
                            if (nIdx < 0 || nIdx >= normals.Count)
                                nIdx = 0;
                        }

                        var v = vertices[vIdx];
                        var n = normals.Count > 0 ? normals[nIdx] : Vector3.UnitY;

                        finalVertices.Add(new VertexPositionNormalColor(
                            v, n, new RgbaFloat(0.7f, 0.7f, 0.7f, 1f)));
                        finalIndices.Add((uint)finalVertices.Count - 1);
                    }
                    break;
            }
        }

        return new MeshData
        {
            Vertices = finalVertices.ToArray(),
            Indices = finalIndices.ToArray(),
            Topology = MeshTopology.Triangles
        };
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[ModelImporter] Loading: {FilePath}")]
        public static partial void Loading(ILogger logger, string filePath);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "[ModelImporter] Loaded {VertexCount} vertices, {IndexCount} indices")]
        public static partial void Loaded(ILogger logger, int vertexCount, int indexCount);

        [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "[ModelImporter] Initializing GPU resources...")]
        public static partial void InitializingGpuResources(ILogger logger);

        [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "[ModelImporter] Successfully loaded: {FilePath}")]
        public static partial void LoadSucceeded(ILogger logger, string filePath);

        [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "[ModelImporter] Failed to load: {FilePath}: {Error}")]
        public static partial void LoadFailed(ILogger logger, string filePath, string error, Exception exception);

        [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "[SharpGLTF] Total: {VertexCount} vertices, {IndexCount} indices")]
        public static partial void SharpGltfTotals(ILogger logger, int vertexCount, int indexCount);

        [LoggerMessage(EventId = 7, Level = LogLevel.Warning, Message = "[SharpGLTF] Strict validation failed for {FilePath}; retrying with ValidationMode.TryFix")]
        public static partial void StrictValidationFailed(ILogger logger, string filePath, Exception exception);

        [LoggerMessage(EventId = 8, Level = LogLevel.Debug, Message = "[SharpGLTF] Processing node: {NodeName} with mesh: {MeshName}")]
        public static partial void ProcessingNode(ILogger logger, string? nodeName, string? meshName);

        [LoggerMessage(EventId = 9, Level = LogLevel.Warning, Message = "[SharpGLTF] Skipping primitive without positions")]
        public static partial void SkippingPrimitiveWithoutPositions(ILogger logger);

        [LoggerMessage(EventId = 10, Level = LogLevel.Warning, Message = "[SharpGLTF] Index overflow, stopping")]
        public static partial void IndexOverflow(ILogger logger);

        [LoggerMessage(EventId = 11, Level = LogLevel.Debug, Message = "[SharpGLTF] Processed {VertexCount} vertices, current offset: {IndexOffset}")]
        public static partial void ProcessedVertices(ILogger logger, int vertexCount, uint indexOffset);
    }
}
