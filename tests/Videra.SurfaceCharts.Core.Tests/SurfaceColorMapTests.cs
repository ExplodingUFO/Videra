using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class SurfaceColorMapTests
{
    [Fact]
    public void Map_ReturnsBoundaryColorsAtRangeLimits()
    {
        var colorMap = CreateColorMap();

        colorMap.Map(0.0).Should().Be(0xFF000000u);
        colorMap.Map(10.0).Should().Be(0xFFFFFFFFu);
    }

    [Fact]
    public void Map_ReturnsInterpolatedColorAtRangeMidpoint()
    {
        var colorMap = CreateColorMap();

        colorMap.Map(5.0).Should().Be(0xFF808080u);
    }

    [Fact]
    public void Map_DoesNotTreatEpsilonSpanRangeAsDegenerate()
    {
        var colorMap = new SurfaceColorMap(
            new SurfaceValueRange(0.0, double.Epsilon),
            new SurfaceColorMapPalette(0xFF000000u, 0xFFFFFFFFu));

        colorMap.Map(0.0).Should().Be(0xFF000000u);
        colorMap.Map(double.Epsilon).Should().Be(0xFFFFFFFFu);
    }

    [Fact]
    public void Map_RemainsStableForLargeFiniteRanges()
    {
        var colorMap = new SurfaceColorMap(
            new SurfaceValueRange(-1e308, 1e308),
            new SurfaceColorMapPalette(0xFF000000u, 0xFFFFFFFFu));

        colorMap.Map(-1e308).Should().Be(0xFF000000u);
        colorMap.Map(1e308).Should().Be(0xFFFFFFFFu);
        colorMap.Map(0.0).Should().Be(0xFF808080u);
    }

    [Fact]
    public void PaletteCtor_RejectsFewerThanTwoColors()
    {
        var act = () => new SurfaceColorMapPalette(0xFF000000u);

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "colors");
    }

    private static SurfaceColorMap CreateColorMap()
    {
        return new SurfaceColorMap(
            new SurfaceValueRange(0.0, 10.0),
            new SurfaceColorMapPalette(0xFF000000u, 0xFFFFFFFFu));
    }
}
