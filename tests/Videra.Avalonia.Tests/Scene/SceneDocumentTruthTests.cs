using System.Reflection;
using FluentAssertions;
using Videra.Core.Graphics;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Avalonia.Tests.Scene;

public sealed class SceneDocumentTruthTests
{
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
}
