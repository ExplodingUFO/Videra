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

    [Fact]
    public async Task Build_IgnoresNonFiniteSamplesWhenReducingOverviewValuesAndStatistics()
    {
        var source = CreateMatrix(4, 4, new float[]
        {
            1, float.NaN, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16
        });

        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);
        ISurfaceTileSource tileSource = builder.Build(source);
        var overviewTile = await tileSource.GetRequiredTileAsync(new SurfaceTileKey(0, 0, 0, 0));

        overviewTile.Values.ToArray().Should().Equal(4f, 5.5f, 11.5f, 13.5f);
        overviewTile.Statistics.Range.Should().Be(new SurfaceValueRange(1d, 16d));
        overviewTile.Statistics.Average.Should().BeApproximately(8.933333333333334d, 1e-12d);
        overviewTile.Statistics.SampleCount.Should().Be(16);
    }

    [Fact]
    public void Build_PropagatesMissingMaskWhenReducedBlockHasNoValidSamples()
    {
        var source = CreateMatrix(4, 4, new float[]
        {
            float.NaN, float.NaN, 3, 4,
            float.NaN, float.NaN, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16
        });

        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);
        var tileSource = builder.Build(source);
        var overviewLevel = ((InMemorySurfaceTileSource)tileSource).Levels
            .Single(level => level.LevelX == 0 && level.LevelY == 0);

        overviewLevel.Matrix.Mask.Should().NotBeNull();
        overviewLevel.Matrix.Mask!.Values.ToArray().Should().Equal(false, true, true, true);
    }

    [Fact]
    public void Build_PreservesExplicitGeometryAndReducesColorFieldAcrossGeneratedLevels()
    {
        var source = CreateExplicitMatrixWithColorField();
        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);

        var tileSource = (InMemorySurfaceTileSource)builder.Build(source);
        var overviewLevel = tileSource.Levels
            .Single(level => level.LevelX == 0 && level.LevelY == 0);

        var explicitGrid = overviewLevel.Matrix.Metadata.Geometry.Should().BeOfType<SurfaceExplicitGrid>().Which;
        explicitGrid.HorizontalCoordinates.ToArray().Should().Equal(15d, 60d);
        explicitGrid.VerticalCoordinates.ToArray().Should().Equal(115d, 250d);
        overviewLevel.Matrix.ColorField.Should().NotBeNull();
        overviewLevel.Matrix.ColorField!.Values.ToArray().Should().Equal(102.5f, 104.5f, 110.5f, 112.5f);
    }

    [Fact]
    public async Task Build_IgnoresExplicitlyMaskedFiniteSamplesWhenReducingOverviewValuesAndStatistics()
    {
        var metadata = CreateMetadata(4, 4);
        var source = new SurfaceMatrix(
            metadata,
            new SurfaceScalarField(
                width: 4,
                height: 4,
                values: new float[]
                {
                    100, 2, 3, 4,
                    5, 6, 7, 8,
                    9, 10, 11, 12,
                    13, 14, 15, 16
                },
                range: metadata.ValueRange),
            colorField: null,
            mask: new SurfaceMask(
                width: 4,
                height: 4,
                values: new bool[]
                {
                    false, true, true, true,
                    true, true, true, true,
                    true, true, true, true,
                    true, true, true, true
                }));

        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);
        ISurfaceTileSource tileSource = builder.Build(source);
        var overviewTile = await tileSource.GetRequiredTileAsync(new SurfaceTileKey(0, 0, 0, 0));

        overviewTile.Values.ToArray().Should().Equal(4.3333335f, 5.5f, 11.5f, 13.5f);
        overviewTile.Statistics.Range.Should().Be(new SurfaceValueRange(2d, 16d));
        overviewTile.Statistics.Average.Should().Be(9d);
        overviewTile.Statistics.SampleCount.Should().Be(16);
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
