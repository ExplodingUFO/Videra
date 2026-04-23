using FluentAssertions;
using Videra.Avalonia.Runtime.Scene;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Avalonia.Tests.Scene;

public sealed class SceneResidencyRegistryTests
{
    private readonly SceneDocumentMutator _mutator = new();

    [Fact]
    public void Apply_marks_imported_deferred_entries_as_pending_upload()
    {
        var asset = SceneTestMeshes.CreateImportedAsset();
        var sceneObject = SceneObjectFactory.CreateDeferred(asset);
        var entry = _mutator.CreateImportedEntry(sceneObject, asset);
        var delta = new SceneDelta([entry], Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDeltaChange>());
        var registry = new SceneResidencyRegistry();

        registry.Apply(delta, resourceEpoch: 1);

        registry.TryGet(entry.Id, out var record).Should().BeTrue();
        record.State.Should().Be(SceneResidencyState.PendingUpload);
        record.ApproximateUploadBytes.Should().Be(asset.Metrics!.ApproximateGpuBytes);
    }

    [Fact]
    public void Mark_dirty_for_resource_epoch_only_marks_resident_rehydratable_objects()
    {
        var registry = new SceneResidencyRegistry();
        var sceneObject = SceneTestMeshes.CreateDeferredObject();
        var entry = _mutator.CreateExternalEntry(sceneObject);
        registry.Apply(new SceneDelta([entry], Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDocumentEntry>(), Array.Empty<SceneDeltaChange>()), 1);
        registry.MarkResident(entry.Id, resourceEpoch: 1);

        var dirty = registry.MarkDirtyForResourceEpoch(2);

        dirty.Should().ContainSingle();
        dirty[0].State.Should().Be(SceneResidencyState.Dirty);
    }
}
