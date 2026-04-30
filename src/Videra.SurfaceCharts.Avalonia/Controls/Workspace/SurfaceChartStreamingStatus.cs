namespace Videra.SurfaceCharts.Avalonia.Controls.Workspace;

/// <summary>
/// Per-chart streaming and high-density data status for workspace evidence.
/// </summary>
public sealed record SurfaceChartStreamingStatus
{
    /// <summary>
    /// Gets the streaming update mode (e.g. "Replace", "Append", "FifoTrim").
    /// </summary>
    public required string UpdateMode { get; init; }

    /// <summary>
    /// Gets the current retained point count in the series.
    /// </summary>
    public required int RetainedPointCount { get; init; }

    /// <summary>
    /// Gets the optional FIFO capacity limit, or null when FIFO trimming is not configured.
    /// </summary>
    public int? FifoCapacity { get; init; }

    /// <summary>
    /// Gets the number of pickable points in the series.
    /// </summary>
    public int PickablePointCount { get; init; }

    /// <summary>
    /// Gets the number of append batches applied to this series.
    /// </summary>
    public int AppendBatchCount { get; init; }

    /// <summary>
    /// Gets the number of replacement batches applied to this series.
    /// </summary>
    public int ReplaceBatchCount { get; init; }

    /// <summary>
    /// Gets the total number of points dropped by FIFO trimming.
    /// </summary>
    public long DroppedFifoPointCount { get; init; }

    /// <summary>
    /// Gets the live-view mode description, or null when not applicable.
    /// </summary>
    public string? LiveViewMode { get; init; }

    /// <summary>
    /// Gets whether this status is evidence-only (not used for runtime chart behavior).
    /// </summary>
    public required bool EvidenceOnly { get; init; }
}
