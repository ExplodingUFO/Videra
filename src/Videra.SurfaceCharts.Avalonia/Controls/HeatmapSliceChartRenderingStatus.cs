using Avalonia;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes the current readiness state for heatmap slice chart rendering.
/// </summary>
public sealed record HeatmapSliceChartRenderingStatus
{
    /// <summary>
    /// Gets whether the view currently has heatmap slice data.
    /// </summary>
    public bool HasSource { get; init; }

    /// <summary>
    /// Gets whether the view has enough state to render the slice.
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
    /// Gets the number of cells in the current slice.
    /// </summary>
    public int CellCount { get; init; }

    /// <summary>
    /// Gets the slice axis.
    /// </summary>
    public HeatmapSliceAxis Axis { get; init; }

    /// <summary>
    /// Gets the normalized slice position (0..1).
    /// </summary>
    public double Position { get; init; }

    /// <summary>
    /// Gets the last arranged view size.
    /// </summary>
    public Size ViewSize { get; init; }
}
