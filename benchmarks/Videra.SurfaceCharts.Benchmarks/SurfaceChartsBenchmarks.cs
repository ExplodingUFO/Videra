using BenchmarkDotNet.Attributes;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Processing;

namespace Videra.SurfaceCharts.Benchmarks;

[MemoryDiagnoser]
public sealed class SurfaceChartsBenchmarks
{
    private static readonly SurfaceTileKey OverviewKey = new(0, 0, 0, 0);
    private static readonly SurfaceTileKey DetailKeyA = new(3, 3, 2, 2);
    private static readonly SurfaceTileKey DetailKeyB = new(3, 3, 3, 2);
    private static readonly SurfaceTileKey DetailKeyC = new(3, 3, 2, 3);
    private static readonly SurfaceTileKey DetailKeyD = new(3, 3, 3, 3);

    private readonly SurfaceLodPolicy lodPolicy = SurfaceLodPolicy.Default;
    private readonly SurfacePyramidBuilder pyramidBuilder = new(maxTileWidth: 128, maxTileHeight: 128);

    private SurfaceMatrix matrix = null!;
    private SurfaceViewportRequest viewportRequest;
    private string cachePath = string.Empty;
    private SurfaceCacheReader cacheReader = null!;
    private IReadOnlyList<SurfaceTileKey> cacheBatchKeys = null!;

    public SurfaceChartsBenchmarks()
    {
        viewportRequest = new SurfaceViewportRequest(
            CreateMetadata(width: 1024, height: 1024),
            new SurfaceViewport(256, 256, 512, 512),
            outputWidth: 128,
            outputHeight: 128);
    }

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        matrix = CreateMatrix(width: 1024, height: 1024);
        cacheBatchKeys = new[] { OverviewKey, DetailKeyA, DetailKeyB, DetailKeyC, DetailKeyD };

        var cacheDirectory = Path.Combine(
            Path.GetTempPath(),
            "Videra.SurfaceCharts.Benchmarks",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(cacheDirectory);

        cachePath = Path.Combine(cacheDirectory, "surface-cache.json");
        await SurfaceCacheWriter.WriteAsync(cachePath, pyramidBuilder.Build(matrix), cacheBatchKeys).ConfigureAwait(false);
        cacheReader = await SurfaceCacheReader.ReadAsync(cachePath).ConfigureAwait(false);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        var cacheDirectory = Path.GetDirectoryName(cachePath);
        if (cacheDirectory is not null && Directory.Exists(cacheDirectory))
        {
            Directory.Delete(cacheDirectory, recursive: true);
        }
    }

    [Benchmark]
    public SurfaceTileKey[] SelectViewportTiles()
    {
        return lodPolicy.Select(viewportRequest).EnumerateTileKeys().ToArray();
    }

    [Benchmark]
    public Task<IReadOnlyList<SurfaceTile?>> ReadBatchFromCache()
    {
        return cacheReader.LoadTilesAsync(cacheBatchKeys).AsTask();
    }

    [Benchmark]
    public ISurfaceTileSource BuildPyramidWithStatistics()
    {
        return pyramidBuilder.Build(matrix);
    }

    private static SurfaceMatrix CreateMatrix(int width, int height)
    {
        var values = new float[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                values[(y * width) + x] = (float)(Math.Sin(x / 32d) + Math.Cos(y / 24d));
            }
        }

        return new SurfaceMatrix(CreateMetadata(width, height), values);
    }

    private static SurfaceMetadata CreateMetadata(int width, int height)
    {
        return new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("X", unit: null, minimum: 0, maximum: width - 1),
            new SurfaceAxisDescriptor("Y", unit: null, minimum: 0, maximum: height - 1),
            new SurfaceValueRange(-2, 2));
    }
}
