using FluentAssertions;
using Videra.Core.Graphics;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Avalonia.Tests.Scene;

public sealed class SceneDocumentMutatorTests
{
    private readonly SceneDocumentMutator _mutator = new();

    [Fact]
    public void Add_imported_entry_assigns_runtime_owned_identity_and_increments_version()
    {
        var asset = SceneTestMeshes.CreateImportedAsset();
        var sceneObject = SceneObjectFactory.CreateDeferred(asset);

        var document = _mutator.Add(
            SceneDocument.Empty,
            sceneObject,
            asset,
            SceneOwnership.RuntimeOwnedImported);

        document.Version.Should().Be(1);
        document.Entries.Should().ContainSingle();
        document.Entries[0].Name.Should().Be(asset.Name);
        document.Entries[0].Ownership.Should().Be(SceneOwnership.RuntimeOwnedImported);
        document.Entries[0].ImportedAsset.Should().BeSameAs(asset);
        document.Entries[0].Id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Rebuild_preserves_existing_entry_identity_for_same_object_reference()
    {
        var first = new Object3D { Name = "first" };
        var second = new Object3D { Name = "second" };

        var initial = _mutator.Add(SceneDocument.Empty, first);
        initial = _mutator.Add(initial, second);

        var rebuilt = _mutator.RebuildPreservingEntries(initial, [second, first]);

        rebuilt.Version.Should().Be(initial.Version + 1);
        rebuilt.Entries.Select(static entry => entry.Id).Should().BeEquivalentTo(initial.Entries.Select(static entry => entry.Id));
        rebuilt.Entries.Select(static entry => entry.Name).Should().ContainInOrder("second", "first");
    }

    [Fact]
    public void Rebuild_refreshes_entry_name_when_object_name_changes()
    {
        var sceneObject = new Object3D { Name = "original" };

        var initial = _mutator.Add(SceneDocument.Empty, sceneObject);
        sceneObject.Name = "renamed";

        var rebuilt = _mutator.RebuildPreservingEntries(initial, [sceneObject]);

        rebuilt.Version.Should().Be(initial.Version + 1);
        rebuilt.Entries.Should().ContainSingle();
        rebuilt.Entries[0].Id.Should().Be(initial.Entries[0].Id);
        rebuilt.Entries[0].Name.Should().Be("renamed");
        rebuilt.Entries[0].ImportedAsset.Should().BeNull();
        rebuilt.Entries[0].Ownership.Should().Be(SceneOwnership.ExternalObject);
    }
}
