using Avalonia;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes the current readiness and interaction state for polygon rendering.
/// </summary>
public sealed record PolygonChartRenderingStatus
{
    public bool HasSource { get; init; }
    public bool IsReady { get; init; }
    public SurfaceChartRenderBackendKind BackendKind { get; init; } = SurfaceChartRenderBackendKind.Software;
    public int VertexCount { get; init; }
    public Size ViewSize { get; init; }
}
