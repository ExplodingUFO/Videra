namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents the binning mode for a histogram.
/// </summary>
public enum HistogramMode
{
    /// <summary>
    /// Standard count-based histogram.
    /// </summary>
    Count,

    /// <summary>
    /// Normalized density histogram (area sums to 1).
    /// </summary>
    Density,

    /// <summary>
    /// Cumulative count histogram.
    /// </summary>
    Cumulative,
}

/// <summary>
/// Represents one immutable histogram dataset.
/// </summary>
public sealed class HistogramData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HistogramData"/> class.
    /// </summary>
    /// <param name="values">The raw values to bin.</param>
    /// <param name="binCount">The number of bins. Must be positive.</param>
    /// <param name="mode">The histogram mode.</param>
    public HistogramData(IReadOnlyList<double> values, int binCount = 20, HistogramMode mode = HistogramMode.Count)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(binCount);

        Values = values;
        BinCount = binCount;
        Mode = mode;

        var min = double.MaxValue;
        var max = double.MinValue;
        foreach (var v in values)
        {
            if (v < min) min = v;
            if (v > max) max = v;
        }

        RangeMin = min;
        RangeMax = max;

        Bins = ComputeBins(values, binCount, mode, min, max);
    }

    /// <summary>
    /// Gets the raw values.
    /// </summary>
    public IReadOnlyList<double> Values { get; }

    /// <summary>
    /// Gets the number of bins.
    /// </summary>
    public int BinCount { get; }

    /// <summary>
    /// Gets the histogram mode.
    /// </summary>
    public HistogramMode Mode { get; }

    /// <summary>
    /// Gets the minimum value in the dataset.
    /// </summary>
    public double RangeMin { get; }

    /// <summary>
    /// Gets the maximum value in the dataset.
    /// </summary>
    public double RangeMax { get; }

    /// <summary>
    /// Gets the computed bin heights.
    /// </summary>
    public IReadOnlyList<double> Bins { get; }

    /// <summary>
    /// Gets the width of each bin.
    /// </summary>
    public double BinWidth => (RangeMax - RangeMin) / BinCount;

    private static IReadOnlyList<double> ComputeBins(IReadOnlyList<double> values, int binCount, HistogramMode mode, double min, double max)
    {
        if (values.Count == 0 || min >= max)
        {
            return Array.AsReadOnly(new double[binCount]);
        }

        var bins = new double[binCount];
        var binWidth = (max - min) / binCount;

        foreach (var v in values)
        {
            var index = (int)((v - min) / binWidth);
            if (index >= binCount) index = binCount - 1;
            if (index < 0) index = 0;
            bins[index]++;
        }

        if (mode == HistogramMode.Density)
        {
            var totalArea = values.Count * binWidth;
            for (var i = 0; i < binCount; i++)
            {
                bins[i] /= totalArea;
            }
        }
        else if (mode == HistogramMode.Cumulative)
        {
            for (var i = 1; i < binCount; i++)
            {
                bins[i] += bins[i - 1];
            }
        }

        return Array.AsReadOnly(bins);
    }
}
