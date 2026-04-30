using Avalonia;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes the current readiness and interaction state for function plot rendering.
/// </summary>
public sealed record FunctionPlotChartRenderingStatus
{
    /// <summary>
    /// Gets whether the view currently has function plot data.
    /// </summary>
    public bool HasSource { get; init; }

    /// <summary>
    /// Gets whether the view has enough state to render a function plot scene.
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
    /// Gets the number of sample points.
    /// </summary>
    public int SampleCount { get; init; }

    /// <summary>
    /// Gets the domain range.
    /// </summary>
    public double XMin { get; init; }

    /// <summary>
    /// Gets the domain range.
    /// </summary>
    public double XMax { get; init; }

    /// <summary>
    /// Gets the last arranged view size.
    /// </summary>
    public Size ViewSize { get; init; }
}
