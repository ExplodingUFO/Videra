using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Rendering;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class RenderSessionIntegrationTests
{
    [Fact]
    public void RenderSessionHandle_RebindsWithNewGeneration()
    {
        var initial = RenderSessionHandle.Create(new IntPtr(0x10));

        var destroyed = initial.Clear();
        var rebound = destroyed.Rebind(new IntPtr(0x20));

        destroyed.IsBound.Should().BeFalse();
        destroyed.Generation.Should().Be(2);
        rebound.IsBound.Should().BeTrue();
        rebound.Generation.Should().Be(3);
        rebound.Handle.Should().Be(new IntPtr(0x20));
    }

    [Fact]
    public void RenderSession_BindHandle_DestroyThenCreate_RecreatesBackend()
    {
        var backendFactory = new TrackingBackendFactory();
        using var engine = new VideraEngine();
        using var session = new RenderSession(engine, backendFactory.CreateBackend);

        session.Attach(GraphicsBackendPreference.D3D11);
        session.Resize(128, 96, 1f);
        session.BindHandle(new IntPtr(0x1234));

        backendFactory.CreatedBackends.Should().HaveCount(1);
        backendFactory.CreatedBackends[0].InitializeCalls.Should().Be(1);
        session.HandleState.Generation.Should().Be(1);

        session.BindHandle(IntPtr.Zero);

        backendFactory.CreatedBackends[0].DisposeCalls.Should().Be(1);
        session.HandleState.IsBound.Should().BeFalse();
        session.HandleState.Generation.Should().Be(2);

        session.BindHandle(new IntPtr(0x5678));

        backendFactory.CreatedBackends.Should().HaveCount(2);
        backendFactory.CreatedBackends[1].InitializeCalls.Should().Be(1);
        session.HandleState.IsBound.Should().BeTrue();
        session.HandleState.Generation.Should().Be(3);
        session.HandleState.Handle.Should().Be(new IntPtr(0x5678));
    }

    [Fact]
    public void DefaultNativeHostFactory_CreateHost_ReturnsPlatformSpecificHost()
    {
        var factory = new DefaultNativeHostFactory();

        var host = factory.CreateHost();

        host.Should().NotBeNull();
    }

    private sealed class TrackingBackendFactory
    {
        public List<TrackingBackend> CreatedBackends { get; } = new();

        public IGraphicsBackend CreateBackend(GraphicsBackendPreference preference)
        {
            _ = preference;
            var backend = new TrackingBackend();
            CreatedBackends.Add(backend);
            return backend;
        }
    }

    private sealed class TrackingBackend : IGraphicsBackend
    {
        private readonly TrackingResourceFactory _resourceFactory = new();
        private readonly TrackingCommandExecutor _commandExecutor = new();

        public bool IsInitialized { get; private set; }
        public int InitializeCalls { get; private set; }
        public int ResizeCalls { get; private set; }
        public int DisposeCalls { get; private set; }

        public void Initialize(IntPtr windowHandle, int width, int height)
        {
            IsInitialized = true;
            InitializeCalls++;
        }

        public void Resize(int width, int height)
        {
            ResizeCalls++;
        }

        public void BeginFrame()
        {
        }

        public void EndFrame()
        {
        }

        public void SetClearColor(System.Numerics.Vector4 color)
        {
        }

        public IResourceFactory GetResourceFactory() => _resourceFactory;

        public ICommandExecutor GetCommandExecutor() => _commandExecutor;

        public void Dispose()
        {
            DisposeCalls++;
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
