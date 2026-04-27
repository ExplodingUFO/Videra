using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable scatter-chart dataset.
/// </summary>
public sealed class ScatterChartData
{
    private readonly ReadOnlyCollection<ScatterSeries> _seriesView;
    private readonly ReadOnlyCollection<ScatterColumnarSeries> _columnarSeriesView;
    private readonly int _pointSeriesPointCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScatterChartData"/> class.
    /// </summary>
    /// <param name="metadata">The dataset metadata.</param>
    /// <param name="series">The immutable series collection.</param>
    public ScatterChartData(ScatterChartMetadata metadata, IReadOnlyList<ScatterSeries> series)
        : this(metadata, series, [])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScatterChartData"/> class.
    /// </summary>
    /// <param name="metadata">The dataset metadata.</param>
    /// <param name="series">The immutable point-object series collection.</param>
    /// <param name="columnarSeries">The columnar series collection.</param>
    public ScatterChartData(
        ScatterChartMetadata metadata,
        IReadOnlyList<ScatterSeries> series,
        IReadOnlyList<ScatterColumnarSeries> columnarSeries)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(series);
        ArgumentNullException.ThrowIfNull(columnarSeries);

        Metadata = metadata;
        _seriesView = Array.AsReadOnly(series.ToArray());
        _columnarSeriesView = Array.AsReadOnly(columnarSeries.ToArray());
        _pointSeriesPointCount = ValidateAndCountPoints(metadata, _seriesView, _columnarSeriesView);
    }

    /// <summary>
    /// Gets the dataset metadata.
    /// </summary>
    public ScatterChartMetadata Metadata { get; }

    /// <summary>
    /// Gets the immutable scatter series collection.
    /// </summary>
    public IReadOnlyList<ScatterSeries> Series => _seriesView;

    /// <summary>
    /// Gets the columnar scatter series collection.
    /// </summary>
    public IReadOnlyList<ScatterColumnarSeries> ColumnarSeries => _columnarSeriesView;

    /// <summary>
    /// Gets the number of series in the dataset.
    /// </summary>
    public int SeriesCount => _seriesView.Count + _columnarSeriesView.Count;

    /// <summary>
    /// Gets the number of columnar series in the dataset.
    /// </summary>
    public int ColumnarSeriesCount => _columnarSeriesView.Count;

    /// <summary>
    /// Gets the total point count across every series.
    /// </summary>
    public int PointCount => _pointSeriesPointCount + ColumnarPointCount;

    /// <summary>
    /// Gets the retained point count across columnar series.
    /// </summary>
    public int ColumnarPointCount => CountColumnarPoints(_columnarSeriesView);

    /// <summary>
    /// Gets the number of points marked pickable by high-volume columnar series.
    /// </summary>
    public int PickablePointCount => CountPickablePoints(_columnarSeriesView);

    /// <summary>
    /// Gets the total append batches applied to columnar series.
    /// </summary>
    public int StreamingAppendBatchCount => SumColumnarSeries(_columnarSeriesView, static series => series.AppendBatchCount);

    /// <summary>
    /// Gets the total replacement batches applied to columnar series.
    /// </summary>
    public int StreamingReplaceBatchCount => SumColumnarSeries(_columnarSeriesView, static series => series.ReplaceBatchCount);

    /// <summary>
    /// Gets the total points dropped by FIFO trimming across columnar series.
    /// </summary>
    public long StreamingDroppedPointCount => SumColumnarSeries(_columnarSeriesView, static series => series.TotalDroppedPointCount);

    /// <summary>
    /// Gets the points dropped by the most recent update across columnar series.
    /// </summary>
    public int LastStreamingDroppedPointCount => SumColumnarSeries(_columnarSeriesView, static series => series.LastDroppedPointCount);

    /// <summary>
    /// Gets the sum of configured FIFO capacities across bounded columnar series. Zero means no bounded series.
    /// </summary>
    public int ConfiguredFifoCapacity => SumColumnarSeries(_columnarSeriesView, static series => series.FifoCapacity ?? 0);

    private static int ValidateAndCountPoints(
        ScatterChartMetadata metadata,
        IReadOnlyList<ScatterSeries> series,
        IReadOnlyList<ScatterColumnarSeries> columnarSeries)
    {
        var totalCount = 0;

        for (var seriesIndex = 0; seriesIndex < series.Count; seriesIndex++)
        {
            var scatterSeries = series[seriesIndex];
            if (scatterSeries is null)
            {
                throw new ArgumentException("Scatter series entries must not be null.", nameof(series));
            }

            totalCount += scatterSeries.Points.Count;
            if (scatterSeries.Points.Any(point => !metadata.Contains(point)))
            {
                throw new ArgumentException("Scatter points must remain within the declared metadata bounds.", nameof(series));
            }
        }

        for (var seriesIndex = 0; seriesIndex < columnarSeries.Count; seriesIndex++)
        {
            var scatterSeries = columnarSeries[seriesIndex];
            if (scatterSeries is null)
            {
                throw new ArgumentException("Columnar scatter series entries must not be null.", nameof(columnarSeries));
            }

            ValidateColumnarPoints(metadata, scatterSeries);
        }

        return totalCount;
    }

    private static void ValidateColumnarPoints(ScatterChartMetadata metadata, ScatterColumnarSeries series)
    {
        var x = series.X.Span;
        var y = series.Y.Span;
        var z = series.Z.Span;

        for (var index = 0; index < series.Count; index++)
        {
            if (float.IsNaN(x[index]) || float.IsNaN(y[index]) || float.IsNaN(z[index]))
            {
                continue;
            }

            if (x[index] < metadata.HorizontalAxis.Minimum
                || x[index] > metadata.HorizontalAxis.Maximum
                || z[index] < metadata.DepthAxis.Minimum
                || z[index] > metadata.DepthAxis.Maximum
                || y[index] < metadata.ValueRange.Minimum
                || y[index] > metadata.ValueRange.Maximum)
            {
                throw new ArgumentException("Columnar scatter points must remain within the declared metadata bounds.", nameof(series));
            }
        }
    }

    private static int CountPickablePoints(IReadOnlyList<ScatterColumnarSeries> columnarSeries)
    {
        var totalCount = 0;
        foreach (var series in columnarSeries)
        {
            if (series.Pickable)
            {
                totalCount += series.Count;
            }
        }

        return totalCount;
    }

    private static int CountColumnarPoints(IReadOnlyList<ScatterColumnarSeries> columnarSeries)
    {
        return SumColumnarSeries(columnarSeries, static series => series.Count);
    }

    private static int SumColumnarSeries(
        IReadOnlyList<ScatterColumnarSeries> columnarSeries,
        Func<ScatterColumnarSeries, int> selector)
    {
        var total = 0;
        foreach (var series in columnarSeries)
        {
            total += selector(series);
        }

        return total;
    }

    private static long SumColumnarSeries(
        IReadOnlyList<ScatterColumnarSeries> columnarSeries,
        Func<ScatterColumnarSeries, long> selector)
    {
        var total = 0L;
        foreach (var series in columnarSeries)
        {
            total += selector(series);
        }

        return total;
    }
}
