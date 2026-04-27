using System.Numerics;
using Avalonia;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes the current readiness and interaction state for <see cref="ScatterChartView"/>.
/// </summary>
public sealed record ScatterChartRenderingStatus
{
    /// <summary>
    /// Gets whether the view currently has a source.
    /// </summary>
    public bool HasSource { get; init; }

    /// <summary>
    /// Gets whether the view has enough state to render a scatter scene.
    /// </summary>
    public bool IsReady { get; init; }

    /// <summary>
    /// Gets the renderer path chosen by the control.
    /// </summary>
    public SurfaceChartRenderBackendKind BackendKind { get; init; } = SurfaceChartRenderBackendKind.Software;

    /// <summary>
    /// Gets whether a direct pointer interaction is currently active.
    /// </summary>
    public bool IsInteracting { get; init; }

    /// <summary>
    /// Gets the current interaction-quality mode for chart-local render work.
    /// </summary>
    public SurfaceChartInteractionQuality InteractionQuality { get; init; } = SurfaceChartInteractionQuality.Refine;

    /// <summary>
    /// Gets the number of series in the current source.
    /// </summary>
    public int SeriesCount { get; init; }

    /// <summary>
    /// Gets the number of points in the current source.
    /// </summary>
    public int PointCount { get; init; }

    /// <summary>
    /// Gets the number of columnar series in the current source.
    /// </summary>
    public int ColumnarSeriesCount { get; init; }

    /// <summary>
    /// Gets the retained point count across columnar series.
    /// </summary>
    public int ColumnarPointCount { get; init; }

    /// <summary>
    /// Gets the number of columnar points marked pickable.
    /// </summary>
    public int PickablePointCount { get; init; }

    /// <summary>
    /// Gets the total append batches applied to columnar series.
    /// </summary>
    public int StreamingAppendBatchCount { get; init; }

    /// <summary>
    /// Gets the total replacement batches applied to columnar series.
    /// </summary>
    public int StreamingReplaceBatchCount { get; init; }

    /// <summary>
    /// Gets the total points dropped by FIFO trimming across columnar series.
    /// </summary>
    public long StreamingDroppedPointCount { get; init; }

    /// <summary>
    /// Gets the points dropped by the most recent columnar update.
    /// </summary>
    public int LastStreamingDroppedPointCount { get; init; }

    /// <summary>
    /// Gets the sum of configured FIFO capacities across bounded columnar series.
    /// </summary>
    public int ConfiguredFifoCapacity { get; init; }

    /// <summary>
    /// Gets the last arranged view size.
    /// </summary>
    public Size ViewSize { get; init; }

    /// <summary>
    /// Gets the camera target used for the current render pose.
    /// </summary>
    public Vector3 CameraTarget { get; init; }

    /// <summary>
    /// Gets the camera distance used for the current render pose.
    /// </summary>
    public double CameraDistance { get; init; }
}
