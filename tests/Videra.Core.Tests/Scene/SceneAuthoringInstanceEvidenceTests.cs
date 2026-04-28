using System.Numerics;
using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Core.Tests.Scene;

public sealed class SceneAuthoringInstanceEvidenceTests
{
    [Theory]
    [InlineData(1_000)]
    [InlineData(10_000)]
    public void Build_WithRepeatedAuthoredCubeMarkers_RetainsOneSharedGeometryMaterialBatch(int instanceCount)
    {
        var material = SceneMaterials.Matte("marker", RgbaFloat.White);
        var mesh = SceneGeometry.Cube(size: 1f, material.BaseColorFactor);
        var transforms = CreateLinearTransforms(instanceCount);
        var colors = CreateMarkerColors(instanceCount);
        var objectIds = CreateObjectIds(instanceCount);

        var document = SceneAuthoring.Create($"cube-markers-{instanceCount}")
            .AddInstances(
                "cube-markers",
                mesh,
                material,
                transforms,
                colors,
                objectIds,
                pickable: true)
            .Build();

        document.Entries.Should().BeEmpty("repeated authored cubes should stay in instance-batch truth, not expand to retained primitives");
        var batch = document.InstanceBatches.Should().ContainSingle().Which;

        batch.Name.Should().Be("cube-markers");
        batch.Mesh.Name.Should().Be("cube-markers");
        batch.Mesh.MeshData.Should().BeSameAs(mesh);
        batch.Mesh.MaterialId.Should().Be(material.Id);
        batch.Mesh.MeshData.Vertices.Should().HaveCount(24);
        batch.Mesh.MeshData.Indices.Should().HaveCount(36);
        batch.Material.Should().BeSameAs(material);

        batch.InstanceCount.Should().Be(instanceCount);
        batch.Transforms.Length.Should().Be(instanceCount);
        batch.Colors.Length.Should().Be(instanceCount);
        batch.ObjectIds.Length.Should().Be(instanceCount);
        batch.HasPerInstanceColors.Should().BeTrue();
        batch.HasObjectIds.Should().BeTrue();
        batch.Pickable.Should().BeTrue();

        batch.Transforms.Span[0].Should().Be(Matrix4x4.Identity);
        batch.Transforms.Span[^1].Should().Be(Matrix4x4.CreateTranslation((instanceCount - 1) * 2f, 0f, 0f));
        batch.Colors.Span[0].Should().Be(RgbaFloat.Red);
        batch.Colors.Span[^1].Should().Be(instanceCount % 2 == 0 ? RgbaFloat.Green : RgbaFloat.Red);
        var retainedObjectIds = batch.ObjectIds.ToArray();
        retainedObjectIds.Should().OnlyContain(objectId => objectId != Guid.Empty);
        retainedObjectIds.Should().OnlyHaveUniqueItems();

        batch.Bounds.Min.Should().Be(new Vector3(-0.5f, -0.5f, -0.5f));
        batch.Bounds.Max.Should().Be(new Vector3(((instanceCount - 1) * 2f) + 0.5f, 0.5f, 0.5f));
    }

    private static Matrix4x4[] CreateLinearTransforms(int count)
    {
        var transforms = new Matrix4x4[count];
        for (var i = 0; i < transforms.Length; i++)
        {
            transforms[i] = Matrix4x4.CreateTranslation(i * 2f, 0f, 0f);
        }

        return transforms;
    }

    private static RgbaFloat[] CreateMarkerColors(int count)
    {
        var colors = new RgbaFloat[count];
        for (var i = 0; i < colors.Length; i++)
        {
            colors[i] = i % 2 == 0 ? RgbaFloat.Red : RgbaFloat.Green;
        }

        return colors;
    }

    private static Guid[] CreateObjectIds(int count)
    {
        var objectIds = new Guid[count];
        for (var i = 0; i < objectIds.Length; i++)
        {
            objectIds[i] = Guid.NewGuid();
        }

        return objectIds;
    }
}
