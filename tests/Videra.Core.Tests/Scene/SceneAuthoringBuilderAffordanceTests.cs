using System.Numerics;
using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Core.Tests.Scene;

public sealed class SceneAuthoringBuilderAffordanceTests
{
    [Fact]
    public void Build_WithPlacementOverload_ProducesSameRetainedTransformAsMatrixPath()
    {
        var material = SceneMaterials.Matte("paint", RgbaFloat.Red);
        var nodeId = SceneNodeId.New();
        var primitiveId = MeshPrimitiveId.New();
        var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI / 2f);
        var placement = SceneAuthoringPlacement.From(
            new Vector3(1f, 2f, 3f),
            rotation,
            new Vector3(2f, 3f, 4f));
        var expectedTransform = Matrix4x4.CreateScale(2f, 3f, 4f)
            * Matrix4x4.CreateFromQuaternion(rotation)
            * Matrix4x4.CreateTranslation(1f, 2f, 3f);

        var document = SceneAuthoring.Create("placed")
            .AddCube("box", material, placement, size: 2f, nodeId: nodeId, primitiveId: primitiveId)
            .Build();

        var asset = document.Entries.Should().ContainSingle().Which.ImportedAsset;
        asset.Should().NotBeNull();

        var node = asset!.Nodes.Should().ContainSingle().Which;
        node.Id.Should().Be(nodeId);
        node.Name.Should().Be("box");
        node.LocalTransform.Should().Be(expectedTransform);
        node.PrimitiveIds.Should().Equal(primitiveId);

        var primitive = asset.Primitives.Should().ContainSingle().Which;
        primitive.Id.Should().Be(primitiveId);
        primitive.Name.Should().Be("box");
        primitive.MaterialId.Should().Be(material.Id);
    }

    [Fact]
    public void Build_WithInlineMatteColor_UsesNameColorAndPlacementInRetainedDocument()
    {
        var color = new RgbaFloat(0.2f, 0.4f, 0.8f, 1f);
        var placement = SceneAuthoringPlacement.At(0f, 1.5f, 0f);

        var document = SceneAuthoring.Create("inline-style")
            .AddSphere("focus", color, placement, radius: 1f, segments: 8, rings: 4)
            .Build();

        var asset = document.Entries.Should().ContainSingle().Which.ImportedAsset;
        asset.Should().NotBeNull();

        asset!.Nodes.Should().ContainSingle().Which.LocalTransform.Should().Be(Matrix4x4.CreateTranslation(0f, 1.5f, 0f));

        var material = asset.Materials.Should().ContainSingle().Which;
        material.Name.Should().Be("focus");
        material.BaseColorFactor.Should().Be(color);

        var primitive = asset.Primitives.Should().ContainSingle().Which;
        primitive.Name.Should().Be("focus");
        primitive.MaterialId.Should().Be(material.Id);
        primitive.MeshData.Vertices.Select(vertex => vertex.Color).Should().OnlyContain(vertexColor => vertexColor == color);
    }

    [Fact]
    public void Build_WithPlacementInstances_WritesMatricesToInstanceBatchEntry()
    {
        var material = SceneMaterials.Matte("marker", RgbaFloat.White);
        var placements = new[]
        {
            SceneAuthoringPlacement.Identity,
            SceneAuthoringPlacement.At(3f, 0f, 0f),
            SceneAuthoringPlacement.At(6f, 0f, 0f)
        };
        var expectedTransforms = placements.Select(placement => placement.ToMatrix()).ToArray();

        var document = SceneAuthoring.Create("placed-instances")
            .AddInstances(
                "markers",
                SceneGeometry.Cube(size: 1f, material.BaseColorFactor),
                material,
                placements)
            .Build();

        document.Entries.Should().BeEmpty();
        var batch = document.InstanceBatches.Should().ContainSingle().Which;
        batch.Name.Should().Be("markers");
        batch.Mesh.Name.Should().Be("markers");
        batch.Material.Should().BeSameAs(material);
        batch.Transforms.ToArray().Should().Equal(expectedTransforms);
        batch.InstanceCount.Should().Be(3);
        batch.Bounds.Min.Should().Be(new Vector3(-0.5f, -0.5f, -0.5f));
        batch.Bounds.Max.Should().Be(new Vector3(6.5f, 0.5f, 0.5f));
    }
}
