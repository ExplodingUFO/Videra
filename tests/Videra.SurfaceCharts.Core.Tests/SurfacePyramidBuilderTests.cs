using FluentAssertions;
using Xunit;

#pragma warning disable CA2007

namespace Videra.SurfaceCharts.Core.Tests;

public class SurfacePyramidBuilderTests
{
    [Fact]
    public void Build_ExposesReplaceableTileSourceContract()
    {
        var buildMethod = typeof(SurfacePyramidBuilder).GetMethod(nameof(SurfacePyramidBuilder.Build));

        buildMethod.Should().NotBeNull();
        buildMethod!.ReturnType.Should().Be(typeof(ISurfaceTileSource));
    }

    [Fact]
    public async Task Build_CreatesOverviewLevelFromSourceMatrix()
    {
        var source = CreateMatrix(
            4,
            4,
            new float[]
            {
                1, 2, 3, 4,
                5, 6, 7, 8,
                9, 10, 11, 12,
                13, 14, 15, 16
            });

        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);

        ISurfaceTileSource tileSource = builder.Build(source);
        var overviewTile = await tileSource.GetRequiredTileAsync(new SurfaceTileKey(0, 0, 0, 0));

        overviewTile.Width.Should().Be(2);
        overviewTile.Height.Should().Be(2);
        overviewTile.Bounds.Should().Be(new SurfaceTileBounds(0, 0, 4, 4));
        overviewTile.Values.ToArray().Should().Equal(3.5f, 5.5f, 11.5f, 13.5f);
    }

    [Fact]
    public async Task Build_ReportsIntermediateTileBoundsInOriginalSampleSpace()
    {
        var source = CreateMatrix(8, 4, CreateSequentialValues(8, 4));
        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);

        ISurfaceTileSource tileSource = builder.Build(source);
        var tile = await tileSource.GetRequiredTileAsync(new SurfaceTileKey(1, 0, 1, 0));

        tile.Width.Should().Be(2);
        tile.Height.Should().Be(2);
        tile.Bounds.Should().Be(new SurfaceTileBounds(4, 0, 4, 4));
        tile.Values.ToArray().Should().Equal(8.5f, 10.5f, 24.5f, 26.5f);
    }

    [Fact]
    public async Task Build_PreservesMetadataAcrossGeneratedLevels()
    {
        var source = CreateMatrix(8, 4, CreateSequentialValues(8, 4));
        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);

        ISurfaceTileSource tileSource = builder.Build(source);
        var overviewTile = await tileSource.GetRequiredTileAsync(new SurfaceTileKey(0, 0, 0, 0));
        var detailTile = await tileSource.GetRequiredTileAsync(new SurfaceTileKey(2, 1, 0, 0));

        tileSource.Metadata.Should().BeSameAs(source.Metadata);
        overviewTile.Key.LevelX.Should().Be(0);
        overviewTile.Key.LevelY.Should().Be(0);
        detailTile.Key.LevelX.Should().Be(2);
        detailTile.Key.LevelY.Should().Be(1);
        tileSource.Metadata.HorizontalAxis.Should().Be(source.Metadata.HorizontalAxis);
        tileSource.Metadata.VerticalAxis.Should().Be(source.Metadata.VerticalAxis);
        tileSource.Metadata.ValueRange.Should().Be(source.Metadata.ValueRange);
    }

    [Fact]
    public void Build_TracksPeakScratchSampleCountWithinLargestGeneratedLevel()
    {
        var source = CreateMatrix(8, 4, CreateSequentialValues(8, 4));
        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);

        _ = builder.Build(source);

        builder.LastBuildPeakScratchSampleCount.Should().Be(16);
    }

    [Fact]
    public async Task Build_PreservesAverageSamplesWhileProducingTileStatistics()
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

        overviewTile.Values.ToArray().Should().Equal(3.5f, 5.5f, 11.5f, 13.5f);
        overviewTile.ValueRange.Should().Be(new SurfaceValueRange(3.5, 13.5));
        overviewTile.Statistics.Range.Should().Be(new SurfaceValueRange(1, 16));
        overviewTile.Statistics.Average.Should().Be(8.5d);
        overviewTile.Statistics.SampleCount.Should().Be(16);
        overviewTile.Statistics.IsExact.Should().BeFalse();
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
