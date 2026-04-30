using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests.Workspace;

public sealed class SurfaceChartWorkspaceTests
{
    [Fact]
    public void Register_single_chart_sets_it_as_active()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var workspace = new SurfaceChartWorkspace();
            var chart = new VideraChartView();
            var info = new SurfaceChartPanelInfo("chart-a", "Surface A", Plot3DSeriesKind.Surface);

            workspace.Register(chart, info);

            workspace.GetActiveChartId().Should().Be("chart-a");
        });
    }

    [Fact]
    public void Register_multiple_charts_first_becomes_active()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var workspace = new SurfaceChartWorkspace();
            var chartA = new VideraChartView();
            var chartB = new VideraChartView();
            var chartC = new VideraChartView();

            workspace.Register(chartA, new SurfaceChartPanelInfo("chart-a", "Surface A", Plot3DSeriesKind.Surface));
            workspace.Register(chartB, new SurfaceChartPanelInfo("chart-b", "Bar B", Plot3DSeriesKind.Bar));
            workspace.Register(chartC, new SurfaceChartPanelInfo("chart-c", "Scatter C", Plot3DSeriesKind.Scatter));

            workspace.GetActiveChartId().Should().Be("chart-a");
        });
    }

    [Fact]
    public void SetActiveChart_changes_active()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var workspace = new SurfaceChartWorkspace();
            var chartA = new VideraChartView();
            var chartB = new VideraChartView();

            workspace.Register(chartA, new SurfaceChartPanelInfo("chart-a", "Surface A", Plot3DSeriesKind.Surface));
            workspace.Register(chartB, new SurfaceChartPanelInfo("chart-b", "Bar B", Plot3DSeriesKind.Bar));

            workspace.SetActiveChart("chart-b");

            workspace.GetActiveChartId().Should().Be("chart-b");
        });
    }

    [Fact]
    public void SetActiveChart_throws_for_unregistered_id()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var workspace = new SurfaceChartWorkspace();

            var act = () => workspace.SetActiveChart("nonexistent");

            act.Should().Throw<InvalidOperationException>();
        });
    }

    [Fact]
    public void Unregister_removes_chart()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var workspace = new SurfaceChartWorkspace();
            var chartA = new VideraChartView();
            var chartB = new VideraChartView();

            workspace.Register(chartA, new SurfaceChartPanelInfo("chart-a", "Surface A", Plot3DSeriesKind.Surface));
            workspace.Register(chartB, new SurfaceChartPanelInfo("chart-b", "Bar B", Plot3DSeriesKind.Bar));

            workspace.Unregister("chart-a");

            workspace.GetRegisteredCharts().Should().HaveCount(1);
            workspace.GetRegisteredCharts()[0].Info.ChartId.Should().Be("chart-b");
        });
    }

    [Fact]
    public void Unregister_active_chart_promotes_first_remaining()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var workspace = new SurfaceChartWorkspace();
            var chartA = new VideraChartView();
            var chartB = new VideraChartView();

            workspace.Register(chartA, new SurfaceChartPanelInfo("chart-a", "Surface A", Plot3DSeriesKind.Surface));
            workspace.Register(chartB, new SurfaceChartPanelInfo("chart-b", "Bar B", Plot3DSeriesKind.Bar));

            workspace.GetActiveChartId().Should().Be("chart-a");

            workspace.Unregister("chart-a");

            workspace.GetActiveChartId().Should().Be("chart-b");
        });
    }

    [Fact]
    public void Unregister_last_chart_clears_active()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var workspace = new SurfaceChartWorkspace();
            var chartA = new VideraChartView();

            workspace.Register(chartA, new SurfaceChartPanelInfo("chart-a", "Surface A", Plot3DSeriesKind.Surface));

            workspace.Unregister("chart-a");

            workspace.GetActiveChartId().Should().BeNull();
        });
    }

    [Fact]
    public void Register_duplicate_chart_id_throws()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var workspace = new SurfaceChartWorkspace();
            var chartA = new VideraChartView();
            var chartB = new VideraChartView();

            workspace.Register(chartA, new SurfaceChartPanelInfo("a", "Surface A", Plot3DSeriesKind.Surface));

            var act = () => workspace.Register(chartB, new SurfaceChartPanelInfo("a", "Surface B", Plot3DSeriesKind.Surface));

            act.Should().Throw<InvalidOperationException>();
        });
    }

    [Fact]
    public void Register_after_dispose_throws()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var workspace = new SurfaceChartWorkspace();
            var chart = new VideraChartView();

            workspace.Dispose();

            var act = () => workspace.Register(chart, new SurfaceChartPanelInfo("chart-a", "Surface A", Plot3DSeriesKind.Surface));

            act.Should().Throw<ObjectDisposedException>();
        });
    }

    [Fact]
    public void Dispose_clears_references()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var workspace = new SurfaceChartWorkspace();
            var chart = new VideraChartView();

            workspace.Register(chart, new SurfaceChartPanelInfo("chart-a", "Surface A", Plot3DSeriesKind.Surface));

            workspace.Dispose();

            workspace.GetRegisteredCharts().Should().BeEmpty();
        });
    }
}
