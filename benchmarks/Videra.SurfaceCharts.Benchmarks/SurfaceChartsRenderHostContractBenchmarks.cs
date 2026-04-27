using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using Videra.SurfaceCharts.Rendering;
using Videra.SurfaceCharts.Rendering.Software;

namespace Videra.SurfaceCharts.Benchmarks;

[MemoryDiagnoser]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "BenchmarkDotNet invokes IterationCleanup, which disposes per-iteration resources.")]
public class SurfaceChartsRenderHostContractBenchmarks : SurfaceChartsBenchmarkBase
{
    private BenchmarkGraphicsBackend _graphicsBackend = null!;
    private SurfaceChartRenderHost _host = null!;

    [IterationSetup]
    public void IterationSetup()
    {
        _graphicsBackend = new BenchmarkGraphicsBackend();
        _host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(_graphicsBackend),
            allowSoftwareFallback: false);
        EnsureGpuContractSnapshot(_host.UpdateInputs(Context.GpuRenderInputs), nameof(Context.GpuRenderInputs));
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _graphicsBackend.Dispose();
        _host = null!;
        _graphicsBackend = null!;
    }

    [Benchmark]
    public SurfaceChartRenderSnapshot RecolorResidentTilesGpuContractPath()
    {
        return EnsureGpuContractSnapshot(
            _host.UpdateInputs(Context.ReplacementColorGpuRenderInputs),
            nameof(Context.ReplacementColorGpuRenderInputs));
    }

    [Benchmark]
    public SurfaceChartRenderSnapshot OrbitInteractiveFrameGpuContractPath()
    {
        return EnsureGpuContractSnapshot(
            _host.UpdateInputs(Context.OrbitGpuRenderInputs),
            nameof(Context.OrbitGpuRenderInputs));
    }

    [Benchmark]
    public SurfaceChartRenderSnapshot ResizeAndRebindHandleGpuContractPath()
    {
        EnsureGpuContractSnapshot(
            _host.UpdateInputs(Context.ResizedGpuRenderInputs),
            nameof(Context.ResizedGpuRenderInputs));
        return EnsureGpuContractSnapshot(
            _host.UpdateInputs(Context.ReboundGpuRenderInputs),
            nameof(Context.ReboundGpuRenderInputs));
    }

    private static SurfaceChartRenderSnapshot EnsureGpuContractSnapshot(
        SurfaceChartRenderSnapshot snapshot,
        string scenario)
    {
        if (snapshot.ActiveBackend != SurfaceChartRenderBackendKind.Gpu
            || snapshot.IsFallback
            || !snapshot.UsesNativeSurface
            || !snapshot.IsReady)
        {
            throw new InvalidOperationException(
                $"Render-host contract benchmark scenario '{scenario}' must stay on the GPU contract path without fallback.");
        }

        return snapshot;
    }
}
