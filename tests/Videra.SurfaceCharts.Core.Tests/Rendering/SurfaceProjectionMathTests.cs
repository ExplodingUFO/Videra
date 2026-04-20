using System.Numerics;
using FluentAssertions;
using Videra.SurfaceCharts.Core.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests.Rendering;

public sealed class SurfaceProjectionMathTests
{
    [Fact]
    public void CreateCameraFrame_ProjectAndUnproject_RoundTripsPlotPoint()
    {
        var metadata = CreateMetadata();
        var viewState = SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height));
        var frame = SurfaceProjectionMath.CreateCameraFrame(metadata, viewState, 640d, 360d, 1f);
        var plotPoint = new Vector3(14.5f, 12f, 160f);

        var screenPoint = SurfaceProjectionMath.ProjectToScreen(plotPoint, frame);
        var roundTrip = SurfaceProjectionMath.UnprojectFromScreen(screenPoint, frame);

        roundTrip.X.Should().BeApproximately(plotPoint.X, 0.001f);
        roundTrip.Y.Should().BeApproximately(plotPoint.Y, 0.001f);
        roundTrip.Z.Should().BeApproximately(plotPoint.Z, 0.001f);
    }

    [Fact]
    public void CreateCameraFrame_DefaultView_KeepsPlotBoundsInsideViewport()
    {
        var metadata = CreateMetadata();
        var viewState = SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height));
        var frame = SurfaceProjectionMath.CreateCameraFrame(metadata, viewState, 640d, 360d, 1f);

        foreach (var corner in frame.PlotBounds.GetCorners())
        {
            var screenPoint = SurfaceProjectionMath.ProjectToScreen(corner, frame);
            screenPoint.X.Should().BeInRange(-0.001f, frame.ViewportPixels.X + 0.001f);
            screenPoint.Y.Should().BeInRange(-0.001f, frame.ViewportPixels.Y + 0.001f);
            screenPoint.Z.Should().BeInRange(0f, 1f);
        }
    }

    [Fact]
    public void CreateCameraFrame_PreservesProjectionSettingsCompatibility()
    {
        var metadata = CreateMetadata();
        var camera = new SurfaceCameraPose(
            new Vector3(14.5f, 12f, 160f),
            yawDegrees: 210d,
            pitchDegrees: 15d,
            distance: 48d,
            fieldOfViewDegrees: 45d);
        var viewState = new SurfaceViewState(new SurfaceDataWindow(8d, 4d, 24d, 20d), camera);

        var frame = SurfaceProjectionMath.CreateCameraFrame(metadata, viewState, 640d, 360d, 1f);

        frame.ProjectionSettings.Should().Be(camera.ToProjectionSettings());
    }

    [Fact]
    public void CreatePlotBounds_UsesExplicitGridCoordinates()
    {
        var metadata = new SurfaceMetadata(
            new SurfaceExplicitGrid(
                horizontalCoordinates: new double[] { 10d, 20d, 40d, 80d },
                verticalCoordinates: new double[] { 100d, 130d, 190d }),
            new SurfaceAxisDescriptor("Time", "s", 10d, 80d, SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 190d, SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceValueRange(-8d, 32d));

        var bounds = SurfaceProjectionMath.CreatePlotBounds(
            metadata,
            new SurfaceDataWindow(startX: 1d, startY: 0d, width: 2d, height: 3d));

        bounds.Minimum.X.Should().BeApproximately(20f, 0.0001f);
        bounds.Maximum.X.Should().BeApproximately(40f, 0.0001f);
        bounds.Minimum.Z.Should().BeApproximately(100f, 0.0001f);
        bounds.Maximum.Z.Should().BeApproximately(190f, 0.0001f);
    }

    private static SurfaceMetadata CreateMetadata()
    {
        return new SurfaceMetadata(
            width: 64,
            height: 48,
            new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
            new SurfaceAxisDescriptor("Frequency", "kHz", 100d, 220d),
            new SurfaceValueRange(-8d, 32d));
    }
}
