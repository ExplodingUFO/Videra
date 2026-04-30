using Avalonia;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes the current readiness state for box plot chart rendering.
/// </summary>
public sealed record BoxPlotChartRenderingStatus
{
    /// <summary>
    /// Gets whether the view currently has box plot data.
    /// </summary>
    public bool HasSource { get; init; }

    /// <summary>
    /// Gets whether the view has enough state to render boxes.
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
    /// Gets the number of categories in the current box plot.
    /// </summary>
    public int CategoryCount { get; init; }

    /// <summary>
    /// Gets the number of rendered boxes.
    /// </summary>
    public int BoxCount { get; init; }

    /// <summary>
    /// Gets the last arranged view size.
    /// </summary>
    public Size ViewSize { get; init; }
}
