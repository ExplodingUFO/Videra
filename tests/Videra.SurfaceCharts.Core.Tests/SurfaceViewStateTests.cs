using System.Numerics;
using FluentAssertions;
using Videra.SurfaceCharts.Rendering;
using Videra.SurfaceCharts.Core.Rendering;
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
    public void SurfaceViewState_CreateDefault_UsesExplicitGridWindowCenter()
    {
        var metadata = new SurfaceMetadata(
            new SurfaceExplicitGrid(
                horizontalCoordinates: new double[] { 10d, 20d, 40d, 80d },
                verticalCoordinates: new double[] { 100d, 130d, 190d }),
            new SurfaceAxisDescriptor("Time", "s", 10d, 80d, SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 190d, SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceValueRange(-5d, 15d));
        var dataWindow = new SurfaceDataWindow(1.0, 0.0, 2.0, 3.0);

        var state = SurfaceViewState.CreateDefault(metadata, dataWindow);

        state.Camera.Target.X.Should().BeApproximately(30.0f, 0.0001f);
        state.Camera.Target.Y.Should().BeApproximately(5.0f, 0.0001f);
        state.Camera.Target.Z.Should().BeApproximately(145.0f, 0.0001f);
    }

    [Fact]
    public void SurfaceCameraPose_CreateCameraFrame_UsesTargetAndFieldOfView()
    {
        var metadata = CreateMetadata();
        var dataWindow = new SurfaceDataWindow(25.0, 10.0, 20.0, 16.0);
        var pose = new SurfaceCameraPose(
            new Vector3(12.0f, 5.0f, 20.0f),
            yawDegrees: 210.0,
            pitchDegrees: 15.0,
            distance: 48.0,
            fieldOfViewDegrees: 40.0);

        var frame = pose.CreateCameraFrame(metadata, dataWindow, 320d, 200d, 1f);

        frame.Target.Should().Be(pose.Target);
        frame.Position.Should().NotBe(pose.Target);
        frame.NearPlane.Should().BePositive();
        frame.FarPlane.Should().BeGreaterThan(frame.NearPlane);
        frame.ProjectionSettings.Should().Be(new SurfaceChartProjectionSettings(210.0, 15.0));
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
