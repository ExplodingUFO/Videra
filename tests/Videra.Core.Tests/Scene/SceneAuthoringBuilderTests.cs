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
    public void Build_WithAuthoredInstances_AddsSingleBatchWithPerformanceTruth()
    {
        var material = SceneMaterials.Matte("paint", RgbaFloat.White);
        var transforms = new[]
        {
            Matrix4x4.Identity,
            Matrix4x4.CreateTranslation(3f, 0f, 0f),
            Matrix4x4.CreateTranslation(6f, 0f, 0f)
        };
        var objectIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var document = SceneAuthoring.Create("markers")
            .AddInstances(
                "marker-cubes",
                SceneGeometry.Cube(size: 1f, material.BaseColorFactor),
                material,
                transforms,
                objectIds: objectIds,
                pickable: true)
            .Build();

        document.Entries.Should().BeEmpty();
        var batch = document.InstanceBatches.Should().ContainSingle().Which;
        batch.Name.Should().Be("marker-cubes");
        batch.Mesh.Name.Should().Be("marker-cubes");
        batch.Material.Should().BeSameAs(material);
        batch.InstanceCount.Should().Be(3);
        batch.ObjectIds.ToArray().Should().Equal(objectIds);
        batch.Pickable.Should().BeTrue();
        batch.Bounds.Min.Should().Be(new Vector3(-0.5f, -0.5f, -0.5f));
        batch.Bounds.Max.Should().Be(new Vector3(6.5f, 0.5f, 0.5f));
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
    public void AxisLine_CreatesBoundedLineTopologyWithColor()
    {
        var color = new RgbaFloat(0.8f, 0.2f, 0.1f, 1f);

        var mesh = SceneGeometry.AxisLine(Vector3.UnitX, length: 2f, color);

        mesh.Topology.Should().Be(MeshTopology.Lines);
        mesh.Vertices.Should().HaveCount(2);
        mesh.Indices.Should().Equal(0u, 1u);
        mesh.Vertices[0].Position.Should().Be(Vector3.Zero);
        mesh.Vertices[1].Position.Should().Be(new Vector3(2f, 0f, 0f));
        mesh.Vertices.Select(vertex => vertex.Color).Should().OnlyContain(vertexColor => vertexColor == color);
    }

    [Fact]
    public void Build_WithAxisTriad_CreatesRetainedAxisLinePrimitivesAndMaterials()
    {
        var document = SceneAuthoring.Create("axis-scene")
            .AddAxisTriad("world", length: 3f, transform: Matrix4x4.CreateTranslation(1f, 2f, 3f))
            .Build();

        document.InstanceBatches.Should().BeEmpty();
        var asset = document.Entries.Should().ContainSingle().Which.ImportedAsset;
        asset.Should().NotBeNull();
        asset!.Nodes.Should().HaveCount(3);
        asset.Primitives.Should().HaveCount(3);
        asset.Materials.Should().HaveCount(3);

        asset.Nodes.Should().OnlyContain(node => node.LocalTransform == Matrix4x4.CreateTranslation(1f, 2f, 3f));
        asset.Primitives.Should().Contain(primitive =>
            primitive.Name == "world-x-axis" &&
            primitive.MeshData.Topology == MeshTopology.Lines &&
            primitive.MeshData.Vertices[1].Position == new Vector3(3f, 0f, 0f));
        asset.Primitives.Should().Contain(primitive =>
            primitive.Name == "world-y-axis" &&
            primitive.MeshData.Topology == MeshTopology.Lines &&
            primitive.MeshData.Vertices[1].Position == new Vector3(0f, 3f, 0f));
        asset.Primitives.Should().Contain(primitive =>
            primitive.Name == "world-z-axis" &&
            primitive.MeshData.Topology == MeshTopology.Lines &&
            primitive.MeshData.Vertices[1].Position == new Vector3(0f, 0f, 3f));

        asset.Materials.Should().Contain(material => material.Name == "world-x-axis" && material.BaseColorFactor == RgbaFloat.Red);
        asset.Materials.Should().Contain(material => material.Name == "world-y-axis" && material.BaseColorFactor == RgbaFloat.Green);
        asset.Materials.Should().Contain(material => material.Name == "world-z-axis" && material.BaseColorFactor == RgbaFloat.Blue);
    }

    [Fact]
    public void Validate_WithRepeatedAxisTriads_DoesNotReportGeneratedIdErrors()
    {
        var diagnostics = SceneAuthoring.Create("axis-scene")
            .AddAxisTriad("world")
            .AddAxisTriad("local", transform: Matrix4x4.CreateTranslation(1f, 0f, 0f))
            .Validate();

        diagnostics.Should().NotContain(diagnostic =>
            diagnostic.Code == "node.id.duplicate" || diagnostic.Code == "mesh.id.duplicate");
    }

    [Fact]
    public void Build_WithAxisTriadAndInstances_PreservesDocumentAndBatchTruth()
    {
        var material = SceneMaterials.Matte("marker", RgbaFloat.White);

        var document = SceneAuthoring.Create("axis-and-markers")
            .AddAxisTriad("world")
            .AddInstances(
                "markers",
                SceneGeometry.Cube(size: 0.2f, material.BaseColorFactor),
                material,
                new[] { Matrix4x4.Identity, Matrix4x4.CreateTranslation(1f, 0f, 0f) })
            .Build();

        document.Entries.Should().ContainSingle();
        document.Entries[0].ImportedAsset!.Primitives.Should().HaveCount(3);
        document.InstanceBatches.Should().ContainSingle().Which.Name.Should().Be("markers");
    }

    [Fact]
    public void Sphere_CreatesIndexedTriangleMeshWithExpectedBoundsAndColor()
    {
        var color = new RgbaFloat(0.2f, 0.4f, 0.8f, 1f);

        var mesh = SceneGeometry.Sphere(radius: 2f, segments: 8, rings: 4, color: color);

        mesh.Topology.Should().Be(MeshTopology.Triangles);
        mesh.Vertices.Should().HaveCount(26);
        mesh.Indices.Should().HaveCount(144);
        mesh.Indices.Should().OnlyContain(index => index < mesh.Vertices.Length);
        mesh.Vertices.Select(vertex => vertex.Color).Should().OnlyContain(vertexColor => vertexColor == color);
        mesh.Vertices.Select(vertex => vertex.Position.Y).Should().Contain(2f);
        mesh.Vertices.Select(vertex => vertex.Position.Y).Should().Contain(-2f);
        mesh.Vertices.Should().OnlyContain(vertex => Math.Abs(vertex.Normal.Length() - 1f) < 0.001f);
    }

    [Fact]
    public void Build_WithSphere_AddsAuthoredSpherePrimitive()
    {
        var material = SceneMaterials.Matte("paint", RgbaFloat.Blue);

        var document = SceneAuthoring.Create("sphere-scene")
            .AddSphere("marker", material, radius: 1.5f, segments: 12, rings: 6)
            .Build();

        var primitive = document.Entries.Single().ImportedAsset!.Primitives.Should().ContainSingle().Which;
        primitive.Name.Should().Be("marker");
        primitive.MaterialId.Should().Be(material.Id);
        primitive.MeshData.Vertices.Should().HaveCount(62);
        primitive.MeshData.Indices.Should().HaveCount(360);
    }

    [Fact]
    public void Build_WithAuthoredPrimitiveAndMarkerInstances_PreservesDocumentAndBatchTruth()
    {
        var ground = SceneMaterials.Matte("ground", RgbaFloat.LightGrey);
        var marker = SceneMaterials.Matte("marker", RgbaFloat.White);
        var markerTransforms = new[]
        {
            Matrix4x4.CreateTranslation(-1f, 0.25f, 0f),
            Matrix4x4.CreateTranslation(0f, 0.25f, 0f),
            Matrix4x4.CreateTranslation(1f, 0.25f, 0f)
        };
        var markerColors = new[]
        {
            RgbaFloat.Red,
            RgbaFloat.Green,
            RgbaFloat.Blue
        };

        var document = SceneAuthoring.Create("minimal-authoring")
            .AddPlane("ground", ground, width: 4f, depth: 2f)
            .AddSphere("focus", marker, radius: 0.35f, transform: Matrix4x4.CreateTranslation(0f, 0.35f, 0f))
            .AddInstances(
                "sample-markers",
                SceneGeometry.Sphere(radius: 0.1f, segments: 8, rings: 4, color: marker.BaseColorFactor),
                marker,
                markerTransforms,
                markerColors,
                objectIds: new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() },
                pickable: true)
            .Build();

        document.Entries.Should().ContainSingle();
        document.Entries[0].ImportedAsset!.Primitives.Should().HaveCount(2);
        var batch = document.InstanceBatches.Should().ContainSingle().Which;
        batch.Name.Should().Be("sample-markers");
        batch.InstanceCount.Should().Be(3);
        batch.HasPerInstanceColors.Should().BeTrue();
        batch.HasObjectIds.Should().BeTrue();
        batch.Pickable.Should().BeTrue();
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
