using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Videra.Avalonia.Runtime.Scene;
using Videra.Core.Graphics;
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
            new SceneEngineApplicator(),
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
}
