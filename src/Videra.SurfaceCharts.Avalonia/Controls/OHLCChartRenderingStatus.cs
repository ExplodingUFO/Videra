using Avalonia;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes the current readiness and interaction state for OHLC rendering.
/// </summary>
public sealed record OHLCChartRenderingStatus
{
    /// <summary>
    /// Gets whether the view currently has OHLC data.
    /// </summary>
    public bool HasSource { get; init; }

    /// <summary>
    /// Gets whether the view has enough state to render an OHLC scene.
    /// </summary>
    public bool IsReady { get; init; }

    /// <summary>
    /// Gets the renderer path chosen by the control.
    /// </summary>
    public SurfaceChartRenderBackendKind BackendKind { get; init; } = SurfaceChartRenderBackendKind.Software;

    /// <summary>
    /// Gets the number of bars.
    /// </summary>
    public int BarCount { get; init; }

    /// <summary>
    /// Gets the OHLC style.
    /// </summary>
    public OHLCStyle Style { get; init; }

    /// <summary>
    /// Gets the last arranged view size.
    /// </summary>
    public Size ViewSize { get; init; }
}
