using System.Numerics;
using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public sealed class MarchingSquaresExtractorTests
{
    [Fact]
    public void Extract_FlatField_ReturnsEmptySegments()
    {
        var field = CreateField(3, 3, [5f, 5f, 5f, 5f, 5f, 5f, 5f, 5f, 5f]);

        var segments = MarchingSquaresExtractor.Extract(field, 3f);

        segments.Should().BeEmpty();
    }

    [Fact]
    public void Extract_SingleValueField_ReturnsEmptySegments()
    {
        var field = CreateField(2, 2, [1f, 1f, 1f, 1f]);

        var segments = MarchingSquaresExtractor.Extract(field, 0.5f);

        segments.Should().BeEmpty();
    }

    [Fact]
    public void Extract_IsoValueBelowMinimum_ReturnsEmptySegments()
    {
        var field = CreateField(3, 3, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f]);

        var segments = MarchingSquaresExtractor.Extract(field, 0f);

        segments.Should().BeEmpty();
    }

    [Fact]
    public void Extract_IsoValueAboveMaximum_ReturnsEmptySegments()
    {
        var field = CreateField(3, 3, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f]);

        var segments = MarchingSquaresExtractor.Extract(field, 100f);

        segments.Should().BeEmpty();
    }

    [Fact]
    public void Extract_RadialField_ProducesSegments()
    {
        // Create a radial field (distance from center)
        var width = 5;
        var height = 5;
        var values = new float[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var dx = x - 2f;
                var dy = y - 2f;
                values[y * width + x] = MathF.Sqrt(dx * dx + dy * dy);
            }
        }

        var field = CreateField(width, height, values);

        var segments = MarchingSquaresExtractor.Extract(field, 1.5f);

        segments.Should().NotBeEmpty();
        foreach (var segment in segments)
        {
            segment.Start.Should().NotBe(segment.End);
        }
    }

    [Fact]
    public void Extract_LinearGradient_ProducesHorizontalSegments()
    {
        // Linear gradient from left to right
        var width = 5;
        var height = 3;
        var values = new float[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                values[y * width + x] = x;
            }
        }

        var field = CreateField(width, height, values);

        var segments = MarchingSquaresExtractor.Extract(field, 2.5f);

        segments.Should().NotBeEmpty();
        // All segments should be vertical (same X coordinate)
        foreach (var segment in segments)
        {
            segment.Start.X.Should().Be(segment.End.X);
        }
    }

    [Fact]
    public void Extract_WithMask_SkipsMaskedCells()
    {
        var field = CreateField(3, 3, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f]);
        var mask = new SurfaceMask(3, 3, new ReadOnlyMemory<bool>([true, true, true, true, false, false, true, true, true]));

        var segmentsWithMask = MarchingSquaresExtractor.Extract(field, 5f, mask);
        var segmentsWithoutMask = MarchingSquaresExtractor.Extract(field, 5f);

        segmentsWithMask.Count.Should().BeLessThan(segmentsWithoutMask.Count);
    }

    [Fact]
    public void Extract_WithNaNValues_SkipsNaNCorners()
    {
        var field = CreateField(3, 3, [1f, 2f, float.NaN, 4f, 5f, 6f, 7f, 8f, 9f]);

        var segments = MarchingSquaresExtractor.Extract(field, 5f);

        // Should still produce some segments (from cells without NaN)
        segments.Should().NotBeEmpty();
    }

    [Fact]
    public void Extract_AllCornersAboveIsoValue_ReturnsEmptyForCell()
    {
        var field = CreateField(2, 2, [10f, 10f, 10f, 10f]);

        var segments = MarchingSquaresExtractor.Extract(field, 5f);

        segments.Should().BeEmpty();
    }

    [Fact]
    public void Extract_AllCornersBelowIsoValue_ReturnsEmptyForCell()
    {
        var field = CreateField(2, 2, [1f, 1f, 1f, 1f]);

        var segments = MarchingSquaresExtractor.Extract(field, 5f);

        segments.Should().BeEmpty();
    }

    [Fact]
    public void Extract_SingleCell_Case1_ProducesBottomToLeftSegment()
    {
        // Case 1: only bottom-left above iso-value (v00=1, v10=0, v11=0, v01=0)
        var field = CreateField(2, 2, [1f, 0f, 0f, 0f]);

        var segments = MarchingSquaresExtractor.Extract(field, 0.5f);

        segments.Should().HaveCount(1);
    }

    [Fact]
    public void Extract_SingleCell_Case15_ReturnsEmpty()
    {
        // Case 15: all corners above iso-value
        var field = CreateField(2, 2, [1f, 1f, 1f, 1f]);

        var segments = MarchingSquaresExtractor.Extract(field, 0.5f);

        segments.Should().BeEmpty();
    }

    private static SurfaceScalarField CreateField(int width, int height, float[] values)
    {
        var finiteValues = values.Where(float.IsFinite).ToArray();
        var min = finiteValues.Length > 0 ? finiteValues.Min() : 0f;
        var max = finiteValues.Length > 0 ? finiteValues.Max() : 0f;
        return new SurfaceScalarField(width, height, values, new SurfaceValueRange(min, max));
    }
}
