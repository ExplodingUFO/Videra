using Avalonia;
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
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var surfaceSource = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);
            var waterfallSource = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 7f);
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
        });
    }

    [Fact]
    public void Plot3D_OwnsProfessionalPresentationOptions()
    {
        AvaloniaHeadlessTestSession.Run(() =>
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

            view.Plot.Add.Surface(new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f));
            view.Plot.Clear();
            view.Plot.Series.Should().BeEmpty();
            view.Plot.ColorMap.Should().BeSameAs(colorMap);
            view.Plot.OverlayOptions.Should().BeSameAs(overlayOptions);

            var action = () => view.Plot.OverlayOptions = null!;
            action.Should().Throw<ArgumentNullException>();
        });
    }

    [Fact]
    public Task PlotAddSurface_ActivatesRuntimeSourceWithoutPublicSourceAssignment()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var view = new VideraChartView();
            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);

            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Plot.Add.Surface(source, "surface");
            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [4f]);
            view.FitToData();

            view.RenderingStatus.IsReady.Should().BeTrue();
            view.RenderingStatus.VisibleTileCount.Should().BeGreaterThan(0);
            view.Source.Should().BeNull();
            view.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, source.Metadata.Width, source.Metadata.Height));
        });
    }

    [Fact]
    public Task PlotAddWaterfall_ReplacesActiveRuntimeSource()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var view = new VideraChartView();
            var surface = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);
            var waterfall = new ScriptedSurfaceTileSource(CreateWaterfallMetadata(), defaultTileValue: 7f);

            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Plot.Add.Surface(surface, "surface");
            view.Plot.Add.Waterfall(waterfall, "waterfall");
            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [7f]);
            view.FitToData();

            view.RenderingStatus.IsReady.Should().BeTrue();
            view.Source.Should().BeNull();
            view.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, waterfall.Metadata.Width, waterfall.Metadata.Height));
        });
    }

    [Fact]
    public Task PlotClear_ClearsActiveRuntimeSource()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var view = new VideraChartView();
            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);

            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Plot.Add.Surface(source, "surface");
            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [4f]);
            view.RenderingStatus.IsReady.Should().BeTrue();

            view.Plot.Clear();

            view.RenderingStatus.IsReady.Should().BeFalse();
            view.RenderingStatus.VisibleTileCount.Should().Be(0);
            view.Source.Should().BeNull();
        });
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

    private static SurfaceMetadata CreateWaterfallMetadata()
    {
        var horizontalCoordinates = new[] { 0d, 2d, 5d, 9d };
        var verticalCoordinates = new[] { 0d, 0.15d, 0.3d, 1d, 1.15d, 1.3d };
        return new SurfaceMetadata(
            new SurfaceExplicitGrid(horizontalCoordinates, verticalCoordinates),
            new SurfaceAxisDescriptor("Time", "s", 0d, 9d, SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceAxisDescriptor("Sweep", null, 0d, 1.3d, SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceValueRange(0d, 10d));
    }
}
