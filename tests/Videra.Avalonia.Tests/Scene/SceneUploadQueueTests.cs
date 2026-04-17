using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Videra.Avalonia.Runtime.Scene;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Avalonia.Tests.Scene;

public sealed class SceneUploadQueueTests
{
    private readonly SceneDocumentMutator _mutator = new();

    [Fact]
    public void Drain_uploads_pending_entries_within_budget_and_marks_them_resident()
    {
        var asset = SceneTestMeshes.CreateImportedAsset();
        var sceneObject = SceneObjectFactory.CreateDeferred(asset);
        var entry = _mutator.CreateImportedEntry(sceneObject, asset);
        var registry = new SceneResidencyRegistry();
        registry.Apply(new SceneDelta([entry], Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDocumentEntry>()), 1);

        var queue = new SceneUploadQueue();
        queue.Enqueue([entry]);

        var result = queue.Drain(
            new RecordingResourceFactory(),
            SceneUploadBudget.Idle,
            resourceEpoch: 2,
            registry,
            NullLogger.Instance);

        result.UploadedRecords.Should().ContainSingle();
        registry.TryGet(entry.Id, out var record).Should().BeTrue();
        record.State.Should().Be(SceneResidencyState.Resident);
    }

    [Fact]
    public void Drain_respects_object_budget_and_leaves_remaining_entries_pending()
    {
        var assetA = SceneTestMeshes.CreateImportedAsset("triangle-a.obj");
        var assetB = SceneTestMeshes.CreateImportedAsset("triangle-b.obj");
        var entryA = _mutator.CreateImportedEntry(SceneObjectFactory.CreateDeferred(assetA), assetA);
        var entryB = _mutator.CreateImportedEntry(SceneObjectFactory.CreateDeferred(assetB), assetB);
        var registry = new SceneResidencyRegistry();
        registry.Apply(new SceneDelta([entryA, entryB], Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDocumentEntry>()), 1);

        var queue = new SceneUploadQueue();
        queue.Enqueue([entryA, entryB]);

        var result = queue.Drain(
            new RecordingResourceFactory(),
            new SceneUploadBudget(MaxObjectsPerFrame: 1, MaxBytesPerFrame: long.MaxValue),
            resourceEpoch: 2,
            registry,
            NullLogger.Instance);

        result.UploadedRecords.Should().ContainSingle();
        registry.TryGet(entryA.Id, out var recordA).Should().BeTrue();
        registry.TryGet(entryB.Id, out var recordB).Should().BeTrue();
        new[] { recordA.State, recordB.State }.Should().Contain(SceneResidencyState.Resident);
        new[] { recordA.State, recordB.State }.Should().Contain(SceneResidencyState.PendingUpload);
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
