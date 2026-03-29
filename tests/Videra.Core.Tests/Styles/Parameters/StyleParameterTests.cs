using System.Numerics;
using FluentAssertions;
using Videra.Core.Styles.Parameters;
using Xunit;

namespace Videra.Core.Tests.Styles.Parameters;

public sealed class StyleParameterTests
{
    // --- LightingParameters ---

    [Fact]
    public void LightingParameters_Defaults_AreExpected()
    {
        var p = new LightingParameters();
        p.AmbientIntensity.Should().Be(0.3f);
        p.DiffuseIntensity.Should().Be(0.7f);
        p.SpecularIntensity.Should().Be(0.5f);
        p.SpecularPower.Should().Be(32f);
        p.LightDirection.Should().Be(Vector3.Normalize(new Vector3(0.5f, 1.0f, 0.3f)));
    }

    [Fact]
    public void LightingParameters_Clone_ProducesDeepCopy()
    {
        var original = new LightingParameters { AmbientIntensity = 0.8f, SpecularPower = 64f };
        var clone = original.Clone();

        clone.Should().NotBeSameAs(original);
        clone.Should().Be(original);

        clone.AmbientIntensity = 0.1f;
        original.AmbientIntensity.Should().Be(0.8f);
    }

    [Fact]
    public void LightingParameters_Equals_DifferentValues_ReturnsFalse()
    {
        var a = new LightingParameters();
        var b = new LightingParameters { AmbientIntensity = 0.9f };
        a.Should().NotBe(b);
    }

    [Fact]
    public void LightingParameters_GetHashCode_SameValues_SameHash()
    {
        var a = new LightingParameters();
        var b = new LightingParameters();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void LightingParameters_Equals_Null_ReturnsFalse()
    {
        new LightingParameters().Equals((LightingParameters?)null).Should().BeFalse();
        new LightingParameters().Equals((object?)null).Should().BeFalse();
    }

    // --- ColorParameters ---

    [Fact]
    public void ColorParameters_Defaults_AreExpected()
    {
        var p = new ColorParameters();
        p.TintColor.Should().Be(Vector3.One);
        p.Saturation.Should().Be(1.0f);
        p.Contrast.Should().Be(1.0f);
        p.Brightness.Should().Be(0f);
    }

    [Fact]
    public void ColorParameters_Clone_ProducesDeepCopy()
    {
        var original = new ColorParameters { Saturation = 0.5f, Contrast = 1.5f };
        var clone = original.Clone();

        clone.Should().NotBeSameAs(original);
        clone.Should().Be(original);

        clone.Saturation = 2.0f;
        original.Saturation.Should().Be(0.5f);
    }

    [Fact]
    public void ColorParameters_Equals_Null_ReturnsFalse()
    {
        new ColorParameters().Equals((ColorParameters?)null).Should().BeFalse();
        new ColorParameters().Equals((object?)null).Should().BeFalse();
    }

    [Fact]
    public void ColorParameters_GetHashCode_SameValues_SameHash()
    {
        var a = new ColorParameters { Brightness = 0.3f };
        var b = new ColorParameters { Brightness = 0.3f };
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    // --- OutlineParameters ---

    [Fact]
    public void OutlineParameters_Defaults_AreExpected()
    {
        var p = new OutlineParameters();
        p.Enabled.Should().BeFalse();
        p.Color.Should().Be(Videra.Core.Geometry.RgbaFloat.Black);
        p.Width.Should().Be(1.0f);
    }

    [Fact]
    public void OutlineParameters_Clone_ProducesDeepCopy()
    {
        var original = new OutlineParameters { Enabled = true, Width = 3.0f };
        var clone = original.Clone();

        clone.Should().NotBeSameAs(original);
        clone.Should().Be(original);

        clone.Enabled = false;
        original.Enabled.Should().BeTrue();
    }

    [Fact]
    public void OutlineParameters_Equals_Null_ReturnsFalse()
    {
        new OutlineParameters().Equals((OutlineParameters?)null).Should().BeFalse();
        new OutlineParameters().Equals((object?)null).Should().BeFalse();
    }

    [Fact]
    public void OutlineParameters_Equals_DifferentWidth_ReturnsFalse()
    {
        var a = new OutlineParameters { Width = 1.0f };
        var b = new OutlineParameters { Width = 2.0f };
        a.Should().NotBe(b);
    }

    // --- MaterialParameters ---

    [Fact]
    public void MaterialParameters_Defaults_AreExpected()
    {
        var p = new MaterialParameters();
        p.Opacity.Should().Be(1.0f);
        p.UseVertexColor.Should().BeTrue();
        p.OverrideColor.Should().Be(Videra.Core.Geometry.RgbaFloat.LightGrey);
        p.WireframeMode.Should().BeFalse();
    }

    [Fact]
    public void MaterialParameters_Clone_ProducesDeepCopy()
    {
        var original = new MaterialParameters { Opacity = 0.5f, WireframeMode = true };
        var clone = original.Clone();

        clone.Should().NotBeSameAs(original);
        clone.Should().Be(original);

        clone.Opacity = 1.0f;
        original.Opacity.Should().Be(0.5f);
    }

    [Fact]
    public void MaterialParameters_Equals_Null_ReturnsFalse()
    {
        new MaterialParameters().Equals((MaterialParameters?)null).Should().BeFalse();
        new MaterialParameters().Equals((object?)null).Should().BeFalse();
    }

    [Fact]
    public void MaterialParameters_GetHashCode_SameValues_SameHash()
    {
        var a = new MaterialParameters { Opacity = 0.7f };
        var b = new MaterialParameters { Opacity = 0.7f };
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
