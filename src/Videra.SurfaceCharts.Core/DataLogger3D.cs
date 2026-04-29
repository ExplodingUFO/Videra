namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Provides a small live-scatter facade over a mutable columnar scatter series.
/// </summary>
public sealed class DataLogger3D
{
    private readonly ScatterColumnarSeries _series;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataLogger3D"/> class.
    /// </summary>
    /// <param name="color">The default ARGB series color.</param>
    /// <param name="label">The optional series label.</param>
    /// <param name="isSortedX">Whether appended data is known to be sorted by X.</param>
    /// <param name="containsNaN">Whether the series accepts NaN gaps in coordinate columns.</param>
    /// <param name="pickable">Whether points in this high-volume path participate in picking.</param>
    /// <param name="fifoCapacity">The optional maximum retained point count.</param>
    public DataLogger3D(
        uint color,
        string? label = null,
        bool isSortedX = false,
        bool containsNaN = false,
        bool pickable = false,
        int? fifoCapacity = null)
        : this(new ScatterColumnarSeries(color, label, isSortedX, containsNaN, pickable, fifoCapacity))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataLogger3D"/> class over an existing columnar series.
    /// </summary>
    /// <param name="series">The mutable columnar scatter series.</param>
    public DataLogger3D(ScatterColumnarSeries series)
    {
        ArgumentNullException.ThrowIfNull(series);

        _series = series;
    }

    /// <summary>
    /// Gets the mutable columnar series used by chart datasets.
    /// </summary>
    public ScatterColumnarSeries Series => _series;

    /// <summary>
    /// Gets the retained point count.
    /// </summary>
    public int Count => _series.Count;

    /// <summary>
    /// Gets the horizontal-axis coordinates.
    /// </summary>
    public ReadOnlyMemory<float> X => _series.X;

    /// <summary>
    /// Gets the value-axis coordinates.
    /// </summary>
    public ReadOnlyMemory<float> Y => _series.Y;

    /// <summary>
    /// Gets the depth-axis coordinates.
    /// </summary>
    public ReadOnlyMemory<float> Z => _series.Z;

    /// <summary>
    /// Gets the optional per-point marker sizes.
    /// </summary>
    public ReadOnlyMemory<float> Size => _series.Size;

    /// <summary>
    /// Gets the optional per-point ARGB colors.
    /// </summary>
    public ReadOnlyMemory<uint> PointColor => _series.PointColor;

    /// <summary>
    /// Gets the optional maximum retained point count.
    /// </summary>
    public int? FifoCapacity => _series.FifoCapacity;

    /// <summary>
    /// Gets the number of append batches applied to this stream.
    /// </summary>
    public int AppendBatchCount => _series.AppendBatchCount;

    /// <summary>
    /// Gets the number of replacement batches applied to this stream.
    /// </summary>
    public int ReplaceBatchCount => _series.ReplaceBatchCount;

    /// <summary>
    /// Gets the total number of points accepted through append batches.
    /// </summary>
    public long TotalAppendedPointCount => _series.TotalAppendedPointCount;

    /// <summary>
    /// Gets the total number of points dropped by FIFO trimming.
    /// </summary>
    public long TotalDroppedPointCount => _series.TotalDroppedPointCount;

    /// <summary>
    /// Gets the number of points dropped by the most recent replace or append operation.
    /// </summary>
    public int LastDroppedPointCount => _series.LastDroppedPointCount;

    /// <summary>
    /// Replaces the retained columns with a new batch.
    /// </summary>
    /// <param name="data">The replacement data.</param>
    public void Replace(ScatterColumnarData data)
    {
        _series.ReplaceRange(data);
    }

    /// <summary>
    /// Appends a batch and trims the oldest points when FIFO capacity is configured.
    /// </summary>
    /// <param name="data">The data to append.</param>
    public void Append(ScatterColumnarData data)
    {
        _series.AppendRange(data);
    }
}
