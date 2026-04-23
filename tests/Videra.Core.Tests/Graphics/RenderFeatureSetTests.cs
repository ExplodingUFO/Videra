using FluentAssertions;
using Videra.Core.Graphics.RenderPipeline;
using Videra.Core.Graphics.RenderPipeline.Extensibility;
using Xunit;

namespace Videra.Core.Tests.Graphics;

public sealed class RenderFeatureSetTests
{
    [Fact]
    public void ToFeatureNames_ReturnsCanonicalFeatureOrder()
    {
        var names = (RenderFeatureSet.Screenshot |
            RenderFeatureSet.Transparent |
            RenderFeatureSet.Opaque |
            RenderFeatureSet.Picking |
            RenderFeatureSet.Overlay).ToFeatureNames();

        names.Should().Equal("Opaque", "Transparent", "Overlay", "Picking", "Screenshot");
    }

    [Fact]
    public void ToFeatureNames_WithNoFlags_ReturnsEmpty()
    {
        RenderFeatureSet.None.ToFeatureNames().Should().BeEmpty();
    }

    [Theory]
    [InlineData(RenderPassSlot.Grid)]
    [InlineData(RenderPassSlot.Wireframe)]
    [InlineData(RenderPassSlot.Axis)]
    public void Resolve_MapsOverlaySlotsToOverlay(RenderPassSlot slot)
    {
        RenderPassSlotFeatureMap.Resolve(slot).Should().Be(RenderFeatureSet.Overlay);
    }

    [Fact]
    public void Resolve_MapsSolidGeometryToOpaque()
    {
        RenderPassSlotFeatureMap.Resolve(RenderPassSlot.SolidGeometry).Should().Be(RenderFeatureSet.Opaque);
    }
}
