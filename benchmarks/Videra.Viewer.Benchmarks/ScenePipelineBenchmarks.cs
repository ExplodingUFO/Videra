using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using Videra.Avalonia.Runtime.Scene;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.IO;
using Videra.Core.Scene;

namespace Videra.Viewer.Benchmarks;

[MemoryDiagnoser]
public class ScenePipelineBenchmarks
{
    private readonly SceneDocumentMutator _mutator = new();
    private readonly SceneUploadBudget _idleBudget = SceneUploadBudget.Idle;

    private string _workspace = string.Empty;
    private string _singleImportPath = string.Empty;
    private string[] _batchImportPaths = Array.Empty<string>();

    private SceneImportService _importService = null!;
    private SceneDocument _nextDocument = SceneDocument.Empty;
    private SceneDelta _sceneDelta = SceneDelta.Empty;
    private SceneDocumentEntry _rehydrateEntry = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _workspace = Path.Combine(Path.GetTempPath(), "Videra.Viewer.Benchmarks", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_workspace);

        _singleImportPath = WriteTriangleObj("single-import.obj", 0f);
        _batchImportPaths =
        [
            WriteTriangleObj("batch-import-a.obj", 0f),
            WriteTriangleObj("batch-import-b.obj", 1f),
            WriteTriangleObj("batch-import-c.obj", 2f),
            WriteTriangleObj("batch-import-d.obj", 3f)
        ];

        _importService = new SceneImportService(_mutator);

        var previousAsset = ModelImporter.Import(WriteTriangleObj("delta-previous.obj", 4f));
        var nextAsset = ModelImporter.Import(WriteTriangleObj("delta-next.obj", 5f));

        var previousObject = SceneObjectFactory.CreateDeferred(previousAsset);
        var nextObject = SceneObjectFactory.CreateDeferred(nextAsset);
        var retainedObject = SceneObjectFactory.CreateDeferred(previousAsset);

        var retainedEntry = _mutator.CreateImportedEntry(retainedObject, previousAsset);
        var removedEntry = _mutator.CreateImportedEntry(previousObject, previousAsset);
        var addedEntry = _mutator.CreateImportedEntry(nextObject, nextAsset);

        var previousDocument = _mutator.ReplaceEntries(SceneDocument.Empty, [retainedEntry, removedEntry]);
        _nextDocument = _mutator.ReplaceEntries(SceneDocument.Empty, [retainedEntry, addedEntry]);
        _sceneDelta = SceneDeltaPlanner.Diff(previousDocument, _nextDocument);

        var rehydrateAsset = ModelImporter.Import(WriteTriangleObj("rehydrate.obj", 6f));
        var rehydrateObject = SceneObjectFactory.CreateDeferred(rehydrateAsset);
        _rehydrateEntry = _mutator.CreateImportedEntry(rehydrateObject, rehydrateAsset);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        if (!string.IsNullOrWhiteSpace(_workspace) && Directory.Exists(_workspace))
        {
            Directory.Delete(_workspace, recursive: true);
        }
    }

    [Benchmark]
    public int ModelImporter_Import()
    {
        return ModelImporter.Import(_singleImportPath).MeshData.Vertices.Length;
    }

    [Benchmark]
    public async Task<int> SceneImportService_ImportBatchAsync()
    {
        var result = await _importService.ImportBatchAsync(_batchImportPaths, CancellationToken.None).ConfigureAwait(false);
        return result.SceneObjects.Count;
    }

    [Benchmark]
    public int SceneResidencyRegistry_ApplyDelta()
    {
        var registry = new SceneResidencyRegistry();
        registry.Apply(_sceneDelta, resourceEpoch: 1);
        return registry.CreateDiagnostics(_nextDocument.Version).PendingUploads;
    }

    [Benchmark]
    public int SceneUploadQueue_Drain()
    {
        var registry = new SceneResidencyRegistry();
        registry.Apply(new SceneDelta([_rehydrateEntry], Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDocumentEntry>()), resourceEpoch: 1);

        var queue = new SceneUploadQueue();
        queue.Enqueue([_rehydrateEntry]);

        return queue.Drain(
            new RecordingResourceFactory(),
            _idleBudget,
            resourceEpoch: 2,
            registry,
            NullLogger.Instance).UploadedRecords.Count;
    }

    [Benchmark]
    public int ScenePipeline_RehydrateAfterBackendReady()
    {
        var registry = new SceneResidencyRegistry();
        registry.Apply(new SceneDelta([_rehydrateEntry], Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDocumentEntry>()), resourceEpoch: 1);

        var queue = new SceneUploadQueue();
        queue.Enqueue([_rehydrateEntry]);
        _ = queue.Drain(
            new RecordingResourceFactory(),
            _idleBudget,
            resourceEpoch: 2,
            registry,
            NullLogger.Instance);

        var dirtyRecords = registry.MarkDirtyForResourceEpoch(resourceEpoch: 3);
        queue.Enqueue(dirtyRecords);

        return queue.Drain(
            new RecordingResourceFactory(),
            _idleBudget,
            resourceEpoch: 3,
            registry,
            NullLogger.Instance).UploadedRecords.Count;
    }

    private string WriteTriangleObj(string fileName, float offsetX)
    {
        var path = Path.Combine(_workspace, fileName);
        File.WriteAllText(path, $$"""
            v {{offsetX:0.0}} 0.0 0.0
            v {{offsetX + 1f:0.0}} 0.0 0.0
            v {{offsetX:0.0}} 1.0 0.0
            vn 0.0 0.0 1.0
            f 1//1 2//1 3//1
            """);
        return path;
    }

    private sealed class RecordingResourceFactory : IResourceFactory
    {
        public IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices) => new RecordingBuffer((uint)(vertices.Length * sizeof(float) * 10));

        public IBuffer CreateVertexBuffer(uint sizeInBytes) => new RecordingBuffer(sizeInBytes);

        public IBuffer CreateIndexBuffer(uint[] indices) => new RecordingBuffer((uint)(indices.Length * sizeof(uint)));

        public IBuffer CreateIndexBuffer(uint sizeInBytes) => new RecordingBuffer(sizeInBytes);

        public IBuffer CreateUniformBuffer(uint sizeInBytes) => new RecordingBuffer(sizeInBytes);

        public IPipeline CreatePipeline(PipelineDescription description) => new RecordingPipeline();

        public IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors) => new RecordingPipeline();

        public IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint) => new RecordingShader();

        public IResourceSet CreateResourceSet(ResourceSetDescription description) => new RecordingResourceSet();
    }

    private sealed class RecordingBuffer(uint sizeInBytes) : IBuffer
    {
        public uint SizeInBytes { get; } = sizeInBytes;

        public void Update<T>(T data) where T : unmanaged
        {
        }

        public void UpdateArray<T>(T[] data) where T : unmanaged
        {
        }

        public void SetData<T>(T data, uint offset) where T : unmanaged
        {
        }

        public void SetData<T>(T[] data, uint offset) where T : unmanaged
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class RecordingPipeline : IPipeline
    {
        public void Dispose()
        {
        }
    }

    private sealed class RecordingShader : IShader
    {
        public void Dispose()
        {
        }
    }

    private sealed class RecordingResourceSet : IResourceSet
    {
        public void Dispose()
        {
        }
    }
}
