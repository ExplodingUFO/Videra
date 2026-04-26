using System.Diagnostics;
using System.Numerics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Videra.Core.Exceptions;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;

namespace Videra.Import.Obj;

public static partial class ObjModelImporter
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".obj"
    };

    public static string[] SupportedFormats => ["*.obj"];

    public static IVideraModelImporter Create(ILogger? logger = null)
    {
        return new RegistryModelImporter(logger);
    }

    public static ImportedSceneAsset Import(string filePath, ILogger? logger = null)
    {
        var log = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger("ObjModelImporter");

        try
        {
            ValidateFilePath(filePath);

            Log.Loading(log, filePath);

            var meshData = LoadSimpleObj(filePath);
            var material = new MaterialInstance(
                MaterialInstanceId.New(),
                $"{Path.GetFileName(filePath)}#material0",
                RgbaFloat.LightGrey);
            var primitive = new MeshPrimitive(
                MeshPrimitiveId.New(),
                $"{Path.GetFileName(filePath)}#primitive0",
                meshData,
                material.Id);
            var rootNode = new SceneNode(
                SceneNodeId.New(),
                Path.GetFileName(filePath),
                Matrix4x4.Identity,
                parentId: null,
                [primitive.Id]);
            var asset = new ImportedSceneAsset(
                filePath,
                Path.GetFileName(filePath),
                [rootNode],
                [primitive],
                [material]);

            Log.Loaded(log, meshData.Vertices.Length, meshData.Indices.Length);
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

    private static MeshData LoadSimpleObj(string path)
    {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var finalVertices = new List<VertexPositionNormalColor>();
        var finalIndices = new List<uint>();

        foreach (var line in File.ReadLines(path))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                continue;
            }

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
                        {
                            continue;
                        }

                        var vIdx = vIdxRaw > 0 ? vIdxRaw - 1 : vIdxRaw + vertices.Count;
                        if (vIdx < 0 || vIdx >= vertices.Count)
                        {
                            continue;
                        }

                        var nIdx = 0;
                        if (indices.Length > 2 &&
                            !string.IsNullOrEmpty(indices[2]) &&
                            int.TryParse(indices[2], out var nIdxRaw))
                        {
                            nIdx = nIdxRaw > 0 ? nIdxRaw - 1 : nIdxRaw + normals.Count;
                            if (nIdx < 0 || nIdx >= normals.Count)
                            {
                                nIdx = 0;
                            }
                        }

                        var vertex = vertices[vIdx];
                        var normal = normals.Count > 0 ? normals[nIdx] : Vector3.UnitY;

                        finalVertices.Add(new VertexPositionNormalColor(
                            vertex,
                            normal,
                            new RgbaFloat(0.7f, 0.7f, 0.7f, 1f)));
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

    private sealed class RegistryModelImporter : IVideraModelImporter
    {
        private readonly ILogger? _logger;

        public RegistryModelImporter(ILogger? logger)
        {
            _logger = logger;
        }

        public bool CanImport(string path)
        {
            return !string.IsNullOrWhiteSpace(path) && AllowedExtensions.Contains(Path.GetExtension(path));
        }

        public ValueTask<ModelImportResult> ImportAsync(
            ModelImportRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            var startedAt = Stopwatch.StartNew();
            try
            {
                var asset = Import(request.Path, _logger);
                cancellationToken.ThrowIfCancellationRequested();
                var duration = startedAt.Elapsed == TimeSpan.Zero ? TimeSpan.FromTicks(1) : startedAt.Elapsed;
                return ValueTask.FromResult(ModelImportResult.Success(
                    asset,
                    [new ModelImportDiagnostic(ModelImportDiagnosticSeverity.Info, $"Imported OBJ model '{request.Path}'.")],
                    duration));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var duration = startedAt.Elapsed == TimeSpan.Zero ? TimeSpan.FromTicks(1) : startedAt.Elapsed;
                return ValueTask.FromResult(ModelImportResult.Failed(ex, duration));
            }
        }
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[ObjModelImporter] Loading: {FilePath}")]
        public static partial void Loading(ILogger logger, string filePath);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "[ObjModelImporter] Loaded {VertexCount} vertices, {IndexCount} indices")]
        public static partial void Loaded(ILogger logger, int vertexCount, int indexCount);

        [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "[ObjModelImporter] Initializing GPU resources...")]
        public static partial void InitializingGpuResources(ILogger logger);

        [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "[ObjModelImporter] Successfully loaded: {FilePath}")]
        public static partial void LoadSucceeded(ILogger logger, string filePath);

        [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "[ObjModelImporter] Failed to load: {FilePath}: {Error}")]
        public static partial void LoadFailed(ILogger logger, string filePath, string error, Exception exception);
    }
}
