using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class VideraChartViewContourIntegrationTests
{
    [Fact]
    public void PlotAddContour_CreatesSeriesWithCorrectKind()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var values = CreateRadialField(5, 5);

            var contour = view.Plot.Add.Contour(values, "contour");

            contour.Kind.Should().Be(Plot3DSeriesKind.Contour);
            contour.Name.Should().Be("contour");
            contour.ContourData.Should().NotBeNull();
            contour.SurfaceSource.Should().BeNull();
            contour.ScatterData.Should().BeNull();
        });
    }

    [Fact]
    public void PlotAddContour_IncrementsRevision()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var values = CreateRadialField(5, 5);

            view.Plot.Revision.Should().Be(0);

            view.Plot.Add.Contour(values, "contour");

            view.Plot.Revision.Should().Be(1);
            view.LastRefreshRevision.Should().Be(1);
        });
    }

    [Fact]
    public void PlotAddContour_UpdatesContourRenderingStatus()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var values = CreateRadialField(5, 5);

            view.ContourRenderingStatus.HasSource.Should().BeFalse();

            view.Plot.Add.Contour(values, "contour");

            view.ContourRenderingStatus.HasSource.Should().BeTrue();
            view.ContourRenderingStatus.LevelCount.Should().Be(10);
        });
    }

    [Fact]
    public void PlotAddContour_WithCustomLevelCount_UsesSpecifiedCount()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var field = CreateScalarField(5, 5);
            var data = new ContourChartData(field, levelCount: 5);

            view.Plot.Add.Contour(data, "contour");

            view.ContourRenderingStatus.LevelCount.Should().Be(5);
        });
    }

    [Fact]
    public void PlotRemoveContour_ClearsContourRenderingStatus()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var values = CreateRadialField(5, 5);

            var contour = view.Plot.Add.Contour(values, "contour");
            view.ContourRenderingStatus.HasSource.Should().BeTrue();

            view.Plot.Remove(contour);

            view.ContourRenderingStatus.HasSource.Should().BeFalse();
        });
    }

    [Fact]
    public void PlotClear_ClearsContourRenderingStatus()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var values = CreateRadialField(5, 5);

            view.Plot.Add.Contour(values, "contour");
            view.ContourRenderingStatus.HasSource.Should().BeTrue();

            view.Plot.Clear();

            view.ContourRenderingStatus.HasSource.Should().BeFalse();
            view.Plot.ActiveContourSeries.Should().BeNull();
        });
    }

    [Fact]
    public void PlotAddContour_ActiveContourSeries_ReturnsLastContour()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var values1 = CreateRadialField(5, 5);
            var values2 = CreateRadialField(7, 7);

            var contour1 = view.Plot.Add.Contour(values1, "contour1");
            view.Plot.ActiveContourSeries.Should().BeSameAs(contour1);

            var contour2 = view.Plot.Add.Contour(values2, "contour2");
            view.Plot.ActiveContourSeries.Should().BeSameAs(contour2);
        });
    }

    [Fact]
    public void PlotAddContour_DatasetEvidence_ReportsContourMetadata()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var values = CreateRadialField(5, 5);

            view.Plot.Add.Contour(values, "contour");

            var series = view.Plot.CreateDatasetEvidence().Series.Should().ContainSingle().Subject;
            series.Identity.Should().Be("PlotSeries[0]:Contour:contour");
            series.Kind.Should().Be(Plot3DSeriesKind.Contour);
            series.Width.Should().Be(5);
            series.Height.Should().Be(5);
            series.SampleCount.Should().Be(25);
            series.SamplingProfile.Should().Contain("ContourPlot");
        });
    }

    [Fact]
    public void PlotAddContour_WithSurface_CoexistsCorrectly()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();
            var source = new ScriptedSurfaceTileSource(VideraChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 4f);
            var values = CreateRadialField(5, 5);

            view.Plot.Add.Surface(source, "surface");
            view.Plot.Add.Contour(values, "contour");

            view.Plot.Series.Should().HaveCount(2);
            view.Plot.Series[0].Kind.Should().Be(Plot3DSeriesKind.Surface);
            view.Plot.Series[1].Kind.Should().Be(Plot3DSeriesKind.Contour);
        });
    }

    [Fact]
    public void PlotAddContour_RejectsNullValues()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new VideraChartView();

            var action = () => view.Plot.Add.Contour((double[,])null!);
            action.Should().Throw<ArgumentNullException>();
        });
    }

    private static double[,] CreateRadialField(int width, int height)
    {
        var values = new double[width, height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var dx = x - (width - 1) / 2.0;
                var dy = y - (height - 1) / 2.0;
                values[x, y] = Math.Sqrt(dx * dx + dy * dy);
            }
        }
        return values;
    }

    private static SurfaceScalarField CreateScalarField(int width, int height)
    {
        var values = new float[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var dx = x - (width - 1) / 2f;
                var dy = y - (height - 1) / 2f;
                values[y * width + x] = MathF.Sqrt(dx * dx + dy * dy);
            }
        }

        var min = values.Min();
        var max = values.Max();
        return new SurfaceScalarField(width, height, values, new SurfaceValueRange(min, max));
    }
}
