namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Provides a live-streaming facade for bar charts, supporting append (new categories)
/// and replace (full data swap) semantics.
/// </summary>
public sealed class BarDataLogger3D
{
    private BarChartData _data;
    private int _appendBatchCount;
    private int _replaceBatchCount;
    private long _totalAppendedSeriesCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="BarDataLogger3D"/> class.
    /// </summary>
    /// <param name="data">The initial bar chart data.</param>
    public BarDataLogger3D(BarChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        _data = data;
    }

    /// <summary>
    /// Gets the current bar chart data.
    /// </summary>
    public BarChartData Data => _data;

    /// <summary>
    /// Gets the current category count.
    /// </summary>
    public int CategoryCount => _data.CategoryCount;

    /// <summary>
    /// Gets the current series count.
    /// </summary>
    public int SeriesCount => _data.Series.Count;

    /// <summary>
    /// Gets the number of append batches applied.
    /// </summary>
    public int AppendBatchCount => _appendBatchCount;

    /// <summary>
    /// Gets the number of replace batches applied.
    /// </summary>
    public int ReplaceBatchCount => _replaceBatchCount;

    /// <summary>
    /// Gets the total number of series accepted through append batches.
    /// </summary>
    public long TotalAppendedSeriesCount => _totalAppendedSeriesCount;

    /// <summary>
    /// Replaces the entire bar chart data with new data.
    /// </summary>
    /// <param name="data">The replacement data.</param>
    public void Replace(BarChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        _data = data;
        _replaceBatchCount++;
    }

    /// <summary>
    /// Appends new series to the bar chart data.
    /// </summary>
    /// <param name="newSeries">The bar series to append.</param>
    public void Append(params BarSeries[] newSeries)
    {
        ArgumentNullException.ThrowIfNull(newSeries);
        if (newSeries.Length == 0)
        {
            return;
        }

        var existing = _data.Series;
        var combined = new BarSeries[existing.Count + newSeries.Length];
        for (var i = 0; i < existing.Count; i++)
        {
            combined[i] = existing[i];
        }

        for (var i = 0; i < newSeries.Length; i++)
        {
            combined[existing.Count + i] = newSeries[i];
        }

        _data = new BarChartData(combined, _data.Layout);
        _appendBatchCount++;
        _totalAppendedSeriesCount += newSeries.Length;
    }
}
