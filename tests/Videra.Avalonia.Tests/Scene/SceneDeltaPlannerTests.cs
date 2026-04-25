using FluentAssertions;
using Videra.Avalonia.Runtime.Scene;
using Videra.Core.Graphics;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Avalonia.Tests.Scene;

public sealed class SceneDeltaPlannerTests
{
    private readonly SceneDocumentMutator _mutator = new();

    [Fact]
    public void Diff_tracks_added_removed_and_retained_entries_by_entry_id()
    {
        var first = new Object3D { Name = "first" };
        var second = new Object3D { Name = "second" };
        var third = new Object3D { Name = "third" };

        var previous = _mutator.Add(SceneDocument.Empty, first);
        previous = _mutator.Add(previous, second);

        var next = _mutator.ReplaceEntries(previous, [previous.Entries[1], _mutator.CreateExternalEntry(third)]);

        var delta = SceneDeltaPlanner.Diff(previous, next);

        delta.Added.Should().ContainSingle().Which.SceneObject.Should().BeSameAs(third);
        delta.Removed.Should().ContainSingle().Which.SceneObject.Should().BeSameAs(first);
        delta.Retained.Should().ContainSingle().Which.SceneObject.Should().BeSameAs(second);
        delta.Changed.Should().BeEmpty();
    }

    [Fact]
    public void Diff_classifies_runtime_object_and_imported_asset_changes()
    {
        var asset = SceneTestMeshes.CreateImportedAsset();
        var original = _mutator.CreateImportedEntry(SceneObjectFactory.CreateDeferred(asset), asset);
        var previous = new SceneDocument([original]);
        var replacementObject = SceneTestMeshes.CreateDeferredObject("replacement");
        var runtimeChangedEntry = new SceneDocumentEntry(
            original.Id,
            original.Name,
            [replacementObject],
            original.ImportedAsset,
            original.Ownership);
        var differentAsset = SceneTestMeshes.CreateImportedAsset("other.obj");
        var assetChangedEntry = new SceneDocumentEntry(
            original.Id,
            original.Name,
            original.RuntimeObjects,
            differentAsset,
            original.Ownership);

        var runtimeChanged = SceneDeltaPlanner.Diff(previous, previous.WithEntries([runtimeChangedEntry], previous.Version + 1));
        var assetChanged = SceneDeltaPlanner.Diff(previous, previous.WithEntries([assetChangedEntry], previous.Version + 1));

        runtimeChanged.Changed.Should().ContainSingle();
        runtimeChanged.Changed[0].Kind.Should().Be(SceneDeltaChangeKind.RuntimeObjectsChanged);
        assetChanged.Changed.Should().ContainSingle();
        assetChanged.Changed[0].Kind.Should().Be(SceneDeltaChangeKind.ImportedAssetChanged);
    }

    [Fact]
    public void Diff_detects_runtime_object_changes_in_multi_primitive_entry()
    {
        var first = SceneTestMeshes.CreateDeferredObject("first");
        var second = SceneTestMeshes.CreateDeferredObject("second");
        var replacement = SceneTestMeshes.CreateDeferredObject("replacement");

        var entry = new SceneDocumentEntry(
            SceneEntryId.New(),
            "multi",
            [first, second],
            importedAsset: null,
            SceneOwnership.ExternalObject);

        var previous = new SceneDocument([entry]);
        var changedEntry = new SceneDocumentEntry(
            entry.Id,
            entry.Name,
            [first, replacement],
            entry.ImportedAsset,
            entry.Ownership);
        var next = previous.WithEntries([changedEntry], previous.Version + 1);

        var delta = SceneDeltaPlanner.Diff(previous, next);

        delta.Changed.Should().ContainSingle();
        delta.Changed[0].Kind.Should().Be(SceneDeltaChangeKind.RuntimeObjectsChanged);
    }
}
