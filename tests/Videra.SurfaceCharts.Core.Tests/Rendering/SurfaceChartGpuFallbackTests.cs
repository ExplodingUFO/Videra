using System.Numerics;
using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;
using Videra.SurfaceCharts.Rendering.Software;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests.Rendering;

public sealed class SurfaceChartGpuFallbackTests
{
    [Fact]
    public void HandleBoundInputs_WithAvailableGpuBackend_SelectsGpu()
    {
        var graphicsBackend = new FakeGraphicsBackend();
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend),
            allowSoftwareFallback: true);

        host.UpdateInputs(CreateInputs(handleBound: true));

        host.Snapshot.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Gpu);
        host.Snapshot.UsesNativeSurface.Should().BeTrue();
        host.RenderingStatus.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Gpu);
        host.RenderingStatus.UsesNativeSurface.Should().BeTrue();
        host.SoftwareScene.Should().BeNull();
        graphicsBackend.InitializeCallCount.Should().Be(1);
        graphicsBackend.BeginFrameCallCount.Should().Be(1);
    }

    [Fact]
    public void HandleBoundInputs_WithSameShapeTiles_ReusesSharedIndexBuffer()
    {
        var resourceFactory = new FakeResourceFactory();
        var graphicsBackend = new FakeGraphicsBackend(resourceFactory: resourceFactory);
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend),
            allowSoftwareFallback: true);
        var metadata = CreateMetadata(width: 8, height: 8);
        var leftTile = CreateTile(metadata, new SurfaceTileKey(1, 1, 0, 0), tileValue: 12f);
        var rightTile = CreateTile(metadata, new SurfaceTileKey(1, 1, 1, 0), tileValue: 24f);

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [leftTile, rightTile],
            ColorMap = CreateColorMap(metadata),
        });

        resourceFactory.IndexBufferCreationCount.Should().Be(1);
        resourceFactory.VertexBufferCreationCount.Should().Be(2);
    }

    [Fact]
    public void ColorMapChanges_UpdateExistingGpuVertexBuffers_WithoutRecreatingResources()
    {
        var resourceFactory = new FakeResourceFactory();
        var graphicsBackend = new FakeGraphicsBackend(resourceFactory: resourceFactory);
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend),
            allowSoftwareFallback: true);
        var metadata = CreateMetadata(width: 4, height: 4);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 18f);
        var initialColorMap = CreateColorMap(metadata);
        var replacementColorMap = new SurfaceColorMap(metadata.ValueRange, new SurfaceColorMapPalette(0xFF804020u, 0xFF20C0F0u));

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = initialColorMap,
        });

        resourceFactory.VertexBufferCreationCount.Should().Be(1);
        resourceFactory.IndexBufferCreationCount.Should().Be(1);
        resourceFactory.VertexBufferUpdateCount.Should().Be(0);

        host.UpdateInputs(CreateInputs(handleBound: true) with
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = replacementColorMap,
        });

        resourceFactory.VertexBufferCreationCount.Should().Be(1);
        resourceFactory.IndexBufferCreationCount.Should().Be(1);
        resourceFactory.VertexBufferUpdateCount.Should().Be(1);
    }

    [Fact]
    public void GpuInitializationFailure_WithFallbackAllowed_SwitchesToSoftware()
    {
        var graphicsBackend = new FakeGraphicsBackend(
            initializeException: new InvalidOperationException("gpu init failed"));
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend),
            allowSoftwareFallback: true);

        host.UpdateInputs(CreateInputs(handleBound: true));

        host.Snapshot.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Software);
        host.Snapshot.IsFallback.Should().BeTrue();
        host.Snapshot.UsesNativeSurface.Should().BeFalse();
        host.RenderingStatus.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Software);
        host.RenderingStatus.IsFallback.Should().BeTrue();
        host.SoftwareScene.Should().NotBeNull();
    }

    [Fact]
    public void GpuFramePreparationFailure_RetainsFallbackReason()
    {
        var graphicsBackend = new FakeGraphicsBackend(
            beginFrameException: new InvalidOperationException("gpu frame prep failed"));
        var host = new SurfaceChartRenderHost(
            softwareBackend: new SurfaceChartSoftwareRenderBackend(),
            gpuBackend: new SurfaceChartGpuRenderBackend(graphicsBackend),
            allowSoftwareFallback: true);

        host.UpdateInputs(CreateInputs(handleBound: true));

        host.RenderingStatus.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Software);
        host.RenderingStatus.IsFallback.Should().BeTrue();
        host.RenderingStatus.FallbackReason.Should().Contain("gpu frame prep failed");
        host.Snapshot.FallbackReason.Should().Contain("gpu frame prep failed");
    }

    private static SurfaceChartRenderInputs CreateInputs(bool handleBound)
    {
        var metadata = CreateMetadata(width: 4, height: 4);
        var tile = CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 12f);

        return new SurfaceChartRenderInputs
        {
            Metadata = metadata,
            LoadedTiles = [tile],
            ColorMap = CreateColorMap(metadata),
            Viewport = new SurfaceViewport(0, 0, 4, 4),
            ProjectionSettings = default,
            ViewWidth = 320d,
            ViewHeight = 180d,
            NativeHandle = handleBound ? new IntPtr(0x1234) : IntPtr.Zero,
            HandleBound = handleBound,
            RenderScale = 1f,
        };
    }

    private static SurfaceMetadata CreateMetadata(int width, int height)
    {
        return new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("X", unit: null, minimum: 0d, maximum: width - 1d),
            new SurfaceAxisDescriptor("Y", unit: null, minimum: 0d, maximum: height - 1d),
            new SurfaceValueRange(0d, 100d));
    }

    private static SurfaceTile CreateTile(SurfaceMetadata metadata, SurfaceTileKey key, float tileValue)
    {
        var tileCountX = 1 << key.LevelX;
        var tileCountY = 1 << key.LevelY;
        var startX = (metadata.Width * key.TileX) / tileCountX;
        var endX = (metadata.Width * (key.TileX + 1)) / tileCountX;
        var startY = (metadata.Height * key.TileY) / tileCountY;
        var endY = (metadata.Height * (key.TileY + 1)) / tileCountY;
        var width = endX - startX;
        var height = endY - startY;
        var bounds = new SurfaceTileBounds(startX, startY, width, height);
        var values = new float[width * height];
        Array.Fill(values, tileValue);
        return new SurfaceTile(key, width, height, bounds, values, metadata.ValueRange);
    }

    private static SurfaceColorMap CreateColorMap(SurfaceMetadata metadata)
    {
        return new SurfaceColorMap(metadata.ValueRange, new SurfaceColorMapPalette(0xFF203040u, 0xFFE0F0FFu));
    }

    private sealed class FakeGraphicsBackend : IGraphicsBackend
    {
        private readonly Exception? _initializeException;
        private readonly Exception? _beginFrameException;
        private readonly FakeResourceFactory _resourceFactory;

        public FakeGraphicsBackend(
            Exception? initializeException = null,
            Exception? beginFrameException = null,
            FakeResourceFactory? resourceFactory = null)
        {
            _initializeException = initializeException;
            _beginFrameException = beginFrameException;
            _resourceFactory = resourceFactory ?? new FakeResourceFactory();
        }

        public bool IsInitialized { get; private set; }

        public int InitializeCallCount { get; private set; }

        public int BeginFrameCallCount { get; private set; }

        public IResourceFactory GetResourceFactory()
        {
            return _resourceFactory;
        }

        public ICommandExecutor GetCommandExecutor()
        {
            return new FakeCommandExecutor();
        }

        public void Initialize(IntPtr windowHandle, int width, int height)
        {
            InitializeCallCount++;
            if (_initializeException is not null)
            {
                throw _initializeException;
            }

            IsInitialized = true;
        }

        public void Resize(int width, int height)
        {
        }

        public void BeginFrame()
        {
            BeginFrameCallCount++;
            if (_beginFrameException is not null)
            {
                throw _beginFrameException;
            }
        }

        public void EndFrame()
        {
        }

        public void SetClearColor(Vector4 color)
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class FakeResourceFactory : IResourceFactory
    {
        public int VertexBufferCreationCount { get; private set; }

        public int IndexBufferCreationCount { get; private set; }

        public int VertexBufferUpdateCount { get; private set; }

        public IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices)
        {
            VertexBufferCreationCount++;
            return new FakeBuffer((uint)(vertices.Length * 40), this, trackVertexUpdates: true);
        }

        public IBuffer CreateVertexBuffer(uint sizeInBytes)
        {
            VertexBufferCreationCount++;
            return new FakeBuffer(sizeInBytes, this, trackVertexUpdates: true);
        }

        public IBuffer CreateIndexBuffer(uint[] indices)
        {
            IndexBufferCreationCount++;
            return new FakeBuffer((uint)(indices.Length * sizeof(uint)), this, trackVertexUpdates: false);
        }

        public IBuffer CreateIndexBuffer(uint sizeInBytes)
        {
            IndexBufferCreationCount++;
            return new FakeBuffer(sizeInBytes, this, trackVertexUpdates: false);
        }

        public IBuffer CreateUniformBuffer(uint sizeInBytes)
        {
            return new FakeBuffer(sizeInBytes, this, trackVertexUpdates: false);
        }

        public IPipeline CreatePipeline(PipelineDescription description)
        {
            return new FakePipeline();
        }

        public IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors)
        {
            return new FakePipeline();
        }

        public IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint)
        {
            return new FakeShader();
        }

        public IResourceSet CreateResourceSet(ResourceSetDescription description)
        {
            return new FakeResourceSet();
        }

        public void RecordVertexBufferUpdate()
        {
            VertexBufferUpdateCount++;
        }
    }

    private sealed class FakeCommandExecutor : ICommandExecutor
    {
        public void SetPipeline(IPipeline pipeline)
        {
        }

        public void SetVertexBuffer(IBuffer buffer, uint index = 0)
        {
        }

        public void SetIndexBuffer(IBuffer buffer)
        {
        }

        public void SetResourceSet(uint slot, IResourceSet resourceSet)
        {
        }

        public void DrawIndexed(uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
        {
        }

        public void DrawIndexed(uint primitiveType, uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
        {
        }

        public void Draw(uint vertexCount, uint instanceCount = 1, uint firstVertex = 0, uint firstInstance = 0)
        {
        }

        public void SetViewport(float x, float y, float width, float height, float minDepth = 0f, float maxDepth = 1f)
        {
        }

        public void SetScissorRect(int x, int y, int width, int height)
        {
        }

        public void Clear(float r, float g, float b, float a)
        {
        }

        public void SetDepthState(bool testEnabled, bool writeEnabled)
        {
        }

        public void ResetDepthState()
        {
        }
    }

    private sealed class FakeBuffer : IBuffer
    {
        private readonly FakeResourceFactory _owner;
        private readonly bool _trackVertexUpdates;

        public FakeBuffer(uint sizeInBytes, FakeResourceFactory owner, bool trackVertexUpdates)
        {
            SizeInBytes = sizeInBytes;
            _owner = owner;
            _trackVertexUpdates = trackVertexUpdates;
        }

        public uint SizeInBytes { get; }

        public void Update<T>(T data)
            where T : unmanaged
        {
            if (_trackVertexUpdates)
            {
                _owner.RecordVertexBufferUpdate();
            }
        }

        public void UpdateArray<T>(T[] data)
            where T : unmanaged
        {
            if (_trackVertexUpdates)
            {
                _owner.RecordVertexBufferUpdate();
            }
        }

        public void SetData<T>(T data, uint offset)
            where T : unmanaged
        {
        }

        public void SetData<T>(T[] data, uint offset)
            where T : unmanaged
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class FakePipeline : IPipeline
    {
        public void Dispose()
        {
        }
    }

    private sealed class FakeShader : IShader
    {
        public void Dispose()
        {
        }
    }

    private sealed class FakeResourceSet : IResourceSet
    {
        public void Dispose()
        {
        }
    }
}
