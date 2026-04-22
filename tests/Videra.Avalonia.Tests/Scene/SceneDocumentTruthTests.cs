using System.Reflection;
using FluentAssertions;
using Videra.Core.Graphics;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Avalonia.Tests.Scene;

public sealed class SceneDocumentTruthTests
{
    private readonly SceneDocumentMutator _mutator = new();

    [Fact]
    public void Public_scene_document_exposes_truth_entries_without_public_object_references()
    {
        var asset = SceneTestMeshes.CreateImportedAsset("sample.obj");
        var sceneObject = SceneObjectFactory.CreateDeferred(asset);

        var document = _mutator.Add(
            SceneDocument.Empty,
            sceneObject,
            asset,
            SceneOwnership.RuntimeOwnedImported);

        document.Version.Should().Be(1);
        document.Entries.Should().ContainSingle();

        var entry = document.Entries[0];
        entry.Id.Value.Should().NotBe(Guid.Empty);
        entry.Name.Should().Be(asset.Name);
        entry.ImportedAsset.Should().BeSameAs(asset);
        entry.Ownership.Should().Be(SceneOwnership.RuntimeOwnedImported);

        typeof(SceneDocument)
            .GetProperty("SceneObjects", BindingFlags.Instance | BindingFlags.Public)
            .Should()
            .BeNull();

        typeof(SceneDocumentEntry)
            .GetProperty("SceneObject", BindingFlags.Instance | BindingFlags.Public)
            .Should()
            .BeNull();
    }
}
