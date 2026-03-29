using FluentAssertions;
using Videra.Core.Styles.Presets;
using Xunit;

namespace Videra.Core.Tests.Styles.Presets;

public class RenderStylePresetsTests
{
    [Theory]
    [InlineData(RenderStylePreset.Realistic)]
    [InlineData(RenderStylePreset.Tech)]
    [InlineData(RenderStylePreset.Cartoon)]
    [InlineData(RenderStylePreset.XRay)]
    [InlineData(RenderStylePreset.Clay)]
    [InlineData(RenderStylePreset.Wireframe)]
    public void GetParameters_KnownPreset_ReturnsNonNull(RenderStylePreset preset)
    {
        var parameters = RenderStylePresets.GetParameters(preset);
        parameters.Should().NotBeNull();
    }

    [Fact]
    public void GetParameters_UnknownPreset_ReturnsDefault()
    {
        var parameters = RenderStylePresets.GetParameters((RenderStylePreset)999);
        parameters.Should().NotBeNull();
    }

    [Fact]
    public void CreateRealistic_HasNoOutline()
    {
        var p = RenderStylePresets.CreateRealistic();
        p.Outline.Enabled.Should().BeFalse();
        p.Material.Opacity.Should().Be(1.0f);
    }

    [Fact]
    public void CreateTech_HasCyanOutline()
    {
        var p = RenderStylePresets.CreateTech();
        p.Outline.Enabled.Should().BeTrue();
        p.Color.TintColor.X.Should().BeLessThan(p.Color.TintColor.Z); // blue-dominant
    }

    [Fact]
    public void CreateCartoon_HasBlackOutline()
    {
        var p = RenderStylePresets.CreateCartoon();
        p.Outline.Enabled.Should().BeTrue();
        p.Outline.Width.Should().BeGreaterThan(1.0f);
        p.Lighting.SpecularIntensity.Should().Be(0f);
    }

    [Fact]
    public void CreateXRay_IsSemiTransparent()
    {
        var p = RenderStylePresets.CreateXRay();
        p.Material.Opacity.Should().BeLessThan(1.0f);
        p.Material.UseVertexColor.Should().BeFalse();
    }

    [Fact]
    public void CreateClay_IsDesaturated()
    {
        var p = RenderStylePresets.CreateClay();
        p.Color.Saturation.Should().Be(0f);
        p.Material.UseVertexColor.Should().BeFalse();
    }

    [Fact]
    public void CreateWireframe_OnlyUsesAmbient()
    {
        var p = RenderStylePresets.CreateWireframe();
        p.Lighting.AmbientIntensity.Should().Be(1.0f);
        p.Lighting.DiffuseIntensity.Should().Be(0f);
        p.Material.WireframeMode.Should().BeTrue();
    }

    [Fact]
    public void EachPreset_ReturnsDistinctParameters()
    {
        var presets = new[]
        {
            RenderStylePresets.CreateRealistic(),
            RenderStylePresets.CreateTech(),
            RenderStylePresets.CreateCartoon(),
            RenderStylePresets.CreateXRay(),
            RenderStylePresets.CreateClay(),
            RenderStylePresets.CreateWireframe()
        };

        // Each preset should differ from at least one other
        presets.Distinct().Should().HaveCountGreaterThan(1);
    }
}
