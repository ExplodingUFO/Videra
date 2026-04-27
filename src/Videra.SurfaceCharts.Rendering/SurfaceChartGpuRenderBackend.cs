using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;

namespace Videra.SurfaceCharts.Rendering;

public sealed class SurfaceChartGpuRenderBackend : ISurfaceChartRenderBackend, IDisposable
{
    private readonly IGraphicsBackend _graphicsBackend;
    private readonly Dictionary<SurfaceTileKey, SurfaceChartGpuResidentTile> _tileResources = [];
    private readonly SurfacePatchTopologyCache _topologyCache = new();
    private readonly SurfaceColorMapUploadCache _colorMapUploadCache = new();
    private readonly List<SurfaceChartGpuResidentTile> _pendingTileResourceReleases = [];
    private IBuffer? _cameraUniformBuffer;
    private IBuffer? _worldUniformBuffer;
    private IBuffer? _colorMapUniformBuffer;
    private bool _colorMapUniformInitialized;
    private SurfaceColorMap? _uploadedColorMap;
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
                VisibleTileCount = 0,
                ResidentTileBytes = 0,
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
            VisibleTileCount = state.ResidentTileCount,
            ResidentTileBytes = GetResidentTileBytes(),
        };
    }

    public void Dispose()
    {
        DisposeTileResources();
        DisposePendingTileResources();
        _topologyCache.Dispose();
        _cameraUniformBuffer?.Dispose();
        _cameraUniformBuffer = null;
        _worldUniformBuffer?.Dispose();
        _worldUniformBuffer = null;
        _colorMapUniformBuffer?.Dispose();
        _colorMapUniformBuffer = null;
        _colorMapUniformInitialized = false;
        _uploadedColorMap = null;
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

    private long GetResidentTileBytes()
    {
        return _tileResources.Values.Sum(static tile => (long)tile.ResidentBytes);
    }

    private void EnsureSharedResources()
    {
        var resourceFactory = _graphicsBackend.GetResourceFactory();
        _pipeline ??= resourceFactory.CreatePipeline(CreateSurfaceChartPipelineDescription());
        _cameraUniformBuffer ??= resourceFactory.CreateUniformBuffer(128);
        _worldUniformBuffer ??= resourceFactory.CreateUniformBuffer(64);
        if (_colorMapUniformBuffer is null)
        {
            _colorMapUniformBuffer = resourceFactory.CreateUniformBuffer(SurfaceChartGpuUniformPayloads.ColorMapBufferSize);
            _colorMapUniformInitialized = false;
            _uploadedColorMap = null;
        }
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
            UpdateColorMapUniform(inputs.ColorMap);
            RebuildTileResources(state);

            return;
        }

        if (changeSet.ResidencyDirty)
        {
            RebuildTileResources(state);
        }
        else
        {
            EnsureResidentTileResources(state);
        }

        if (!_colorMapUniformInitialized || !ReferenceEquals(_uploadedColorMap, inputs.ColorMap))
        {
            UpdateColorMapUniform(inputs.ColorMap);
        }
    }

    private void RebuildTileResources(SurfaceChartRenderState state)
    {
        DisposeTileResources();

        foreach (var residentTile in state.ResidentTiles)
        {
            AddOrReplaceTileResources(residentTile, state);
        }
    }

    private void AddOrReplaceTileResources(SurfaceChartResidentTile residentTile, SurfaceChartRenderState state)
    {
        if (_tileResources.Remove(residentTile.Key, out var previousResources))
        {
            ReleaseTileResources(previousResources);
        }

        var resourceFactory = _graphicsBackend.GetResourceFactory();
        var vertices = BuildVertices(residentTile, state);
        var vertexBuffer = resourceFactory.CreateVertexBuffer(vertices);
        var scalarPayload = SurfaceChartGpuUniformPayloads.CreateScalarPayload(residentTile.SampleValueMemory.Span);
        var scalarBuffer = resourceFactory.CreateUniformBuffer(SurfaceChartGpuUniformPayloads.ScalarBufferSize);
        scalarBuffer.UpdateArray(scalarPayload);
        var topology = _topologyCache.Acquire(residentTile.SoftwareRenderTile.Geometry, resourceFactory);
        _tileResources[residentTile.Key] = new SurfaceChartGpuResidentTile(
            residentTile.Key,
            vertices,
            vertexBuffer,
            (uint)(vertices.Length * VertexPositionNormalColor.SizeInBytes),
            scalarBuffer,
            SurfaceChartGpuUniformPayloads.ScalarBufferSize,
            topology);
    }

    private void RenderFrame(SurfaceChartRenderInputs inputs, SurfaceChartRenderState state)
    {
        var executor = _graphicsBackend.GetCommandExecutor();
        var viewportWidth = Math.Max(1f, (float)(inputs.ViewWidth * inputs.RenderScale));
        var viewportHeight = Math.Max(1f, (float)(inputs.ViewHeight * inputs.RenderScale));
        var cameraFrame = inputs.CameraFrame;

        _cameraUniformBuffer?.Update(new SurfaceChartGpuCameraUniform(
            cameraFrame?.ViewMatrix ?? Matrix4x4.Identity,
            cameraFrame?.ProjectionMatrix ?? Matrix4x4.Identity));
        _worldUniformBuffer?.Update(Matrix4x4.Identity);

        _graphicsBackend.SetClearColor(new Vector4(0.0625f, 0.0825f, 0.125f, 1f));
        _graphicsBackend.BeginFrame();
        FlushPendingTileResourceReleases(executor);
        executor.Clear(0.0625f, 0.0825f, 0.125f, 1f);
        executor.SetViewport(0f, 0f, viewportWidth, viewportHeight);
        executor.SetDepthState(testEnabled: true, writeEnabled: true);
        if (_pipeline is not null)
        {
            executor.SetPipeline(_pipeline);
        }

        if (_cameraUniformBuffer is not null)
        {
            executor.SetVertexBuffer(_cameraUniformBuffer, RenderBindingSlots.Camera);
        }

        if (_worldUniformBuffer is not null)
        {
            executor.SetVertexBuffer(_worldUniformBuffer, RenderBindingSlots.World);
        }

        if (_colorMapUniformBuffer is not null)
        {
            executor.SetVertexBuffer(_colorMapUniformBuffer, RenderBindingSlots.SurfaceColorMap);
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
            executor.SetVertexBuffer(tileResource.ScalarBuffer, RenderBindingSlots.SurfaceTileScalars);
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
            ReleaseTileResources(tileResource);
        }

        _tileResources.Clear();
    }

    private void EnsureResidentTileResources(SurfaceChartRenderState state)
    {
        foreach (var residentTile in state.ResidentTiles)
        {
            if (!_tileResources.ContainsKey(residentTile.Key))
            {
                AddOrReplaceTileResources(residentTile, state);
            }
        }
    }

    private void ReleaseTileResources(SurfaceChartGpuResidentTile tileResource)
    {
        if (_graphicsBackend.IsInitialized)
        {
            _pendingTileResourceReleases.Add(tileResource);
            return;
        }

        tileResource.Dispose();
    }

    private void UpdateColorMapUniform(SurfaceColorMap colorMap)
    {
        var colorMapLut = _colorMapUploadCache.Resolve(colorMap);
        _colorMapUniformBuffer?.UpdateArray(SurfaceChartGpuUniformPayloads.CreateColorMapPayload(colorMapLut));
        _colorMapUniformInitialized = true;
        _uploadedColorMap = colorMap;
    }

    private void FlushPendingTileResourceReleases(ICommandExecutor executor)
    {
        if (_pendingTileResourceReleases.Count == 0)
        {
            return;
        }

        var releaseSink = executor as IBufferBindingCacheInvalidator;
        foreach (var tileResource in _pendingTileResourceReleases)
        {
            releaseSink?.ReleaseBuffer(tileResource.ScalarBuffer);
            tileResource.Dispose();
        }

        _pendingTileResourceReleases.Clear();
    }

    private void DisposePendingTileResources()
    {
        foreach (var tileResource in _pendingTileResourceReleases)
        {
            tileResource.Dispose();
        }

        _pendingTileResourceReleases.Clear();
    }

    private static VertexPositionNormalColor[] BuildVertices(
        SurfaceChartResidentTile residentTile,
        SurfaceChartRenderState state)
    {
        var renderTile = residentTile.SoftwareRenderTile;
        var vertices = new VertexPositionNormalColor[renderTile.Vertices.Count];
        var sampleWidth = renderTile.Geometry.SampleWidth;
        for (var index = 0; index < renderTile.Vertices.Count; index++)
        {
            var vertex = renderTile.Vertices[index];
            var row = index / sampleWidth;
            var column = index % sampleWidth;
            vertices[index] = new VertexPositionNormalColor(
                vertex.Position,
                DeriveNormal(residentTile, state, row, column),
                ToRgbaFloat(vertex.Color));
        }

        return vertices;
    }

    private static Vector3 DeriveNormal(
        SurfaceChartResidentTile residentTile,
        SurfaceChartRenderState state,
        int row,
        int column)
    {
        var renderTile = residentTile.SoftwareRenderTile;
        var current = GetPosition(renderTile, row, column);
        if (!IsFinite(current))
        {
            return Vector3.UnitY;
        }

        var hasLeft = TryGetHorizontalNeighborPosition(residentTile, state, row, column, offset: -1, out var left);
        var hasRight = TryGetHorizontalNeighborPosition(residentTile, state, row, column, offset: 1, out var right);
        var hasUp = TryGetVerticalNeighborPosition(residentTile, state, row, column, offset: -1, out var up);
        var hasDown = TryGetVerticalNeighborPosition(residentTile, state, row, column, offset: 1, out var down);

        if (!TryResolveTangent(current, hasLeft, left, hasRight, right, out var tangentX)
            || !TryResolveTangent(current, hasUp, up, hasDown, down, out var tangentZ))
        {
            return Vector3.UnitY;
        }

        var normal = Vector3.Cross(tangentZ, tangentX);
        if (!IsFinite(normal) || normal.LengthSquared() <= float.Epsilon)
        {
            return Vector3.UnitY;
        }

        return Vector3.Normalize(normal);
    }

    private static bool TryResolveTangent(
        Vector3 current,
        bool hasNegative,
        Vector3 negative,
        bool hasPositive,
        Vector3 positive,
        out Vector3 tangent)
    {
        tangent = default;

        if (hasNegative && hasPositive)
        {
            tangent = positive - negative;
        }
        else if (hasPositive)
        {
            tangent = positive - current;
        }
        else if (hasNegative)
        {
            tangent = current - negative;
        }

        return IsFinite(tangent) && tangent.LengthSquared() > float.Epsilon;
    }

    private static bool TryGetHorizontalNeighborPosition(
        SurfaceChartResidentTile residentTile,
        SurfaceChartRenderState state,
        int row,
        int column,
        int offset,
        out Vector3 position)
    {
        var renderTile = residentTile.SoftwareRenderTile;
        var sampleWidth = renderTile.Geometry.SampleWidth;
        var targetColumn = column + offset;
        if (targetColumn >= 0 && targetColumn < sampleWidth)
        {
            position = GetPosition(renderTile, row, targetColumn);
            return IsFinite(position);
        }

        var neighborTileX = residentTile.Key.TileX + offset;
        if (neighborTileX < 0
            || !state.TryGetResidentTile(new SurfaceTileKey(residentTile.Key.LevelX, residentTile.Key.LevelY, neighborTileX, residentTile.Key.TileY), out var neighborTile))
        {
            position = default;
            return false;
        }

        var neighborRenderTile = neighborTile.SoftwareRenderTile;
        var edgeColumn = offset < 0 ? neighborRenderTile.Geometry.SampleWidth - 1 : 0;
        var current = GetPosition(renderTile, row, column);
        var mappedRow = FindClosestRow(neighborRenderTile, current.Z, edgeColumn);
        position = GetPosition(neighborRenderTile, mappedRow, edgeColumn);
        return IsFinite(position);
    }

    private static bool TryGetVerticalNeighborPosition(
        SurfaceChartResidentTile residentTile,
        SurfaceChartRenderState state,
        int row,
        int column,
        int offset,
        out Vector3 position)
    {
        var renderTile = residentTile.SoftwareRenderTile;
        var sampleHeight = renderTile.Geometry.SampleHeight;
        var targetRow = row + offset;
        if (targetRow >= 0 && targetRow < sampleHeight)
        {
            position = GetPosition(renderTile, targetRow, column);
            return IsFinite(position);
        }

        var neighborTileY = residentTile.Key.TileY + offset;
        if (neighborTileY < 0
            || !state.TryGetResidentTile(new SurfaceTileKey(residentTile.Key.LevelX, residentTile.Key.LevelY, residentTile.Key.TileX, neighborTileY), out var neighborTile))
        {
            position = default;
            return false;
        }

        var neighborRenderTile = neighborTile.SoftwareRenderTile;
        var edgeRow = offset < 0 ? neighborRenderTile.Geometry.SampleHeight - 1 : 0;
        var current = GetPosition(renderTile, row, column);
        var mappedColumn = FindClosestColumn(neighborRenderTile, current.X, edgeRow);
        position = GetPosition(neighborRenderTile, edgeRow, mappedColumn);
        return IsFinite(position);
    }

    private static int FindClosestRow(SurfaceRenderTile renderTile, float targetZ, int column)
    {
        var sampleHeight = renderTile.Geometry.SampleHeight;
        var bestRow = 0;
        var bestDistance = float.MaxValue;
        for (var row = 0; row < sampleHeight; row++)
        {
            var candidate = GetPosition(renderTile, row, column);
            if (!IsFinite(candidate))
            {
                continue;
            }

            var distance = MathF.Abs(candidate.Z - targetZ);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestRow = row;
            }
        }

        return bestRow;
    }

    private static int FindClosestColumn(SurfaceRenderTile renderTile, float targetX, int row)
    {
        var sampleWidth = renderTile.Geometry.SampleWidth;
        var bestColumn = 0;
        var bestDistance = float.MaxValue;
        for (var column = 0; column < sampleWidth; column++)
        {
            var candidate = GetPosition(renderTile, row, column);
            if (!IsFinite(candidate))
            {
                continue;
            }

            var distance = MathF.Abs(candidate.X - targetX);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestColumn = column;
            }
        }

        return bestColumn;
    }

    private static Vector3 GetPosition(SurfaceRenderTile renderTile, int row, int column)
    {
        var sampleWidth = renderTile.Geometry.SampleWidth;
        return renderTile.Vertices[(row * sampleWidth) + column].Position;
    }

    private static bool IsFinite(Vector3 value)
    {
        return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
    }

    private static PipelineDescription CreateSurfaceChartPipelineDescription()
    {
        return new PipelineDescription
        {
            VertexLayout = new VertexLayoutDescription
            {
                Elements =
                [
                    new VertexElementDescription { Name = "POSITION", Format = VertexElementFormat.Float3, Offset = 0 },
                    new VertexElementDescription { Name = "NORMAL", Format = VertexElementFormat.Float3, Offset = 12 },
                    new VertexElementDescription { Name = "SCALAR_COLOR", Format = VertexElementFormat.Float4, Offset = 24 },
                ],
            },
            ResourceLayouts =
            [
                new ResourceLayoutDescription
                {
                    Elements =
                    [
                        new ResourceLayoutElementDescription { Binding = RenderBindingSlots.Camera, Kind = ResourceKind.UniformBuffer, Stages = ShaderStage.Vertex },
                        new ResourceLayoutElementDescription { Binding = RenderBindingSlots.World, Kind = ResourceKind.UniformBuffer, Stages = ShaderStage.Vertex },
                        new ResourceLayoutElementDescription { Binding = RenderBindingSlots.SurfaceColorMap, Kind = ResourceKind.UniformBuffer, Stages = ShaderStage.Vertex },
                        new ResourceLayoutElementDescription { Binding = RenderBindingSlots.SurfaceTileScalars, Kind = ResourceKind.UniformBuffer, Stages = ShaderStage.Vertex },
                    ],
                },
            ],
            Topology = PrimitiveTopology.TriangleList,
            DepthTestEnabled = true,
            DepthWriteEnabled = true,
        };
    }

    private static RgbaFloat ToRgbaFloat(uint argb)
    {
        var a = ((argb >> 24) & 0xFF) / 255f;
        var r = ((argb >> 16) & 0xFF) / 255f;
        var g = ((argb >> 8) & 0xFF) / 255f;
        var b = (argb & 0xFF) / 255f;
        return new RgbaFloat(r, g, b, a);
    }

    private readonly record struct SurfaceChartGpuCameraUniform(Matrix4x4 ViewMatrix, Matrix4x4 ProjectionMatrix);

}
