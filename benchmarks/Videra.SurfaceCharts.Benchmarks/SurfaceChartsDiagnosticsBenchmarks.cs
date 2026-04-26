using System.Text;
using BenchmarkDotNet.Attributes;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Benchmarks;

[MemoryDiagnoser]
public class SurfaceChartsDiagnosticsBenchmarks
{
    private SurfaceChartRenderSnapshot _snapshot = null!;
    private SurfaceChartRenderingStatus _status = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _snapshot = new SurfaceChartRenderSnapshot
        {
            ActiveBackend = SurfaceChartRenderBackendKind.Software,
            IsReady = true,
            IsFallback = true,
            FallbackReason = "gpu init failed",
            UsesNativeSurface = false,
            ResidentTileCount = 12
        };

        _status = SurfaceChartRenderingStatus.FromSnapshot(_snapshot);
    }

    [Benchmark]
    public SurfaceChartRenderingStatus RenderingStatus_FromSnapshot()
    {
        return SurfaceChartRenderingStatus.FromSnapshot(_snapshot);
    }

    [Benchmark]
    public int SupportSummaryFormatting()
    {
        var builder = new StringBuilder();
        builder.AppendLine("SurfaceCharts support summary");
        builder.AppendLine("RenderingStatus:");
        builder.AppendLine($"  ActiveBackend: {_status.ActiveBackend}");
        builder.AppendLine($"  IsReady: {_status.IsReady}");
        builder.AppendLine($"  IsFallback: {_status.IsFallback}");
        builder.AppendLine($"  FallbackReason: {_status.FallbackReason ?? "<none>"}");
        builder.AppendLine($"  UsesNativeSurface: {_status.UsesNativeSurface}");
        builder.AppendLine($"  ResidentTileCount: {_status.ResidentTileCount}");
        return builder.ToString().Length;
    }
}
