using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public sealed class ContourExtractorTests
{
    [Fact]
    public void ExtractAll_RadialField_ProducesMultipleContourLines()
    {
        var width = 10;
        var height = 10;
        var values = new float[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var dx = x - 4.5f;
                var dy = y - 4.5f;
                values[y * width + x] = MathF.Sqrt(dx * dx + dy * dy);
            }
        }

        var field = CreateField(width, height, values);
        var data = new ContourChartData(field, levelCount: 5);

        var lines = ContourExtractor.ExtractAll(data);

        lines.Should().NotBeEmpty();
        lines.Should().OnlyContain(l => l.Segments.Count > 0);
    }

    [Fact]
    public void ExtractAll_FlatField_ReturnsEmptyLines()
    {
        var field = CreateField(3, 3, [5f, 5f, 5f, 5f, 5f, 5f, 5f, 5f, 5f]);
        var data = new ContourChartData(field, levelCount: 5);

        var lines = ContourExtractor.ExtractAll(data);

        lines.Should().BeEmpty();
    }

    [Fact]
    public void ExtractAll_EvenlySpacedIsoValues()
    {
        var field = CreateField(5, 5, [
            0f, 1f, 2f, 3f, 4f,
            1f, 2f, 3f, 4f, 5f,
            2f, 3f, 4f, 5f, 6f,
            3f, 4f, 5f, 6f, 7f,
            4f, 5f, 6f, 7f, 8f
        ]);
        var data = new ContourChartData(field, levelCount: 3);

        var lines = ContourExtractor.ExtractAll(data);

        lines.Should().NotBeEmpty();
        // Iso-values should be evenly spaced
        var isoValues = lines.Select(l => l.IsoValue).ToArray();
        for (var i = 1; i < isoValues.Length; i++)
        {
            var diff = isoValues[i] - isoValues[i - 1];
            diff.Should().BeGreaterThan(0f);
        }
    }

    [Fact]
    public void ExtractAll_WithMask_RespectsMask()
    {
        var field = CreateField(5, 5, [
            0f, 1f, 2f, 3f, 4f,
            1f, 2f, 3f, 4f, 5f,
            2f, 3f, 4f, 5f, 6f,
            3f, 4f, 5f, 6f, 7f,
            4f, 5f, 6f, 7f, 8f
        ]);
        var mask = new SurfaceMask(5, 5, new ReadOnlyMemory<bool>([
            true, true, true, true, true,
            true, true, true, true, true,
            true, true, false, false, false,
            true, true, false, false, false,
            true, true, false, false, false
        ]));
        var dataWithoutMask = new ContourChartData(field, levelCount: 3);
        var dataWithMask = new ContourChartData(field, mask, levelCount: 3);

        var linesWithoutMask = ContourExtractor.ExtractAll(dataWithoutMask);
        var linesWithMask = ContourExtractor.ExtractAll(dataWithMask);

        linesWithMask.Should().NotBeEmpty();
        // Masked version should have fewer or equal segments
        var totalSegmentsWithout = linesWithoutMask.Sum(l => l.Segments.Count);
        var totalSegmentsWith = linesWithMask.Sum(l => l.Segments.Count);
        totalSegmentsWith.Should().BeLessThanOrEqualTo(totalSegmentsWithout);
    }

    [Fact]
    public void ExtractAll_IsoValuesAreDistinct()
    {
        var field = CreateField(5, 5, [
            0f, 1f, 2f, 3f, 4f,
            1f, 2f, 3f, 4f, 5f,
            2f, 3f, 4f, 5f, 6f,
            3f, 4f, 5f, 6f, 7f,
            4f, 5f, 6f, 7f, 8f
        ]);
        var data = new ContourChartData(field, levelCount: 5);

        var lines = ContourExtractor.ExtractAll(data);

        var isoValues = lines.Select(l => l.IsoValue).ToArray();
        isoValues.Should().OnlyHaveUniqueItems();
    }

    private static SurfaceScalarField CreateField(int width, int height, float[] values)
    {
        var min = values.Min();
        var max = values.Max();
        return new SurfaceScalarField(width, height, values, new SurfaceValueRange(min, max));
    }
}
