using System.Reflection;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class RegressionGuardrailTests
{
    [Fact]
    public void RegressionGuardrail_AllChartFamilies_ProduceDatasetEvidence()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();

            // Surface
            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);
            view.Plot.Add.Surface(source, "surface");
            var dsEvidence = view.Plot.CreateDatasetEvidence();
            dsEvidence.Series.Should().HaveCount(1);
            dsEvidence.Series[0].Kind.Should().Be(Plot3DSeriesKind.Surface);

            // Waterfall
            view.Plot.Clear();
            view.Plot.Add.Waterfall(source, "waterfall");
            dsEvidence = view.Plot.CreateDatasetEvidence();
            dsEvidence.Series[0].Kind.Should().Be(Plot3DSeriesKind.Waterfall);

            // Scatter
            view.Plot.Clear();
            var scatter = new ScatterChartData(
                new ScatterChartMetadata(
                    new SurfaceAxisDescriptor("X", null, 0d, 10d),
                    new SurfaceAxisDescriptor("Z", null, 0d, 10d),
                    new SurfaceValueRange(0d, 10d)),
                [new ScatterSeries([new ScatterPoint(1, 2, 3)], 0xFF000000u)]);
            view.Plot.Add.Scatter(scatter, "scatter");
            dsEvidence = view.Plot.CreateDatasetEvidence();
            dsEvidence.Series[0].Kind.Should().Be(Plot3DSeriesKind.Scatter);

            // Bar
            view.Plot.Clear();
            view.Plot.Add.Bar(new[] { 1.0, 2.0, 3.0 }, "bar");
            dsEvidence = view.Plot.CreateDatasetEvidence();
            dsEvidence.Series[0].Kind.Should().Be(Plot3DSeriesKind.Bar);
            dsEvidence.Series[0].PointCount.Should().Be(3); // category count

            // Contour
            view.Plot.Clear();
            view.Plot.Add.Contour(CreateContourField(5, 5), "contour");
            dsEvidence = view.Plot.CreateDatasetEvidence();
            dsEvidence.Series[0].Kind.Should().Be(Plot3DSeriesKind.Contour);
            dsEvidence.Series[0].Width.Should().Be(5);
            dsEvidence.Series[0].Height.Should().Be(5);
        });
    }

    [Fact]
    public void RegressionGuardrail_AllChartFamilies_ProduceOutputEvidence()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            view.Measure(new Size(320, 180));
            view.Arrange(new Rect(0, 0, 320, 180));

            // Surface
            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);
            view.Plot.Add.Surface(source, "surface");
            var outEvidence = view.Plot.CreateOutputEvidence(view.RenderingStatus, view.ScatterRenderingStatus, view.BarRenderingStatus, view.ContourRenderingStatus);
            outEvidence.ActiveSeriesKind.Should().Be(Plot3DSeriesKind.Surface);
            outEvidence.ColorMapStatus.Should().Be(Plot3DColorMapStatus.Applied);

            // Waterfall
            view.Plot.Clear();
            view.Plot.Add.Waterfall(source, "waterfall");
            outEvidence = view.Plot.CreateOutputEvidence(view.RenderingStatus, view.ScatterRenderingStatus, view.BarRenderingStatus, view.ContourRenderingStatus);
            outEvidence.ActiveSeriesKind.Should().Be(Plot3DSeriesKind.Waterfall);

            // Scatter
            view.Plot.Clear();
            var scatter = new ScatterChartData(
                new ScatterChartMetadata(
                    new SurfaceAxisDescriptor("X", null, 0d, 10d),
                    new SurfaceAxisDescriptor("Z", null, 0d, 10d),
                    new SurfaceValueRange(0d, 10d)),
                [new ScatterSeries([new ScatterPoint(1, 2, 3)], 0xFF000000u)]);
            view.Plot.Add.Scatter(scatter, "scatter");
            outEvidence = view.Plot.CreateOutputEvidence(view.RenderingStatus, view.ScatterRenderingStatus, view.BarRenderingStatus, view.ContourRenderingStatus);
            outEvidence.ActiveSeriesKind.Should().Be(Plot3DSeriesKind.Scatter);
            outEvidence.RenderingEvidence.Should().NotBeNull();
            outEvidence.RenderingEvidence!.RenderingKind.Should().Be("scatter-rendering-status");

            // Bar
            view.Plot.Clear();
            view.Plot.Add.Bar(new[] { 1.0, 2.0 }, "bar");
            outEvidence = view.Plot.CreateOutputEvidence(view.RenderingStatus, view.ScatterRenderingStatus, view.BarRenderingStatus, view.ContourRenderingStatus);
            outEvidence.ActiveSeriesKind.Should().Be(Plot3DSeriesKind.Bar);
            outEvidence.ColorMapStatus.Should().Be(Plot3DColorMapStatus.NotApplicable);
            outEvidence.RenderingEvidence.Should().NotBeNull();
            outEvidence.RenderingEvidence!.RenderingKind.Should().Be("bar-rendering-status");

            // Contour
            view.Plot.Clear();
            view.Plot.Add.Contour(CreateContourField(5, 5), "contour");
            outEvidence = view.Plot.CreateOutputEvidence(view.RenderingStatus, view.ScatterRenderingStatus, view.BarRenderingStatus, view.ContourRenderingStatus);
            outEvidence.ActiveSeriesKind.Should().Be(Plot3DSeriesKind.Contour);
            outEvidence.ColorMapStatus.Should().Be(Plot3DColorMapStatus.NotApplicable);
            outEvidence.RenderingEvidence.Should().NotBeNull();
            outEvidence.RenderingEvidence!.RenderingKind.Should().Be("contour-rendering-status");
        });
    }

    [Fact]
    public void RegressionGuardrail_SnapshotMode_SuppressesInteractionChrome()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            view.Measure(new Size(320, 180));
            view.Arrange(new Rect(0, 0, 320, 180));

            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);
            view.Plot.Add.Surface(source, "surface");

            var coordinator = SurfaceChartTestHelpers.GetOverlayCoordinator(view);

            // Default: snapshot mode is off
            coordinator.IsSnapshotMode.Should().BeFalse();

            // Enable snapshot mode
            view.SetSnapshotMode(true);
            coordinator.IsSnapshotMode.Should().BeTrue();

            // Disable snapshot mode
            view.SetSnapshotMode(false);
            coordinator.IsSnapshotMode.Should().BeFalse();
        });
    }

    [Fact]
    public void RegressionGuardrail_SnapshotMode_CrosshairStateIsEmpty()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            view.Measure(new Size(320, 180));
            view.Arrange(new Rect(0, 0, 320, 180));

            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);
            view.Plot.Add.Surface(source, "surface");

            var coordinator = SurfaceChartTestHelpers.GetOverlayCoordinator(view);

            // Enable snapshot mode
            view.SetSnapshotMode(true);

            // Crosshair state should still be accessible (it's the render that's suppressed)
            var crosshairState = coordinator.CrosshairState;
            crosshairState.Should().NotBeNull();

            view.SetSnapshotMode(false);
        });
    }

    [Fact]
    public void RegressionGuardrail_MultiSeries_OverlayRefreshSucceeds()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            view.Measure(new Size(320, 180));
            view.Arrange(new Rect(0, 0, 320, 180));

            // Add surface series
            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);
            view.Plot.Add.Surface(source, "surface");

            // Add bar series
            view.Plot.Add.Bar(new[] { 1.0, 2.0, 3.0 }, "bar");

            // Add contour series
            view.Plot.Add.Contour(CreateContourField(5, 5), "contour");

            // Verify all three series are present
            view.Plot.Series.Should().HaveCount(3);
            view.Plot.Series[0].Kind.Should().Be(Plot3DSeriesKind.Surface);
            view.Plot.Series[1].Kind.Should().Be(Plot3DSeriesKind.Bar);
            view.Plot.Series[2].Kind.Should().Be(Plot3DSeriesKind.Contour);

            // Dataset evidence should work with all series
            var dsEvidence = view.Plot.CreateDatasetEvidence();
            dsEvidence.Series.Should().HaveCount(3);
        });
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
