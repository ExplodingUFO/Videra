using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceLegendOverlayTests
{
    [Fact]
    public void LegendPosition_DefaultValue_IsTopRight()
    {
        var options = SurfaceChartOverlayOptions.Default;
        options.LegendPosition.Should().Be(SurfaceChartLegendPosition.TopRight);
    }

    [Theory]
    [InlineData(SurfaceChartLegendPosition.TopLeft)]
    [InlineData(SurfaceChartLegendPosition.TopRight)]
    [InlineData(SurfaceChartLegendPosition.BottomLeft)]
    [InlineData(SurfaceChartLegendPosition.BottomRight)]
    public void LegendPosition_CanBeSet(SurfaceChartLegendPosition position)
    {
        var options = new SurfaceChartOverlayOptions { LegendPosition = position };
        options.LegendPosition.Should().Be(position);
    }

    [Fact]
    public void LegendIndicatorKind_HasExpectedValues()
    {
        var values = Enum.GetValues<LegendIndicatorKind>();
        values.Should().Contain(LegendIndicatorKind.Swatch);
        values.Should().Contain(LegendIndicatorKind.Dot);
        values.Should().Contain(LegendIndicatorKind.Line);
    }

    [Fact]
    public void SurfaceLegendEntry_CanBeCreated()
    {
        var entry = new SurfaceLegendEntry(
            seriesName: "Test Series",
            seriesKind: Plot3DSeriesKind.Surface,
            isVisible: true,
            color: 0xFF4DA3FF,
            indicatorKind: LegendIndicatorKind.Swatch);

        entry.SeriesName.Should().Be("Test Series");
        entry.SeriesKind.Should().Be(Plot3DSeriesKind.Surface);
        entry.IsVisible.Should().BeTrue();
        entry.Color.Should().Be(0xFF4DA3FF);
        entry.IndicatorKind.Should().Be(LegendIndicatorKind.Swatch);
    }

    [Fact]
    public void SurfaceLegendOverlayState_Empty_HasNoEntries()
    {
        var state = SurfaceLegendOverlayState.Empty;
        state.Entries.Should().BeEmpty();
        state.IsTruncated.Should().BeFalse();
    }

    [Fact]
    public void SurfaceLegendOverlayState_CanBeCreated()
    {
        var entries = new List<SurfaceLegendEntry>
        {
            new("Series 1", Plot3DSeriesKind.Surface, true, 0xFF4DA3FF, LegendIndicatorKind.Swatch),
            new("Series 2", Plot3DSeriesKind.Scatter, true, 0xFFFF6B6B, LegendIndicatorKind.Dot),
        };

        var state = new SurfaceLegendOverlayState(
            entries: entries,
            position: SurfaceChartLegendPosition.TopLeft,
            bounds: new Rect(10, 10, 200, 100),
            isTruncated: false);

        state.Entries.Should().HaveCount(2);
        state.Position.Should().Be(SurfaceChartLegendPosition.TopLeft);
        state.Bounds.Should().Be(new Rect(10, 10, 200, 100));
        state.IsTruncated.Should().BeFalse();
    }

    [Fact]
    public void VideraChartView_Plot_Series_CanBeUsedForLegend()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var metadata = CreateMetadata();
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 4f);
            var scatter = CreateScatterData();

            view.Plot.Add.Surface(source, "Surface");
            view.Plot.Add.Scatter(scatter, "Scatter");

            view.Plot.Series.Should().HaveCount(2);
            view.Plot.Series[0].Name.Should().Be("Surface");
            view.Plot.Series[0].Kind.Should().Be(Plot3DSeriesKind.Surface);
            view.Plot.Series[1].Name.Should().Be("Scatter");
            view.Plot.Series[1].Kind.Should().Be(Plot3DSeriesKind.Scatter);
        });
    }

    [Fact]
    public void SurfaceChartLegendPosition_HasFourValues()
    {
        var values = Enum.GetValues<SurfaceChartLegendPosition>();
        values.Should().HaveCount(4);
        values.Should().Contain(SurfaceChartLegendPosition.TopLeft);
        values.Should().Contain(SurfaceChartLegendPosition.TopRight);
        values.Should().Contain(SurfaceChartLegendPosition.BottomLeft);
        values.Should().Contain(SurfaceChartLegendPosition.BottomRight);
    }

    private static SurfaceMetadata CreateMetadata()
    {
        return new SurfaceMetadata(
            width: 8,
            height: 8,
            new SurfaceAxisDescriptor("X", unit: null, minimum: 0, maximum: 7),
            new SurfaceAxisDescriptor("Y", unit: null, minimum: 0, maximum: 7),
            new SurfaceValueRange(0, 100));
    }

    private static ScatterChartData CreateScatterData()
    {
        var metadata = new ScatterChartMetadata(
            new SurfaceAxisDescriptor("X", "m", 0d, 1d),
            new SurfaceAxisDescriptor("Z", "m", 0d, 1d),
            new SurfaceValueRange(0d, 1d));
        var series = new ScatterSeries(
            [
                new ScatterPoint(0.25f, 0.5f, 0.75f),
            ],
            0xFF2F80ED,
            "points");

        return new ScatterChartData(metadata, [series]);
    }
}
