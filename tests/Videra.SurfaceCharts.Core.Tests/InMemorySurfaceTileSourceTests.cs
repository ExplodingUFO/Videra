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

    [Fact]
    public async Task GetTileAsync_ReportsStatisticsFromCoveredSourceRegion()
    {
        var source = CreateMatrix(4, 4, new float[]
        {
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16
        });

        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);
        ISurfaceTileSource tileSource = builder.Build(source);

        var overviewTile = await tileSource.GetRequiredTileAsync(new SurfaceTileKey(0, 0, 0, 0));
        overviewTile.ValueRange.Should().Be(new SurfaceValueRange(3.5, 13.5));
        overviewTile.Statistics.Range.Should().Be(new SurfaceValueRange(1, 16));
        overviewTile.Statistics.Average.Should().Be(8.5d);
        overviewTile.Statistics.SampleCount.Should().Be(16);
        overviewTile.Statistics.IsExact.Should().BeFalse();
    }

    [Fact]
    public async Task GetTileAsync_PreservesReducedColorFieldOnGeneratedTiles()
    {
        var source = CreateExplicitMatrixWithColorField();
        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);
        ISurfaceTileSource tileSource = builder.Build(source);

        var overviewTile = await tileSource.GetRequiredTileAsync(new SurfaceTileKey(0, 0, 0, 0));

        overviewTile.ColorField.Should().NotBeNull();
        overviewTile.ColorField!.Values.ToArray().Should().Equal(102.5f, 104.5f, 110.5f, 112.5f);
    }

    [Fact]
    public async Task GetTileAsync_IgnoresNonFiniteSamplesWhenComputingDetailValueRangeAndMask()
    {
        var source = CreateMatrix(2, 2, new float[]
        {
            1f, float.NaN,
            3f, 4f
        });

        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);
        ISurfaceTileSource tileSource = builder.Build(source);
        var detailTile = await tileSource.GetRequiredTileAsync(new SurfaceTileKey(0, 0, 0, 0));

        detailTile.ValueRange.Should().Be(new SurfaceValueRange(1d, 4d));
        detailTile.Mask.Should().NotBeNull();
        detailTile.Mask!.Values.ToArray().Should().Equal(true, false, true, true);
    }

    [Fact]
    public async Task GetTileAsync_IgnoresExplicitlyMaskedFiniteSamplesWhenComputingDetailValueRangeAndStatistics()
    {
        var metadata = CreateMetadata(2, 2);
        var source = new SurfaceMatrix(
            metadata,
            new SurfaceScalarField(
                width: 2,
                height: 2,
                values: new float[]
                {
                    100, 2,
                    5, 6
                },
                range: metadata.ValueRange),
            colorField: null,
            mask: new SurfaceMask(
                width: 2,
                height: 2,
                values: new bool[]
                {
                    false, true,
                    true, true
                }));

        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);
        ISurfaceTileSource tileSource = builder.Build(source);
        var detailTile = await tileSource.GetRequiredTileAsync(new SurfaceTileKey(0, 0, 0, 0));

        detailTile.ValueRange.Should().Be(new SurfaceValueRange(2d, 6d));
        detailTile.Statistics.Range.Should().Be(new SurfaceValueRange(2d, 6d));
        detailTile.Statistics.Average.Should().Be((2d + 5d + 6d) / 3d);
        detailTile.Mask.Should().NotBeNull();
        detailTile.Mask!.Values.ToArray().Should().Equal(false, true, true, true);
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

    private static SurfaceMatrix CreateExplicitMatrixWithColorField()
    {
        var metadata = new SurfaceMetadata(
            new SurfaceExplicitGrid(
                horizontalCoordinates: new double[] { 10d, 20d, 40d, 80d },
                verticalCoordinates: new double[] { 100d, 130d, 190d, 310d }),
            new SurfaceAxisDescriptor("Time", "s", 10d, 80d, SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceAxisDescriptor("Frequency", "Hz", 100d, 310d, SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceValueRange(1d, 16d));

        return new SurfaceMatrix(
            metadata,
            new SurfaceScalarField(
                width: 4,
                height: 4,
                values: new float[]
                {
                    1, 2, 3, 4,
                    5, 6, 7, 8,
                    9, 10, 11, 12,
                    13, 14, 15, 16
                },
                range: metadata.ValueRange),
            new SurfaceScalarField(
                width: 4,
                height: 4,
                values: new float[]
                {
                    100, 101, 102, 103,
                    104, 105, 106, 107,
                    108, 109, 110, 111,
                    112, 113, 114, 115
                },
                range: new SurfaceValueRange(100d, 115d)));
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
