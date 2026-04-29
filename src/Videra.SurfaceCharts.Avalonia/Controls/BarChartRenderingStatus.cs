using Avalonia;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes the current readiness and interaction state for bar chart rendering.
/// </summary>
public sealed record BarChartRenderingStatus
{
    /// <summary>
    /// Gets whether the view currently has bar data.
    /// </summary>
    public bool HasSource { get; init; }

    /// <summary>
    /// Gets whether the view has enough state to render a bar scene.
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
    /// Gets the number of bar series in the current source.
    /// </summary>
    public int SeriesCount { get; init; }

    /// <summary>
    /// Gets the number of categories in the current source.
    /// </summary>
    public int CategoryCount { get; init; }

    /// <summary>
    /// Gets the total number of bars across all series.
    /// </summary>
    public int BarCount { get; init; }

    /// <summary>
    /// Gets the bar layout mode.
    /// </summary>
    public BarChartLayout Layout { get; init; }

    /// <summary>
    /// Gets the last arranged view size.
    /// </summary>
    public Size ViewSize { get; init; }
}
