using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Videra.Avalonia.Runtime.Scene;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Avalonia.Tests.Scene;

public sealed class RuntimeFramePreludeTests
{
    private readonly SceneDocumentMutator _mutator = new();

    [Fact]
    public void Execute_does_not_dirty_resident_entries_without_an_event()
    {
        var asset = SceneTestMeshes.CreateImportedAsset();
        var sceneObject = SceneObjectFactory.CreateDeferred(asset);
        var entry = _mutator.CreateImportedEntry(sceneObject, asset);
        var registry = new SceneResidencyRegistry();
        registry.Apply(
            new SceneDelta([entry], Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDocumentEntry>()),
            resourceEpoch: 1);
        registry.MarkResident(entry.Id, resourceEpoch: 1);

        using var engine = new VideraEngine();
        var prelude = new RuntimeFramePrelude(
            new SceneUploadQueue(),
            registry,
            engine,
            resourceFactoryAccessor: static () => null,
            isInteractiveAccessor: static () => false,
            resourceEpochAccessor: static () => 1UL,
            afterSceneApplied: static () => { },
            NullLogger.Instance);

        var result = prelude.Execute();

        result.HasChanges.Should().BeFalse();
        registry.CreateDiagnostics(sceneDocumentVersion: 1).DirtyObjects.Should().Be(0);
        registry.CreateDiagnostics(sceneDocumentVersion: 1).ResidentObjects.Should().Be(1);
    }

    [Fact]
    public void Execute_reports_upload_budget_and_flush_telemetry()
    {
        var asset = SceneTestMeshes.CreateImportedAsset();
        var sceneObject = SceneObjectFactory.CreateDeferred(asset);
        var entry = _mutator.CreateImportedEntry(sceneObject, asset);
        var registry = new SceneResidencyRegistry();
        registry.Apply(
            new SceneDelta([entry], Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDocumentEntry>()),
            resourceEpoch: 1);

        using var engine = new VideraEngine();
        var queue = new SceneUploadQueue();
        queue.Enqueue([entry]);
        var prelude = new RuntimeFramePrelude(
            queue,
            registry,
            engine,
            resourceFactoryAccessor: static () => new RecordingResourceFactory(),
            isInteractiveAccessor: static () => false,
            resourceEpochAccessor: static () => 2UL,
            afterSceneApplied: static () => { },
            NullLogger.Instance);

        var result = prelude.Execute();

        result.UploadedRecords.Should().ContainSingle();
        result.UploadedBytes.Should().BeGreaterThan(0);
        result.ResolvedBudget.MaxObjectsPerFrame.Should().BeGreaterThan(0);
        result.ResolvedBudget.MaxBytesPerFrame.Should().BeGreaterThan(0);
        result.Duration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
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
