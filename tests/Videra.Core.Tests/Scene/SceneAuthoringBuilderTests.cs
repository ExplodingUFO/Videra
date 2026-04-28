using System.Numerics;
using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Core.Tests.Scene;

public sealed class SceneAuthoringBuilderTests
{
    [Fact]
    public void Build_WithPrimitiveAndMaterialPreset_CreatesRetainedSceneDocument()
    {
        var material = SceneMaterials.Matte("paint", RgbaFloat.Red);

        var document = SceneAuthoring.Create("authoring-flow")
            .AddCube("box", material, size: 2f, transform: Matrix4x4.CreateTranslation(1f, 2f, 3f))
            .Build();

        document.Entries.Should().ContainSingle();
        document.InstanceBatches.Should().BeEmpty();
        var entry = document.Entries[0];
        entry.Name.Should().Be("authoring-flow");
        entry.Ownership.Should().Be(SceneOwnership.RuntimeOwnedImported);
        entry.ImportedAsset.Should().NotBeNull();
        entry.ImportedAsset!.Name.Should().Be("authoring-flow");
        entry.ImportedAsset.Nodes.Should().ContainSingle(node => node.Name == "box");
        entry.ImportedAsset.Primitives.Should().ContainSingle(primitive =>
            primitive.Name == "box" &&
            primitive.MaterialId == material.Id &&
            primitive.MeshData.Vertices.Length == 24 &&
            primitive.MeshData.Indices.Length == 36);
        entry.ImportedAsset.Materials.Should().ContainSingle().Which.Should().BeSameAs(material);
        entry.RuntimeObjects.Should().ContainSingle(runtimeObject => runtimeObject.Name == "authoring-flow::box::box");
    }

    [Fact]
    public void TryBuild_WithInvalidMesh_ReturnsDiagnosticsWithoutDocument()
    {
        var material = SceneMaterials.Matte("paint", RgbaFloat.White);
        var mesh = new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(Vector3.Zero, Vector3.UnitZ, RgbaFloat.White)
            ],
            Indices = [0, 1, 2]
        };

        var result = SceneAuthoring.Create("invalid")
            .AddMesh("bad-triangle", mesh, material)
            .TryBuild();

        result.Succeeded.Should().BeFalse();
        result.Document.Should().BeNull();
        result.Diagnostics.Should().ContainSingle(diagnostic =>
            diagnostic.Severity == SceneAuthoringDiagnosticSeverity.Error &&
            diagnostic.Code == "mesh.index.range" &&
            diagnostic.Target == "bad-triangle");
    }

    [Fact]
    public void Build_WithInstanceBatch_AddsBatchToDocument()
    {
        var material = SceneMaterials.AlphaMask("masked", RgbaFloat.Green);
        var mesh = new MeshPrimitive(MeshPrimitiveId.New(), "quad", SceneGeometry.Quad(2f, 2f), material.Id);
        var batch = new InstanceBatchDescriptor(
            "tiles",
            mesh,
            material,
            new[] { Matrix4x4.Identity, Matrix4x4.CreateTranslation(5f, 0f, 0f) },
            objectIds: new[] { Guid.NewGuid(), Guid.NewGuid() },
            pickable: true);

        var document = SceneAuthoring.Create("batched")
            .AddInstanceBatch(batch)
            .Build();

        document.Entries.Should().BeEmpty();
        var entry = document.InstanceBatches.Should().ContainSingle().Which;
        entry.Name.Should().Be("tiles");
        entry.Mesh.Should().BeSameAs(mesh);
        entry.Material.Should().BeSameAs(material);
        entry.InstanceCount.Should().Be(2);
    }

    [Fact]
    public void Build_WithPlaneGridPolylineAndPointCloud_CreatesExpectedTopologies()
    {
        var material = SceneMaterials.Matte("paint", RgbaFloat.White);

        var document = SceneAuthoring.Create("primitives")
            .AddPlane("plane", material, width: 4f, depth: 2f)
            .AddGrid("grid", material, width: 4f, depth: 2f, divisions: 2)
            .AddPolyline("path", material, [Vector3.Zero, Vector3.UnitX, Vector3.One])
            .AddPointCloud("markers", material, [Vector3.Zero, Vector3.UnitY])
            .Build();

        var primitives = document.Entries.Single().ImportedAsset!.Primitives;
        primitives.Should().Contain(primitive =>
            primitive.Name == "plane" &&
            primitive.MeshData.Topology == MeshTopology.Triangles &&
            primitive.MeshData.TextureCoordinateSets.Single().Coordinates.Length == 4);
        primitives.Should().Contain(primitive =>
            primitive.Name == "grid" &&
            primitive.MeshData.Topology == MeshTopology.Lines &&
            primitive.MeshData.Indices.Length == 12);
        primitives.Should().Contain(primitive =>
            primitive.Name == "path" &&
            primitive.MeshData.Topology == MeshTopology.Lines &&
            primitive.MeshData.Indices.Length == 4);
        primitives.Should().Contain(primitive =>
            primitive.Name == "markers" &&
            primitive.MeshData.Topology == MeshTopology.Points &&
            primitive.MeshData.Indices.Length == 2);
    }

    [Fact]
    public void Validate_WithDuplicateIds_ReportsAuthoringErrors()
    {
        var material = SceneMaterials.Matte("paint", RgbaFloat.Blue);
        var nodeId = SceneNodeId.New();
        var primitiveId = MeshPrimitiveId.New();

        var diagnostics = SceneAuthoring.Create("duplicates")
            .AddTriangle("a", material, nodeId: nodeId, primitiveId: primitiveId)
            .AddTriangle("b", material, nodeId: nodeId, primitiveId: primitiveId)
            .Validate();

        diagnostics.Should().Contain(diagnostic => diagnostic.Code == "node.id.duplicate" && diagnostic.Target == "b");
        diagnostics.Should().Contain(diagnostic => diagnostic.Code == "mesh.id.duplicate" && diagnostic.Target == "b");
    }
}
