using System.Numerics;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests.Rendering;

public class BarRendererTests
{
    [Fact]
    public void BarSeriesCtor_RejectsEmptyValues()
    {
        var act = () => new BarSeries([], 0xFF102030u);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*must not be empty*");
    }

    [Fact]
    public void BarSeriesCtor_RejectsNaNValues()
    {
        var act = () => new BarSeries([1.0, double.NaN, 3.0], 0xFF102030u);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*must not contain NaN*");
    }

    [Fact]
    public void BarSeriesCtor_AcceptsValidValues()
    {
        var series = new BarSeries([10.0, 20.0, 30.0], 0xFF102030u, "Sales");

        series.Values.Should().Equal(10.0, 20.0, 30.0);
        series.Color.Should().Be(0xFF102030u);
        series.Label.Should().Be("Sales");
        series.CategoryCount.Should().Be(3);
    }

    [Fact]
    public void BarSeriesCtor_ImmutabilityEnforced()
    {
        var series = new BarSeries([10.0, 20.0], 0xFF102030u);

        var act = () => ((IList<double>)series.Values)[0] = 99.0;

        act.Should().Throw<NotSupportedException>();
        series.Values[0].Should().Be(10.0);
    }

    [Fact]
    public void BarChartDataCtor_RejectsEmptySeries()
    {
        var act = () => new BarChartData([]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*must contain at least one series*");
    }

    [Fact]
    public void BarChartDataCtor_RejectsMismatchedCategoryCounts()
    {
        var series1 = new BarSeries([10.0, 20.0], 0xFF102030u);
        var series2 = new BarSeries([10.0, 20.0, 30.0], 0xFF405060u);

        var act = () => new BarChartData([series1, series2]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*same number of categories*");
    }

    [Fact]
    public void BarChartDataCtor_AcceptsMatchingSeries()
    {
        var series1 = new BarSeries([10.0, 20.0, 30.0], 0xFF102030u);
        var series2 = new BarSeries([15.0, 25.0, 35.0], 0xFF405060u);

        var data = new BarChartData([series1, series2]);

        data.SeriesCount.Should().Be(2);
        data.CategoryCount.Should().Be(3);
        data.Layout.Should().Be(BarChartLayout.Grouped);
    }

    [Fact]
    public void BarChartDataCtor_AcceptsMatchingCategoryLabels()
    {
        var series = new BarSeries([10.0, 20.0, 30.0], 0xFF102030u);

        var data = new BarChartData([series], ["Q1", "Q2", "Q3"]);

        data.CategoryLabels.Should().Equal("Q1", "Q2", "Q3");
    }

    [Fact]
    public void BarChartDataCtor_RejectsMismatchedCategoryLabels()
    {
        var series = new BarSeries([10.0, 20.0, 30.0], 0xFF102030u);

        var act = () => new BarChartData([series], ["Q1", "Q2"]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Category label count*");
    }

    [Fact]
    public void BarChartDataCtor_CategoryLabelImmutabilityEnforced()
    {
        var series = new BarSeries([10.0, 20.0], 0xFF102030u);
        var data = new BarChartData([series], ["Q1", "Q2"]);

        var act = () => ((IList<string>)data.CategoryLabels)[0] = "Changed";

        act.Should().Throw<NotSupportedException>();
        data.CategoryLabels[0].Should().Be("Q1");
    }

    [Fact]
    public void BarChartDataCtor_SupportsStackedLayout()
    {
        var series = new BarSeries([10.0, 20.0], 0xFF102030u);

        var data = new BarChartData([series], BarChartLayout.Stacked);

        data.Layout.Should().Be(BarChartLayout.Stacked);
    }

    [Fact]
    public void BuildScene_GroupedLayout_PositionsSeriesSideBySide()
    {
        var series1 = new BarSeries([100.0, 200.0], 0xFF102030u, "A");
        var series2 = new BarSeries([150.0, 250.0], 0xFF405060u, "B");
        var data = new BarChartData([series1, series2], BarChartLayout.Grouped);

        var scene = BarRenderer.BuildScene(data);

        scene.CategoryCount.Should().Be(2);
        scene.SeriesCount.Should().Be(2);
        scene.Layout.Should().Be(BarChartLayout.Grouped);
        scene.Bars.Should().HaveCount(4); // 2 categories * 2 series

        // First bar: series 0, category 0
        var bar00 = scene.Bars[0];
        bar00.Color.Should().Be(0xFF102030u);

        // Second bar: series 0, category 1
        var bar01 = scene.Bars[1];
        bar01.Color.Should().Be(0xFF102030u);

        // Third bar: series 1, category 0
        var bar10 = scene.Bars[2];
        bar10.Color.Should().Be(0xFF405060u);

        // Grouped bars at same category should have different X positions
        bar00.Position.X.Should().NotBe(bar10.Position.X);
    }

    [Fact]
    public void BuildScene_StackedLayout_StacksBarsVertically()
    {
        var series1 = new BarSeries([100.0, 200.0], 0xFF102030u, "A");
        var series2 = new BarSeries([50.0, 100.0], 0xFF405060u, "B");
        var data = new BarChartData([series1, series2], BarChartLayout.Stacked);

        var scene = BarRenderer.BuildScene(data);

        scene.CategoryCount.Should().Be(2);
        scene.SeriesCount.Should().Be(2);
        scene.Layout.Should().Be(BarChartLayout.Stacked);
        scene.Bars.Should().HaveCount(4);

        // Stacked bars at same category should have same X position
        var bar00 = scene.Bars[0]; // series 0, category 0
        var bar10 = scene.Bars[2]; // series 1, category 0
        bar00.Position.X.Should().Be(bar10.Position.X);

        // Second series bar should be above first series bar
        bar10.Position.Y.Should().BeGreaterThan(bar00.Position.Y);
    }

    [Fact]
    public void BuildScene_SingleSeries_GroupedLayout()
    {
        var series = new BarSeries([10.0, 20.0, 30.0], 0xFF102030u);
        var data = new BarChartData([series], BarChartLayout.Grouped);

        var scene = BarRenderer.BuildScene(data);

        scene.Bars.Should().HaveCount(3);
        scene.Bars[0].Position.X.Should().Be(0f);
        scene.Bars[1].Position.X.Should().Be(1f);
        scene.Bars[2].Position.X.Should().Be(2f);
    }

    [Fact]
    public void BuildScene_BarHeightsProportionalToValues()
    {
        var series = new BarSeries([100.0, 200.0], 0xFF102030u);
        var data = new BarChartData([series]);

        var scene = BarRenderer.BuildScene(data);

        var bar0Height = scene.Bars[0].Size.Y;
        var bar1Height = scene.Bars[1].Size.Y;

        // Bar 1 should be twice as tall as bar 0
        bar1Height.Should().BeApproximately(bar0Height * 2f, 0.01f);
    }

    [Fact]
    public void BuildScene_BarWidthIncludesEpsilonGap()
    {
        var series = new BarSeries([10.0], 0xFF102030u);
        var data = new BarChartData([series]);

        var scene = BarRenderer.BuildScene(data);

        // Bar width should be less than full category width (0.8 - epsilon)
        scene.Bars[0].Size.X.Should().BeLessThan(0.8f);
    }

    [Fact]
    public void PlotAddBar_CreatesBarSeries()
    {
        var view = new VideraChartView();
        var plot = view.Plot;

        var series = plot.Add.Bar([10.0, 20.0, 30.0]);

        series.Kind.Should().Be(Plot3DSeriesKind.Bar);
        series.BarData.Should().NotBeNull();
        series.BarData!.SeriesCount.Should().Be(1);
        series.BarData.CategoryCount.Should().Be(3);
    }

    [Fact]
    public void PlotAddBar_WithFullData_CreatesBarSeries()
    {
        var view = new VideraChartView();
        var plot = view.Plot;
        var barSeries = new BarSeries([10.0, 20.0], 0xFF102030u, "Sales");
        var data = new BarChartData([barSeries]);

        var series = plot.Add.Bar(data, "Revenue");

        series.Kind.Should().Be(Plot3DSeriesKind.Bar);
        series.Name.Should().Be("Revenue");
        series.BarData.Should().BeSameAs(data);
    }

    [Fact]
    public void PlotActiveBarSeries_ReturnsBarSeriesWhenActive()
    {
        var view = new VideraChartView();
        var plot = view.Plot;

        plot.Add.Bar([10.0, 20.0]);

        plot.ActiveBarSeries.Should().NotBeNull();
        plot.ActiveBarSeries!.Kind.Should().Be(Plot3DSeriesKind.Bar);
    }

    [Fact]
    public void PlotActiveBarSeries_ReturnsNullWhenScatterActive()
    {
        var view = new VideraChartView();
        var plot = view.Plot;

        var metadata = new ScatterChartMetadata(
            new SurfaceAxisDescriptor("X", null, 0, 100),
            new SurfaceAxisDescriptor("Z", null, 0, 100),
            new SurfaceValueRange(0, 100));
        var scatterData = new ScatterChartData(metadata, [
            new ScatterSeries([new ScatterPoint(10, 20, 30)], 0xFF102030u)
        ]);
        plot.Add.Scatter(scatterData);

        plot.ActiveBarSeries.Should().BeNull();
    }

    [Fact]
    public void BarChartData_ImmutabilityEnforced()
    {
        var series = new BarSeries([10.0, 20.0], 0xFF102030u);
        var data = new BarChartData([series]);

        var act = () => ((IList<BarSeries>)data.Series)[0] = new BarSeries([99.0], 0xFF405060u);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void BuildScene_AllZeroValues_ProducesZeroHeightBars()
    {
        var series = new BarSeries([0.0, 0.0], 0xFF102030u);
        var data = new BarChartData([series]);

        var scene = BarRenderer.BuildScene(data);

        scene.Bars.Should().HaveCount(2);
        scene.Bars[0].Size.Y.Should().Be(0f);
        scene.Bars[1].Size.Y.Should().Be(0f);
    }
}
