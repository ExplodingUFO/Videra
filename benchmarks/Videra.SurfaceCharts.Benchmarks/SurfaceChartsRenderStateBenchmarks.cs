using BenchmarkDotNet.Attributes;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Benchmarks;

[MemoryDiagnoser]
public class SurfaceChartsRenderStateBenchmarks : SurfaceChartsBenchmarkBase
{
    [Benchmark]
    public SurfaceChartRenderChangeSet BuildResidentRenderState()
    {
        var renderState = new SurfaceChartRenderState();
        return renderState.Update(Context.RenderInputs);
    }

    [Benchmark]
    public SurfaceChartRenderChangeSet ApplyColorMapChangeToResidentRenderState()
    {
        var renderState = new SurfaceChartRenderState();
        renderState.Update(Context.RenderInputs);
        return renderState.Update(Context.ReplacementColorRenderInputs);
    }

    [Benchmark]
    public int ApplyResidencyChurnUnderCameraMovement()
    {
        var renderState = new SurfaceChartRenderState();
        renderState.Update(Context.OverviewRenderInputs);
        var detailChange = renderState.Update(Context.FocusedDetailRenderInputs);
        var returnChange = renderState.Update(Context.OverviewRenderInputs);

        return detailChange.AddedResidentKeys.Count
            + detailChange.RemovedResidentKeys.Count
            + returnChange.AddedResidentKeys.Count
            + returnChange.RemovedResidentKeys.Count;
    }
}
