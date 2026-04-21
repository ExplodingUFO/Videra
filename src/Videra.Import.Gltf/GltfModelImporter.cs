using System.Numerics;
using Microsoft.Extensions.Logging;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using Videra.Core.Exceptions;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;

namespace Videra.Import.Gltf;

public static partial class GltfModelImporter
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".gltf", ".glb"
    };

    public static string[] SupportedFormats => ["*.gltf", "*.glb"];

    public static ImportedSceneAsset Import(string filePath, ILogger? logger = null)
    {
        var log = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger("GltfModelImporter");

        try
        {
            ValidateFilePath(filePath);

            Log.Loading(log, filePath);

            var meshData = LoadWithSharpGltf(filePath, log);

            Log.Loaded(log, meshData.Vertices.Length, meshData.Indices.Length);
            Log.LoadSucceeded(log, filePath);

            return new ImportedSceneAsset(
                filePath,
                Path.GetFileName(filePath),
                meshData)
            {
                Metrics = SceneAssetMetrics.FromMesh(meshData)
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

    public static Object3D Load(string filePath, IResourceFactory factory, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        var log = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger("GltfModelImporter");

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

    private static MeshData LoadWithSharpGltf(string filePath, ILogger logger)
    {
        var model = LoadModelRootWithFallback(filePath, logger);

        var allVertices = new List<VertexPositionNormalColor>();
        var allIndices = new List<uint>();
        uint indexOffset = 0;

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
        Node node,
        Matrix4x4 parentTransform,
        List<VertexPositionNormalColor> allVertices,
        List<uint> allIndices,
        ref uint indexOffset,
        ILogger logger)
    {
        var localTransform = node.LocalMatrix;
        var worldTransform = localTransform * parentTransform;

        if (node.Mesh != null)
        {
            Log.ProcessingNode(logger, node.Name, node.Mesh.Name);
            ProcessMesh(node.Mesh, worldTransform, allVertices, allIndices, ref indexOffset, logger);
        }

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
        Matrix4x4.Invert(transform, out var inverseTransform);
        var normalTransform = Matrix4x4.Transpose(inverseTransform);

        foreach (var primitive in mesh.Primitives)
        {
            var positions = primitive.GetVertexAccessor("POSITION")?.AsVector3Array();
            var normals = primitive.GetVertexAccessor("NORMAL")?.AsVector3Array();
            var colors = primitive.GetVertexAccessor("COLOR_0")?.AsVector4Array();

            if (positions == null)
            {
                Log.SkippingPrimitiveWithoutPositions(logger);
                continue;
            }

            var material = primitive.Material;
            var baseColor = material?.FindChannel("BaseColor")?.Parameter ?? Vector4.One;
            var defaultColor = new RgbaFloat(baseColor.X, baseColor.Y, baseColor.Z, baseColor.W);

            for (int i = 0; i < positions.Count; i++)
            {
                var pos = Vector3.Transform(positions[i], transform);
                var normal = normals != null && i < normals.Count
                    ? Vector3.Normalize(Vector3.TransformNormal(normals[i], normalTransform))
                    : Vector3.UnitY;

                var color = colors != null && i < colors.Count
                    ? new RgbaFloat(colors[i].X, colors[i].Y, colors[i].Z, colors[i].W)
                    : defaultColor;

                allVertices.Add(new VertexPositionNormalColor(pos, normal, color));
            }

            foreach (var idx in primitive.GetIndices())
            {
                if (indexOffset + idx > uint.MaxValue)
                {
                    Log.IndexOverflow(logger);
                    break;
                }

                allIndices.Add(indexOffset + idx);
            }

            indexOffset += (uint)positions.Count;
            Log.ProcessedVertices(logger, positions.Count, indexOffset);
        }
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[GltfModelImporter] Loading: {FilePath}")]
        public static partial void Loading(ILogger logger, string filePath);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "[GltfModelImporter] Loaded {VertexCount} vertices, {IndexCount} indices")]
        public static partial void Loaded(ILogger logger, int vertexCount, int indexCount);

        [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "[GltfModelImporter] Initializing GPU resources...")]
        public static partial void InitializingGpuResources(ILogger logger);

        [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "[GltfModelImporter] Successfully loaded: {FilePath}")]
        public static partial void LoadSucceeded(ILogger logger, string filePath);

        [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "[GltfModelImporter] Failed to load: {FilePath}: {Error}")]
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
