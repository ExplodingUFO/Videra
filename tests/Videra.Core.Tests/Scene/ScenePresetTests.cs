using System.Numerics;
using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Core.Tests.Scene;

public sealed class ScenePresetTests
{
    [Fact]
    public void Emissive_CreatesRendererNeutralMaterialPreset()
    {
        var color = new RgbaFloat(0.2f, 0.4f, 0.8f, 1f);

        var material = SceneMaterials.Emissive("status-light", color, intensity: 2f);

        material.Name.Should().Be("status-light");
        material.BaseColorFactor.Should().Be(color);
        material.MetallicRoughness.Should().Be(new MaterialMetallicRoughness(0f, 1f));
        material.Alpha.Should().Be(MaterialAlphaSettings.Opaque);
        material.Emissive.Should().Be(new MaterialEmissive(new Vector3(0.4f, 0.8f, 1.6f)));
        material.BaseColorTexture.Should().BeNull();
        material.NormalTexture.Should().BeNull();
        material.OcclusionTexture.Should().BeNull();
    }

    [Fact]
    public void Emissive_RejectsNegativeIntensity()
    {
        var act = () => SceneMaterials.Emissive("invalid", RgbaFloat.White, intensity: -0.1f);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("intensity");
    }

    [Fact]
    public void BoxOutline_CreatesLineTopologyWithExpectedCornersAndEdges()
    {
        var color = new RgbaFloat(1f, 0.5f, 0.25f, 1f);

        var mesh = SceneGeometry.BoxOutline(width: 4f, height: 2f, depth: 6f, color);

        mesh.Topology.Should().Be(MeshTopology.Lines);
        mesh.Vertices.Should().HaveCount(8);
        mesh.Indices.Should().Equal(
            0u, 1u, 1u, 2u, 2u, 3u, 3u, 0u,
            4u, 5u, 5u, 6u, 6u, 7u, 7u, 4u,
            0u, 4u, 1u, 5u, 2u, 6u, 3u, 7u);
        mesh.Indices.Should().OnlyContain(index => index < mesh.Vertices.Length);
        mesh.Vertices.Select(vertex => vertex.Position).Should().Equal(
            new Vector3(-2f, -1f, -3f),
            new Vector3(2f, -1f, -3f),
            new Vector3(2f, 1f, -3f),
            new Vector3(-2f, 1f, -3f),
            new Vector3(-2f, -1f, 3f),
            new Vector3(2f, -1f, 3f),
            new Vector3(2f, 1f, 3f),
            new Vector3(-2f, 1f, 3f));
        mesh.Vertices.Select(vertex => vertex.Color).Should().OnlyContain(vertexColor => vertexColor == color);
    }

    [Fact]
    public void BoxOutline_RejectsNonPositiveDimensions()
    {
        var act = () => SceneGeometry.BoxOutline(width: 0f);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("width");
    }
}
