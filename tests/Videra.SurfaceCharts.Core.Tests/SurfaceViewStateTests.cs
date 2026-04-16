using System.Numerics;
using FluentAssertions;
using Videra.SurfaceCharts.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public sealed class SurfaceViewStateTests
{
    [Fact]
    public void SurfaceDataWindow_FromViewport_PreservesStartAndSpan()
    {
        var viewport = new SurfaceViewport(10.5, 4.25, 20.0, 12.5);

        var dataWindow = SurfaceDataWindow.FromViewport(viewport);

        dataWindow.StartX.Should().Be(10.5);
        dataWindow.StartY.Should().Be(4.25);
        dataWindow.Width.Should().Be(20.0);
        dataWindow.Height.Should().Be(12.5);
        dataWindow.ToViewport().Should().Be(viewport);
    }

    [Fact]
    public void SurfaceCameraPose_ToProjectionSettings_UsesYawAndPitch()
    {
        var pose = new SurfaceCameraPose(
            new Vector3(12.0f, 5.0f, 20.0f),
            yawDegrees: 210.0,
            pitchDegrees: 15.0,
            distance: 48.0,
            fieldOfViewDegrees: 45.0);

        SurfaceChartProjectionSettings projectionSettings = pose.ToProjectionSettings();

        projectionSettings.Should().Be(new SurfaceChartProjectionSettings(210.0, 15.0));
    }

    [Fact]
    public void SurfaceViewState_CreateDefault_UsesWindowCenterAndMetadataValueRange()
    {
        var metadata = CreateMetadata();
        var dataWindow = new SurfaceDataWindow(25.0, 10.0, 20.0, 16.0);

        var state = SurfaceViewState.CreateDefault(metadata, dataWindow);

        state.DataWindow.Should().Be(dataWindow);
        state.Camera.Target.X.Should().BeApproximately(13.5f, 0.0001f);
        state.Camera.Target.Y.Should().BeApproximately(5.0f, 0.0001f);
        state.Camera.Target.Z.Should().BeApproximately(172.0f, 0.0001f);
        state.Camera.ToProjectionSettings().Should().Be(SurfaceChartProjectionSettings.Default);
    }

    [Fact]
    public void SurfaceViewportRequest_DataWindowConstructor_PreservesZoomDensity()
    {
        var metadata = CreateMetadata();
        var dataWindow = new SurfaceDataWindow(25.0, 10.0, 20.0, 16.0);

        var request = new SurfaceViewportRequest(metadata, dataWindow, 40, 20);

        request.DataWindow.Should().Be(dataWindow);
        request.Viewport.Should().Be(dataWindow.ToViewport());
        request.HorizontalZoomDensity.Should().Be(0.5);
        request.VerticalZoomDensity.Should().Be(0.8);
        request.ZoomDensity.Should().Be(0.8);
    }

    private static SurfaceMetadata CreateMetadata()
    {
        return new SurfaceMetadata(
            width: 100,
            height: 50,
            new SurfaceAxisDescriptor("Time", "s", 10.0, 20.0),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100.0, 300.0),
            new SurfaceValueRange(-5.0, 15.0));
    }
}
