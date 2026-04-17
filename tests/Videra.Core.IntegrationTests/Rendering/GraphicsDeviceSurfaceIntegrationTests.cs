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
}
