using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class VideraChartViewPlotApiTests
{
    [Fact]
    public void VideraChartView_ExposesSinglePlotAddApi_ForAllChartFamilies()
    {
        var view = new VideraChartView();
        var surfaceSource = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);
        var waterfallSource = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 7f);
        var scatter = CreateScatterData();

        var surface = view.Plot.Add.Surface(surfaceSource, "surface");
        var waterfall = view.Plot.Add.Waterfall(waterfallSource, "waterfall");
        var scatterSeries = view.Plot.Add.Scatter(scatter, "scatter");

        view.Plot.Series.Should().Equal(surface, waterfall, scatterSeries);
        surface.Kind.Should().Be(Plot3DSeriesKind.Surface);
        surface.Name.Should().Be("surface");
        surface.SurfaceSource.Should().BeSameAs(surfaceSource);
        surface.ScatterData.Should().BeNull();

        waterfall.Kind.Should().Be(Plot3DSeriesKind.Waterfall);
        waterfall.Name.Should().Be("waterfall");
        waterfall.SurfaceSource.Should().BeSameAs(waterfallSource);
        waterfall.ScatterData.Should().BeNull();

        scatterSeries.Kind.Should().Be(Plot3DSeriesKind.Scatter);
        scatterSeries.Name.Should().Be("scatter");
        scatterSeries.ScatterData.Should().BeSameAs(scatter);
        scatterSeries.SurfaceSource.Should().BeNull();

        view.Plot.Revision.Should().Be(3);
        view.LastRefreshRevision.Should().Be(3);
    }

    [Fact]
    public void VideraChartView_IsNotAnOldChartViewWrapper()
    {
        typeof(VideraChartView).Assembly.GetType("Videra.SurfaceCharts.Avalonia.Controls.SurfaceChartView").Should().BeNull();
        typeof(VideraChartView).Assembly.GetType("Videra.SurfaceCharts.Avalonia.Controls.WaterfallChartView").Should().BeNull();
        typeof(VideraChartView).Assembly.GetType("Videra.SurfaceCharts.Avalonia.Controls.ScatterChartView").Should().BeNull();
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
