using Avalonia;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes the current readiness and interaction state for violin rendering.
/// </summary>
public sealed record ViolinChartRenderingStatus
{
    public bool HasSource { get; init; }
    public bool IsReady { get; init; }
    public SurfaceChartRenderBackendKind BackendKind { get; init; } = SurfaceChartRenderBackendKind.Software;
    public int GroupCount { get; init; }
    public Size ViewSize { get; init; }
}
