using System.Numerics;
using FluentAssertions;
using Videra.SurfaceCharts.Core.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests.LOD;

public sealed class SurfaceScreenErrorEstimatorTests
{
    [Fact]
    public void EstimateTileFootprint_VisibleTileReportsPositiveAreaAndDepth()
    {
        var metadata = CreateMetadata(width: 64, height: 64);
        var dataWindow = new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height);
        var frame = SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            SurfaceViewState.CreateDefault(metadata, dataWindow),
            viewWidth: 320d,
            viewHeight: 240d,
            renderScale: 1f);

        var footprint = SurfaceScreenErrorEstimator.EstimateTileFootprint(
            metadata,
            new SurfaceTileBounds(16, 16, 16, 16),
            frame);

        footprint.IsVisible.Should().BeTrue();
        footprint.ProjectedWidthPixels.Should().BeGreaterThan(0f);
        footprint.ProjectedHeightPixels.Should().BeGreaterThan(0f);
        footprint.ScreenAreaPixels.Should().BeGreaterThan(0f);
        footprint.ViewDepth.Should().BeGreaterThan(0f);
        footprint.MaxSamplesPerPixel.Should().BeGreaterThan(0d);
    }

    [Fact]
    public void EstimateDataWindowFootprint_FartherCameraIncreasesScreenError()
    {
        var metadata = CreateMetadata(width: 4096, height: 2048);
        var dataWindow = new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height);
        var nearCamera = SurfaceCameraPose.CreateDefault(metadata, dataWindow);
        var farCamera = new SurfaceCameraPose(
            nearCamera.Target,
            nearCamera.YawDegrees,
            nearCamera.PitchDegrees,
            nearCamera.Distance * 4d,
            nearCamera.FieldOfViewDegrees);
        var nearFrame = SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            new SurfaceViewState(dataWindow, nearCamera),
            viewWidth: 256d,
            viewHeight: 256d,
            renderScale: 1f);
        var farFrame = SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            new SurfaceViewState(dataWindow, farCamera),
            viewWidth: 256d,
            viewHeight: 256d,
            renderScale: 1f);

        var nearFootprint = SurfaceScreenErrorEstimator.EstimateDataWindowFootprint(metadata, dataWindow, nearFrame);
        var farFootprint = SurfaceScreenErrorEstimator.EstimateDataWindowFootprint(metadata, dataWindow, farFrame);

        farFootprint.ProjectedWidthPixels.Should().BeLessThan(nearFootprint.ProjectedWidthPixels);
        farFootprint.ProjectedHeightPixels.Should().BeLessThan(nearFootprint.ProjectedHeightPixels);
        farFootprint.MaxSamplesPerPixel.Should().BeGreaterThan(nearFootprint.MaxSamplesPerPixel);
    }

    private static SurfaceMetadata CreateMetadata(int width, int height)
    {
        return new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("Time", "s", 0d, width - 1d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 0d, height - 1d),
            new SurfaceValueRange(-4d, 16d));
    }
}
