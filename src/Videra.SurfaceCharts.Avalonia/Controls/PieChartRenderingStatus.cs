using Avalonia;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes the current readiness and interaction state for pie rendering.
/// </summary>
public sealed record PieChartRenderingStatus
{
    /// <summary>
    /// Gets whether the view currently has pie data.
    /// </summary>
    public bool HasSource { get; init; }

    /// <summary>
    /// Gets whether the view has enough state to render a pie scene.
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
    /// Gets the number of slices.
    /// </summary>
    public int SliceCount { get; init; }

    /// <summary>
    /// Gets the donut hole ratio.
    /// </summary>
    public double HoleRatio { get; init; }

    /// <summary>
    /// Gets the last arranged view size.
    /// </summary>
    public Size ViewSize { get; init; }
}
