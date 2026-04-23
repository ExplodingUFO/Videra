using System.Text.Json;
using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Styles.Parameters;
using Videra.Core.Styles.Presets;
using Videra.Core.Styles.Serialization;
using Xunit;

namespace Videra.Core.Tests.Styles.Serialization;

public sealed class StyleJsonConverterTests
{
    [Fact]
    public void SerializeDeserialize_RoundTripsParameters()
    {
        var params_ = new RenderStyleParameters
        {
            Lighting = { AmbientIntensity = 0.3f, DiffuseIntensity = 0.9f, LightDirection = new System.Numerics.Vector3(1, 2, 3), FillIntensity = 0.25f },
            Color = { Saturation = 1.2f, TintColor = new System.Numerics.Vector3(0.5f, 0.6f, 0.7f) },
            Outline = { Enabled = true, Width = 3.5f },
            Material = { Opacity = 0.75f, UseVertexColor = true }
        };

        var json = StyleJsonConverter.Serialize(params_, RenderStylePreset.Tech);
        var (deserializedParams, deserializedPreset) = StyleJsonConverter.Deserialize(json);

        deserializedPreset.Should().Be(RenderStylePreset.Tech);
        deserializedParams.Lighting.AmbientIntensity.Should().Be(0.3f);
        deserializedParams.Lighting.DiffuseIntensity.Should().Be(0.9f);
        deserializedParams.Lighting.LightDirection.Should().Be(new System.Numerics.Vector3(1, 2, 3));
        deserializedParams.Lighting.FillIntensity.Should().Be(0.25f);
        deserializedParams.Color.Saturation.Should().Be(1.2f);
        deserializedParams.Color.TintColor.X.Should().BeApproximately(0.5f, 0.001f);
        deserializedParams.Outline.Enabled.Should().BeTrue();
        deserializedParams.Outline.Width.Should().Be(3.5f);
        deserializedParams.Material.Opacity.Should().Be(0.75f);
        deserializedParams.Material.UseVertexColor.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_InvalidJson_ThrowsJsonException()
    {
        var act = () => StyleJsonConverter.Deserialize("not valid json");

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Serialize_ProducesCamelCaseOutput()
    {
        var json = StyleJsonConverter.Serialize(new RenderStyleParameters(), RenderStylePreset.Realistic);

        json.Should().Contain("version");
        json.Should().Contain("preset");
        json.Should().Contain("parameters");
    }
}
