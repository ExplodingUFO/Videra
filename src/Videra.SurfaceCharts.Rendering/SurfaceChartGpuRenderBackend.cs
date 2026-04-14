using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering;

public sealed class SurfaceChartGpuRenderBackend : ISurfaceChartRenderBackend, IDisposable
{
    private readonly IGraphicsBackend _graphicsBackend;
    private readonly SurfaceRenderer _renderer;
    private readonly List<SurfaceChartGpuTileResources> _tileResources = [];
    private IBuffer? _frameUniformBuffer;
    private IPipeline? _pipeline;
    private IntPtr _currentHandle;
    private int _pixelWidth;
    private int _pixelHeight;

    public SurfaceChartGpuRenderBackend(IGraphicsBackend graphicsBackend, SurfaceRenderer? renderer = null)
    {
        _graphicsBackend = graphicsBackend ?? throw new ArgumentNullException(nameof(graphicsBackend));
        _renderer = renderer ?? new SurfaceRenderer();
    }

    public SurfaceChartRenderBackendKind Kind => SurfaceChartRenderBackendKind.Gpu;

    public bool UsesNativeSurface => true;

    public SurfaceRenderScene? SoftwareScene { get; private set; }

    public SurfaceChartRenderSnapshot Render(SurfaceChartRenderInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (!inputs.HandleBound
            || inputs.NativeHandle == IntPtr.Zero
            || inputs.Metadata is null
            || inputs.ColorMap is null
            || inputs.LoadedTiles.Count == 0
            || inputs.ViewWidth <= 0d
            || inputs.ViewHeight <= 0d)
        {
            SoftwareScene = null;
            return new SurfaceChartRenderSnapshot
            {
                ActiveBackend = Kind,
                IsReady = false,
                IsFallback = false,
                FallbackReason = null,
                UsesNativeSurface = UsesNativeSurface,
                ResidentTileCount = 0,
            };
        }

        EnsureBackend(inputs);

        SoftwareScene = _renderer.BuildScene(inputs.Metadata, inputs.LoadedTiles, inputs.ColorMap);
        RebuildTileResources(SoftwareScene);
        RenderFrame(inputs);

        return new SurfaceChartRenderSnapshot
        {
            ActiveBackend = Kind,
            IsReady = true,
            IsFallback = false,
            FallbackReason = null,
            UsesNativeSurface = UsesNativeSurface,
            ResidentTileCount = SoftwareScene.Tiles.Count,
        };
    }

    public void Dispose()
    {
        DisposeTileResources();
        _frameUniformBuffer?.Dispose();
        _frameUniformBuffer = null;
        _pipeline?.Dispose();
        _pipeline = null;
        _graphicsBackend.Dispose();
    }

    private void EnsureBackend(SurfaceChartRenderInputs inputs)
    {
        var widthPx = Math.Max(1, (int)Math.Round(inputs.ViewWidth * inputs.RenderScale));
        var heightPx = Math.Max(1, (int)Math.Round(inputs.ViewHeight * inputs.RenderScale));

        if (!_graphicsBackend.IsInitialized || _currentHandle != inputs.NativeHandle)
        {
            _graphicsBackend.Initialize(inputs.NativeHandle, widthPx, heightPx);
            _currentHandle = inputs.NativeHandle;
            _pixelWidth = widthPx;
            _pixelHeight = heightPx;
            return;
        }

        if (_pixelWidth != widthPx || _pixelHeight != heightPx)
        {
            _graphicsBackend.Resize(widthPx, heightPx);
            _pixelWidth = widthPx;
            _pixelHeight = heightPx;
        }
    }

    private void RebuildTileResources(SurfaceRenderScene scene)
    {
        DisposeTileResources();

        var resourceFactory = _graphicsBackend.GetResourceFactory();
        _pipeline?.Dispose();
        _pipeline = resourceFactory.CreatePipeline(VertexPositionNormalColor.SizeInBytes, hasNormals: true, hasColors: true);
        foreach (var tile in scene.Tiles)
        {
            var vertices = new VertexPositionNormalColor[tile.Vertices.Count];
            for (var index = 0; index < tile.Vertices.Count; index++)
            {
                var vertex = tile.Vertices[index];
                vertices[index] = new VertexPositionNormalColor(
                    vertex.Position,
                    Vector3.UnitY,
                    ToRgbaFloat(vertex.Color));
            }

            var indices = tile.Geometry.Indices.ToArray();
            var vertexBuffer = resourceFactory.CreateVertexBuffer(vertices);
            var indexBuffer = resourceFactory.CreateIndexBuffer(indices);
            _tileResources.Add(new SurfaceChartGpuTileResources(vertexBuffer, indexBuffer, (uint)indices.Length));
        }

        _frameUniformBuffer?.Dispose();
        _frameUniformBuffer = resourceFactory.CreateUniformBuffer(32);
    }

    private void RenderFrame(SurfaceChartRenderInputs inputs)
    {
        var executor = _graphicsBackend.GetCommandExecutor();
        var viewportWidth = Math.Max(1f, (float)(inputs.ViewWidth * inputs.RenderScale));
        var viewportHeight = Math.Max(1f, (float)(inputs.ViewHeight * inputs.RenderScale));

        _frameUniformBuffer?.Update(new SurfaceChartGpuFrameUniform(
            new Vector4(viewportWidth, viewportHeight, inputs.RenderScale, 0f),
            new Vector4(
                (float)inputs.Viewport.StartX,
                (float)inputs.Viewport.StartY,
                (float)inputs.Viewport.Width,
                (float)inputs.Viewport.Height)));

        _graphicsBackend.SetClearColor(new Vector4(0.0625f, 0.0825f, 0.125f, 1f));
        _graphicsBackend.BeginFrame();
        executor.Clear(0.0625f, 0.0825f, 0.125f, 1f);
        executor.SetViewport(0f, 0f, viewportWidth, viewportHeight);
        executor.SetDepthState(testEnabled: true, writeEnabled: true);
        if (_pipeline is not null)
        {
            executor.SetPipeline(_pipeline);
        }

        foreach (var tileResource in _tileResources)
        {
            if (tileResource.IndexCount == 0)
            {
                continue;
            }

            executor.SetVertexBuffer(tileResource.VertexBuffer);
            executor.SetIndexBuffer(tileResource.IndexBuffer);
            executor.DrawIndexed(tileResource.IndexCount);
        }

        executor.ResetDepthState();
        _graphicsBackend.EndFrame();
    }

    private void DisposeTileResources()
    {
        foreach (var tileResource in _tileResources)
        {
            tileResource.Dispose();
        }

        _tileResources.Clear();
    }

    private static RgbaFloat ToRgbaFloat(uint argb)
    {
        var a = ((argb >> 24) & 0xFF) / 255f;
        var r = ((argb >> 16) & 0xFF) / 255f;
        var g = ((argb >> 8) & 0xFF) / 255f;
        var b = (argb & 0xFF) / 255f;
        return new RgbaFloat(r, g, b, a);
    }

    private readonly record struct SurfaceChartGpuFrameUniform(Vector4 ViewParameters, Vector4 ViewportParameters);

    private sealed class SurfaceChartGpuTileResources : IDisposable
    {
        public SurfaceChartGpuTileResources(IBuffer vertexBuffer, IBuffer indexBuffer, uint indexCount)
        {
            VertexBuffer = vertexBuffer ?? throw new ArgumentNullException(nameof(vertexBuffer));
            IndexBuffer = indexBuffer ?? throw new ArgumentNullException(nameof(indexBuffer));
            IndexCount = indexCount;
        }

        public IBuffer VertexBuffer { get; }

        public IBuffer IndexBuffer { get; }

        public uint IndexCount { get; }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}
