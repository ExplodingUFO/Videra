namespace Videra.SurfaceCharts.Rendering;

public sealed record SurfaceChartRenderingStatus
{
    public SurfaceChartRenderBackendKind ActiveBackend { get; init; } = SurfaceChartRenderBackendKind.Software;

    public bool IsReady { get; init; }

    public bool IsFallback { get; init; }

    public string? FallbackReason { get; init; }

    public bool UsesNativeSurface { get; init; }

    public int ResidentTileCount { get; init; }

    public int VisibleTileCount { get; init; }

    public long ResidentTileBytes { get; init; }

    public static SurfaceChartRenderingStatus FromSnapshot(SurfaceChartRenderSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return new SurfaceChartRenderingStatus
        {
            ActiveBackend = snapshot.ActiveBackend,
            IsReady = snapshot.IsReady,
            IsFallback = snapshot.IsFallback,
            FallbackReason = snapshot.FallbackReason,
            UsesNativeSurface = snapshot.UsesNativeSurface,
            ResidentTileCount = snapshot.ResidentTileCount,
            VisibleTileCount = snapshot.VisibleTileCount,
            ResidentTileBytes = snapshot.ResidentTileBytes,
        };
    }
}
