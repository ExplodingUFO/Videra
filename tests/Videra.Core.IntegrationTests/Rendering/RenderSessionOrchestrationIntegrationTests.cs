using FluentAssertions;
using Videra.Avalonia.Rendering;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.RenderPipeline;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class RenderSessionOrchestrationIntegrationTests
{
    [Fact]
    public void Native_backend_waits_for_size_then_handle_then_becomes_ready()
    {
        using var engine = new VideraEngine();
        var factory = new TrackingBackendFactory();
        using var orchestrator = new RenderSessionOrchestrator(
            engine,
            backendFactory: factory.CreateBackend);

        var initial = orchestrator.Snapshot;
        initial.State.Should().Be(RenderSessionState.Detached);

        orchestrator.Attach(GraphicsBackendPreference.D3D11);
        orchestrator.Snapshot.State.Should().Be(RenderSessionState.WaitingForSize);

        orchestrator.Resize(128, 96, 1f);
        orchestrator.Snapshot.State.Should().Be(RenderSessionState.WaitingForHandle);
        orchestrator.BindHandle(new IntPtr(0x1337));

        orchestrator.IsReady.Should().BeTrue();
        orchestrator.Snapshot.State.Should().Be(RenderSessionState.Ready);
        orchestrator.Snapshot.Inputs.RequestedBackend.Should().Be(GraphicsBackendPreference.D3D11);
        orchestrator.Snapshot.Inputs.Width.Should().Be(128u);
        orchestrator.Snapshot.Inputs.Height.Should().Be(96u);
        orchestrator.Snapshot.HandleState.IsBound.Should().BeTrue();
        factory.CreatedBackends.Should().HaveCount(1);
        factory.CreatedBackends[0].InitializeCalls.Should().Be(1);
    }

    [Fact]
    public void Software_backend_can_be_ready_without_native_handle()
    {
        using var engine = new VideraEngine();
        var factory = new TrackingBackendFactory();
        using var orchestrator = new RenderSessionOrchestrator(
            engine,
            backendFactory: factory.CreateBackend);

        orchestrator.Attach(GraphicsBackendPreference.Software);
        orchestrator.Resize(256, 128, 1f);

        orchestrator.Snapshot.State.Should().Be(RenderSessionState.Ready);
        orchestrator.IsReady.Should().BeTrue();
        orchestrator.HandleState.IsBound.Should().BeFalse();
        orchestrator.Snapshot.UsesSoftwarePresentationCopy.Should().BeTrue();
        factory.CreatedBackends.Should().HaveCount(1);
        factory.CreatedBackends[0].IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void Native_ready_handle_destroy_and_rebind_triggers_suspend_and_recreate()
    {
        using var engine = new VideraEngine();
        var factory = new TrackingBackendFactory();
        using var orchestrator = new RenderSessionOrchestrator(
            engine,
            backendFactory: factory.CreateBackend);

        orchestrator.Attach(GraphicsBackendPreference.D3D11);
        orchestrator.Resize(160, 120, 1f);
        orchestrator.BindHandle(new IntPtr(0x1111));

        factory.CreatedBackends.Should().HaveCount(1);
        var firstBackend = factory.CreatedBackends[0];
        orchestrator.Snapshot.State.Should().Be(RenderSessionState.Ready);
        firstBackend.DisposeCalls.Should().Be(0);

        orchestrator.BindHandle(IntPtr.Zero);

        firstBackend.DisposeCalls.Should().Be(1);
        orchestrator.Snapshot.State.Should().Be(RenderSessionState.WaitingForHandle);
        orchestrator.Snapshot.HandleState.Generation.Should().Be(2);

        orchestrator.BindHandle(new IntPtr(0x2222));

        factory.CreatedBackends.Should().HaveCount(2);
        factory.CreatedBackends[1].InitializeCalls.Should().Be(1);
        orchestrator.Snapshot.State.Should().Be(RenderSessionState.Ready);
        orchestrator.Snapshot.HandleState.Generation.Should().Be(3);
        factory.CreatedBackends[1].IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void Dispose_is_idempotent_and_allows_followup_noop_calls()
    {
        using var engine = new VideraEngine();
        var factory = new TrackingBackendFactory();
        var orchestrator = new RenderSessionOrchestrator(
            engine,
            backendFactory: factory.CreateBackend);

        orchestrator.Attach(GraphicsBackendPreference.D3D11);
        orchestrator.Resize(128, 96, 1f);
        orchestrator.BindHandle(new IntPtr(0x3333));

        factory.CreatedBackends.Should().HaveCount(1);
        orchestrator.IsReady.Should().BeTrue();

        orchestrator.Dispose();

        var act = () =>
        {
            orchestrator.Attach(GraphicsBackendPreference.D3D11);
            orchestrator.BindHandle(new IntPtr(0x4444));
            orchestrator.Resize(256, 192, 1f);
            orchestrator.RenderOnce();
            orchestrator.Dispose();
            orchestrator.BindHandle(IntPtr.Zero);
        };

        act.Should().NotThrow();
        factory.CreatedBackends.Should().HaveCount(1);
        factory.CreatedBackends[0].DisposeCalls.Should().Be(1);
        orchestrator.IsReady.Should().BeFalse();
        orchestrator.Snapshot.State.Should().Be(RenderSessionState.Disposed);
    }

    [Fact]
    public void RenderOnce_exposes_latest_pipeline_snapshot()
    {
        using var engine = new VideraEngine();
        using var orchestrator = new RenderSessionOrchestrator(
            engine,
            backendFactory: _ => new TrackingSoftwareBackend());

        orchestrator.Attach(GraphicsBackendPreference.Software);
        orchestrator.Resize(64, 64, 1f);

        orchestrator.RenderOnce();
        orchestrator.RenderOnce();

        orchestrator.Snapshot.LastPipelineSnapshot.Should().NotBeNull();
        orchestrator.Snapshot.LastPipelineSnapshot!.Profile.Should().Be(RenderPipelineProfile.Standard);
        orchestrator.Snapshot.LastPipelineSnapshot.StageNames.Should().Contain("PrepareFrame");
        orchestrator.Snapshot.LastPipelineSnapshot.StageNames.Should().Contain("PresentFrame");
    }

    private sealed class TrackingBackendFactory
    {
        public List<ITrackingBackend> CreatedBackends { get; } = new();

        public IGraphicsBackend CreateBackend(GraphicsBackendPreference preference)
        {
            IGraphicsBackend backend = preference == GraphicsBackendPreference.Software
                ? new TrackingSoftwareBackend()
                : new TrackingBackend();
            CreatedBackends.Add((ITrackingBackend)backend);
            return backend;
        }
    }

    private interface ITrackingBackend
    {
        bool IsInitialized { get; }
        int InitializeCalls { get; }
        int DisposeCalls { get; }
    }

    private sealed class TrackingBackend : IGraphicsBackend, ITrackingBackend
    {
        private readonly TrackingResourceFactory _resourceFactory = new();
        private readonly TrackingCommandExecutor _commandExecutor = new();

        public bool IsInitialized { get; private set; }
        public int InitializeCalls { get; private set; }
        public int DisposeCalls { get; private set; }

        public void Initialize(IntPtr windowHandle, int width, int height)
        {
            IsInitialized = true;
            InitializeCalls++;
        }

        public void Resize(int width, int height)
        {
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

    private sealed class TrackingSoftwareBackend : IGraphicsBackend, ISoftwareBackend, ITrackingBackend
    {
        private readonly TrackingResourceFactory _resourceFactory = new();
        private readonly TrackingCommandExecutor _commandExecutor = new();

        public bool IsInitialized { get; private set; }
        public int InitializeCalls { get; private set; }
        public int DisposeCalls { get; private set; }

        public int Width => 64;

        public int Height => 64;

        public void Initialize(IntPtr windowHandle, int width, int height)
        {
            _ = windowHandle;
            _ = width;
            _ = height;
            IsInitialized = true;
            InitializeCalls++;
        }

        public void Resize(int width, int height)
        {
            _ = width;
            _ = height;
        }

        public void BeginFrame()
        {
        }

        public void EndFrame()
        {
        }

        public void SetClearColor(System.Numerics.Vector4 color)
        {
            _ = color;
        }

        public IResourceFactory GetResourceFactory() => _resourceFactory;

        public ICommandExecutor GetCommandExecutor() => _commandExecutor;

        public void Dispose()
        {
            DisposeCalls++;
            IsInitialized = false;
        }

        public void CopyFrameTo(IntPtr destination, int destinationStride)
        {
            _ = destination;
            _ = destinationStride;
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
