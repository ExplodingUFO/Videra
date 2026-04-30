using System.Numerics;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;
using Videra.SurfaceCharts.Core;
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
    public void CameraOnly_policy_creates_camera_only_links()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var group = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.CameraOnly);
            var chartA = new VideraChartView();
            var chartB = new VideraChartView();

            var originalDataWindow = new SurfaceDataWindow(0, 0, 100, 100);
            var originalCamera = new SurfaceCameraPose(Vector3.Zero, 0d, 0d, 10d, 45d);
            var modifiedCamera = new SurfaceCameraPose(Vector3.Zero, 45d, 30d, 10d, 45d);

            chartA.ViewState = new SurfaceViewState(originalDataWindow, originalCamera);
            chartB.ViewState = new SurfaceViewState(originalDataWindow, originalCamera);

            group.Add(chartA);
            group.Add(chartB);

            // Change chartA's camera
            chartA.ViewState = new SurfaceViewState(originalDataWindow, modifiedCamera);

            // chartB's camera should match chartA's
            chartB.ViewState.Camera.Should().Be(modifiedCamera);

            // chartB's DataWindow should remain unchanged
            chartB.ViewState.DataWindow.Should().Be(originalDataWindow);
        });
    }

    [Fact]
    public void AxisOnly_policy_creates_axis_only_links()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var group = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.AxisOnly);
            var chartA = new VideraChartView();
            var chartB = new VideraChartView();

            var originalDataWindow = new SurfaceDataWindow(0, 0, 100, 100);
            var modifiedDataWindow = new SurfaceDataWindow(10, 10, 200, 200);
            var originalCamera = new SurfaceCameraPose(Vector3.Zero, 0d, 0d, 10d, 45d);

            chartA.ViewState = new SurfaceViewState(originalDataWindow, originalCamera);
            chartB.ViewState = new SurfaceViewState(originalDataWindow, originalCamera);

            group.Add(chartA);
            group.Add(chartB);

            // Change chartA's data window
            chartA.ViewState = new SurfaceViewState(modifiedDataWindow, originalCamera);

            // chartB's DataWindow should match chartA's
            chartB.ViewState.DataWindow.Should().Be(modifiedDataWindow);

            // chartB's Camera should remain unchanged
            chartB.ViewState.Camera.Should().Be(originalCamera);
        });
    }

    [Fact]
    public void CameraOnly_policy_does_not_sync_data_window()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var group = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.CameraOnly);
            var chartA = new VideraChartView();
            var chartB = new VideraChartView();

            var originalDataWindow = new SurfaceDataWindow(0, 0, 100, 100);
            var modifiedDataWindow = new SurfaceDataWindow(10, 10, 200, 200);
            var originalCamera = new SurfaceCameraPose(Vector3.Zero, 0d, 0d, 10d, 45d);

            chartA.ViewState = new SurfaceViewState(originalDataWindow, originalCamera);
            chartB.ViewState = new SurfaceViewState(originalDataWindow, originalCamera);

            group.Add(chartA);
            group.Add(chartB);

            // Change only chartA's data window (camera stays same)
            chartA.ViewState = new SurfaceViewState(modifiedDataWindow, originalCamera);

            // chartB's DataWindow should NOT change because CameraOnly policy
            chartB.ViewState.DataWindow.Should().Be(originalDataWindow);
        });
    }

    [Fact]
    public void AxisOnly_policy_does_not_sync_camera()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var group = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.AxisOnly);
            var chartA = new VideraChartView();
            var chartB = new VideraChartView();

            var originalDataWindow = new SurfaceDataWindow(0, 0, 100, 100);
            var originalCamera = new SurfaceCameraPose(Vector3.Zero, 0d, 0d, 10d, 45d);
            var modifiedCamera = new SurfaceCameraPose(Vector3.Zero, 45d, 30d, 10d, 45d);

            chartA.ViewState = new SurfaceViewState(originalDataWindow, originalCamera);
            chartB.ViewState = new SurfaceViewState(originalDataWindow, originalCamera);

            group.Add(chartA);
            group.Add(chartB);

            // Change only chartA's camera (data window stays same)
            chartA.ViewState = new SurfaceViewState(originalDataWindow, modifiedCamera);

            // chartB's Camera should NOT change because AxisOnly policy
            chartB.ViewState.Camera.Should().Be(originalCamera);
        });
    }

    [Fact]
    public void Policy_property_returns_configured_policy()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            using var cameraOnly = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.CameraOnly);
            using var axisOnly = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.AxisOnly);
            using var fullView = new SurfaceChartLinkGroup();

            cameraOnly.Policy.Should().Be(SurfaceChartLinkPolicy.CameraOnly);
            axisOnly.Policy.Should().Be(SurfaceChartLinkPolicy.AxisOnly);
            fullView.Policy.Should().Be(SurfaceChartLinkPolicy.FullViewState);
        });
    }
}
