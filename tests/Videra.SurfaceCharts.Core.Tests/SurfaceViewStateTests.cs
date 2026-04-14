using System.Numerics;
using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class SurfaceViewStateTests
{
    [Fact]
    public void SurfaceViewState_CanBeConstructedFromDataWindowAndCameraPose()
    {
        var dataWindow = new SurfaceDataWindow(10.0, 90.0, 5.0, 45.0);
        var camera = new SurfaceCameraPose(
            new Vector3(1.0f, 2.0f, 3.0f),
            yaw: 30.0,
            pitch: 20.0,
            distance: 120.0,
            fieldOfView: 60.0,
            projectionMode: SurfaceProjectionMode.Perspective);

        var viewState = new SurfaceViewState(dataWindow, camera);

        viewState.DataWindow.Should().Be(dataWindow);
        viewState.Camera.Should().Be(camera);
    }

    [Fact]
    public void SurfaceViewportRequest_FromDataWindow_UsesWindowSpanForZoomDensity()
    {
        var metadata = new SurfaceMetadata(
            200,
            100,
            new SurfaceAxisDescriptor("Time", "s", 0.0, 12.0),
            new SurfaceAxisDescriptor("Frequency", "Hz", 10.0, 20_000.0),
            new SurfaceValueRange(-3.5, 9.25));
        var dataWindow = new SurfaceDataWindow(20.0, 100.0, 10.0, 50.0);

        var request = new SurfaceViewportRequest(metadata, dataWindow, 40, 20);

        request.HorizontalZoomDensity.Should().Be(2.0);
        request.VerticalZoomDensity.Should().Be(2.0);
        request.ZoomDensity.Should().Be(2.0);
        request.Viewport.Should().Be(SurfaceViewport.FromDataWindow(dataWindow));
    }
}
