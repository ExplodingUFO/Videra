using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class SurfacePyramidBuilderTests
{
    [Fact]
    public void Build_CreatesOverviewLevelFromSourceMatrix()
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

        var tileSource = builder.Build(source);
        var overviewLevel = tileSource.Levels.Single(level => level.LevelX == 0 && level.LevelY == 0);

        overviewLevel.Matrix.Metadata.Width.Should().Be(2);
        overviewLevel.Matrix.Metadata.Height.Should().Be(2);
        overviewLevel.Matrix.Values.ToArray().Should().Equal(3.5f, 5.5f, 11.5f, 13.5f);
    }

    [Fact]
    public void Build_PreservesMetadataAcrossGeneratedLevels()
    {
        var source = CreateMatrix(8, 4, CreateSequentialValues(8, 4));
        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);

        var tileSource = builder.Build(source);

        tileSource.Metadata.Should().BeSameAs(source.Metadata);
        tileSource.Levels.Should().HaveCount(6);

        foreach (var level in tileSource.Levels)
        {
            level.Matrix.Metadata.HorizontalAxis.Should().Be(source.Metadata.HorizontalAxis);
            level.Matrix.Metadata.VerticalAxis.Should().Be(source.Metadata.VerticalAxis);
            level.Matrix.Metadata.ValueRange.Should().Be(source.Metadata.ValueRange);
        }
    }

    [Fact]
    public void Build_TracksPeakScratchSampleCountWithinLargestGeneratedLevel()
    {
        var source = CreateMatrix(8, 4, CreateSequentialValues(8, 4));
        var builder = new SurfacePyramidBuilder(maxTileWidth: 2, maxTileHeight: 2);

        _ = builder.Build(source);

        builder.LastBuildPeakScratchSampleCount.Should().Be(16);
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
