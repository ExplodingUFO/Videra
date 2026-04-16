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
    /// <returns>The derived statistics.</returns>
    public static SurfaceTileStatistics FromValues(ReadOnlySpan<float> values, bool isExact)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Tile statistics require at least one value.", nameof(values));
        }

        var minimum = double.PositiveInfinity;
        var maximum = double.NegativeInfinity;
        double sum = 0d;

        foreach (var value in values)
        {
            minimum = Math.Min(minimum, value);
            maximum = Math.Max(maximum, value);
            sum += value;
        }

        return new SurfaceTileStatistics(
            new SurfaceValueRange(minimum, maximum),
            sum / values.Length,
            values.Length,
            isExact);
    }
}
