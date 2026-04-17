using BenchmarkDotNet.Attributes;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Videra.SurfaceCharts.Processing;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Benchmarks;

[MemoryDiagnoser]
public class SurfaceChartsBenchmarks
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
    private SurfaceCameraFrame cameraFrame;
    private string cachePath = string.Empty;
    private SurfaceCacheReader cacheReader = null!;
    private IReadOnlyList<SurfaceTileKey> cacheBatchKeys = null!;
    private SurfaceChartRenderInputs renderInputs = null!;
    private SurfaceChartRenderInputs replacementColorRenderInputs = null!;

    public SurfaceChartsBenchmarks()
    {
        viewportRequest = new SurfaceViewportRequest(
            CreateMetadata(width: 1024, height: 1024),
            new SurfaceViewport(256, 256, 512, 512),
            outputWidth: 128,
            outputHeight: 128);
        var dataWindow = viewportRequest.ClampedViewport.ToDataWindow();
        var viewState = SurfaceViewState.CreateDefault(viewportRequest.Metadata, dataWindow);
        cameraFrame = SurfaceProjectionMath.CreateCameraFrame(
            viewportRequest.Metadata,
            viewState,
            viewportRequest.OutputWidth,
            viewportRequest.OutputHeight,
            1f);
    }

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        matrix = CreateMatrix(width: 1024, height: 1024);
        cacheBatchKeys = new[] { OverviewKey, DetailKeyA, DetailKeyB, DetailKeyC, DetailKeyD };
        SurfaceTile[] renderResidentTiles =
        [
            CreateSurfaceTile(viewportRequest.Metadata, new SurfaceTileKey(2, 2, 0, 0), 0.5f),
            CreateSurfaceTile(viewportRequest.Metadata, new SurfaceTileKey(2, 2, 1, 0), 1.0f),
            CreateSurfaceTile(viewportRequest.Metadata, new SurfaceTileKey(2, 2, 0, 1), 1.5f),
            CreateSurfaceTile(viewportRequest.Metadata, new SurfaceTileKey(2, 2, 1, 1), 2.0f),
        ];
        renderInputs = new SurfaceChartRenderInputs
        {
            Metadata = viewportRequest.Metadata,
            LoadedTiles = renderResidentTiles,
            ColorMap = new SurfaceColorMap(viewportRequest.Metadata.ValueRange, new SurfaceColorMapPalette(0xFF203040u, 0xFFE0F0FFu)),
            ViewState = SurfaceViewState.CreateDefault(viewportRequest.Metadata, viewportRequest.ClampedViewport.ToDataWindow()),
            CameraFrame = cameraFrame,
            ViewWidth = viewportRequest.OutputWidth,
            ViewHeight = viewportRequest.OutputHeight,
            RenderScale = 1f,
            NativeHandle = IntPtr.Zero,
            HandleBound = false,
        };
        replacementColorRenderInputs = renderInputs with
        {
            ColorMap = new SurfaceColorMap(viewportRequest.Metadata.ValueRange, new SurfaceColorMapPalette(0xFF804020u, 0xFF20C0F0u)),
        };

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
    public SurfaceTileKey[] SelectCameraAwareViewportTiles()
    {
        return lodPolicy.Select(viewportRequest, cameraFrame).EnumerateTileKeys().ToArray();
    }

    [Benchmark]
    public SurfaceChartRenderChangeSet BuildResidentRenderState()
    {
        var renderState = new SurfaceChartRenderState();
        return renderState.Update(renderInputs);
    }

    [Benchmark]
    public SurfaceChartRenderChangeSet ApplyColorMapChangeToResidentRenderState()
    {
        var renderState = new SurfaceChartRenderState();
        renderState.Update(renderInputs);
        return renderState.Update(replacementColorRenderInputs);
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

    private static SurfaceTile CreateSurfaceTile(SurfaceMetadata metadata, SurfaceTileKey key, float tileValue)
    {
        var tileCountX = 1 << key.LevelX;
        var tileCountY = 1 << key.LevelY;
        var startX = (metadata.Width * key.TileX) / tileCountX;
        var endX = (metadata.Width * (key.TileX + 1)) / tileCountX;
        var startY = (metadata.Height * key.TileY) / tileCountY;
        var endY = (metadata.Height * (key.TileY + 1)) / tileCountY;
        var tileWidth = endX - startX;
        var tileHeight = endY - startY;
        var values = new float[tileWidth * tileHeight];
        Array.Fill(values, tileValue);
        return new SurfaceTile(
            key,
            tileWidth,
            tileHeight,
            new SurfaceTileBounds(startX, startY, tileWidth, tileHeight),
            values,
            metadata.ValueRange);
    }
}
