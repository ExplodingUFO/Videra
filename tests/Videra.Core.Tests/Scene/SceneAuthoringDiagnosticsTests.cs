using System.Numerics;
using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Core.Tests.Scene;

public sealed class SceneAuthoringDiagnosticsTests
{
    [Fact]
    public void TryBuild_WithInvalidInstanceInputs_ReturnsDiagnosticsWithoutFallbackBatch()
    {
        var material = SceneMaterials.Matte("marker", RgbaFloat.White);

        var result = SceneAuthoring.Create("invalid-instances")
            .AddInstances(
                "markers",
                SceneGeometry.Cube(color: material.BaseColorFactor),
                material,
                Array.Empty<Matrix4x4>(),
                colors: new[] { RgbaFloat.Red },
                objectIds: new[] { Guid.Empty })
            .TryBuild();

        result.Succeeded.Should().BeFalse();
        result.Document.Should().BeNull();
        result.Diagnostics.Should().Contain(diagnostic =>
            diagnostic.Code == "instance.transforms.empty" &&
            diagnostic.Target == "markers");
        result.Diagnostics.Should().Contain(diagnostic =>
            diagnostic.Code == "instance.colors.length" &&
            diagnostic.Target == "markers");
        result.Diagnostics.Should().Contain(diagnostic =>
            diagnostic.Code == "instance.objectIds.length" &&
            diagnostic.Target == "markers");
        result.Diagnostics.Should().Contain(diagnostic =>
            diagnostic.Code == "instance.objectId.empty" &&
            diagnostic.Target == "markers");
    }

    [Fact]
    public void TryBuild_WithInvalidPlacementInstances_ReturnsNonFiniteTransformDiagnostic()
    {
        var material = SceneMaterials.Matte("marker", RgbaFloat.White);
        var placements = new[]
        {
            SceneAuthoringPlacement.At(float.NaN, 0f, 0f)
        };

        var result = SceneAuthoring.Create("invalid-placement")
            .AddInstances(
                "markers",
                SceneGeometry.Cube(color: material.BaseColorFactor),
                material,
                placements)
            .TryBuild();

        result.Succeeded.Should().BeFalse();
        result.Document.Should().BeNull();
        result.Diagnostics.Should().ContainSingle(diagnostic =>
            diagnostic.Code == "instance.transform.nonfinite" &&
            diagnostic.Target == "markers");
    }

    [Fact]
    public void TryBuild_WithBlendInstanceMaterial_ReturnsDiagnosticWithoutCreatingBatch()
    {
        var material = SceneMaterials.Transparent("transparent", RgbaFloat.White);

        var result = SceneAuthoring.Create("invalid-material")
            .AddInstances(
                "markers",
                SceneGeometry.Cube(color: material.BaseColorFactor),
                material,
                new[] { Matrix4x4.Identity })
            .TryBuild();

        result.Succeeded.Should().BeFalse();
        result.Document.Should().BeNull();
        result.Diagnostics.Should().ContainSingle(diagnostic =>
            diagnostic.Code == "instance.material.blend" &&
            diagnostic.Target == "markers");
    }

    [Fact]
    public void Build_WithInvalidInstanceInputThrowsAuthoringDiagnostics()
    {
        var material = SceneMaterials.Matte("marker", RgbaFloat.White);
        var builder = SceneAuthoring.Create("invalid-instances")
            .AddInstances(
                "markers",
                SceneGeometry.Cube(color: material.BaseColorFactor),
                material,
                Array.Empty<Matrix4x4>());

        var act = builder.Build;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*instance.transforms.empty*");
    }
}
