using Avalonia;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes the current readiness and interaction state for histogram rendering.
/// </summary>
public sealed record HistogramChartRenderingStatus
{
    /// <summary>
    /// Gets whether the view currently has histogram data.
    /// </summary>
    public bool HasSource { get; init; }

    /// <summary>
    /// Gets whether the view has enough state to render a histogram scene.
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
    /// Gets the number of bins in the current histogram.
    /// </summary>
    public int BinCount { get; init; }

    /// <summary>
    /// Gets the histogram mode.
    /// </summary>
    public HistogramMode Mode { get; init; }

    /// <summary>
    /// Gets the number of source values.
    /// </summary>
    public int ValueCount { get; init; }

    /// <summary>
    /// Gets the last arranged view size.
    /// </summary>
    public Size ViewSize { get; init; }
}
