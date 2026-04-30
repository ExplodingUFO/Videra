using Avalonia;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes the current readiness state for vector field chart rendering.
/// </summary>
public sealed record VectorFieldChartRenderingStatus
{
    /// <summary>
    /// Gets whether the view currently has vector field data.
    /// </summary>
    public bool HasSource { get; init; }

    /// <summary>
    /// Gets whether the view has enough state to render arrows.
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
    /// Gets the number of arrows in the current scene.
    /// </summary>
    public int ArrowCount { get; init; }

    /// <summary>
    /// Gets the magnitude range of the vector field data.
    /// </summary>
    public SurfaceValueRange MagnitudeRange { get; init; }

    /// <summary>
    /// Gets the last arranged view size.
    /// </summary>
    public Size ViewSize { get; init; }
}
