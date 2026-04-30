namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Provides a live-streaming facade over a mutable surface matrix, supporting
/// append (new rows), replace (full matrix swap), and FIFO (row-cap) semantics.
/// </summary>
public sealed class SurfaceDataLogger3D
{
    private SurfaceMatrix _matrix;
    private int _appendBatchCount;
    private int _replaceBatchCount;
    private long _totalAppendedRowCount;
    private int _lastDroppedRowCount;
    private int? _fifoRowCapacity;

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceDataLogger3D"/> class.
    /// </summary>
    /// <param name="matrix">The initial surface matrix.</param>
    /// <param name="fifoRowCapacity">Optional maximum retained row count. When set, oldest rows are trimmed on append.</param>
    public SurfaceDataLogger3D(SurfaceMatrix matrix, int? fifoRowCapacity = null)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        if (fifoRowCapacity is { } cap && cap <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fifoRowCapacity), "FIFO row capacity must be positive.");
        }

        _matrix = matrix;
        _fifoRowCapacity = fifoRowCapacity;
    }

    /// <summary>
    /// Gets the current surface matrix.
    /// </summary>
    public SurfaceMatrix Matrix => _matrix;

    /// <summary>
    /// Gets the current row count.
    /// </summary>
    public int RowCount => _matrix.Metadata.Height;

    /// <summary>
    /// Gets the column count.
    /// </summary>
    public int ColumnCount => _matrix.Metadata.Width;

    /// <summary>
    /// Gets the optional FIFO row capacity.
    /// </summary>
    public int? FifoRowCapacity => _fifoRowCapacity;

    /// <summary>
    /// Gets the number of append batches applied.
    /// </summary>
    public int AppendBatchCount => _appendBatchCount;

    /// <summary>
    /// Gets the number of replace batches applied.
    /// </summary>
    public int ReplaceBatchCount => _replaceBatchCount;

    /// <summary>
    /// Gets the total number of rows accepted through append batches.
    /// </summary>
    public long TotalAppendedRowCount => _totalAppendedRowCount;

    /// <summary>
    /// Gets the number of rows dropped by the most recent append operation.
    /// </summary>
    public int LastDroppedRowCount => _lastDroppedRowCount;

    /// <summary>
    /// Replaces the entire matrix with a new one.
    /// </summary>
    /// <param name="matrix">The replacement matrix.</param>
    public void Replace(SurfaceMatrix matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        _matrix = matrix;
        _replaceBatchCount++;
        _lastDroppedRowCount = 0;
    }

    /// <summary>
    /// Appends new rows to the matrix. If FIFO capacity is configured, oldest rows are trimmed.
    /// </summary>
    /// <param name="newRows">Matrix containing the new rows to append. Must have matching column count.</param>
    public void Append(SurfaceMatrix newRows)
    {
        ArgumentNullException.ThrowIfNull(newRows);
        if (newRows.Metadata.Width != ColumnCount)
        {
            throw new ArgumentException("Appended rows must have matching column count.", nameof(newRows));
        }

        var currentValues = _matrix.Values.Span;
        var appendValues = newRows.Values.Span;
        var cols = ColumnCount;
        var currentRows = RowCount;
        var appendRowCount = newRows.Metadata.Height;
        var newTotalRows = currentRows + appendRowCount;

        // Apply FIFO trimming
        var trimRows = 0;
        _lastDroppedRowCount = 0;
        if (_fifoRowCapacity is { } cap && newTotalRows > cap)
        {
            trimRows = newTotalRows - cap;
            _lastDroppedRowCount = trimRows;
        }

        var retainedRows = currentRows - trimRows;
        var finalRows = retainedRows + appendRowCount;
        var flatValues = new float[finalRows * cols];

        // Copy retained existing rows
        for (var r = 0; r < retainedRows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                flatValues[r * cols + c] = currentValues[(trimRows + r) * cols + c];
            }
        }

        // Copy appended rows
        for (var r = 0; r < appendRowCount; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                flatValues[(retainedRows + r) * cols + c] = appendValues[r * cols + c];
            }
        }

        // Compute new value range
        var min = float.MaxValue;
        var max = float.MinValue;
        foreach (var v in flatValues)
        {
            if (float.IsFinite(v))
            {
                if (v < min) min = v;
                if (v > max) max = v;
            }
        }

        var range = new SurfaceValueRange(min, max);
        var metadata = new SurfaceMetadata(
            cols,
            finalRows,
            _matrix.Metadata.HorizontalAxis,
            new SurfaceAxisDescriptor(_matrix.Metadata.VerticalAxis.Label, _matrix.Metadata.VerticalAxis.Unit, 0d, finalRows - 1),
            range);

        _matrix = new SurfaceMatrix(metadata, flatValues);
        _appendBatchCount++;
        _totalAppendedRowCount += appendRowCount;
    }
}
