using System.Numerics;
using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class GraphicsDeviceSurfaceIntegrationTests
{
    [Fact]
    public void LegacyGraphicsBackendAdapter_DrivesLegacyBackendThroughSurfaceLifecycle()
    {
        using var backend = new TrackingBackend();
        using var device = new LegacyGraphicsBackendAdapter(backend, GraphicsBackendPreference.D3D11);
        using var surface = device.CreateRenderSurface();

        surface.Initialize(new IntPtr(0x1234), 320, 180);
        using var frame = surface.BeginFrame(new Vector4(0.25f, 0.5f, 0.75f, 1f));
        frame.Dispose();

        surface.Resize(640, 360);

        backend.InitializeCalls.Should().Be(1);
        backend.LastWindowHandle.Should().Be(new IntPtr(0x1234));
        backend.LastInitializedWidth.Should().Be(320);
        backend.LastInitializedHeight.Should().Be(180);
        backend.SetClearColorCalls.Should().Be(1);
        backend.BeginFrameCalls.Should().Be(1);
        backend.EndFrameCalls.Should().Be(1);
        backend.ResizeCalls.Should().ContainInOrder((640, 360));
    }

    [Fact]
    public void SoftwareBackend_ExposesDirectGraphicsDeviceAndSurfaceContracts()
    {
        using IGraphicsBackend backend = new SoftwareBackend();

        var device = backend as IGraphicsDevice;
        var surface = backend as IRenderSurface;

        device.Should().NotBeNull();
        surface.Should().NotBeNull();
        device!.ActiveBackendPreference.Should().Be(GraphicsBackendPreference.Software);
        device.IsSoftwareBackend.Should().BeTrue();

        using var createdSurface = device.CreateRenderSurface();
        createdSurface.Should().BeSameAs(surface);

        createdSurface.Initialize(IntPtr.Zero, 96, 64);
        using var frame = createdSurface.BeginFrame(new Vector4(0.2f, 0.3f, 0.4f, 1f));
        frame.Should().NotBeNull();
    }

    [Fact]
    public void VideraEngine_CanRenderThroughGraphicsDeviceAndSurface()
    {
        using var backend = new SoftwareBackend();
        using var device = new LegacyGraphicsBackendAdapter(backend, GraphicsBackendPreference.Software);
        using var surface = device.CreateRenderSurface();
        surface.Initialize(IntPtr.Zero, 200, 200);

        using var engine = new VideraEngine();
        engine.Initialize(device, surface);
        engine.Resize(200, 200);
        engine.AddObject(DemoMeshFactory.CreateWhiteQuad(device.ResourceFactory));

        engine.Draw();

        var capabilities = engine.GetRenderCapabilities();
        capabilities.IsInitialized.Should().BeTrue();
        capabilities.ActiveBackendPreference.Should().Be(GraphicsBackendPreference.Software);
        capabilities.LastPipelineSnapshot.Should().NotBeNull();

        var frame = DemoMeshFactory.CaptureFrame(backend);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.White).Should().BeGreaterThan(0);
    }

    [Fact]
    public void VideraEngine_Suspend_WaitsForDeviceIdle_BeforeReleasingGpuBuffers()
    {
        using var device = new IdleTrackingGraphicsDevice();
        using var surface = device.CreateRenderSurface();
        surface.Initialize(IntPtr.Zero, 160, 120);

        using var engine = new VideraEngine();
        engine.Initialize(device, surface);
        engine.Resize(160, 120);
        engine.AddObject(CreateDeferredQuad());

        var act = () => engine.Suspend();

        act.Should().NotThrow();
        device.WaitForIdleCalls.Should().BeGreaterThan(0);
        device.BufferDisposedBeforeIdle.Should().BeFalse();
        device.Events.Should().ContainInOrder("wait-idle", "buffer-dispose");
    }

    private sealed class TrackingBackend : IGraphicsBackend
    {
        private readonly TrackingResourceFactory _resourceFactory = new();
        private readonly TrackingCommandExecutor _commandExecutor = new();

        public int InitializeCalls { get; private set; }
        public IntPtr LastWindowHandle { get; private set; }
        public int LastInitializedWidth { get; private set; }
        public int LastInitializedHeight { get; private set; }
        public int SetClearColorCalls { get; private set; }
        public int BeginFrameCalls { get; private set; }
        public int EndFrameCalls { get; private set; }
        public List<(int Width, int Height)> ResizeCalls { get; } = [];

        public bool IsInitialized { get; private set; }

        public void Initialize(IntPtr windowHandle, int width, int height)
        {
            InitializeCalls++;
            LastWindowHandle = windowHandle;
            LastInitializedWidth = width;
            LastInitializedHeight = height;
            IsInitialized = true;
        }

        public void Resize(int width, int height)
        {
            ResizeCalls.Add((width, height));
        }

        public void BeginFrame()
        {
            BeginFrameCalls++;
        }

        public void EndFrame()
        {
            EndFrameCalls++;
        }

        public void SetClearColor(Vector4 color)
        {
            _ = color;
            SetClearColorCalls++;
        }

        public IResourceFactory GetResourceFactory() => _resourceFactory;

        public ICommandExecutor GetCommandExecutor() => _commandExecutor;

        public void Dispose()
        {
            IsInitialized = false;
        }
    }

    private sealed class TrackingResourceFactory : IResourceFactory
    {
        public IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices) => new TrackingBuffer((uint)(vertices.Length * sizeof(float) * 10));

        public IBuffer CreateVertexBuffer(uint sizeInBytes) => new TrackingBuffer(sizeInBytes);

        public IBuffer CreateIndexBuffer(uint[] indices) => new TrackingBuffer((uint)(indices.Length * sizeof(uint)));

        public IBuffer CreateIndexBuffer(uint sizeInBytes) => new TrackingBuffer(sizeInBytes);

        public IBuffer CreateUniformBuffer(uint sizeInBytes) => new TrackingBuffer(sizeInBytes);

        public IPipeline CreatePipeline(PipelineDescription description) => new TrackingPipeline();

        public IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors) => new TrackingPipeline();

        public IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint) => new TrackingShader();

        public IResourceSet CreateResourceSet(ResourceSetDescription description) => new TrackingResourceSet();
    }

    private sealed class TrackingCommandExecutor : ICommandExecutor
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

    private sealed class TrackingBuffer(uint sizeInBytes) : IBuffer
    {
        public uint SizeInBytes { get; } = sizeInBytes;

        public void Update<T>(T data) where T : unmanaged
        {
        }

        public void UpdateArray<T>(T[] data) where T : unmanaged
        {
        }

        public void SetData<T>(T data, uint offset) where T : unmanaged
        {
        }

        public void SetData<T>(T[] data, uint offset) where T : unmanaged
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class TrackingPipeline : IPipeline
    {
        public void Dispose()
        {
        }
    }

    private sealed class TrackingShader : IShader
    {
        public void Dispose()
        {
        }
    }

    private sealed class TrackingResourceSet : IResourceSet
    {
        public void Dispose()
        {
        }
    }

    private sealed class IdleTrackingGraphicsDevice : IGraphicsDevice, IGraphicsDeviceIdleBarrier
    {
        private readonly IdleTrackingResourceFactory _resourceFactory;
        private readonly TrackingCommandExecutor _commandExecutor = new();

        public IdleTrackingGraphicsDevice()
        {
            _resourceFactory = new IdleTrackingResourceFactory(this);
        }

        public List<string> Events { get; } = [];

        public GraphicsBackendPreference? ActiveBackendPreference => GraphicsBackendPreference.Vulkan;

        public bool IsSoftwareBackend => false;

        public IResourceFactory ResourceFactory => _resourceFactory;

        public ICommandExecutor CommandExecutor => _commandExecutor;

        public bool WaitedForIdle { get; private set; }

        public int WaitForIdleCalls { get; private set; }

        public bool BufferDisposedBeforeIdle { get; private set; }

        public IRenderSurface CreateRenderSurface() => new IdleTrackingRenderSurface();

        public void WaitForIdle()
        {
            WaitForIdleCalls++;
            WaitedForIdle = true;
            Events.Add("wait-idle");
        }

        public void Dispose()
        {
            Events.Add("device-dispose");
        }

        private sealed class IdleTrackingRenderSurface : IRenderSurface
        {
            public bool IsInitialized { get; private set; }

            public bool UsesSoftwarePresentationCopy => false;

            public void Initialize(IntPtr windowHandle, int width, int height)
            {
                _ = windowHandle;
                _ = width;
                _ = height;
                IsInitialized = true;
            }

            public void Resize(int width, int height)
            {
                _ = width;
                _ = height;
            }

            public IFrameContext BeginFrame(Vector4 clearColor)
            {
                _ = clearColor;
                return new IdleTrackingFrameContext();
            }

            public void Dispose()
            {
                IsInitialized = false;
            }
        }

        private sealed class IdleTrackingFrameContext : IFrameContext
        {
            public void Dispose()
            {
            }
        }

        private sealed class IdleTrackingResourceFactory(IdleTrackingGraphicsDevice owner) : IResourceFactory
        {
            public IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices) => new IdleTrackingBuffer(owner, (uint)(vertices.Length * sizeof(float) * 10));

            public IBuffer CreateVertexBuffer(uint sizeInBytes) => new IdleTrackingBuffer(owner, sizeInBytes);

            public IBuffer CreateIndexBuffer(uint[] indices) => new IdleTrackingBuffer(owner, (uint)(indices.Length * sizeof(uint)));

            public IBuffer CreateIndexBuffer(uint sizeInBytes) => new IdleTrackingBuffer(owner, sizeInBytes);

            public IBuffer CreateUniformBuffer(uint sizeInBytes) => new IdleTrackingBuffer(owner, sizeInBytes);

            public IPipeline CreatePipeline(PipelineDescription description) => new TrackingPipeline();

            public IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors) => new TrackingPipeline();

            public IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint) => new TrackingShader();

            public IResourceSet CreateResourceSet(ResourceSetDescription description) => new TrackingResourceSet();
        }

        private sealed class IdleTrackingBuffer(IdleTrackingGraphicsDevice owner, uint sizeInBytes) : IBuffer
        {
            private bool _disposed;

            public uint SizeInBytes { get; } = sizeInBytes;

            public void Update<T>(T data) where T : unmanaged
            {
            }

            public void UpdateArray<T>(T[] data) where T : unmanaged
            {
            }

            public void SetData<T>(T data, uint offset) where T : unmanaged
            {
            }

            public void SetData<T>(T[] data, uint offset) where T : unmanaged
            {
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                owner.Events.Add("buffer-dispose");
                if (!owner.WaitedForIdle)
                {
                    owner.BufferDisposedBeforeIdle = true;
                }
            }
        }
    }

    private static Object3D CreateDeferredQuad(float halfExtent = 0.8f)
    {
        var mesh = new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(-halfExtent, -halfExtent, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(halfExtent, -halfExtent, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(halfExtent, halfExtent, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(-halfExtent, halfExtent, 0f), Vector3.UnitZ, RgbaFloat.White)
            ],
            Indices = [0, 1, 2, 0, 2, 3],
            Topology = MeshTopology.Triangles
        };

        var quad = new Object3D { Name = "DeferredQuad" };
        quad.PrepareDeferredMesh(mesh);
        return quad;
    }
}
