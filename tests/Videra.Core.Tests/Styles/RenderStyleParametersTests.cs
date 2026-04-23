using System.Numerics;
using FluentAssertions;
using Videra.Core.Styles.Parameters;
using Xunit;

namespace Videra.Core.Tests.Styles.Parameters;

public sealed class RenderStyleParametersTests
{
    [Fact]
    public void Clone_ProducesDeepCopy()
    {
        var original = new RenderStyleParameters
        {
            Lighting = { AmbientIntensity = 0.8f },
            Color = { Saturation = 1.5f },
            Outline = { Width = 3.0f },
            Material = { Opacity = 0.5f }
        };

        var clone = original.Clone();

        clone.Should().NotBeSameAs(original);
        clone.Should().Be(original);

        // Mutating clone should not affect original
        clone.Lighting.AmbientIntensity = 0.1f;
        original.Lighting.AmbientIntensity.Should().Be(0.8f);
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var a = new RenderStyleParameters();
        var b = new RenderStyleParameters();

        a.Should().Be(b);
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var a = new RenderStyleParameters();
        var b = new RenderStyleParameters { Lighting = { AmbientIntensity = 99f } };

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var a = new RenderStyleParameters();
        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        var a = new RenderStyleParameters();
        var b = new RenderStyleParameters();

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void ToUniformData_MapsFieldsCorrectly()
    {
        var params_ = new RenderStyleParameters
        {
            Lighting = { AmbientIntensity = 0.5f, DiffuseIntensity = 0.7f, SpecularIntensity = 0.3f, SpecularPower = 32f, LightDirection = new Vector3(1, 2, 3) },
            Color = { Saturation = 1.2f, Contrast = 0.9f, Brightness = 0.1f },
            Outline = { Enabled = true, Width = 2.5f },
            Material = { Opacity = 0.8f, UseVertexColor = true }
        };

        var data = params_.ToUniformData();

        data.AmbientIntensity.Should().Be(0.5f);
        data.DiffuseIntensity.Should().Be(0.7f);
        data.SpecularIntensity.Should().Be(0.3f);
        data.SpecularPower.Should().Be(32f);
        data.LightDirection.Should().Be(new Vector3(1, 2, 3));
        data.FillIntensity.Should().Be(0f);
        data.Saturation.Should().Be(1.2f);
        data.Contrast.Should().Be(0.9f);
        data.Brightness.Should().Be(0.1f);
        data.OutlineEnabled.Should().Be(1);
        data.OutlineWidth.Should().Be(2.5f);
        data.Opacity.Should().Be(0.8f);
        data.UseVertexColor.Should().Be(1);
    }
}
