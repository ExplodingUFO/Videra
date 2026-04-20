using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests.Picking;

public sealed class SurfaceProbeInfoTests
{
    public static TheoryData<double, double, double, double> SampleMappingCases => new()
    {
        { 0d, 0d, 10d, 100d },
        { 4d, 3d, 15d, 160d },
        { 8d, 6d, 20d, 220d }
    };

    [Theory]
    [MemberData(nameof(SampleMappingCases))]
    public void FromResolvedSample_MapsSampleCoordinatesToAxisCoordinates(
        double sampleX,
        double sampleY,
        double expectedAxisX,
        double expectedAxisY)
    {
        var probe = SurfaceProbeInfo.FromResolvedSample(
            CreateMetadata(),
            CreateTile(width: 9, height: 7, boundsWidth: 9, boundsHeight: 7),
            new SurfaceProbeRequest(sampleX, sampleY),
            value: 12.5d);

        probe.SampleX.Should().Be(sampleX);
        probe.SampleY.Should().Be(sampleY);
        probe.AxisX.Should().BeApproximately(expectedAxisX, 0.0001d);
        probe.AxisY.Should().BeApproximately(expectedAxisY, 0.0001d);
        probe.Value.Should().Be(12.5d);
        probe.IsApproximate.Should().BeFalse();
    }

    [Fact]
    public void FromResolvedSample_MarksProbeApproximateWhenTileGridIsCoarserThanTileBounds()
    {
        var probe = SurfaceProbeInfo.FromResolvedSample(
            CreateMetadata(),
            CreateTile(width: 3, height: 2, boundsWidth: 9, boundsHeight: 7),
            new SurfaceProbeRequest(4d, 3d),
            value: -8d);

        probe.AxisX.Should().BeApproximately(15d, 0.0001d);
        probe.AxisY.Should().BeApproximately(160d, 0.0001d);
        probe.IsApproximate.Should().BeTrue();
    }

    [Fact]
    public void FromPickHit_PublishesWorldAndTileTruth()
    {
        var hit = new SurfacePickHit(
            new SurfaceTileKey(2, 1, 3, 4),
            sampleX: 4d,
            sampleY: 3d,
            worldPosition: new System.Numerics.Vector3(15f, 12.5f, 160f),
            value: 12.5d,
            isApproximate: true,
            distanceToCamera: 48d);

        var probe = SurfaceProbeInfo.FromPickHit(CreateMetadata(), hit);

        probe.SampleX.Should().Be(4d);
        probe.SampleY.Should().Be(3d);
        probe.AxisX.Should().BeApproximately(15d, 0.0001d);
        probe.AxisY.Should().BeApproximately(160d, 0.0001d);
        probe.Value.Should().Be(12.5d);
        probe.IsApproximate.Should().BeTrue();
        probe.WorldPosition.Should().Be(hit.WorldPosition);
        probe.TileKey.Should().Be(hit.TileKey);
        probe.DistanceToCamera.Should().Be(48d);
    }

    [Fact]
    public void FromResolvedSample_UsesExplicitGridAxisCoordinates()
    {
        var metadata = new SurfaceMetadata(
            new SurfaceExplicitGrid(
                horizontalCoordinates: new double[] { 10d, 20d, 40d, 80d },
                verticalCoordinates: new double[] { 100d, 130d, 190d }),
            new SurfaceAxisDescriptor("Time", "s", 10d, 80d, SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 190d, SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceValueRange(-12d, 36d));
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 4,
            height: 3,
            new SurfaceTileBounds(0, 0, 4, 3),
            new float[12],
            new SurfaceValueRange(-12d, 36d));

        var probe = SurfaceProbeInfo.FromResolvedSample(
            metadata,
            tile,
            new SurfaceProbeRequest(2.5d, 1.5d),
            value: 12.5d);

        probe.AxisX.Should().BeApproximately(60d, 0.0001d);
        probe.AxisY.Should().BeApproximately(160d, 0.0001d);
    }

    private static SurfaceMetadata CreateMetadata()
    {
        return new SurfaceMetadata(
            width: 9,
            height: 7,
            new SurfaceAxisDescriptor("Time", "s", 10d, 20d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 220d),
            new SurfaceValueRange(-12d, 36d));
    }

    private static SurfaceTile CreateTile(int width, int height, int boundsWidth, int boundsHeight)
    {
        return new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width,
            height,
            new SurfaceTileBounds(0, 0, boundsWidth, boundsHeight),
            new float[width * height],
            new SurfaceValueRange(-12d, 36d));
    }
}
