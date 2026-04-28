using System.Globalization;
using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class SurfaceChartOutputEvidenceTests
{
    [Fact]
    public void Create_ReportsPaletteStopsAndPrecisionProfile()
    {
        var palette = SurfaceColorMapPresets.CreateProfessional();

        var evidence = SurfaceChartEvidenceFormatter.Create(
            "Professional",
            palette,
            -12.345678d,
            0.0d,
            98.7654321d);

        evidence.EvidenceKind.Should().Be("SurfaceChartOutputEvidence");
        evidence.PaletteName.Should().Be("Professional");
        evidence.ColorStops.Should().Equal(
            "#FF08111F",
            "#FF154C79",
            "#FF2DD4BF",
            "#FFFDE68A",
            "#FFF97316");
        evidence.PrecisionProfile.Should().Be("InvariantCulture:G6");
        evidence.SampleFormattedLabels.Should().Equal("-12.3457", "0", "98.7654");
    }

    [Fact]
    public void Create_UsesColorMapRangeAsSampleLabels()
    {
        var colorMap = new SurfaceColorMap(
            new SurfaceValueRange(-1e308, 1e308),
            new SurfaceColorMapPalette(0xFF000000u, 0xFFFFFFFFu));

        var evidence = SurfaceChartEvidenceFormatter.Create("Grayscale", colorMap);

        evidence.ColorStops.Should().Equal("#FF000000", "#FFFFFFFF");
        evidence.SampleFormattedLabels.Should().Equal("-1E+308", "0", "1E+308");
    }

    [Fact]
    public void Create_WithExplicitPrecisionProfile_UsesSuppliedFormatterWithoutFallback()
    {
        var evidence = SurfaceChartEvidenceFormatter.Create(
            "Default",
            SurfaceColorMapPresets.CreateDefault(),
            "custom-profile",
            value => "label:" + value.ToString("0.0", CultureInfo.InvariantCulture),
            1.234d,
            5.678d);

        evidence.PrecisionProfile.Should().Be("custom-profile");
        evidence.SampleFormattedLabels.Should().Equal("label:1.2", "label:5.7");
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Create_RejectsNonFiniteSampleLabels(double sampleValue)
    {
        var act = () => SurfaceChartEvidenceFormatter.Create(
            "Default",
            SurfaceColorMapPresets.CreateDefault(),
            sampleValue);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Surface chart evidence sample labels must be finite.*");
    }
}
