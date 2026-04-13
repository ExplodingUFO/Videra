using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class SurfaceLodPolicyTests
{
    [Fact]
    public void DefaultPolicy_IsOverviewFirst()
    {
        var metadata = CreateMetadata(1024, 512);
        var request = new SurfaceViewportRequest(
            metadata,
            new SurfaceViewport(0.0, 0.0, 512.0, 256.0),
            1024,
            512);

        var selection = SurfaceLodPolicy.Default.Select(request);

        selection.LevelX.Should().Be(0);
        selection.LevelY.Should().Be(0);
    }

    [Fact]
    public void Select_MapsPerAxisZoomDensityToTargetPyramidLevels()
    {
        var metadata = CreateMetadata(8192, 512);
        var request = new SurfaceViewportRequest(
            metadata,
            new SurfaceViewport(0.0, 0.0, 8192.0, 512.0),
            1024,
            512);

        var selection = SurfaceLodPolicy.Default.Select(request);

        selection.LevelX.Should().Be(3);
        selection.LevelY.Should().Be(0);
        selection.TileXStart.Should().Be(0);
        selection.TileXEnd.Should().Be(7);
        selection.TileYStart.Should().Be(0);
        selection.TileYEnd.Should().Be(0);
    }

    [Fact]
    public void Select_ComputesStableTileRequestRanges()
    {
        var metadata = CreateMetadata(100, 80);
        var request = new SurfaceViewportRequest(
            metadata,
            new SurfaceViewport(25.0, 20.0, 50.0, 40.0),
            10,
            8);

        var selection = SurfaceLodPolicy.Default.Select(request);

        selection.LevelX.Should().Be(2);
        selection.LevelY.Should().Be(2);
        selection.TileXStart.Should().Be(1);
        selection.TileXEnd.Should().Be(2);
        selection.TileYStart.Should().Be(1);
        selection.TileYEnd.Should().Be(2);

        selection.EnumerateTileKeys().Should().Equal(
            new SurfaceTileKey(2, 2, 1, 1),
            new SurfaceTileKey(2, 2, 2, 1),
            new SurfaceTileKey(2, 2, 1, 2),
            new SurfaceTileKey(2, 2, 2, 2));
    }

    [Fact]
    public void Ctor_RejectsNegativeTileStarts()
    {
        var request = new SurfaceViewportRequest(
            CreateMetadata(100, 80),
            new SurfaceViewport(0.0, 0.0, 50.0, 40.0),
            10,
            8);

        var act = () => new SurfaceLodSelection(request, 1, 1, -1, 0, 0, 0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "tileXStart");
    }

    private static SurfaceMetadata CreateMetadata(int width, int height)
    {
        return new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("Time", "s", 0.0, 12.0),
            new SurfaceAxisDescriptor("Frequency", "Hz", 10.0, 20_000.0),
            new SurfaceValueRange(-3.5, 9.25));
    }
}
