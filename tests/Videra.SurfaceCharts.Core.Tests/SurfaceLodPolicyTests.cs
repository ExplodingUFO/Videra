using FluentAssertions;
using Xunit;

#pragma warning disable CA2007

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
        selection.TileXStart.Should().Be(0);
        selection.TileXEnd.Should().Be(3);
        selection.TileYStart.Should().Be(0);
        selection.TileYEnd.Should().Be(3);

        selection.EnumerateTileKeys().Should().Equal(
            new SurfaceTileKey(2, 2, 0, 0),
            new SurfaceTileKey(2, 2, 1, 0),
            new SurfaceTileKey(2, 2, 2, 0),
            new SurfaceTileKey(2, 2, 3, 0),
            new SurfaceTileKey(2, 2, 0, 1),
            new SurfaceTileKey(2, 2, 1, 1),
            new SurfaceTileKey(2, 2, 2, 1),
            new SurfaceTileKey(2, 2, 3, 1),
            new SurfaceTileKey(2, 2, 0, 2),
            new SurfaceTileKey(2, 2, 1, 2),
            new SurfaceTileKey(2, 2, 2, 2),
            new SurfaceTileKey(2, 2, 3, 2),
            new SurfaceTileKey(2, 2, 0, 3),
            new SurfaceTileKey(2, 2, 1, 3),
            new SurfaceTileKey(2, 2, 2, 3),
            new SurfaceTileKey(2, 2, 3, 3));
    }

    [Fact]
    public void Select_ClampsNeighborhoodMarginAtDatasetEdges()
    {
        var metadata = CreateMetadata(100, 80);
        var request = new SurfaceViewportRequest(
            metadata,
            new SurfaceViewport(0.0, 0.0, 20.0, 20.0),
            10,
            10);

        var selection = SurfaceLodPolicy.Default.Select(request);

        selection.LevelX.Should().Be(1);
        selection.LevelY.Should().Be(1);
        selection.TileXStart.Should().Be(0);
        selection.TileXEnd.Should().Be(1);
        selection.TileYStart.Should().Be(0);
        selection.TileYEnd.Should().Be(1);
    }

    [Fact]
    public async Task Select_ReturnsOnlyKeysThatResolveThroughBuiltTileSourceForNonPowerOfTwoDatasets()
    {
        var metadata = CreateMetadata(10, 6);
        var source = new SurfaceMatrix(metadata, CreateSequentialValues(metadata.Width, metadata.Height));
        var builder = new SurfacePyramidBuilder(maxTileWidth: 4, maxTileHeight: 4);
        ISurfaceTileSource tileSource = builder.Build(source);
        var request = new SurfaceViewportRequest(
            metadata,
            new SurfaceViewport(0.0, 0.0, metadata.Width, metadata.Height),
            outputWidth: 2,
            outputHeight: 2);

        var selection = SurfaceLodPolicy.Default.Select(request);

        selection.LevelX.Should().Be(2);
        selection.LevelY.Should().Be(1);

        foreach (var tileKey in selection.EnumerateTileKeys())
        {
            var tile = await tileSource.GetTileAsync(tileKey);
            tile.Should().NotBeNull($"policy-selected tile {tileKey} should resolve from the built source");
        }
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

    private static float[] CreateSequentialValues(int width, int height)
    {
        var values = new float[width * height];
        for (var index = 0; index < values.Length; index++)
        {
            values[index] = index;
        }

        return values;
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

#pragma warning restore CA2007
