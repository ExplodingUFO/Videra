using System.Reflection;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;
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
            view.Plot.ActiveSeries.Should().BeSameAs(scatterSeries);
            view.Plot.IndexOf(surface).Should().Be(0);
            view.Plot.IndexOf(waterfall).Should().Be(1);
            view.Plot.IndexOf(scatterSeries).Should().Be(2);
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
            view.ScatterRenderingStatus.IsReady.Should().BeTrue();
            view.ScatterRenderingStatus.SeriesCount.Should().Be(scatter.SeriesCount);
            view.ScatterRenderingStatus.PointCount.Should().Be(scatter.PointCount);
        });
    }

    [Fact]
    public void Plot3D_RemoveAndClear_UpdateLifecycleDeterministically()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var surfaceSource = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);
            var waterfallSource = new ScriptedSurfaceTileSource(CreateWaterfallMetadata(), defaultTileValue: 7f);
            var scatter = CreateScatterData();
            var detached = new VideraChartView().Plot.Add.Surface(
                new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 9f),
                "detached");

            var surface = view.Plot.Add.Surface(surfaceSource, "surface");
            var waterfall = view.Plot.Add.Waterfall(waterfallSource, "waterfall");
            var scatterSeries = view.Plot.Add.Scatter(scatter, "scatter");

            view.Plot.Revision.Should().Be(3);
            view.LastRefreshRevision.Should().Be(3);
            view.Plot.ActiveSeries.Should().BeSameAs(scatterSeries);
            view.Plot.IndexOf(detached).Should().Be(-1);

            view.Plot.Remove(detached).Should().BeFalse();
            view.Plot.Revision.Should().Be(3);
            view.LastRefreshRevision.Should().Be(3);
            view.Plot.ActiveSeries.Should().BeSameAs(scatterSeries);

            view.Plot.Remove(scatterSeries).Should().BeTrue();
            view.Plot.Series.Should().Equal(surface, waterfall);
            view.Plot.ActiveSeries.Should().BeSameAs(waterfall);
            view.Plot.IndexOf(scatterSeries).Should().Be(-1);
            view.Plot.Revision.Should().Be(4);
            view.LastRefreshRevision.Should().Be(4);
            SurfaceChartTestHelpers.GetRuntime(view).Source.Should().BeSameAs(waterfallSource);
            view.ScatterRenderingStatus.HasSource.Should().BeFalse();

            view.Plot.Remove(surface).Should().BeTrue();
            view.Plot.Series.Should().Equal(waterfall);
            view.Plot.ActiveSeries.Should().BeSameAs(waterfall);
            view.Plot.IndexOf(waterfall).Should().Be(0);
            view.Plot.Revision.Should().Be(5);
            view.LastRefreshRevision.Should().Be(5);

            view.Plot.Clear();
            view.Plot.Series.Should().BeEmpty();
            view.Plot.ActiveSeries.Should().BeNull();
            view.Plot.Revision.Should().Be(6);
            view.LastRefreshRevision.Should().Be(6);
            SurfaceChartTestHelpers.GetRuntime(view).Source.Should().BeNull();

            view.Plot.Clear();
            view.Plot.Revision.Should().Be(6);
            view.LastRefreshRevision.Should().Be(6);
        });
    }

    [Fact]
    public void Plot3D_RejectsNullLifecycleArgumentsWithoutChangingRevision()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            view.Plot.Add.Surface(new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f));

            var removeNull = () => view.Plot.Remove(null!);
            var indexNull = () => view.Plot.IndexOf(null!);

            removeNull.Should().Throw<ArgumentNullException>();
            indexNull.Should().Throw<ArgumentNullException>();
            view.Plot.Revision.Should().Be(1);
            view.LastRefreshRevision.Should().Be(1);
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
            SurfaceChartTestHelpers.GetRuntime(view).Source.Should().BeSameAs(source);
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
            SurfaceChartTestHelpers.GetRuntime(view).Source.Should().BeSameAs(waterfall);
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
            SurfaceChartTestHelpers.GetRuntime(view).Source.Should().BeNull();
        });
    }

    [Fact]
    public void PlotAddScatter_ActivatesScatterRenderingStatus()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var scatter = CreateScatterData();

            view.Plot.Add.Scatter(scatter, "scatter");

            view.ScatterRenderingStatus.HasSource.Should().BeTrue();
            view.ScatterRenderingStatus.IsReady.Should().BeTrue();
            view.ScatterRenderingStatus.BackendKind.Should().Be(SurfaceChartRenderBackendKind.Software);
            view.ScatterRenderingStatus.InteractionQuality.Should().Be(SurfaceChartInteractionQuality.Refine);
            view.ScatterRenderingStatus.SeriesCount.Should().Be(scatter.SeriesCount);
            view.ScatterRenderingStatus.PointCount.Should().Be(scatter.PointCount);
            view.ScatterRenderingStatus.ColumnarSeriesCount.Should().Be(scatter.ColumnarSeriesCount);
            view.ScatterRenderingStatus.ColumnarPointCount.Should().Be(scatter.ColumnarPointCount);
            view.ScatterRenderingStatus.PickablePointCount.Should().Be(scatter.PickablePointCount);
            view.ScatterRenderingStatus.StreamingAppendBatchCount.Should().Be(scatter.StreamingAppendBatchCount);
            view.ScatterRenderingStatus.StreamingReplaceBatchCount.Should().Be(scatter.StreamingReplaceBatchCount);
            view.ScatterRenderingStatus.StreamingDroppedPointCount.Should().Be(scatter.StreamingDroppedPointCount);
            view.ScatterRenderingStatus.LastStreamingDroppedPointCount.Should().Be(scatter.LastStreamingDroppedPointCount);
            view.ScatterRenderingStatus.ConfiguredFifoCapacity.Should().Be(scatter.ConfiguredFifoCapacity);
            SurfaceChartTestHelpers.GetRuntime(view).Source.Should().BeNull();
        });
    }

    [Fact]
    public void PlotAddSurface_ClearsActiveScatterRenderingStatus()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var scatter = CreateScatterData();
            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);

            view.Plot.Add.Scatter(scatter, "scatter");
            view.ScatterRenderingStatus.IsReady.Should().BeTrue();

            view.Plot.Add.Surface(source, "surface");

            view.ScatterRenderingStatus.HasSource.Should().BeFalse();
            view.ScatterRenderingStatus.IsReady.Should().BeFalse();
            view.ScatterRenderingStatus.PointCount.Should().Be(0);
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
        typeof(VideraChartView).GetProperty("Source").Should().BeNull();
        typeof(VideraChartView).GetField("SourceProperty", BindingFlags.Public | BindingFlags.Static).Should().BeNull();
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
        var columnarSeries = new ScatterColumnarSeries(0xFF5EEAD4, "columnar", pickable: true, fifoCapacity: 3);
        columnarSeries.ReplaceRange(new ScatterColumnarData(
            new[] { 0.1f, 0.2f },
            new[] { 0.3f, 0.4f },
            new[] { 0.5f, 0.6f }));
        columnarSeries.AppendRange(new ScatterColumnarData(
            new[] { 0.3f, 0.4f, 0.5f },
            new[] { 0.5f, 0.6f, 0.7f },
            new[] { 0.7f, 0.8f, 0.9f }));

        return new ScatterChartData(metadata, [series], [columnarSeries]);
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
