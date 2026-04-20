using BenchmarkDotNet.Attributes;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Benchmarks;

[MemoryDiagnoser]
public class SurfaceChartsProbeBenchmarks : SurfaceChartsBenchmarkBase
{
    [Benchmark]
    public SurfacePickHit? ProbeLatency()
    {
        return SurfaceHeightfieldPicker.Pick(
            Context.RenderInputs.Metadata!,
            Context.DetailTiles,
            Context.ProbeRay);
    }
}
