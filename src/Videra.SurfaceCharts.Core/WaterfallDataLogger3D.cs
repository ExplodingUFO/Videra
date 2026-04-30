namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Provides a live-streaming facade for waterfall charts, delegating to an underlying
/// <see cref="SurfaceDataLogger3D"/> since waterfall data shares the same matrix format.
/// </summary>
public sealed class WaterfallDataLogger3D
{
    private readonly SurfaceDataLogger3D _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaterfallDataLogger3D"/> class.
    /// </summary>
    /// <param name="matrix">The initial surface matrix.</param>
    /// <param name="fifoRowCapacity">Optional maximum retained row count.</param>
    public WaterfallDataLogger3D(SurfaceMatrix matrix, int? fifoRowCapacity = null)
    {
        _inner = new SurfaceDataLogger3D(matrix, fifoRowCapacity);
    }

    /// <summary>
    /// Gets the current surface matrix.
    /// </summary>
    public SurfaceMatrix Matrix => _inner.Matrix;

    /// <summary>
    /// Gets the current row count.
    /// </summary>
    public int RowCount => _inner.RowCount;

    /// <summary>
    /// Gets the column count.
    /// </summary>
    public int ColumnCount => _inner.ColumnCount;

    /// <summary>
    /// Gets the optional FIFO row capacity.
    /// </summary>
    public int? FifoRowCapacity => _inner.FifoRowCapacity;

    /// <summary>
    /// Gets the number of append batches applied.
    /// </summary>
    public int AppendBatchCount => _inner.AppendBatchCount;

    /// <summary>
    /// Gets the number of replace batches applied.
    /// </summary>
    public int ReplaceBatchCount => _inner.ReplaceBatchCount;

    /// <summary>
    /// Gets the total number of rows accepted through append batches.
    /// </summary>
    public long TotalAppendedRowCount => _inner.TotalAppendedRowCount;

    /// <summary>
    /// Gets the number of rows dropped by the most recent append operation.
    /// </summary>
    public int LastDroppedRowCount => _inner.LastDroppedRowCount;

    /// <summary>
    /// Replaces the entire matrix with a new one.
    /// </summary>
    public void Replace(SurfaceMatrix matrix) => _inner.Replace(matrix);

    /// <summary>
    /// Appends new rows to the matrix with optional FIFO trimming.
    /// </summary>
    public void Append(SurfaceMatrix newRows) => _inner.Append(newRows);
}
