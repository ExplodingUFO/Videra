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
