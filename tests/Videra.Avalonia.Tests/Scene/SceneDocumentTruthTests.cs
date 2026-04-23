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
        var sceneObject = new Object3D { Name = "host-owned" };
        var document = new SceneDocument(new[] { sceneObject });

        document.Version.Should().Be(1);
        document.Entries.Should().ContainSingle();

        var entry = document.Entries[0];
        entry.Id.Value.Should().NotBe(Guid.Empty);
        entry.Name.Should().Be(sceneObject.Name);
        entry.ImportedAsset.Should().BeNull();
        entry.Ownership.Should().Be(SceneOwnership.ExternalObject);

        typeof(SceneDocument)
            .GetProperty("SceneObjects", BindingFlags.Instance | BindingFlags.Public)
            .Should()
            .BeNull();

        typeof(SceneDocumentEntry)
            .GetProperty("SceneObject", BindingFlags.Instance | BindingFlags.Public)
            .Should()
            .BeNull();
    }

    [Fact]
    public void Internal_scene_document_flattens_runtime_objects_from_one_imported_entry()
    {
        var asset = SceneTestMeshes.CreateImportedAsset();
        var first = SceneTestMeshes.CreateDeferredObject("first");
        var second = SceneTestMeshes.CreateDeferredObject("second");
        var entry = _mutator.CreateImportedEntry([first, second], asset);
        var document = new SceneDocument([entry]);

        document.Entries.Should().ContainSingle();
        document.SceneObjects.Should().ContainInOrder(first, second);
        entry.RuntimeObjects.Should().ContainInOrder(first, second);
    }
}
