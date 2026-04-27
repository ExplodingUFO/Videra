using System.Numerics;
using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Core.Tests.Scene;

public sealed class InstanceBatchDescriptorTests
{
    [Fact]
    public void Constructor_WithValidSameMeshMaterialBatch_CreatesDescriptorAndBounds()
    {
        var material = CreateMaterial();
        var mesh = CreateMesh(material.Id);
        var objectIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var descriptor = new InstanceBatchDescriptor(
            "markers",
            mesh,
            material,
            new[] { Matrix4x4.Identity, Matrix4x4.CreateTranslation(10f, 0f, 0f) },
            new[] { RgbaFloat.Red, RgbaFloat.Green },
            objectIds,
            pickable: true);

        descriptor.Name.Should().Be("markers");
        descriptor.Mesh.Should().BeSameAs(mesh);
        descriptor.Material.Should().BeSameAs(material);
        descriptor.InstanceCount.Should().Be(2);
        descriptor.HasPerInstanceColors.Should().BeTrue();
        descriptor.HasObjectIds.Should().BeTrue();
        descriptor.Pickable.Should().BeTrue();
        descriptor.Bounds.Min.Should().Be(new Vector3(0f, 0f, 0f));
        descriptor.Bounds.Max.Should().Be(new Vector3(11f, 1f, 0f));
    }

    [Fact]
    public void Constructor_AllowsAlphaMaskButRejectsBlendMaterial()
    {
        var maskedMaterial = CreateMaterial(alpha: new MaterialAlphaSettings(MaterialAlphaMode.Mask, 0.5f, false));
        var maskedMesh = CreateMesh(maskedMaterial.Id);

        var maskedDescriptor = new InstanceBatchDescriptor(
            "masked",
            maskedMesh,
            maskedMaterial,
            new[] { Matrix4x4.Identity });

        maskedDescriptor.Material.Alpha.Mode.Should().Be(MaterialAlphaMode.Mask);

        var blendedMaterial = CreateMaterial(alpha: new MaterialAlphaSettings(MaterialAlphaMode.Blend, 0.5f, false));
        var blendedMesh = CreateMesh(blendedMaterial.Id);

        var act = () => _ = new InstanceBatchDescriptor(
            "transparent",
            blendedMesh,
            blendedMaterial,
            new[] { Matrix4x4.Identity });

        act.Should().Throw<ArgumentException>()
            .WithParameterName("material")
            .WithMessage("*transparent Blend materials*");
    }

    [Fact]
    public void Constructor_RejectsInvalidInstanceDataEarly()
    {
        var material = CreateMaterial();
        var mesh = CreateMesh(material.Id);

        new Action(() => _ = new InstanceBatchDescriptor("empty", mesh, material, Array.Empty<Matrix4x4>()))
            .Should().Throw<ArgumentException>().WithParameterName("transforms");

        new Action(() => _ = new InstanceBatchDescriptor(
                "colors",
                mesh,
                material,
                new[] { Matrix4x4.Identity, Matrix4x4.Identity },
                new[] { RgbaFloat.White }))
            .Should().Throw<ArgumentException>().WithParameterName("colors");

        new Action(() => _ = new InstanceBatchDescriptor(
                "ids",
                mesh,
                material,
                new[] { Matrix4x4.Identity },
                objectIds: new[] { Guid.Empty }))
            .Should().Throw<ArgumentException>().WithParameterName("objectIds");
    }

    [Fact]
    public void Constructor_RejectsMaterialMismatch()
    {
        var material = CreateMaterial();
        var otherMaterial = CreateMaterial();
        var mesh = CreateMesh(material.Id);

        new Action(() => _ = new InstanceBatchDescriptor(
                "mismatch",
                mesh,
                otherMaterial,
                new[] { Matrix4x4.Identity }))
            .Should().Throw<ArgumentException>().WithParameterName("material");
    }

    [Fact]
    public void AddInstanceBatch_AddsRetainedEntryWithoutRuntimeObjects()
    {
        var material = CreateMaterial();
        var mesh = CreateMesh(material.Id);
        var descriptor = new InstanceBatchDescriptor(
            "markers",
            mesh,
            material,
            new[] { Matrix4x4.Identity },
            pickable: false);

        var document = SceneDocument.Empty.AddInstanceBatch(descriptor);

        document.Version.Should().Be(1);
        document.Entries.Should().BeEmpty();
        document.InstanceBatches.Should().ContainSingle();
        document.InstanceBatches[0].EntryId.Value.Should().NotBe(Guid.Empty);
        document.InstanceBatches[0].Name.Should().Be("markers");
        document.InstanceBatches[0].Mesh.Should().BeSameAs(mesh);
        document.InstanceBatches[0].Material.Should().BeSameAs(material);
        document.InstanceBatches[0].InstanceCount.Should().Be(1);
        document.InstanceBatches[0].Pickable.Should().BeFalse();
        document.InstanceBatches[0].Bounds.Should().Be(descriptor.Bounds);
    }

    private static MaterialInstance CreateMaterial(MaterialAlphaSettings? alpha = null)
    {
        return new MaterialInstance(
            MaterialInstanceId.New(),
            "material",
            RgbaFloat.White,
            alpha: alpha);
    }

    private static MeshPrimitive CreateMesh(MaterialInstanceId materialId)
    {
        return new MeshPrimitive(
            MeshPrimitiveId.New(),
            "triangle",
            new MeshData
            {
                Vertices =
                [
                    new VertexPositionNormalColor(new Vector3(0f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                    new VertexPositionNormalColor(new Vector3(1f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                    new VertexPositionNormalColor(new Vector3(0f, 1f, 0f), Vector3.UnitZ, RgbaFloat.White)
                ],
                Indices = [0, 1, 2]
            },
            materialId);
    }
}
