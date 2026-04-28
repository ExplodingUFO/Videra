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

    [Fact]
    public void Presets_ReturnExpectedPaletteStops()
    {
        var defaultMap = SurfaceColorMapPresets.CreateDefault();
        defaultMap[0].Should().Be(0xFF102030u);
        defaultMap[1].Should().Be(0xFFE6EEF5u);

        var professional = SurfaceColorMapPresets.CreateProfessional();
        professional.Count.Should().Be(5);
        professional[0].Should().Be(0xFF08111Fu);
        professional[1].Should().Be(0xFF154C79u);
        professional[2].Should().Be(0xFF2DD4BFu);
        professional[3].Should().Be(0xFFFDE68Au);
        professional[4].Should().Be(0xFFF97316u);
    }

    [Fact]
    public void Presets_MapAcrossTheFullRange()
    {
        var coolWarm = SurfaceColorMapPresets.CreateCoolWarm();
        var colorMap = new SurfaceColorMap(new SurfaceValueRange(-10d, 10d), coolWarm);

        colorMap.Map(-10d).Should().Be(coolWarm[0]);
        colorMap.Map(0d).Should().Be(coolWarm[1]);
        colorMap.Map(10d).Should().Be(coolWarm[^1]);

        var grayscale = SurfaceColorMapPresets.CreateGrayscale();
        var grayscaleMap = new SurfaceColorMap(new SurfaceValueRange(0d, 1d), grayscale);
        grayscaleMap.Map(0d).Should().Be(grayscale[0]);
        grayscaleMap.Map(1d).Should().Be(grayscale[1]);

        var professional = SurfaceColorMapPresets.CreateProfessional();
        var professionalMap = new SurfaceColorMap(new SurfaceValueRange(0d, 1d), professional);
        professionalMap.Map(0d).Should().Be(professional[0]);
        professionalMap.Map(0.25d).Should().Be(professional[1]);
        professionalMap.Map(0.5d).Should().Be(professional[2]);
        professionalMap.Map(0.75d).Should().Be(professional[3]);
        professionalMap.Map(1d).Should().Be(professional[^1]);
    }

    private static SurfaceColorMap CreateColorMap()
    {
        return new SurfaceColorMap(
            new SurfaceValueRange(0.0, 10.0),
            new SurfaceColorMapPalette(0xFF000000u, 0xFFFFFFFFu));
    }
}
