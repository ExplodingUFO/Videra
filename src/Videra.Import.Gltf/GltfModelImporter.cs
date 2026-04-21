using System.Numerics;
using Microsoft.Extensions.Logging;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using Videra.Core.Exceptions;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;
using SceneMeshPrimitive = Videra.Core.Scene.MeshPrimitive;

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

            var asset = LoadWithSharpGltf(filePath, log);

            Log.Loaded(log, asset.Metrics.VertexCount, asset.Metrics.IndexCount);
            Log.LoadSucceeded(log, filePath);

            return asset;
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

    private static ImportedSceneAsset LoadWithSharpGltf(string filePath, ILogger logger)
    {
        var model = LoadModelRootWithFallback(filePath, logger);
        var nodes = new List<SceneNode>();
        var primitives = new Dictionary<SharpGLTF.Schema2.MeshPrimitive, SceneMeshPrimitive>();

        var defaultScene = model.DefaultScene ?? model.LogicalScenes.FirstOrDefault();
        if (defaultScene != null)
        {
            foreach (var node in defaultScene.VisualChildren)
            {
                ProcessNodeRecursive(node, parentId: null, nodes, primitives, logger);
            }
        }
        else
        {
            foreach (var mesh in model.LogicalMeshes.Select((mesh, index) => (mesh, index)))
            {
                var primitiveIds = mesh.mesh.Primitives
                    .Select(primitive => CreateOrGetPrimitive(primitive, mesh.mesh.Name ?? $"Mesh {mesh.index}", primitives, logger).Id)
                    .ToArray();

                nodes.Add(new SceneNode(
                    SceneNodeId.New(),
                    mesh.mesh.Name ?? $"Mesh {mesh.index}",
                    Matrix4x4.Identity,
                    parentId: null,
                    primitiveIds));
            }
        }

        Log.SharpGltfTotals(
            logger,
            primitives.Values.Sum(static primitive => primitive.MeshData.Vertices.Length),
            primitives.Values.Sum(static primitive => primitive.MeshData.Indices.Length));

        return new ImportedSceneAsset(
            filePath,
            Path.GetFileName(filePath),
            nodes,
            primitives.Values.ToArray());
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
        SceneNodeId? parentId,
        List<SceneNode> nodes,
        Dictionary<SharpGLTF.Schema2.MeshPrimitive, SceneMeshPrimitive> primitives,
        ILogger logger)
    {
        var currentNodeId = SceneNodeId.New();
        var primitiveIds = node.Mesh?.Primitives
            .Select(primitive => CreateOrGetPrimitive(primitive, node.Mesh.Name ?? node.Name ?? "Mesh", primitives, logger).Id)
            .ToArray() ?? Array.Empty<MeshPrimitiveId>();

        if (node.Mesh != null)
        {
            Log.ProcessingNode(logger, node.Name, node.Mesh.Name);
        }

        nodes.Add(new SceneNode(
            currentNodeId,
            node.Name ?? $"Node {nodes.Count}",
            node.LocalMatrix,
            parentId,
            primitiveIds));

        foreach (var child in node.VisualChildren)
        {
            ProcessNodeRecursive(child, currentNodeId, nodes, primitives, logger);
        }
    }

    private static SceneMeshPrimitive CreateOrGetPrimitive(
        SharpGLTF.Schema2.MeshPrimitive primitive,
        string meshName,
        Dictionary<SharpGLTF.Schema2.MeshPrimitive, SceneMeshPrimitive> primitives,
        ILogger logger)
    {
        if (primitives.TryGetValue(primitive, out var existing))
        {
            return existing;
        }

        var meshData = CreatePrimitiveMeshData(primitive, logger);
        var created = new SceneMeshPrimitive(
            MeshPrimitiveId.New(),
            $"{meshName}#primitive{primitives.Count}",
            meshData);
        primitives.Add(primitive, created);
        Log.ProcessedVertices(logger, meshData.Vertices.Length, (uint)meshData.Indices.Length);
        return created;
    }

    private static MeshData CreatePrimitiveMeshData(
        SharpGLTF.Schema2.MeshPrimitive primitive,
        ILogger logger)
    {
        var positions = primitive.GetVertexAccessor("POSITION")?.AsVector3Array();
        var normals = primitive.GetVertexAccessor("NORMAL")?.AsVector3Array();
        var colors = primitive.GetVertexAccessor("COLOR_0")?.AsVector4Array();

        if (positions == null)
        {
            Log.SkippingPrimitiveWithoutPositions(logger);
            throw new InvalidOperationException("glTF primitive does not contain POSITION data.");
        }

        var material = primitive.Material;
        var baseColor = material?.FindChannel("BaseColor")?.Parameter ?? Vector4.One;
        var defaultColor = new RgbaFloat(baseColor.X, baseColor.Y, baseColor.Z, baseColor.W);
        var vertices = new VertexPositionNormalColor[positions.Count];

        for (var i = 0; i < positions.Count; i++)
        {
            var normal = normals != null && i < normals.Count
                ? Vector3.Normalize(normals[i])
                : Vector3.UnitY;

            var color = colors != null && i < colors.Count
                ? new RgbaFloat(colors[i].X, colors[i].Y, colors[i].Z, colors[i].W)
                : defaultColor;

            vertices[i] = new VertexPositionNormalColor(positions[i], normal, color);
        }

        var indices = primitive.GetIndices().ToArray();
        return new MeshData
        {
            Vertices = vertices,
            Indices = indices,
            Topology = MeshTopology.Triangles
        };
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
