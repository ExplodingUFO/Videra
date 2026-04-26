using System.Numerics;
using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Rendering;
using Videra.Avalonia.Runtime.Scene;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Rendering;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class VideraViewSessionBridgeIntegrationTests
{
    [Fact]
    public void BackendOptionsChange_RefreshesDiagnosticsSnapshot()
    {
        using var engine = new VideraEngine();
        using var session = new RenderSession(
            engine,
            backendFactory: static _ => new TrackingSoftwareBackend(),
            bitmapFactory: static (_, _) => null);
        var backendOptions = new VideraBackendOptions();
        var diagnosticsOptions = new VideraDiagnosticsOptions();
        var bridge = CreateBridge(session, backendOptions, diagnosticsOptions);

        bridge.CreateDiagnosticsSnapshot(lastInitializationError: null, DefaultSceneDiagnostics).RequestedBackend.Should().Be(GraphicsBackendPreference.Auto);

        backendOptions.PreferredBackend = GraphicsBackendPreference.Metal;
        bridge.OnBackendOptionsChanged(160, 120, 1f);

        bridge.CreateDiagnosticsSnapshot(lastInitializationError: null, DefaultSceneDiagnostics).RequestedBackend.Should().Be(GraphicsBackendPreference.Metal);
    }

    [Fact]
    public void NativeHandleCreateAndDestroy_UpdatesDiagnosticsTruth()
    {
        using var engine = new VideraEngine();
        using var session = new RenderSession(
            engine,
            backendFactory: static _ => new TrackingNativeBackend());
        var backendOptions = new VideraBackendOptions
        {
            PreferredBackend = GraphicsBackendPreference.D3D11,
            AllowSoftwareFallback = true
        };
        var bridge = CreateBridge(session, backendOptions, new VideraDiagnosticsOptions());

        bridge.OnBackendOptionsChanged(256, 144, 1f);
        bridge.OnNativeHandleCreated(new IntPtr(0x1), "XWayland", true, "compat fallback", 256, 144, 1f);

        var afterCreate = bridge.CreateDiagnosticsSnapshot(lastInitializationError: null, DefaultSceneDiagnostics);
        afterCreate.NativeHostBound.Should().BeTrue();
        afterCreate.ResolvedDisplayServer.Should().Be("XWayland");
        afterCreate.DisplayServerFallbackUsed.Should().BeTrue();
        afterCreate.DisplayServerFallbackReason.Should().Be("compat fallback");
        session.HandleState.IsBound.Should().BeTrue();

        bridge.OnNativeHandleDestroyed();

        var afterDestroy = bridge.CreateDiagnosticsSnapshot(lastInitializationError: null, DefaultSceneDiagnostics);
        afterDestroy.NativeHostBound.Should().BeFalse();
        afterDestroy.ResolvedDisplayServer.Should().BeNull();
        afterDestroy.DisplayServerFallbackUsed.Should().BeFalse();
        afterDestroy.DisplayServerFallbackReason.Should().BeNull();
        session.HandleState.IsBound.Should().BeFalse();
    }

    [Fact]
    public void SizeChanges_SynchronizeSessionThroughSinglePath()
    {
        using var engine = new VideraEngine();
        using var session = new RenderSession(
            engine,
            backendFactory: static _ => new TrackingSoftwareBackend(),
            bitmapFactory: static (_, _) => null);
        var backendOptions = new VideraBackendOptions
        {
            PreferredBackend = GraphicsBackendPreference.Software
        };
        var bridge = CreateBridge(session, backendOptions, new VideraDiagnosticsOptions());

        bridge.OnSizeChanged(96, 64, 1f);
        session.OrchestrationSnapshot.Inputs.Width.Should().Be(96u);
        session.OrchestrationSnapshot.Inputs.Height.Should().Be(64u);

        bridge.OnSizeChanged(256, 128, 1f);
        session.OrchestrationSnapshot.Inputs.Width.Should().Be(256u);
        session.OrchestrationSnapshot.Inputs.Height.Should().Be(128u);
    }

    [Fact]
    public void NativeHandleCreate_SynchronizesHostSurfaceThroughSharedSessionPath()
    {
        using var engine = new VideraEngine();
        using var session = new RenderSession(
            engine,
            backendFactory: static _ => new TrackingNativeBackend());
        var backendOptions = new VideraBackendOptions
        {
            PreferredBackend = GraphicsBackendPreference.D3D11,
            AllowSoftwareFallback = true
        };
        var bridge = CreateBridge(session, backendOptions, new VideraDiagnosticsOptions());

        bridge.OnNativeHandleCreated(new IntPtr(0x44), "XWayland", true, "compat fallback", 320, 180, 1f);

        session.OrchestrationSnapshot.State.Should().Be(RenderSessionState.Ready);
        session.OrchestrationSnapshot.Inputs.RequestedBackend.Should().Be(GraphicsBackendPreference.D3D11);
        session.OrchestrationSnapshot.Inputs.Width.Should().Be(320u);
        session.OrchestrationSnapshot.Inputs.Height.Should().Be(180u);
        session.HandleState.IsBound.Should().BeTrue();
    }

    [Fact]
    public void PipelineSnapshot_ProjectsIntoDiagnosticsProfileAndStageNames()
    {
        using var engine = new VideraEngine();
        using var session = new RenderSession(
            engine,
            backendFactory: static _ => new TrackingSoftwareBackend(),
            bitmapFactory: static (_, _) => null);
        var backendOptions = new VideraBackendOptions
        {
            PreferredBackend = GraphicsBackendPreference.Software
        };
        var bridge = CreateBridge(session, backendOptions, new VideraDiagnosticsOptions());

        bridge.OnSizeChanged(128, 96, 1f);
        session.Engine.AddObject(DemoMeshFactory.CreateWhiteQuad(session.ResourceFactory!));
        session.RenderOnce();

        var snapshot = session.OrchestrationSnapshot.LastPipelineSnapshot;
        snapshot.Should().NotBeNull();

        var diagnostics = bridge.CreateDiagnosticsSnapshot(lastInitializationError: null, DefaultSceneDiagnostics);
        diagnostics.RenderPipelineProfile.Should().Be(snapshot!.Profile.ToString());
        diagnostics.LastFrameStageNames.Should().NotBeNull();
        diagnostics.LastFrameStageNames.Should().Contain("PrepareFrame");
        diagnostics.LastFrameStageNames.Should().Contain("PresentFrame");
        diagnostics.LastFrameFeatureNames.Should().NotBeNull();
        diagnostics.LastFrameFeatureNames.Should().Contain("Overlay");
        diagnostics.LastFrameObjectCount.Should().Be(1);
        diagnostics.LastFrameOpaqueObjectCount.Should().Be(1);
        diagnostics.LastFrameTransparentObjectCount.Should().Be(0);
        diagnostics.SupportedRenderFeatureNames.Should().NotBeNull();
        diagnostics.SupportedRenderFeatureNames.Should().Equal("Opaque", "Transparent", "Overlay", "Picking", "Screenshot");
        diagnostics.TransparentFeatureStatus.Should().Be(VideraBackendDiagnostics.CurrentTransparentFeatureStatus);
        diagnostics.SupportsPassContributors.Should().BeTrue();
        diagnostics.SupportsPassReplacement.Should().BeTrue();
        diagnostics.SupportsFrameHooks.Should().BeTrue();
        diagnostics.SupportsPipelineSnapshots.Should().BeTrue();
        diagnostics.SupportsShaderCreation.Should().BeFalse();
        diagnostics.SupportsResourceSetCreation.Should().BeFalse();
        diagnostics.SupportsResourceSetBinding.Should().BeFalse();
    }

    [Fact]
    public void TransparentGeometry_ProjectsTransparentEvidenceIntoDiagnosticsSnapshot()
    {
        using var engine = new VideraEngine();
        using var session = new RenderSession(
            engine,
            backendFactory: static _ => new TrackingSoftwareBackend(),
            bitmapFactory: static (_, _) => null);
        var backendOptions = new VideraBackendOptions
        {
            PreferredBackend = GraphicsBackendPreference.Software
        };
        var bridge = CreateBridge(session, backendOptions, new VideraDiagnosticsOptions());

        bridge.OnSizeChanged(128, 96, 1f);
        engine.AddObject(DemoMeshFactory.CreateBlendedQuad(new RgbaFloat(1f, 0f, 0f, 0.5f), Vector3.Zero));
        session.RenderOnce();

        var diagnostics = bridge.CreateDiagnosticsSnapshot(lastInitializationError: null, DefaultSceneDiagnostics);

        diagnostics.LastFrameFeatureNames.Should().Equal("Transparent", "Overlay");
        diagnostics.LastFrameObjectCount.Should().Be(1);
        diagnostics.LastFrameOpaqueObjectCount.Should().Be(0);
        diagnostics.LastFrameTransparentObjectCount.Should().Be(1);
        diagnostics.TransparentFeatureStatus.Should().Be(VideraBackendDiagnostics.CurrentTransparentFeatureStatus);
        diagnostics.SupportedRenderFeatureNames.Should().Equal("Opaque", "Transparent", "Overlay", "Picking", "Screenshot");
    }

    private static VideraViewSessionBridge CreateBridge(
        RenderSession session,
        VideraBackendOptions backendOptions,
        VideraDiagnosticsOptions diagnosticsOptions)
    {
        return new VideraViewSessionBridge(
            session,
            isPreferredBackendOverrideSet: static () => false,
            preferredBackendValue: static () => GraphicsBackendPreference.Auto,
            backendOptionsAccessor: () => backendOptions,
            diagnosticsOptionsAccessor: () => diagnosticsOptions);
    }

    private static SceneResidencyDiagnostics DefaultSceneDiagnostics => new(
        SceneDocumentVersion: 0,
        PendingUploads: 0,
        PendingUploadBytes: 0,
        ResidentObjects: 0,
        DirtyObjects: 0,
        FailedUploads: 0,
        LastUploadedObjects: 0,
        LastUploadedBytes: 0,
        LastUploadFailures: 0,
        LastUploadDuration: TimeSpan.Zero,
        LastBudgetMaxObjects: 0,
        LastBudgetMaxBytes: 0);

    [Fact]
    public void DiagnosticsSnapshot_Projects_scene_upload_telemetry()
    {
        using var engine = new VideraEngine();
        using var session = new RenderSession(
            engine,
            backendFactory: static _ => new TrackingSoftwareBackend(),
            bitmapFactory: static (_, _) => null);
        var bridge = CreateBridge(session, new VideraBackendOptions(), new VideraDiagnosticsOptions());
        var diagnostics = bridge.CreateDiagnosticsSnapshot(
            lastInitializationError: null,
            new SceneResidencyDiagnostics(
                SceneDocumentVersion: 3,
                PendingUploads: 2,
                PendingUploadBytes: 4096,
                ResidentObjects: 4,
                DirtyObjects: 1,
                FailedUploads: 0,
                LastUploadedObjects: 1,
                LastUploadedBytes: 2048,
                LastUploadFailures: 0,
                LastUploadDuration: TimeSpan.FromMilliseconds(12),
                LastBudgetMaxObjects: 2,
                LastBudgetMaxBytes: 16384));

        diagnostics.PendingSceneUploadBytes.Should().Be(4096);
        diagnostics.LastFrameUploadedObjects.Should().Be(1);
        diagnostics.LastFrameUploadedBytes.Should().Be(2048);
        diagnostics.LastFrameUploadFailures.Should().Be(0);
        diagnostics.LastFrameUploadDuration.Should().Be(TimeSpan.FromMilliseconds(12));
        diagnostics.ResolvedUploadBudgetObjects.Should().Be(2);
        diagnostics.ResolvedUploadBudgetBytes.Should().Be(16384);
    }

    private sealed class TrackingSoftwareBackend : IGraphicsBackend, ISoftwareBackend
    {
        private readonly TrackingResourceFactory _resourceFactory = new();
        private readonly TrackingCommandExecutor _commandExecutor = new();

        public bool IsInitialized { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public void Initialize(IntPtr windowHandle, int width, int height)
        {
            _ = windowHandle;
            IsInitialized = true;
            Width = width;
            Height = height;
        }

        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;
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

        public void CopyFrameTo(IntPtr destination, int destinationStride)
        {
            _ = destination;
            _ = destinationStride;
        }

        public void Dispose()
        {
            IsInitialized = false;
        }
    }

    private sealed class TrackingNativeBackend : IGraphicsBackend
    {
        private readonly TrackingResourceFactory _resourceFactory = new();
        private readonly TrackingCommandExecutor _commandExecutor = new();

        public bool IsInitialized { get; private set; }

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
            _ = pipeline;
        }

        public void SetVertexBuffer(IBuffer buffer, uint index = 0)
        {
            _ = buffer;
            _ = index;
        }

        public void SetIndexBuffer(IBuffer buffer)
        {
            _ = buffer;
        }

        public void SetResourceSet(uint slot, IResourceSet resourceSet)
        {
            _ = slot;
            _ = resourceSet;
        }

        public void DrawIndexed(uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
        {
            _ = indexCount;
            _ = instanceCount;
            _ = firstIndex;
            _ = vertexOffset;
            _ = firstInstance;
        }

        public void DrawIndexed(uint primitiveType, uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
        {
            _ = primitiveType;
            _ = indexCount;
            _ = instanceCount;
            _ = firstIndex;
            _ = vertexOffset;
            _ = firstInstance;
        }

        public void Draw(uint vertexCount, uint instanceCount = 1, uint firstVertex = 0, uint firstInstance = 0)
        {
            _ = vertexCount;
            _ = instanceCount;
            _ = firstVertex;
            _ = firstInstance;
        }

        public void SetViewport(float x, float y, float width, float height, float minDepth = 0f, float maxDepth = 1f)
        {
            _ = x;
            _ = y;
            _ = width;
            _ = height;
            _ = minDepth;
            _ = maxDepth;
        }

        public void SetScissorRect(int x, int y, int width, int height)
        {
            _ = x;
            _ = y;
            _ = width;
            _ = height;
        }

        public void Clear(float r, float g, float b, float a)
        {
            _ = r;
            _ = g;
            _ = b;
            _ = a;
        }

        public void SetDepthState(bool testEnabled, bool writeEnabled)
        {
            _ = testEnabled;
            _ = writeEnabled;
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
            _ = data;
        }

        public void UpdateArray<T>(T[] data) where T : unmanaged
        {
            _ = data;
        }

        public void SetData<T>(T data, uint offset) where T : unmanaged
        {
            _ = data;
            _ = offset;
        }

        public void SetData<T>(T[] data, uint offset) where T : unmanaged
        {
            _ = data;
            _ = offset;
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
