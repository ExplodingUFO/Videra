using System.Threading;
using FluentAssertions;
using Xunit;

#pragma warning disable CA2007

namespace Videra.SurfaceCharts.Core.Tests;

public class InMemorySurfaceTileSourceTests
{
    [Fact]
    public async Task GetTileAsync_ReadsTilesFromMultipleLevels()
    {
        var source = CreateMatrix(8, 4, CreateSequentialValues(8, 4));
        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);
        ISurfaceTileSource tileSource = builder.Build(source);

        var detailTile = await tileSource.GetRequiredTileAsync(new SurfaceTileKey(2, 1, 1, 0));
        var overviewTile = await tileSource.GetRequiredTileAsync(new SurfaceTileKey(0, 0, 0, 0));

        detailTile.Width.Should().Be(2);
        detailTile.Height.Should().Be(2);
        detailTile.Bounds.Should().Be(new SurfaceTileBounds(2, 0, 2, 2));
        detailTile.Values.ToArray().Should().Equal(2f, 3f, 10f, 11f);

        overviewTile.Width.Should().Be(2);
        overviewTile.Height.Should().Be(2);
        overviewTile.Bounds.Should().Be(new SurfaceTileBounds(0, 0, 8, 4));
        overviewTile.Values.ToArray().Should().Equal(5.5f, 9.5f, 21.5f, 25.5f);
    }

    [Fact]
    public async Task GetTileAsync_ReturnsNullWhenLevelOrTileIsOutsideTheBuiltPyramid()
    {
        var source = CreateMatrix(8, 4, CreateSequentialValues(8, 4));
        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);
        ISurfaceTileSource tileSource = builder.Build(source);

        var missingLevelTile = await tileSource.GetTileAsync(new SurfaceTileKey(3, 1, 0, 0));
        var missingTile = await tileSource.GetTileAsync(new SurfaceTileKey(2, 1, 4, 0));

        missingLevelTile.Should().BeNull();
        missingTile.Should().BeNull();
    }

    [Fact]
    public async Task GetTileAsync_HonorsCancellationBeforeReading()
    {
        var source = CreateMatrix(8, 4, CreateSequentialValues(8, 4));
        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);
        ISurfaceTileSource tileSource = builder.Build(source);
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();

        var act = async () => await tileSource.GetTileAsync(
            new SurfaceTileKey(2, 1, 0, 0),
            cancellationSource.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetTileAsync_ReturnsNullForExtremeTileCoordinatesInsteadOfThrowing()
    {
        var source = CreateMatrix(8, 4, CreateSequentialValues(8, 4));
        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);
        ISurfaceTileSource tileSource = builder.Build(source);
        var extremeKey = new SurfaceTileKey(2, 1, int.MaxValue, int.MaxValue);

        var act = async () => await tileSource.GetTileAsync(extremeKey);

        await act.Should().NotThrowAsync();
        var tile = await tileSource.GetTileAsync(extremeKey);
        tile.Should().BeNull();
    }

    private static SurfaceMatrix CreateMatrix(int width, int height, float[] values)
    {
        return new SurfaceMatrix(CreateMetadata(width, height), values);
    }

    private static SurfaceMetadata CreateMetadata(int width, int height)
    {
        return new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("Time", "s", 0.0, 12.0),
            new SurfaceAxisDescriptor("Frequency", "Hz", 10.0, 20_000.0),
            new SurfaceValueRange(0.0, 100.0));
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
}

#pragma warning restore CA2007
