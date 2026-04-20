using System.Numerics;
using FluentAssertions;
using Videra.SurfaceCharts.Core.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests.Picking;

public sealed class SurfaceHeightfieldPickerTests
{
    [Theory]
    [InlineData(0d, 0d)]
    [InlineData(210d, 15d)]
    public void Pick_ProjectedPeakFromMultipleCameraAngles_ResolvesSameSample(
        double yawDegrees,
        double pitchDegrees)
    {
        var metadata = CreateMetadata();
        var tile = CreateExactTile();
        var camera = new SurfaceCameraPose(
            target: new Vector3(15f, 5f, 150f),
            yawDegrees,
            pitchDegrees,
            distance: 40d,
            fieldOfViewDegrees: 45d);
        var frame = SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            new SurfaceViewState(new SurfaceDataWindow(0d, 0d, 5d, 5d), camera),
            viewWidth: 320d,
            viewHeight: 240d,
            renderScale: 1f);
        var peakWorldPosition = new Vector3(15f, 10f, 150f);
        var screenPoint = SurfaceProjectionMath.ProjectToScreen(peakWorldPosition, frame);
        var pickRay = SurfaceHeightfieldPicker.CreatePickRay(new Vector2(screenPoint.X, screenPoint.Y), frame);

        var hit = SurfaceHeightfieldPicker.Pick(metadata, [tile], pickRay);

        hit.Should().NotBeNull();
        hit!.Value.SampleX.Should().BeApproximately(2d, 0.01d);
        hit.Value.SampleY.Should().BeApproximately(2d, 0.01d);
        hit.Value.Value.Should().BeApproximately(10d, 0.001d);
        hit.Value.WorldPosition.X.Should().BeApproximately(peakWorldPosition.X, 0.001f);
        hit.Value.WorldPosition.Y.Should().BeApproximately(peakWorldPosition.Y, 0.001f);
        hit.Value.WorldPosition.Z.Should().BeApproximately(peakWorldPosition.Z, 0.001f);
        hit.Value.TileKey.Should().Be(tile.Key);
        hit.Value.IsApproximate.Should().BeFalse();
        hit.Value.DistanceToCamera.Should().BeGreaterThan(0d);
    }

    [Fact]
    public void Pick_CoarseTileHit_PreservesApproximateTileTruth()
    {
        var metadata = CreateMetadata();
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 2,
            height: 2,
            new SurfaceTileBounds(0, 0, 5, 5),
            new float[]
            {
                1f, 3f,
                5f, 7f
            },
            metadata.ValueRange);
        var camera = new SurfaceCameraPose(
            target: new Vector3(15f, 4f, 150f),
            yawDegrees: 0d,
            pitchDegrees: 0d,
            distance: 40d,
            fieldOfViewDegrees: 45d);
        var frame = SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            new SurfaceViewState(new SurfaceDataWindow(0d, 0d, 5d, 5d), camera),
            viewWidth: 320d,
            viewHeight: 240d,
            renderScale: 1f);
        var approximateWorldPosition = new Vector3(15f, 4f, 150f);
        var screenPoint = SurfaceProjectionMath.ProjectToScreen(approximateWorldPosition, frame);
        var pickRay = SurfaceHeightfieldPicker.CreatePickRay(new Vector2(screenPoint.X, screenPoint.Y), frame);

        var hit = SurfaceHeightfieldPicker.Pick(metadata, [tile], pickRay);

        hit.Should().NotBeNull();
        hit!.Value.SampleX.Should().BeApproximately(2d, 0.01d);
        hit.Value.SampleY.Should().BeApproximately(2d, 0.01d);
        hit.Value.Value.Should().BeApproximately(4d, 0.01d);
        hit.Value.TileKey.Should().Be(tile.Key);
        hit.Value.IsApproximate.Should().BeTrue();
    }

    [Fact]
    public void Pick_ExplicitGridPeak_ResolvesAxisMappedWorldPosition()
    {
        var metadata = new SurfaceMetadata(
            new SurfaceExplicitGrid(
                horizontalCoordinates: new double[] { 10d, 20d, 40d, 80d, 160d },
                verticalCoordinates: new double[] { 100d, 140d, 200d, 290d, 410d }),
            new SurfaceAxisDescriptor("Time", "s", 10d, 160d, SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 410d, SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceValueRange(0d, 10d));
        var tile = CreateExactTile();
        var peakWorldPosition = new Vector3(40f, 10f, 200f);
        var camera = new SurfaceCameraPose(
            target: peakWorldPosition,
            yawDegrees: 210d,
            pitchDegrees: 15d,
            distance: 120d,
            fieldOfViewDegrees: 45d);
        var frame = SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            new SurfaceViewState(new SurfaceDataWindow(0d, 0d, 5d, 5d), camera),
            viewWidth: 320d,
            viewHeight: 240d,
            renderScale: 1f);
        var screenPoint = SurfaceProjectionMath.ProjectToScreen(peakWorldPosition, frame);
        var pickRay = SurfaceHeightfieldPicker.CreatePickRay(new Vector2(screenPoint.X, screenPoint.Y), frame);

        var hit = SurfaceHeightfieldPicker.Pick(metadata, [tile], pickRay);

        hit.Should().NotBeNull();
        hit!.Value.SampleX.Should().BeApproximately(2d, 0.02d);
        hit.Value.SampleY.Should().BeApproximately(2d, 0.02d);
        hit.Value.Value.Should().BeApproximately(10d, 0.2d);
        hit.Value.WorldPosition.X.Should().BeApproximately(peakWorldPosition.X, 0.5f);
        hit.Value.WorldPosition.Y.Should().BeApproximately(peakWorldPosition.Y, 0.2f);
        hit.Value.WorldPosition.Z.Should().BeApproximately(peakWorldPosition.Z, 0.5f);
        hit.Value.IsApproximate.Should().BeFalse();
    }

    [Fact]
    public void Pick_MaskedPeak_ReturnsNull()
    {
        var metadata = CreateMetadata();
        var tile = new SurfaceTile(
            new SurfaceTileKey(2, 2, 0, 0),
            new SurfaceTileBounds(0, 0, 5, 5),
            new SurfaceScalarField(
                width: 5,
                height: 5,
                values: new float[]
                {
                    0f, 1f, 2f, 1f, 0f,
                    1f, 3f, 5f, 3f, 1f,
                    2f, 5f, 10f, 5f, 2f,
                    1f, 3f, 5f, 3f, 1f,
                    0f, 1f, 2f, 1f, 0f,
                },
                range: new SurfaceValueRange(0d, 10d)),
            colorField: null,
            mask: new SurfaceMask(
                width: 5,
                height: 5,
                values: new bool[]
                {
                    true, true, true, true, true,
                    true, true, true, true, true,
                    true, true, false, true, true,
                    true, true, true, true, true,
                    true, true, true, true, true,
                }));
        var peakWorldPosition = new Vector3(15f, 10f, 150f);
        var camera = new SurfaceCameraPose(
            target: new Vector3(15f, 5f, 150f),
            yawDegrees: 210d,
            pitchDegrees: 15d,
            distance: 40d,
            fieldOfViewDegrees: 45d);
        var frame = SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            new SurfaceViewState(new SurfaceDataWindow(0d, 0d, 5d, 5d), camera),
            viewWidth: 320d,
            viewHeight: 240d,
            renderScale: 1f);
        var screenPoint = SurfaceProjectionMath.ProjectToScreen(peakWorldPosition, frame);
        var pickRay = SurfaceHeightfieldPicker.CreatePickRay(new Vector2(screenPoint.X, screenPoint.Y), frame);

        var hit = SurfaceHeightfieldPicker.Pick(metadata, [tile], pickRay);

        hit.Should().BeNull();
    }

    [Fact]
    public void Pick_MaskedDetailedPeak_DoesNotFallBackToOverviewTile()
    {
        var metadata = CreateMetadata();
        var detailTile = new SurfaceTile(
            new SurfaceTileKey(2, 2, 0, 0),
            new SurfaceTileBounds(0, 0, 5, 5),
            new SurfaceScalarField(
                width: 5,
                height: 5,
                values: new float[]
                {
                    0f, 1f, 2f, 1f, 0f,
                    1f, 3f, 5f, 3f, 1f,
                    2f, 5f, 10f, 5f, 2f,
                    1f, 3f, 5f, 3f, 1f,
                    0f, 1f, 2f, 1f, 0f,
                },
                range: new SurfaceValueRange(0d, 10d)),
            colorField: null,
            mask: new SurfaceMask(
                width: 5,
                height: 5,
                values: new bool[]
                {
                    true, true, true, true, true,
                    true, true, true, true, true,
                    true, true, false, true, true,
                    true, true, true, true, true,
                    true, true, true, true, true,
                }));
        var overviewTile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 2,
            height: 2,
            new SurfaceTileBounds(0, 0, 5, 5),
            new float[]
            {
                10f, 10f,
                10f, 10f,
            },
            metadata.ValueRange);
        var peakWorldPosition = new Vector3(15f, 10f, 150f);
        var camera = new SurfaceCameraPose(
            target: new Vector3(15f, 5f, 150f),
            yawDegrees: 210d,
            pitchDegrees: 15d,
            distance: 40d,
            fieldOfViewDegrees: 45d);
        var frame = SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            new SurfaceViewState(new SurfaceDataWindow(0d, 0d, 5d, 5d), camera),
            viewWidth: 320d,
            viewHeight: 240d,
            renderScale: 1f);
        var screenPoint = SurfaceProjectionMath.ProjectToScreen(peakWorldPosition, frame);
        var pickRay = SurfaceHeightfieldPicker.CreatePickRay(new Vector2(screenPoint.X, screenPoint.Y), frame);

        var hit = SurfaceHeightfieldPicker.Pick(metadata, [overviewTile, detailTile], pickRay);

        hit.Should().BeNull();
    }

    private static SurfaceMetadata CreateMetadata()
    {
        return new SurfaceMetadata(
            width: 5,
            height: 5,
            new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 200d),
            new SurfaceValueRange(0d, 10d));
    }

    private static SurfaceTile CreateExactTile()
    {
        return new SurfaceTile(
            new SurfaceTileKey(2, 2, 0, 0),
            width: 5,
            height: 5,
            new SurfaceTileBounds(0, 0, 5, 5),
            new float[]
            {
                0f, 1f, 2f, 1f, 0f,
                1f, 3f, 5f, 3f, 1f,
                2f, 5f, 10f, 5f, 2f,
                1f, 3f, 5f, 3f, 1f,
                0f, 1f, 2f, 1f, 0f,
            },
            new SurfaceValueRange(0d, 10d));
    }
}
