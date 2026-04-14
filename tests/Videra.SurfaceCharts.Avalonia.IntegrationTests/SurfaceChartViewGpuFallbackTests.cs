using System.Reflection;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;
using Videra.SurfaceCharts.Rendering.Software;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartViewGpuFallbackTests
{
    [Fact]
    public Task SurfaceChartView_CreatesChartLocalNativeHostOnlyForGpuStatus()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var renderHost = new SurfaceChartRenderHost(
                softwareBackend: new SurfaceChartSoftwareRenderBackend(),
                gpuBackend: new SurfaceChartGpuRenderBackend(new FakeGraphicsBackend()),
                allowSoftwareFallback: true);
            var nativeHostFactory = new RecordingSurfaceChartNativeHostFactory(new IntPtr(0x4321));
            var view = new TestSurfaceChartView(renderHost, nativeHostFactory);
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 6f);
            var statusChangedCount = 0;

            view.RenderStatusChanged += (_, _) => statusChangedCount++;
            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Source = source;

            await WaitForRenderingStatusAsync(
                view,
                static status => status.ActiveBackend == SurfaceChartRenderBackendKind.Gpu && status.UsesNativeSurface);

            view.RenderingStatus.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Gpu);
            view.RenderingStatus.UsesNativeSurface.Should().BeTrue();
            statusChangedCount.Should().BeGreaterThan(0);
            GetPrivateField(view, "_nativeHost").Should().NotBeNull();
        });
    }

    [Fact]
    public Task SurfaceChartView_ExposesFallbackTruth_AndKeepsChartLocalOverlayPath_WhenGpuFallsBack()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var renderHost = new SurfaceChartRenderHost(
                softwareBackend: new SurfaceChartSoftwareRenderBackend(),
                gpuBackend: new SurfaceChartGpuRenderBackend(
                    new FakeGraphicsBackend(initializeException: new InvalidOperationException("gpu init failed"))),
                allowSoftwareFallback: true);
            var nativeHostFactory = new RecordingSurfaceChartNativeHostFactory(new IntPtr(0x4321));
            var view = new TestSurfaceChartView(renderHost, nativeHostFactory);
            var source = new ScriptedSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata(), defaultTileValue: 8f);
            var statusChangedCount = 0;

            view.RenderStatusChanged += (_, _) => statusChangedCount++;
            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Source = source;

            await WaitForRenderingStatusAsync(
                view,
                static status => status.ActiveBackend == SurfaceChartRenderBackendKind.Software && status.IsFallback);

            view.RenderingStatus.ActiveBackend.Should().Be(SurfaceChartRenderBackendKind.Software);
            view.RenderingStatus.IsFallback.Should().BeTrue();
            view.RenderingStatus.FallbackReason.Should().Contain("gpu init failed");
            view.RenderingStatus.UsesNativeSurface.Should().BeFalse();
            statusChangedCount.Should().BeGreaterThan(0);
            GetPrivateField(view, "_nativeHost").Should().BeNull();

            GetCollectionCount(GetPrivateField(view, "_axisOverlayState"), "Axes").Should().BeGreaterThan(0);
            GetCollectionCount(GetPrivateField(view, "_legendOverlayState"), "Swatches").Should().BeGreaterThan(0);
        });
    }

    private static async Task WaitForRenderingStatusAsync(
        SurfaceChartView view,
        Func<SurfaceChartRenderingStatus, bool> predicate,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(predicate);

        timeout ??= TimeSpan.FromSeconds(2);
        var deadline = DateTime.UtcNow + timeout.Value;

        while (DateTime.UtcNow < deadline)
        {
            if (predicate(view.RenderingStatus))
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
        }

        predicate(view.RenderingStatus).Should().BeTrue();
    }

    private static object? GetPrivateField(object instance, string fieldName)
    {
        for (var currentType = instance.GetType(); currentType is not null; currentType = currentType.BaseType)
        {
            var field = currentType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is not null)
            {
                return field.GetValue(instance);
            }
        }

        return null;
    }

    private static int GetCollectionCount(object? instance, string propertyName)
    {
        instance.Should().NotBeNull();
        var property = instance!.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        property.Should().NotBeNull();
        var value = property!.GetValue(instance);
        value.Should().BeAssignableTo<System.Collections.ICollection>();
        return ((System.Collections.ICollection)value!).Count;
    }

    private sealed class TestSurfaceChartView : SurfaceChartView
    {
        public TestSurfaceChartView(SurfaceChartRenderHost renderHost, ISurfaceChartNativeHostFactory nativeHostFactory)
            : base(renderHost, nativeHostFactory)
        {
        }
    }

    private sealed class RecordingSurfaceChartNativeHostFactory : ISurfaceChartNativeHostFactory
    {
        private readonly RecordingSurfaceChartNativeHost _host;

        public RecordingSurfaceChartNativeHostFactory(IntPtr currentHandle)
        {
            _host = new RecordingSurfaceChartNativeHost(currentHandle);
        }

        public ISurfaceChartNativeHost? CreateHost()
        {
            return _host;
        }
    }

    private sealed class RecordingSurfaceChartNativeHost : NativeControlHost, ISurfaceChartNativeHost
    {
        public RecordingSurfaceChartNativeHost(IntPtr currentHandle)
        {
            CurrentHandle = currentHandle;
        }

        public event Action<IntPtr>? HandleCreated;

        public event Action? HandleDestroyed;

        public IntPtr CurrentHandle { get; }
    }

    private sealed class FakeGraphicsBackend : IGraphicsBackend
    {
        private readonly Exception? _initializeException;

        public FakeGraphicsBackend(Exception? initializeException = null)
        {
            _initializeException = initializeException;
        }

        public bool IsInitialized { get; private set; }

        public IResourceFactory GetResourceFactory()
        {
            return new FakeResourceFactory();
        }

        public ICommandExecutor GetCommandExecutor()
        {
            return new FakeCommandExecutor();
        }

        public void Initialize(IntPtr windowHandle, int width, int height)
        {
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
        public IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices)
        {
            return new FakeBuffer((uint)(vertices.Length * 40));
        }

        public IBuffer CreateVertexBuffer(uint sizeInBytes)
        {
            return new FakeBuffer(sizeInBytes);
        }

        public IBuffer CreateIndexBuffer(uint[] indices)
        {
            return new FakeBuffer((uint)(indices.Length * sizeof(uint)));
        }

        public IBuffer CreateIndexBuffer(uint sizeInBytes)
        {
            return new FakeBuffer(sizeInBytes);
        }

        public IBuffer CreateUniformBuffer(uint sizeInBytes)
        {
            return new FakeBuffer(sizeInBytes);
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
        public FakeBuffer(uint sizeInBytes)
        {
            SizeInBytes = sizeInBytes;
        }

        public uint SizeInBytes { get; }

        public void Update<T>(T data)
            where T : unmanaged
        {
        }

        public void UpdateArray<T>(T[] data)
            where T : unmanaged
        {
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
