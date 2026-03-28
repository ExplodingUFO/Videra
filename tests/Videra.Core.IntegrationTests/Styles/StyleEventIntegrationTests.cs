using FluentAssertions;
using Videra.Core.Styles.Parameters;
using Videra.Core.Styles.Presets;
using Videra.Core.Styles.Services;
using Xunit;

namespace Videra.Core.IntegrationTests.Styles;

public class StyleEventIntegrationTests
{
    [Fact]
    public void RenderStyleService_ApplyPreset_FiresEventAndUpdatesParameters()
    {
        var service = new RenderStyleService();
        StyleChangedEventArgs? firstEvent = null;
        var eventCount = 0;

        service.StyleChanged += (_, args) =>
        {
            eventCount++;
            if (firstEvent is null)
            {
                firstEvent = args;
            }
        };

        service.ApplyPreset(RenderStylePreset.Tech);
        var tech = service.CurrentParameters;

        service.CurrentPreset.Should().Be(RenderStylePreset.Tech);
        eventCount.Should().Be(1);
        firstEvent.Should().NotBeNull();
        firstEvent!.Preset.Should().Be(RenderStylePreset.Tech);
        firstEvent.Parameters.Should().BeEquivalentTo(tech);

        service.ApplyPreset(RenderStylePreset.Wireframe);
        var wireframe = service.CurrentParameters;

        service.CurrentPreset.Should().Be(RenderStylePreset.Wireframe);
        eventCount.Should().Be(2);
        wireframe.Should().NotBeEquivalentTo(tech);
        wireframe.Material.WireframeMode.Should().BeTrue();
        wireframe.Should().BeEquivalentTo(RenderStylePresets.GetParameters(RenderStylePreset.Wireframe));
    }

    [Fact]
    public void RenderStyleService_UpdateParameters_SetsCustomPresetAndFiresEvent()
    {
        var service = new RenderStyleService();
        StyleChangedEventArgs? captured = null;

        service.StyleChanged += (_, args) => captured = args;

        var custom = new RenderStyleParameters();
        custom.Lighting.AmbientIntensity = 0.42f;
        custom.Material.Opacity = 0.65f;
        custom.Material.WireframeMode = true;

        service.UpdateParameters(custom);

        service.CurrentPreset.Should().Be(RenderStylePreset.Custom);
        service.CurrentParameters.Should().BeEquivalentTo(custom);
        captured.Should().NotBeNull();
        captured!.Preset.Should().Be(RenderStylePreset.Custom);
        captured.Parameters.Should().BeEquivalentTo(custom);
    }
}
