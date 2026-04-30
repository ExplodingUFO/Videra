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
    public void PlotAddRawArrays_ReturnsTypedPlottableHandles()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var surfaceValues = new[,]
            {
                { 1d, 2d },
                { 3d, 4d },
                { 5d, 6d },
            };
            var waterfallValues = new[,]
            {
                { 10d, 20d },
                { 30d, 40d },
            };

            Plot3DSeries surface = view.Plot.Add.Surface(surfaceValues, "surface");
            Plot3DSeries waterfall = view.Plot.Add.Waterfall(waterfallValues, "waterfall");
            Plot3DSeries scatter = view.Plot.Add.Scatter(
                new[] { 0d, 1d, 2d },
                new[] { 10d, 20d, 30d },
                new[] { 100d, 110d, 120d },
                "scatter",
                0xFF123456u);
            BarPlot3DSeries bar = view.Plot.Add.Bar(new[] { 3d, 6d, 9d }, "bar");
            ContourPlot3DSeries contour = view.Plot.Add.Contour(new[,] { { 1d, 2d }, { 3d, 4d } }, "contour");
            Plot3DSeries baseBar = bar;
            Plot3DSeries baseContour = contour;

            surface.Should().BeOfType<SurfacePlot3DSeries>();
            waterfall.Should().BeOfType<WaterfallPlot3DSeries>();
            scatter.Should().BeOfType<ScatterPlot3DSeries>();
            bar.Should().BeOfType<BarPlot3DSeries>();
            contour.Should().BeOfType<ContourPlot3DSeries>();
            surface.Should().BeAssignableTo<IPlottable3D>();
            waterfall.Should().BeAssignableTo<IPlottable3D>();
            scatter.Should().BeAssignableTo<IPlottable3D>();
            bar.Should().BeAssignableTo<IPlottable3D>();
            contour.Should().BeAssignableTo<IPlottable3D>();
            baseBar.Should().BeSameAs(bar);
            baseContour.Should().BeSameAs(contour);

            surface.Kind.Should().Be(Plot3DSeriesKind.Surface);
            surface.SurfaceSource!.Metadata.Width.Should().Be(3);
            surface.SurfaceSource.Metadata.Height.Should().Be(2);
            surface.SurfaceSource.Metadata.ValueRange.Should().Be(new SurfaceValueRange(1d, 6d));
            waterfall.Kind.Should().Be(Plot3DSeriesKind.Waterfall);
            waterfall.SurfaceSource!.Metadata.ValueRange.Should().Be(new SurfaceValueRange(10d, 40d));
            scatter.Kind.Should().Be(Plot3DSeriesKind.Scatter);
            scatter.ScatterData!.PointCount.Should().Be(3);
            scatter.ScatterData.Metadata.HorizontalAxis.Maximum.Should().Be(2d);
            scatter.ScatterData.Metadata.ValueRange.Maximum.Should().Be(30d);
            scatter.ScatterData.Metadata.DepthAxis.Maximum.Should().Be(120d);
            scatter.ScatterData.Series[0].Color.Should().Be(0xFF123456u);
            bar.Kind.Should().Be(Plot3DSeriesKind.Bar);
            bar.BarData!.SeriesCount.Should().Be(1);
            bar.BarData.CategoryLabels.Should().BeEmpty();
            contour.Kind.Should().Be(Plot3DSeriesKind.Contour);
            contour.ContourData!.Field.Width.Should().Be(2);

            view.Plot.Series.Should().Equal(surface, waterfall, scatter, bar, contour);
            view.Plot.ActiveSeries.Should().BeSameAs(contour);
        });
    }

    [Fact]
    public void PlotPlottableHandles_UpdateLabelAndVisibilityThroughPlotRevision()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var surface = view.Plot.Add.Surface(new[,] { { 1d, 2d }, { 3d, 4d } }, "surface");
            var scatter = view.Plot.Add.Scatter(
                new[] { 0d, 1d },
                new[] { 1d, 2d },
                new[] { 2d, 3d },
                "scatter");
            IPlottable3D plottable = scatter;

            plottable.Label.Should().Be("scatter");
            plottable.IsVisible.Should().BeTrue();
            view.Plot.ActiveSeries.Should().BeSameAs(scatter);
            view.Plot.Revision.Should().Be(2);

            plottable.Label = "points";
            plottable.IsVisible = false;

            scatter.Name.Should().Be("points");
            view.Plot.ActiveSeries.Should().BeSameAs(surface);
            view.Plot.Revision.Should().Be(4);
            view.Plot.CreateDatasetEvidence().ActiveSeriesIndex.Should().Be(0);
            view.Plot.CreateDatasetEvidence().Series.Select(series => series.Identity).Should().Equal(
                "PlotSeries[0]:Surface:surface",
                "PlotSeries[1]:Scatter:points");
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
    public void Plot3D_MoveAndTypedSeriesQueries_UpdateLifecycleDeterministically()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var detached = new VideraChartView().Plot.Add.Surface(new[,] { { 9d } }, "detached");
            var surface = view.Plot.Add.Surface(new[,] { { 1d, 2d }, { 3d, 4d } }, "surface");
            var waterfall = view.Plot.Add.Waterfall(new[,] { { 10d, 20d }, { 30d, 40d } }, "waterfall");
            var scatter = view.Plot.Add.Scatter(
                new[] { 0d, 1d },
                new[] { 1d, 2d },
                new[] { 2d, 3d },
                "scatter");
            var secondSurface = view.Plot.Add.Surface(new[,] { { 5d, 6d }, { 7d, 8d } }, "surface-2");

            view.Plot.GetSeries<SurfacePlot3DSeries>().Should().Equal(surface, secondSurface);
            view.Plot.GetSeries<WaterfallPlot3DSeries>().Should().Equal(waterfall);
            view.Plot.GetSeries<ScatterPlot3DSeries>().Should().Equal(scatter);
            view.Plot.GetSeries<BarPlot3DSeries>().Should().BeEmpty();
            view.Plot.GetSeries<ContourPlot3DSeries>().Should().BeEmpty();
            view.Plot.Revision.Should().Be(4);
            view.LastRefreshRevision.Should().Be(4);

            view.Plot.Move(detached, 0).Should().BeFalse();
            view.Plot.Move(scatter, 2).Should().BeTrue();
            view.Plot.Revision.Should().Be(4);
            view.LastRefreshRevision.Should().Be(4);

            view.Plot.Move(surface, 3).Should().BeTrue();

            view.Plot.Series.Should().Equal(waterfall, scatter, secondSurface, surface);
            view.Plot.GetSeries<SurfacePlot3DSeries>().Should().Equal(secondSurface, surface);
            view.Plot.IndexOf(surface).Should().Be(3);
            view.Plot.ActiveSeries.Should().BeSameAs(surface);
            view.Plot.Revision.Should().Be(5);
            view.LastRefreshRevision.Should().Be(5);
            view.Plot.CreateDatasetEvidence().Series.Select(series => series.Identity).Should().Equal(
                "PlotSeries[0]:Waterfall:waterfall",
                "PlotSeries[1]:Scatter:scatter",
                "PlotSeries[2]:Surface:surface-2",
                "PlotSeries[3]:Surface:surface");

            var outputEvidence = view.Plot.CreateOutputEvidence(
                view.RenderingStatus,
                view.ScatterRenderingStatus,
                view.BarRenderingStatus,
                view.ContourRenderingStatus);
            outputEvidence.ActiveSeriesIdentity.Should().Be(
                "Composed:Surface:PlotSeries[2]:Surface:surface-2|PlotSeries[3]:Surface:surface");

            var movePastEnd = () => view.Plot.Move(secondSurface, 4);
            movePastEnd.Should().Throw<ArgumentOutOfRangeException>();
            view.Plot.Revision.Should().Be(5);
            view.LastRefreshRevision.Should().Be(5);
        });
    }

    [Fact]
    public void Plot3D_DatasetEvidence_ReportsSurfaceMetadataFromCurrentSeries()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            view.Plot.OverlayOptions = SurfaceChartNumericLabelPresets.Fixed(2);
            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);

            view.Plot.Add.Surface(source, "surface");

            var evidence = view.Plot.CreateDatasetEvidence();
            var series = evidence.Series.Should().ContainSingle().Subject;
            evidence.EvidenceKind.Should().Be("Plot3DDatasetEvidence");
            evidence.PlotRevision.Should().Be(view.Plot.Revision);
            evidence.ActiveSeriesIndex.Should().Be(0);
            evidence.PrecisionProfile.Should().Be("SurfaceChartOverlayOptions:Tick=Fixed(2);Legend=Fixed(2);Formatter=Default");
            series.Identity.Should().Be("PlotSeries[0]:Surface:surface");
            series.Name.Should().Be("surface");
            series.Kind.Should().Be(Plot3DSeriesKind.Surface);
            series.IsActive.Should().BeTrue();
            series.Width.Should().Be(source.Metadata.Width);
            series.Height.Should().Be(source.Metadata.Height);
            series.SampleCount.Should().Be(source.Metadata.SampleCount);
            series.HorizontalAxis!.Label.Should().Be("X");
            series.HorizontalAxis.ScaleKind.Should().Be(SurfaceAxisScaleKind.Linear);
            series.VerticalAxis!.Label.Should().Be("Y");
            series.ValueRange!.Minimum.Should().Be(0d);
            series.ValueRange.Maximum.Should().Be(100d);
            series.SamplingProfile.Should().Be("RegularGrid:XSpacing=1;YSpacing=1");
        });
    }

    [Fact]
    public void Plot3D_DatasetEvidence_ReportsWaterfallExplicitSamplingMetadata()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var source = new ScriptedSurfaceTileSource(CreateWaterfallMetadata(), defaultTileValue: 7f);

            view.Plot.Add.Waterfall(source, "waterfall");

            var series = view.Plot.CreateDatasetEvidence().Series.Should().ContainSingle().Subject;
            series.Identity.Should().Be("PlotSeries[0]:Waterfall:waterfall");
            series.Kind.Should().Be(Plot3DSeriesKind.Waterfall);
            series.Width.Should().Be(4);
            series.Height.Should().Be(6);
            series.SampleCount.Should().Be(24);
            series.HorizontalAxis!.Label.Should().Be("Time");
            series.HorizontalAxis.Unit.Should().Be("s");
            series.HorizontalAxis.ScaleKind.Should().Be(SurfaceAxisScaleKind.ExplicitCoordinates);
            series.VerticalAxis!.Label.Should().Be("Sweep");
            series.SamplingProfile.Should().Be("ExplicitCoordinates:Width=4;Height=6");
        });
    }

    [Fact]
    public void Plot3D_DatasetEvidence_ReportsScatterMetadataCountsAndStreamingCounters()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var scatter = CreateScatterData();

            view.Plot.Add.Scatter(scatter, "scatter");

            var series = view.Plot.CreateDatasetEvidence().Series.Should().ContainSingle().Subject;
            series.Identity.Should().Be("PlotSeries[0]:Scatter:scatter");
            series.Kind.Should().Be(Plot3DSeriesKind.Scatter);
            series.SeriesCount.Should().Be(scatter.SeriesCount);
            series.PointCount.Should().Be(scatter.PointCount);
            series.ColumnarSeriesCount.Should().Be(scatter.ColumnarSeriesCount);
            series.ColumnarPointCount.Should().Be(scatter.ColumnarPointCount);
            series.PickablePointCount.Should().Be(scatter.PickablePointCount);
            series.StreamingAppendBatchCount.Should().Be(scatter.StreamingAppendBatchCount);
            series.StreamingReplaceBatchCount.Should().Be(scatter.StreamingReplaceBatchCount);
            series.StreamingDroppedPointCount.Should().Be(scatter.StreamingDroppedPointCount);
            series.LastStreamingDroppedPointCount.Should().Be(scatter.LastStreamingDroppedPointCount);
            series.ConfiguredFifoCapacity.Should().Be(scatter.ConfiguredFifoCapacity);
            series.HorizontalAxis!.Label.Should().Be("X");
            series.HorizontalAxis.Unit.Should().Be("m");
            series.DepthAxis!.Label.Should().Be("Z");
            series.ValueRange!.Maximum.Should().Be(1d);
            series.SamplingProfile.Should().Be("ScatterPoints");

            var columnar = series.ColumnarSeries.Should().ContainSingle().Subject;
            columnar.Label.Should().Be("columnar");
            columnar.PointCount.Should().Be(3);
            columnar.Pickable.Should().BeTrue();
            columnar.FifoCapacity.Should().Be(3);
            columnar.AppendBatchCount.Should().Be(1);
            columnar.ReplaceBatchCount.Should().Be(1);
            columnar.TotalAppendedPointCount.Should().Be(3);
            columnar.TotalDroppedPointCount.Should().Be(2);
            columnar.LastDroppedPointCount.Should().Be(2);
        });
    }

    [Fact]
    public void Plot3D_DatasetEvidence_ReportsBarCategoryLabelsWhenPresent()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();

            var bar = view.Plot.Add.Bar([10.0, 20.0, 30.0], ["Q1", "Q2", "Q3"], "revenue");

            bar.BarData!.CategoryLabels.Should().Equal("Q1", "Q2", "Q3");
            var series = view.Plot.CreateDatasetEvidence().Series.Should().ContainSingle().Subject;
            series.Identity.Should().Be("PlotSeries[0]:Bar:revenue");
            series.Kind.Should().Be(Plot3DSeriesKind.Bar);
            series.SeriesCount.Should().Be(1);
            series.PointCount.Should().Be(3);
            series.CategoryLabels.Should().Equal("Q1", "Q2", "Q3");
            series.SamplingProfile.Should().Be("BarChart:Categories=3;Series=1;Layout=Grouped;CategoryLabels=3");
        });
    }

    [Fact]
    public void Plot3D_DatasetEvidence_ReadsCurrentSeriesAfterLifecycleChanges()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            view.Plot.Add.Surface(
                new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f),
                "surface");
            var waterfall = view.Plot.Add.Waterfall(
                new ScriptedSurfaceTileSource(CreateWaterfallMetadata(), defaultTileValue: 7f),
                "waterfall");
            var scatter = view.Plot.Add.Scatter(CreateScatterData(), "scatter");

            view.Plot.CreateDatasetEvidence().Series.Select(item => item.Identity).Should().Equal(
                "PlotSeries[0]:Surface:surface",
                "PlotSeries[1]:Waterfall:waterfall",
                "PlotSeries[2]:Scatter:scatter");
            view.Plot.CreateDatasetEvidence().ActiveSeriesIndex.Should().Be(2);

            view.Plot.Remove(waterfall).Should().BeTrue();

            var afterRemove = view.Plot.CreateDatasetEvidence();
            afterRemove.Series.Select(item => item.Identity).Should().Equal(
                "PlotSeries[0]:Surface:surface",
                "PlotSeries[1]:Scatter:scatter");
            afterRemove.Series[1].IsActive.Should().BeTrue();
            afterRemove.ActiveSeriesIndex.Should().Be(1);
            view.Plot.IndexOf(scatter).Should().Be(1);

            view.Plot.Remove(scatter).Should().BeTrue();

            var afterScatterRemove = view.Plot.CreateDatasetEvidence();
            afterScatterRemove.Series.Select(item => item.Identity).Should().Equal("PlotSeries[0]:Surface:surface");
            afterScatterRemove.Series[0].IsActive.Should().BeTrue();

            view.Plot.Clear();

            var empty = view.Plot.CreateDatasetEvidence();
            empty.Series.Should().BeEmpty();
            empty.ActiveSeriesIndex.Should().Be(-1);
            empty.PlotRevision.Should().Be(view.Plot.Revision);
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
            var moveNull = () => view.Plot.Move(null!, 0);

            removeNull.Should().Throw<ArgumentNullException>();
            indexNull.Should().Throw<ArgumentNullException>();
            moveNull.Should().Throw<ArgumentNullException>();
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
    public void Plot3D_AxesFacade_IsDiscoverableAndUpdatesExistingStateOwners()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new RecordingSurfaceTileSource(metadata);
            var view = new VideraChartView();
            SurfaceChartTestHelpers.LoadSurface(view, source);
            view.FitToData();

            view.Plot.Axes.X.Label = "Elapsed";
            view.Plot.Axes.X.Unit = "ms";
            view.Plot.Axes.Y.Label = "Amplitude";
            view.Plot.Axes.Y.Unit = "dB";
            view.Plot.Axes.Z.Label = "Band";
            view.Plot.Axes.Z.Unit = "Hz";
            view.Plot.Axes.X.SetLimits(4d, 20d);
            view.Plot.Axes.Z.SetLimits(8d, 24d);
            view.Plot.Axes.Y.SetLimits(-12d, 18d);

            typeof(Plot3D).GetProperty(nameof(Plot3D.Axes)).Should().NotBeNull();
            typeof(PlotAxes3D).GetProperty(nameof(PlotAxes3D.X)).Should().NotBeNull();
            typeof(PlotAxes3D).GetProperty(nameof(PlotAxes3D.Y)).Should().NotBeNull();
            typeof(PlotAxes3D).GetProperty(nameof(PlotAxes3D.Z)).Should().NotBeNull();
            view.Plot.OverlayOptions.HorizontalAxisTitleOverride.Should().Be("Elapsed");
            view.Plot.OverlayOptions.HorizontalAxisUnitOverride.Should().Be("ms");
            view.Plot.OverlayOptions.ValueAxisTitleOverride.Should().Be("Amplitude");
            view.Plot.OverlayOptions.ValueAxisUnitOverride.Should().Be("dB");
            view.Plot.OverlayOptions.DepthAxisTitleOverride.Should().Be("Band");
            view.Plot.OverlayOptions.DepthAxisUnitOverride.Should().Be("Hz");
            view.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(4d, 8d, 16d, 16d));
            view.Plot.ColorMap!.Range.Should().Be(new SurfaceValueRange(-12d, 18d));
            view.Plot.Axes.X.GetLimits().Should().Be(new PlotAxisLimits(4d, 20d));
            view.Plot.Axes.Y.GetLimits().Should().Be(new PlotAxisLimits(-12d, 18d));
            view.Plot.Axes.Z.GetLimits().Should().Be(new PlotAxisLimits(8d, 24d));

            view.Plot.Axes.X.AutoScale();
            view.Plot.Axes.Z.AutoScale();
            view.Plot.Axes.Y.AutoScale();

            view.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height));
            view.Plot.ColorMap.Range.Should().Be(metadata.ValueRange);
        });
    }

    [Fact]
    public void Plot3D_AxisRules_ClampAutoscaleAndRespectLocks()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var source = new RecordingSurfaceTileSource(metadata);
            var view = new VideraChartView();
            SurfaceChartTestHelpers.LoadSurface(view, source);
            view.FitToData();

            view.Plot.Axes.X.SetBounds(2d, 10d);
            view.Plot.Axes.Z.SetBounds(4d, 12d);
            view.Plot.Axes.Y.SetBounds(-6d, 18d);

            view.Plot.Axes.X.SetLimits(0d, 20d);
            view.Plot.Axes.Z.SetLimits(0d, 20d);
            view.Plot.Axes.Y.SetLimits(-20d, 30d);

            view.Plot.Axes.X.GetLimits().Should().Be(new PlotAxisLimits(2d, 10d));
            view.Plot.Axes.Z.GetLimits().Should().Be(new PlotAxisLimits(4d, 12d));
            view.Plot.Axes.Y.GetLimits().Should().Be(new PlotAxisLimits(-6d, 18d));

            view.Plot.Axes.X.IsLocked = true;
            view.Plot.Axes.X.SetLimits(3d, 6d);
            view.Plot.Axes.X.AutoScale();
            view.Plot.Axes.Y.AutoScale();

            view.Plot.Axes.X.GetLimits().Should().Be(new PlotAxisLimits(2d, 10d));
            view.Plot.Axes.Y.GetLimits().Should().Be(new PlotAxisLimits(0d, 18d));

            view.Plot.Axes.X.IsLocked = false;
            view.Plot.Axes.X.ClearBounds();
            view.Plot.Axes.X.AutoScale();

            view.Plot.Axes.X.Bounds.Should().BeNull();
            view.Plot.Axes.X.GetLimits().Should().Be(new PlotAxisLimits(0d, metadata.Width));
        });
    }

    [Fact]
    public void VideraChartView_LinkViewWith_SynchronizesTwoChartsUntilDisposed()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var first = new VideraChartView();
            var second = new VideraChartView();
            SurfaceChartTestHelpers.LoadSurface(first, new RecordingSurfaceTileSource(metadata));
            SurfaceChartTestHelpers.LoadSurface(second, new RecordingSurfaceTileSource(metadata));
            first.FitToData();
            second.FitToData();

            using var link = first.LinkViewWith(second);

            first.ZoomTo(new SurfaceDataWindow(2d, 3d, 8d, 9d));

            second.ViewState.Should().Be(first.ViewState);

            second.ZoomTo(new SurfaceDataWindow(4d, 5d, 6d, 7d));

            first.ViewState.Should().Be(second.ViewState);

            link.Dispose();
            first.ZoomTo(new SurfaceDataWindow(1d, 1d, 5d, 5d));

            second.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(4d, 5d, 6d, 7d));
        });
    }

    [Fact]
    public void Plot3D_CreateOutputEvidence_ReportsActiveSurfaceContract()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);
            var colorMap = new SurfaceColorMap(
                new SurfaceValueRange(-12d, 12d),
                SurfaceColorMapPresets.CreateProfessional());
            view.Plot.ColorMap = colorMap;
            view.Plot.OverlayOptions = SurfaceChartNumericLabelPresets.Scientific(precision: 2);

            view.Plot.Add.Surface(source, "surface");

            var evidence = view.Plot.CreateOutputEvidence(view.RenderingStatus, view.ScatterRenderingStatus);

            evidence.EvidenceKind.Should().Be("plot-3d-output");
            evidence.SeriesCount.Should().Be(1);
            evidence.ActiveSeriesIndex.Should().Be(0);
            evidence.ActiveSeriesName.Should().Be("surface");
            evidence.ActiveSeriesKind.Should().Be(Plot3DSeriesKind.Surface);
            evidence.ActiveSeriesIdentity.Should().Be("Surface:surface:0");
            evidence.ColorMapStatus.Should().Be(Plot3DColorMapStatus.Applied);
            evidence.ColorMapEvidence.Should().NotBeNull();
            evidence.ColorMapEvidence!.PaletteName.Should().Be("Plot.ColorMap");
            evidence.ColorMapEvidence.ColorStops.Should().Equal(
                "#FF08111F",
                "#FF154C79",
                "#FF2DD4BF",
                "#FFFDE68A",
                "#FFF97316");
            evidence.PrecisionProfile.Should().Be("SurfaceChartOverlayOptions:Tick=Scientific(2);Legend=Scientific(2);Formatter=Default");
            evidence.ColorMapEvidence.PrecisionProfile.Should().Be(evidence.PrecisionProfile);
            evidence.ColorMapEvidence.SampleFormattedLabels.Should().Equal("-1.20E+1", "0.00E+0", "1.20E+1");
            evidence.RenderingEvidence.Should().NotBeNull();
            evidence.RenderingEvidence!.RenderingKind.Should().Be("surface-rendering-status");
            evidence.RenderingEvidence.BackendKind.Should().Be(view.RenderingStatus.ActiveBackend);
            evidence.OutputCapabilityDiagnostics.Should().HaveCount(3);
            evidence.OutputCapabilityDiagnostics.First(d => d.Capability == "ImageExport").IsSupported.Should().BeTrue();
            evidence.OutputCapabilityDiagnostics.First(d => d.Capability == "PdfExport").IsSupported.Should().BeFalse();
            evidence.OutputCapabilityDiagnostics.First(d => d.Capability == "VectorExport").IsSupported.Should().BeFalse();
            evidence.OutputCapabilityDiagnostics.Select(diagnostic => diagnostic.Capability).Should().Equal(
                "ImageExport",
                "PdfExport",
                "VectorExport");
        });
    }

    [Fact]
    public Task Plot3D_SavePngAsync_WritesCallerSelectedPngThroughSnapshotPath()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var view = new VideraChartView();
            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);
            var outputPath = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"videra-savepng-{Guid.NewGuid():N}.png");

            try
            {
                view.Measure(new Size(240, 160));
                view.Arrange(new Rect(0, 0, 240, 160));
                view.Plot.Add.Surface(source, "surface");
                await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [4f]);

                var result = await view.Plot.SavePngAsync(outputPath, width: 160, height: 90);

                result.Succeeded.Should().BeTrue(result.Failure?.Message);
                result.Path.Should().Be(outputPath);
                result.Manifest!.Width.Should().Be(160);
                result.Manifest.Height.Should().Be(90);
                System.IO.File.Exists(outputPath).Should().BeTrue();
                System.IO.File.ReadAllBytes(outputPath).Take(8).Should().Equal(137, 80, 78, 71, 13, 10, 26, 10);
            }
            finally
            {
                if (System.IO.File.Exists(outputPath))
                {
                    System.IO.File.Delete(outputPath);
                }
            }
        });
    }

    [Fact]
    public void Plot3D_CreateOutputEvidence_ReportsScatterWithoutColorMapOrExportFallback()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var scatter = CreateScatterData();

            view.Measure(new Size(320, 180));
            view.Arrange(new Rect(0, 0, 320, 180));
            view.Plot.Add.Scatter(scatter, "scatter");

            var evidence = view.Plot.CreateOutputEvidence(view.RenderingStatus, view.ScatterRenderingStatus);

            evidence.SeriesCount.Should().Be(1);
            evidence.ActiveSeriesIndex.Should().Be(0);
            evidence.ActiveSeriesKind.Should().Be(Plot3DSeriesKind.Scatter);
            evidence.ActiveSeriesIdentity.Should().Be("Scatter:scatter:0");
            evidence.ColorMapStatus.Should().Be(Plot3DColorMapStatus.NotApplicable);
            evidence.ColorMapEvidence.Should().BeNull();
            evidence.RenderingEvidence.Should().NotBeNull();
            evidence.RenderingEvidence!.RenderingKind.Should().Be("scatter-rendering-status");
            evidence.RenderingEvidence.IsReady.Should().BeTrue();
            evidence.RenderingEvidence.ViewWidth.Should().Be(320d);
            evidence.RenderingEvidence.ViewHeight.Should().Be(180d);
            evidence.OutputCapabilityDiagnostics.Should().Contain(diagnostic =>
                diagnostic.Capability == "ImageExport" &&
                diagnostic.DiagnosticCode == "plot-output.export.image.supported" &&
                diagnostic.IsSupported);
        });
    }

    [Fact]
    public void Plot3D_CreateOutputEvidence_ReportsEmptyPlotDeterministically()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();

            var evidence = view.Plot.CreateOutputEvidence();

            evidence.EvidenceKind.Should().Be("plot-3d-output");
            evidence.SeriesCount.Should().Be(0);
            evidence.ActiveSeriesIndex.Should().Be(-1);
            evidence.ActiveSeriesName.Should().BeNull();
            evidence.ActiveSeriesKind.Should().BeNull();
            evidence.ActiveSeriesIdentity.Should().BeNull();
            evidence.ColorMapStatus.Should().Be(Plot3DColorMapStatus.NotApplicable);
            evidence.ColorMapEvidence.Should().BeNull();
            evidence.PrecisionProfile.Should().Be("SurfaceChartOverlayOptions:Tick=General(3);Legend=General(3);Formatter=Default");
            evidence.RenderingEvidence.Should().BeNull();
            evidence.OutputCapabilityDiagnostics.Should().HaveCount(3);
            evidence.OutputCapabilityDiagnostics.First(d => d.Capability == "ImageExport").IsSupported.Should().BeTrue();
        });
    }

    [Fact]
    public void Plot3D_CreateOutputEvidence_ReportsBarChartContract()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            view.Measure(new Size(320, 180));
            view.Arrange(new Rect(0, 0, 320, 180));
            view.Plot.Add.Bar(new[] { 1.0, 2.0, 3.0 }, "bars");

            var evidence = view.Plot.CreateOutputEvidence(view.RenderingStatus, view.ScatterRenderingStatus, view.BarRenderingStatus, view.ContourRenderingStatus);

            evidence.SeriesCount.Should().Be(1);
            evidence.ActiveSeriesIndex.Should().Be(0);
            evidence.ActiveSeriesKind.Should().Be(Plot3DSeriesKind.Bar);
            evidence.ActiveSeriesIdentity.Should().Be("Bar:bars:0");
            evidence.ColorMapStatus.Should().Be(Plot3DColorMapStatus.NotApplicable);
            evidence.ColorMapEvidence.Should().BeNull();
            evidence.RenderingEvidence.Should().NotBeNull();
            evidence.RenderingEvidence!.RenderingKind.Should().Be("bar-rendering-status");
            evidence.RenderingEvidence.IsReady.Should().BeTrue();
        });
    }

    [Fact]
    public void Plot3D_CreateOutputEvidence_ReportsContourContract()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var values = CreateContourField(5, 5);
            view.Plot.Add.Contour(values, "contours");

            var evidence = view.Plot.CreateOutputEvidence(view.RenderingStatus, view.ScatterRenderingStatus, view.BarRenderingStatus, view.ContourRenderingStatus);

            evidence.SeriesCount.Should().Be(1);
            evidence.ActiveSeriesIndex.Should().Be(0);
            evidence.ActiveSeriesKind.Should().Be(Plot3DSeriesKind.Contour);
            evidence.ActiveSeriesIdentity.Should().Be("Contour:contours:0");
            evidence.ColorMapStatus.Should().Be(Plot3DColorMapStatus.NotApplicable);
            evidence.ColorMapEvidence.Should().BeNull();
            evidence.RenderingEvidence.Should().NotBeNull();
            evidence.RenderingEvidence!.RenderingKind.Should().Be("contour-rendering-status");
            evidence.RenderingEvidence.IsReady.Should().BeTrue();
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
    public Task PlotAddMultipleVisibleSurfaces_ComposesRuntimeSourceAndEvidence()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var view = new VideraChartView();
            var metadata = VideraChartViewLifecycleTests.CreateMetadata();
            var low = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 2f);
            var high = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 8f);

            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Plot.Add.Surface(low, "low");
            view.Plot.Add.Surface(high, "high");
            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [5f]);

            SurfaceChartTestHelpers.GetRuntime(view).Source.Should().NotBeSameAs(high);
            SurfaceChartTestHelpers.GetRuntime(view).Source!.Metadata.ValueRange.Should().Be(new SurfaceValueRange(0d, 100d));

            var datasetEvidence = view.Plot.CreateDatasetEvidence();
            datasetEvidence.ActiveSeriesIndex.Should().Be(1);
            datasetEvidence.Series.Select(series => series.IsActive).Should().Equal(true, true);

            var outputEvidence = view.Plot.CreateOutputEvidence(view.RenderingStatus, view.ScatterRenderingStatus, view.BarRenderingStatus, view.ContourRenderingStatus);
            outputEvidence.ActiveSeriesIdentity.Should().Be("Composed:Surface:PlotSeries[0]:Surface:low|PlotSeries[1]:Surface:high");
            outputEvidence.ComposedSeriesCount.Should().Be(2);
            outputEvidence.ComposedSeriesIdentities.Should().Equal(
                "PlotSeries[0]:Surface:low",
                "PlotSeries[1]:Surface:high");
        });
    }

    [Fact]
    public void PlotAddMultipleVisibleScatterSeries_ComposesRenderingStatusAndEvidence()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var first = new ScatterChartData(
                new ScatterChartMetadata(
                    new SurfaceAxisDescriptor("X", "m", 0d, 2d),
                    new SurfaceAxisDescriptor("Z", "m", 0d, 2d),
                    new SurfaceValueRange(0d, 2d)),
                [new ScatterSeries([new ScatterPoint(0.5f, 0.5f, 0.5f)], 0xFF2F80ED, "a")]);
            var second = new ScatterChartData(
                new ScatterChartMetadata(
                    new SurfaceAxisDescriptor("X", "m", 0d, 2d),
                    new SurfaceAxisDescriptor("Z", "m", 0d, 2d),
                    new SurfaceValueRange(0d, 2d)),
                [new ScatterSeries([new ScatterPoint(1.5f, 1.5f, 1.5f)], 0xFFFF6B6B, "b")]);

            view.Plot.Add.Scatter(first, "first");
            view.Plot.Add.Scatter(second, "second");

            view.ScatterRenderingStatus.HasSource.Should().BeTrue();
            view.ScatterRenderingStatus.SeriesCount.Should().Be(2);
            view.ScatterRenderingStatus.PointCount.Should().Be(2);

            var outputEvidence = view.Plot.CreateOutputEvidence(view.RenderingStatus, view.ScatterRenderingStatus, view.BarRenderingStatus, view.ContourRenderingStatus);
            outputEvidence.ActiveSeriesKind.Should().Be(Plot3DSeriesKind.Scatter);
            outputEvidence.ActiveSeriesIdentity.Should().Be("Composed:Scatter:PlotSeries[0]:Scatter:first|PlotSeries[1]:Scatter:second");
            outputEvidence.ComposedSeriesCount.Should().Be(2);
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

    private static double[,] CreateContourField(int width, int height)
    {
        var values = new double[width, height];
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var dx = x - (width - 1) / 2.0;
                var dy = y - (height - 1) / 2.0;
                values[x, y] = Math.Sqrt(dx * dx + dy * dy);
            }
        return values;
    }
}
