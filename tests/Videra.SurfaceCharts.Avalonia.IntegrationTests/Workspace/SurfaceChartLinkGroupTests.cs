using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests.Workspace;

public sealed class SurfaceChartLinkGroupTests
{
    [Fact]
    public void Add_single_chart_to_group()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var group = new SurfaceChartLinkGroup();
            var chart = new VideraChartView();

            group.Add(chart);

            group.Members.Should().HaveCount(1);
            group.Members[0].Should().BeSameAs(chart);
        });
    }

    [Fact]
    public void Add_multiple_charts_creates_pairwise_links()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var group = new SurfaceChartLinkGroup();
            var chartA = new VideraChartView();
            var chartB = new VideraChartView();
            var chartC = new VideraChartView();

            group.Add(chartA);
            group.Add(chartB);
            group.Add(chartC);

            group.Members.Should().HaveCount(3);
        });
    }

    [Fact]
    public void Add_duplicate_chart_throws()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var group = new SurfaceChartLinkGroup();
            var chart = new VideraChartView();

            group.Add(chart);

            var act = () => group.Add(chart);

            act.Should().Throw<InvalidOperationException>();
        });
    }

    [Fact]
    public void Remove_chart_reduces_member_count()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var group = new SurfaceChartLinkGroup();
            var chartA = new VideraChartView();
            var chartB = new VideraChartView();
            var chartC = new VideraChartView();

            group.Add(chartA);
            group.Add(chartB);
            group.Add(chartC);

            group.Remove(chartB);

            group.Members.Should().HaveCount(2);
            group.Members.Should().Contain(chartA);
            group.Members.Should().Contain(chartC);
            group.Members.Should().NotContain(chartB);
        });
    }

    [Fact]
    public void Dispose_clears_all_members()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var group = new SurfaceChartLinkGroup();
            var chartA = new VideraChartView();
            var chartB = new VideraChartView();
            var chartC = new VideraChartView();

            group.Add(chartA);
            group.Add(chartB);
            group.Add(chartC);

            group.Dispose();

            group.Members.Should().BeEmpty();
        });
    }

    [Fact]
    public void Add_after_dispose_throws()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var group = new SurfaceChartLinkGroup();
            var chart = new VideraChartView();

            group.Dispose();

            var act = () => group.Add(chart);

            act.Should().Throw<ObjectDisposedException>();
        });
    }

    [Fact]
    public void CameraOnly_policy_throws_NotSupported()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var act = () => new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.CameraOnly);

            act.Should().Throw<NotSupportedException>();
        });
    }

    [Fact]
    public void AxisOnly_policy_throws_NotSupported()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var act = () => new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.AxisOnly);

            act.Should().Throw<NotSupportedException>();
        });
    }
}
