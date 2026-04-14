namespace Videra.SurfaceCharts.Rendering;

public sealed record SurfaceChartRenderSnapshot
{
    public SurfaceChartRenderBackendKind ActiveBackend { get; init; } = SurfaceChartRenderBackendKind.Software;

    public bool IsReady { get; init; }

    public bool IsFallback { get; init; }

    public string? FallbackReason { get; init; }

    public bool UsesNativeSurface { get; init; }

    public int ResidentTileCount { get; init; }
}
