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
    public void Plot3D_OwnsProfessionalPresentationOptions()
    {
        var view = new VideraChartView();
        var colorMap = new SurfaceColorMap(
            new SurfaceValueRange(-1d, 1d),
            SurfaceColorMapPresets.CreateProfessional());
        var overlayOptions = SurfaceChartNumericLabelPresets.Engineering(precision: 2);

        view.Plot.ColorMap = colorMap;
        view.Plot.OverlayOptions = overlayOptions;

        view.Plot.ColorMap.Should().BeSameAs(colorMap);
        view.Plot.OverlayOptions.Should().BeSameAs(overlayOptions);
        view.Plot.Revision.Should().Be(2);
        view.LastRefreshRevision.Should().Be(2);

        view.Plot.ColorMap = colorMap;
        view.Plot.OverlayOptions = overlayOptions;
        view.Plot.Revision.Should().Be(2);
        view.LastRefreshRevision.Should().Be(2);

        view.Plot.Add.Surface(new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f));
        view.Plot.Clear();
        view.Plot.Series.Should().BeEmpty();
        view.Plot.ColorMap.Should().BeSameAs(colorMap);
        view.Plot.OverlayOptions.Should().BeSameAs(overlayOptions);

        var action = () => view.Plot.OverlayOptions = null!;
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void VideraChartView_IsNotAnOldChartViewWrapper()
    {
        typeof(VideraChartView).Assembly.GetType("Videra.SurfaceCharts.Avalonia.Controls.SurfaceChartView").Should().BeNull();
        typeof(VideraChartView).Assembly.GetType("Videra.SurfaceCharts.Avalonia.Controls.WaterfallChartView").Should().BeNull();
        typeof(VideraChartView).Assembly.GetType("Videra.SurfaceCharts.Avalonia.Controls.ScatterChartView").Should().BeNull();
        typeof(VideraChartView).GetProperty("ColorMap").Should().BeNull();
        typeof(VideraChartView).GetProperty("OverlayOptions").Should().BeNull();
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
