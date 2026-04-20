using BenchmarkDotNet.Attributes;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Processing;

namespace Videra.SurfaceCharts.Benchmarks;

[MemoryDiagnoser]
public class SurfaceChartsSelectionBenchmarks : SurfaceChartsBenchmarkBase
{
    [Benchmark]
    public SurfaceTileKey[] SelectViewportTiles()
    {
        return Context.LodPolicy.Select(Context.ViewportRequest).EnumerateTileKeys().ToArray();
    }

    [Benchmark]
    public SurfaceTileKey[] SelectCameraAwareViewportTiles()
    {
        return Context.LodPolicy.Select(Context.ViewportRequest, Context.CameraFrame).EnumerateTileKeys().ToArray();
    }

    [Benchmark]
    public ISurfaceTileSource BuildPyramidWithStatistics()
    {
        return Context.PyramidBuilder.Build(Context.Matrix);
    }
}
