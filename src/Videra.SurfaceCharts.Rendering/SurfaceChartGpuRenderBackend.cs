using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering;

public sealed class SurfaceChartGpuRenderBackend : ISurfaceChartRenderBackend, IDisposable
{
    private readonly IGraphicsBackend _graphicsBackend;
    private readonly Dictionary<SurfaceTileKey, SurfaceChartGpuResidentTile> _tileResources = [];
    private readonly SurfacePatchTopologyCache _topologyCache = new();
    private readonly SurfaceColorMapUploadCache _colorMapUploadCache = new();
    private IBuffer? _frameUniformBuffer;
    private IPipeline? _pipeline;
    private IntPtr _currentHandle;
    private int _pixelWidth;
    private int _pixelHeight;

    public SurfaceChartGpuRenderBackend(IGraphicsBackend graphicsBackend, SurfaceRenderer? renderer = null)
    {
        _graphicsBackend = graphicsBackend ?? throw new ArgumentNullException(nameof(graphicsBackend));
    }

    public SurfaceChartRenderBackendKind Kind => SurfaceChartRenderBackendKind.Gpu;

    public bool UsesNativeSurface => true;

    public SurfaceRenderScene? SoftwareScene { get; private set; }

    public SurfaceChartRenderSnapshot Render(
        SurfaceChartRenderInputs inputs,
        SurfaceChartRenderState state,
        SurfaceChartRenderChangeSet changeSet)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(changeSet);

        if (changeSet.FullResetRequired && state.ResidentTileCount == 0)
        {
            DisposeTileResources();
            SoftwareScene = null;
        }

        if (!inputs.HandleBound
            || inputs.NativeHandle == IntPtr.Zero
            || state.Metadata is null
            || state.ResidentTileCount == 0
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
        EnsureSharedResources();

        UpdateTileResources(inputs, state, changeSet);
        SoftwareScene = null;
        RenderFrame(inputs, state);

        return new SurfaceChartRenderSnapshot
        {
            ActiveBackend = Kind,
            IsReady = true,
            IsFallback = false,
            FallbackReason = null,
            UsesNativeSurface = UsesNativeSurface,
            ResidentTileCount = state.ResidentTileCount,
        };
    }

    public void Dispose()
    {
        DisposeTileResources();
        _topologyCache.Dispose();
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

    private void EnsureSharedResources()
    {
        var resourceFactory = _graphicsBackend.GetResourceFactory();
        _pipeline ??= resourceFactory.CreatePipeline(VertexPositionNormalColor.SizeInBytes, hasNormals: true, hasColors: true);
        _frameUniformBuffer ??= resourceFactory.CreateUniformBuffer(32);
    }

    private void UpdateTileResources(
        SurfaceChartRenderInputs inputs,
        SurfaceChartRenderState state,
        SurfaceChartRenderChangeSet changeSet)
    {
        if (inputs.ColorMap is null)
        {
            return;
        }

        if (changeSet.FullResetRequired)
        {
            DisposeTileResources();
            foreach (var residentTile in state.ResidentTiles)
            {
                AddOrReplaceTileResources(residentTile, inputs.ColorMap);
            }

            return;
        }

        foreach (var removedKey in changeSet.RemovedResidentKeys)
        {
            if (_tileResources.Remove(removedKey, out var tileResources))
            {
                tileResources.Dispose();
            }
        }

        foreach (var addedKey in changeSet.AddedResidentKeys)
        {
            if (state.TryGetResidentTile(addedKey, out var residentTile))
            {
                AddOrReplaceTileResources(residentTile, inputs.ColorMap);
            }
        }

        if (!changeSet.ColorDirty)
        {
            return;
        }

        var colorMapLut = _colorMapUploadCache.Resolve(inputs.ColorMap);
        foreach (var residentTile in state.ResidentTiles)
        {
            if (_tileResources.TryGetValue(residentTile.Key, out var tileResources))
            {
                tileResources.UpdateColors(residentTile.SampleValues, colorMapLut);
            }
        }
    }

    private void AddOrReplaceTileResources(SurfaceChartResidentTile residentTile, SurfaceColorMap colorMap)
    {
        if (_tileResources.Remove(residentTile.Key, out var previousResources))
        {
            previousResources.Dispose();
        }

        var resourceFactory = _graphicsBackend.GetResourceFactory();
        var renderTile = residentTile.SoftwareRenderTile;
        var colorMapLut = _colorMapUploadCache.Resolve(colorMap);
        var vertices = BuildVertices(residentTile, renderTile, colorMapLut);
        var vertexBuffer = resourceFactory.CreateVertexBuffer(vertices);
        var topology = _topologyCache.Acquire(renderTile.Geometry, resourceFactory);
        _tileResources[residentTile.Key] = new SurfaceChartGpuResidentTile(
            residentTile.Key,
            vertices,
            vertexBuffer,
            (uint)(vertices.Length * VertexPositionNormalColor.SizeInBytes),
            topology);
    }

    private void RenderFrame(SurfaceChartRenderInputs inputs, SurfaceChartRenderState state)
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

        foreach (var residentTile in state.ResidentTiles)
        {
            if (!_tileResources.TryGetValue(residentTile.Key, out var tileResource))
            {
                continue;
            }

            if (tileResource.Topology.IndexCount == 0)
            {
                continue;
            }

            executor.SetVertexBuffer(tileResource.VertexBuffer);
            executor.SetIndexBuffer(tileResource.Topology.IndexBuffer);
            executor.DrawIndexed(tileResource.Topology.IndexCount);
        }

        executor.ResetDepthState();
        _graphicsBackend.EndFrame();
    }

    private void DisposeTileResources()
    {
        foreach (var tileResource in _tileResources.Values)
        {
            tileResource.Dispose();
        }

        _tileResources.Clear();
    }

    private static VertexPositionNormalColor[] BuildVertices(
        SurfaceChartResidentTile residentTile,
        SurfaceRenderTile renderTile,
        SurfaceColorMapLut colorMapLut)
    {
        var vertices = new VertexPositionNormalColor[renderTile.Vertices.Count];
        for (var index = 0; index < renderTile.Vertices.Count; index++)
        {
            var vertex = renderTile.Vertices[index];
            vertices[index] = new VertexPositionNormalColor(
                vertex.Position,
                Vector3.UnitY,
                ToRgbaFloat(colorMapLut.Map(residentTile.SampleValues[index])));
        }

        return vertices;
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

}
