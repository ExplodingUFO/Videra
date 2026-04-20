using System.Numerics;
using BenchmarkDotNet.Attributes;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Videra.SurfaceCharts.Processing;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Benchmarks;

public sealed class SurfaceChartsBenchmarkContext : IDisposable
{
    private static readonly SurfaceTileKey OverviewKey = new(0, 0, 0, 0);

    private SurfaceChartsBenchmarkContext(
        SurfaceMatrix matrix,
        SurfaceViewportRequest viewportRequest,
        SurfaceCameraFrame cameraFrame,
        SurfaceLodPolicy lodPolicy,
        SurfacePyramidBuilder pyramidBuilder,
        SurfaceColorMap baseColorMap,
        SurfaceColorMap replacementColorMap,
        SurfaceChartRenderInputs renderInputs,
        SurfaceChartRenderInputs replacementColorRenderInputs,
        SurfaceChartRenderInputs orbitRenderInputs,
        SurfaceChartRenderInputs overviewRenderInputs,
        SurfaceChartRenderInputs focusedDetailRenderInputs,
        SurfaceChartRenderInputs gpuRenderInputs,
        SurfaceChartRenderInputs replacementColorGpuRenderInputs,
        SurfaceChartRenderInputs orbitGpuRenderInputs,
        SurfaceChartRenderInputs resizedGpuRenderInputs,
        SurfaceChartRenderInputs reboundGpuRenderInputs,
        IReadOnlyList<SurfaceTile> detailTiles,
        IReadOnlyList<SurfaceTileKey> cacheBatchKeys,
        IReadOnlyList<SurfaceTileKey> cacheLookupMissKeys,
        SurfaceCacheReader cacheReader,
        SurfacePickRay probeRay,
        string cacheDirectory)
    {
        Matrix = matrix;
        ViewportRequest = viewportRequest;
        CameraFrame = cameraFrame;
        LodPolicy = lodPolicy;
        PyramidBuilder = pyramidBuilder;
        BaseColorMap = baseColorMap;
        ReplacementColorMap = replacementColorMap;
        RenderInputs = renderInputs;
        ReplacementColorRenderInputs = replacementColorRenderInputs;
        OrbitRenderInputs = orbitRenderInputs;
        OverviewRenderInputs = overviewRenderInputs;
        FocusedDetailRenderInputs = focusedDetailRenderInputs;
        GpuRenderInputs = gpuRenderInputs;
        ReplacementColorGpuRenderInputs = replacementColorGpuRenderInputs;
        OrbitGpuRenderInputs = orbitGpuRenderInputs;
        ResizedGpuRenderInputs = resizedGpuRenderInputs;
        ReboundGpuRenderInputs = reboundGpuRenderInputs;
        DetailTiles = detailTiles;
        CacheBatchKeys = cacheBatchKeys;
        CacheLookupMissKeys = cacheLookupMissKeys;
        CacheReader = cacheReader;
        ProbeRay = probeRay;
        CacheDirectory = cacheDirectory;
    }

    public SurfaceMatrix Matrix { get; }

    public SurfaceViewportRequest ViewportRequest { get; }

    public SurfaceCameraFrame CameraFrame { get; }

    public SurfaceLodPolicy LodPolicy { get; }

    public SurfacePyramidBuilder PyramidBuilder { get; }

    public SurfaceColorMap BaseColorMap { get; }

    public SurfaceColorMap ReplacementColorMap { get; }

    public SurfaceChartRenderInputs RenderInputs { get; }

    public SurfaceChartRenderInputs ReplacementColorRenderInputs { get; }

    public SurfaceChartRenderInputs OrbitRenderInputs { get; }

    public SurfaceChartRenderInputs OverviewRenderInputs { get; }

    public SurfaceChartRenderInputs FocusedDetailRenderInputs { get; }

    public SurfaceChartRenderInputs GpuRenderInputs { get; }

    public SurfaceChartRenderInputs ReplacementColorGpuRenderInputs { get; }

    public SurfaceChartRenderInputs OrbitGpuRenderInputs { get; }

    public SurfaceChartRenderInputs ResizedGpuRenderInputs { get; }

    public SurfaceChartRenderInputs ReboundGpuRenderInputs { get; }

    public IReadOnlyList<SurfaceTile> DetailTiles { get; }

    public IReadOnlyList<SurfaceTileKey> CacheBatchKeys { get; }

    public IReadOnlyList<SurfaceTileKey> CacheLookupMissKeys { get; }

    public SurfaceCacheReader CacheReader { get; }

    public SurfacePickRay ProbeRay { get; }

    public string CacheDirectory { get; }

    public void Dispose()
    {
        if (Directory.Exists(CacheDirectory))
        {
            Directory.Delete(CacheDirectory, recursive: true);
        }
    }

    public static async Task<SurfaceChartsBenchmarkContext> CreateAsync()
    {
        var lodPolicy = SurfaceLodPolicy.Default;
        var pyramidBuilder = new SurfacePyramidBuilder(maxTileWidth: 128, maxTileHeight: 128);
        var metadata = CreateMetadata(width: 1024, height: 1024);
        var gpuMetadata = CreateMetadata(width: 256, height: 256);
        var viewport = new SurfaceViewport(256, 256, 512, 512);
        var viewportRequest = new SurfaceViewportRequest(
            metadata,
            viewport,
            outputWidth: 128,
            outputHeight: 128);
        var dataWindow = viewportRequest.ClampedViewport.ToDataWindow();
        var defaultViewState = SurfaceViewState.CreateDefault(metadata, dataWindow);
        var baseCameraFrame = SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            defaultViewState,
            viewportRequest.OutputWidth,
            viewportRequest.OutputHeight,
            1f);

        var matrix = CreateMatrix(metadata.Width, metadata.Height);
        var baseColorMap = new SurfaceColorMap(metadata.ValueRange, new SurfaceColorMapPalette(0xFF203040u, 0xFFE0F0FFu));
        var replacementColorMap = new SurfaceColorMap(metadata.ValueRange, new SurfaceColorMapPalette(0xFF804020u, 0xFF20C0F0u));
        var detailTiles = new[]
        {
            CreateSurfaceTile(metadata, new SurfaceTileKey(1, 1, 0, 0), 0.5f),
            CreateSurfaceTile(metadata, new SurfaceTileKey(1, 1, 1, 0), 1.0f),
            CreateSurfaceTile(metadata, new SurfaceTileKey(1, 1, 0, 1), 1.5f),
            CreateSurfaceTile(metadata, new SurfaceTileKey(1, 1, 1, 1), 2.0f),
        };
        var overviewTile = CreateSurfaceTile(metadata, OverviewKey, 0.25f);
        var gpuDetailTiles = new[]
        {
            CreateSurfaceTile(gpuMetadata, new SurfaceTileKey(1, 1, 0, 0), 0.5f),
            CreateSurfaceTile(gpuMetadata, new SurfaceTileKey(1, 1, 1, 0), 1.0f),
            CreateSurfaceTile(gpuMetadata, new SurfaceTileKey(1, 1, 0, 1), 1.5f),
            CreateSurfaceTile(gpuMetadata, new SurfaceTileKey(1, 1, 1, 1), 2.0f),
        };
        var gpuViewport = new SurfaceViewport(64, 64, 128, 128);
        var gpuViewportRequest = new SurfaceViewportRequest(
            gpuMetadata,
            gpuViewport,
            outputWidth: 128,
            outputHeight: 128);
        var gpuDataWindow = gpuViewportRequest.ClampedViewport.ToDataWindow();
        var gpuDefaultViewState = SurfaceViewState.CreateDefault(gpuMetadata, gpuDataWindow);
        var gpuBaseCameraFrame = SurfaceProjectionMath.CreateCameraFrame(
            gpuMetadata,
            gpuDefaultViewState,
            gpuViewportRequest.OutputWidth,
            gpuViewportRequest.OutputHeight,
            1f);

        var orbitCamera = new SurfaceCameraPose(
            defaultViewState.Camera.Target,
            defaultViewState.Camera.YawDegrees + 30d,
            defaultViewState.Camera.PitchDegrees + 16d,
            defaultViewState.Camera.Distance,
            defaultViewState.Camera.FieldOfViewDegrees);
        var orbitViewState = new SurfaceViewState(dataWindow, orbitCamera);
        var orbitCameraFrame = SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            orbitViewState,
            viewportRequest.OutputWidth,
            viewportRequest.OutputHeight,
            1f);

        var focusedWindow = new SurfaceDataWindow(320d, 320d, 256d, 256d);
        var focusedViewState = SurfaceViewState.CreateDefault(metadata, focusedWindow);
        var focusedCameraFrame = SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            focusedViewState,
            viewportRequest.OutputWidth,
            viewportRequest.OutputHeight,
            1f);
        var gpuOrbitCamera = new SurfaceCameraPose(
            gpuDefaultViewState.Camera.Target,
            gpuDefaultViewState.Camera.YawDegrees + 30d,
            gpuDefaultViewState.Camera.PitchDegrees + 16d,
            gpuDefaultViewState.Camera.Distance,
            gpuDefaultViewState.Camera.FieldOfViewDegrees);
        var gpuOrbitViewState = new SurfaceViewState(gpuDataWindow, gpuOrbitCamera);
        var gpuOrbitCameraFrame = SurfaceProjectionMath.CreateCameraFrame(
            gpuMetadata,
            gpuOrbitViewState,
            gpuViewportRequest.OutputWidth,
            gpuViewportRequest.OutputHeight,
            1f);

        var renderInputs = CreateInputs(
            metadata,
            detailTiles,
            baseColorMap,
            defaultViewState,
            baseCameraFrame);
        var replacementColorRenderInputs = renderInputs with
        {
            ColorMap = replacementColorMap,
        };
        var orbitRenderInputs = renderInputs with
        {
            ViewState = orbitViewState,
            CameraFrame = orbitCameraFrame,
        };
        var overviewRenderInputs = CreateInputs(
            metadata,
            [overviewTile],
            baseColorMap,
            defaultViewState,
            baseCameraFrame);
        var focusedDetailRenderInputs = CreateInputs(
            metadata,
            detailTiles,
            baseColorMap,
            focusedViewState,
            focusedCameraFrame);

        var gpuRenderInputs = CreateInputs(
            gpuMetadata,
            gpuDetailTiles,
            baseColorMap,
            gpuDefaultViewState,
            gpuBaseCameraFrame) with
        {
            NativeHandle = new IntPtr(0x1234),
            HandleBound = true,
        };
        var replacementColorGpuRenderInputs = gpuRenderInputs with
        {
            ColorMap = replacementColorMap,
        };
        var orbitGpuRenderInputs = gpuRenderInputs with
        {
            ViewState = gpuOrbitViewState,
            CameraFrame = gpuOrbitCameraFrame,
        };
        var resizedGpuRenderInputs = gpuRenderInputs with
        {
            ViewWidth = 192,
            ViewHeight = 160,
        };
        var reboundGpuRenderInputs = orbitGpuRenderInputs with
        {
            ViewWidth = resizedGpuRenderInputs.ViewWidth,
            ViewHeight = resizedGpuRenderInputs.ViewHeight,
            NativeHandle = new IntPtr(0x5678),
        };

        var probeCameraFrame = renderInputs.CameraFrame ?? baseCameraFrame;
        var probeRay = SurfaceHeightfieldPicker.CreatePickRay(
            new Vector2((float)(renderInputs.ViewWidth * 0.5d), (float)(renderInputs.ViewHeight * 0.5d)),
            probeCameraFrame);

        var cacheBatchKeys = new[]
        {
            OverviewKey,
            new SurfaceTileKey(3, 3, 2, 2),
            new SurfaceTileKey(3, 3, 3, 2),
            new SurfaceTileKey(3, 3, 2, 3),
            new SurfaceTileKey(3, 3, 3, 3),
        };
        var cacheLookupMissKeys = Enumerable.Range(0, 16)
            .Select(index => new SurfaceTileKey(6, 6, index % 8, 8 + (index / 8)))
            .ToArray();

        var cacheDirectory = Path.Combine(
            Path.GetTempPath(),
            "Videra.SurfaceCharts.Benchmarks",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(cacheDirectory);

        var cachePath = Path.Combine(cacheDirectory, "surface-cache.json");
        await SurfaceCacheWriter.WriteAsync(cachePath, pyramidBuilder.Build(matrix), cacheBatchKeys).ConfigureAwait(false);
        var cacheReader = await SurfaceCacheReader.ReadAsync(cachePath).ConfigureAwait(false);

        return new SurfaceChartsBenchmarkContext(
            matrix,
            viewportRequest,
            baseCameraFrame,
            lodPolicy,
            pyramidBuilder,
            baseColorMap,
            replacementColorMap,
            renderInputs,
            replacementColorRenderInputs,
            orbitRenderInputs,
            overviewRenderInputs,
            focusedDetailRenderInputs,
            gpuRenderInputs,
            replacementColorGpuRenderInputs,
            orbitGpuRenderInputs,
            resizedGpuRenderInputs,
            reboundGpuRenderInputs,
            detailTiles,
            cacheBatchKeys,
            cacheLookupMissKeys,
            cacheReader,
            probeRay,
            cacheDirectory);
    }

    private static SurfaceChartRenderInputs CreateInputs(
        SurfaceMetadata metadata,
        IReadOnlyList<SurfaceTile> loadedTiles,
        SurfaceColorMap colorMap,
        SurfaceViewState viewState,
        SurfaceCameraFrame cameraFrame)
    {
        return new SurfaceChartRenderInputs
        {
            Metadata = metadata,
            LoadedTiles = loadedTiles,
            ColorMap = colorMap,
            ViewState = viewState,
            CameraFrame = cameraFrame,
            ViewWidth = 128,
            ViewHeight = 128,
            RenderScale = 1f,
            NativeHandle = IntPtr.Zero,
            HandleBound = false,
        };
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

public abstract class SurfaceChartsBenchmarkBase
{
    protected SurfaceChartsBenchmarkContext Context { get; private set; } = null!;

    [GlobalSetup]
    public async Task GlobalSetupAsync()
    {
        Context = await SurfaceChartsBenchmarkContext.CreateAsync().ConfigureAwait(false);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        Context.Dispose();
    }
}
