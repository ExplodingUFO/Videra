using System.Numerics;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartViewViewStateTests
{
    [Fact]
    public void ViewportCompatibilityBridge_UpdatesViewStateDataWindow()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new SurfaceChartView();
            var viewport = new SurfaceViewport(64, 96, 128, 256);

            view.Viewport = viewport;

            view.ViewState.DataWindow.Should().Be(new SurfaceDataWindow(64, 192, 96, 352));
        });
    }

    [Fact]
    public void ViewStateCompatibilityBridge_UpdatesViewport()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new SurfaceChartView();
            var viewState = new SurfaceViewState(
                new SurfaceDataWindow(10, 210, 20, 420),
                CreateCamera(new SurfaceDataWindow(10, 210, 20, 420)));

            view.ViewState = viewState;

            view.Viewport.Should().Be(new SurfaceViewport(10, 20, 200, 400));
        });
    }

    [Fact]
    public void FitToData_SetsFullDatasetWindowAndDefaultCamera()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
            var view = new SurfaceChartView
            {
                Source = new RecordingSurfaceTileSource(metadata),
                ViewState = new SurfaceViewState(
                    new SurfaceDataWindow(100, 200, 300, 500),
                    new SurfaceCameraPose(new Vector3(1, 2, 3), 5, 10, 15, 60, SurfaceProjectionMode.Perspective))
            };

            view.FitToData();

            var expectedWindow = new SurfaceDataWindow(0, metadata.Width, 0, metadata.Height);
            view.ViewState.DataWindow.Should().Be(expectedWindow);
            view.Viewport.Should().Be(new SurfaceViewport(0, 0, metadata.Width, metadata.Height));
            view.ViewState.Camera.Should().Be(CreateCamera(expectedWindow));
        });
    }

    [Fact]
    public void ResetCamera_RestoresDefaultCameraForCurrentDataWindow()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var dataWindow = new SurfaceDataWindow(50, 250, 100, 500);
            var view = new SurfaceChartView
            {
                ViewState = new SurfaceViewState(
                    dataWindow,
                    new SurfaceCameraPose(new Vector3(9, 8, 7), 5, 10, 15, 60, SurfaceProjectionMode.Perspective))
            };

            view.ResetCamera();

            view.ViewState.DataWindow.Should().Be(dataWindow);
            view.ViewState.Camera.Should().Be(CreateCamera(dataWindow));
        });
    }

    [Fact]
    public void ZoomTo_UpdatesViewStateAndViewportBridge()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new SurfaceChartView();

            view.ZoomTo(25, 125, 50, 150);

            var expectedWindow = new SurfaceDataWindow(25, 125, 50, 150);
            view.ViewState.DataWindow.Should().Be(expectedWindow);
            view.Viewport.Should().Be(new SurfaceViewport(25, 50, 100, 100));
        });
    }

    private static SurfaceCameraPose CreateCamera(SurfaceDataWindow dataWindow)
    {
        return new SurfaceCameraPose(
            new Vector3(
                (float)((dataWindow.XMin + dataWindow.XMax) / 2.0),
                (float)((dataWindow.YMin + dataWindow.YMax) / 2.0),
                0f),
            yaw: 45.0,
            pitch: 35.264,
            distance: Math.Max(dataWindow.Width, dataWindow.Height) * 2.0,
            fieldOfView: 45.0,
            projectionMode: SurfaceProjectionMode.Perspective);
    }
}
