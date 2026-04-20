namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes summary statistics for a surface tile's covered source region.
/// </summary>
public readonly record struct SurfaceTileStatistics
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceTileStatistics"/> struct.
    /// </summary>
    /// <param name="range">The inclusive value range for the covered region.</param>
    /// <param name="average">The average value for the covered region.</param>
    /// <param name="sampleCount">The number of source samples summarized by the statistics.</param>
    /// <param name="isExact">Whether the tile values are exact samples rather than an aggregated representation.</param>
    public SurfaceTileStatistics(SurfaceValueRange range, double average, int sampleCount, bool isExact)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleCount);

        Range = range;
        Average = average;
        SampleCount = sampleCount;
        IsExact = isExact;
    }

    /// <summary>
    /// Gets the inclusive range for the covered source region.
    /// </summary>
    public SurfaceValueRange Range { get; }

    /// <summary>
    /// Gets the average value for the covered source region.
    /// </summary>
    public double Average { get; }

    /// <summary>
    /// Gets the number of samples summarized by this statistics object.
    /// </summary>
    public int SampleCount { get; }

    /// <summary>
    /// Gets a value indicating whether the tile values are exact source samples.
    /// </summary>
    public bool IsExact { get; }

    /// <summary>
    /// Creates statistics from a tile-value span.
    /// </summary>
    /// <param name="values">The tile values.</param>
    /// <param name="isExact">Whether the values are exact source samples.</param>
    /// <param name="mask">The optional availability mask.</param>
    /// <returns>The derived statistics.</returns>
    public static SurfaceTileStatistics FromValues(ReadOnlySpan<float> values, bool isExact, SurfaceMask? mask = null)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Tile statistics require at least one value.", nameof(values));
        }

        ReadOnlySpan<bool> maskValues = default;
        if (mask is not null)
        {
            if (mask.Values.Length != values.Length)
            {
                throw new ArgumentException("Mask values must match the value span length.", nameof(mask));
            }

            maskValues = mask.Values.Span;
        }
        var hasMask = maskValues.Length != 0;

        var minimum = double.PositiveInfinity;
        var maximum = double.NegativeInfinity;
        double sum = 0d;
        var validSampleCount = 0;

        for (var index = 0; index < values.Length; index++)
        {
            if (hasMask && !maskValues[index])
            {
                continue;
            }

            var value = values[index];
            if (!float.IsFinite(value))
            {
                continue;
            }

            minimum = Math.Min(minimum, value);
            maximum = Math.Max(maximum, value);
            sum += value;
            validSampleCount++;
        }

        if (validSampleCount == 0)
        {
            return new SurfaceTileStatistics(
                new SurfaceValueRange(0d, 0d),
                average: 0d,
                sampleCount: values.Length,
                isExact);
        }

        return new SurfaceTileStatistics(
            new SurfaceValueRange(minimum, maximum),
            sum / validSampleCount,
            values.Length,
            isExact);
    }
}
