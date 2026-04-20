using BenchmarkDotNet.Attributes;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Benchmarks;

[MemoryDiagnoser]
public class SurfaceChartsCacheBenchmarks : SurfaceChartsBenchmarkBase
{
    [Benchmark]
    public Task<IReadOnlyList<SurfaceTile?>> ReadBatchFromCache()
    {
        return Context.CacheReader.LoadTilesAsync(Context.CacheBatchKeys).AsTask();
    }

    [Benchmark]
    public async Task<int> LookupCacheMissBurst()
    {
        var tiles = await Context.CacheReader.LoadTilesAsync(Context.CacheLookupMissKeys).ConfigureAwait(false);
        return tiles.Count(static tile => tile is null);
    }
}
