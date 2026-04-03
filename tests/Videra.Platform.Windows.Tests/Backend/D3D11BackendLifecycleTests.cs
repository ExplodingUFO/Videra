using System.Numerics;
using System.Runtime.InteropServices;
using FluentAssertions;
using Tests.Common.Platform;
using Videra.Core.Exceptions;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Platform.Windows;
using Xunit;

namespace Videra.Platform.Windows.Tests.Backend;

/// <summary>
/// Granular lifecycle tests for D3D11Backend running against a real Win32 HWND on Windows.
/// These tests exercise individual lifecycle transitions and edge cases that complement
/// the broader smoke tests in <see cref="D3D11BackendSmokeTests"/>.
/// </summary>
public sealed class D3D11BackendLifecycleTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void Dispose_WithoutInitialize_DoesNotThrow()
    {
        if (!IsWindows) return;

        var act = () =>
        {
            using var backend = new D3D11Backend();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void DoubleDispose_DoesNotThrow()
    {
        if (!IsWindows) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();
        backend.Initialize(window.Handle, 64, 64);

        backend.Dispose();

        var act = () => backend.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void Initialize_WithZeroHandle_ThrowsPlatformDependencyException()
    {
        if (!IsWindows) return;

        using var backend = new D3D11Backend();

        var act = () => backend.Initialize(IntPtr.Zero, 64, 64);

        act.Should().Throw<PlatformDependencyException>()
            .Which.Operation.Should().Be("Initialize");
    }

    [Fact]
    public void Initialize_WithZeroDimensions_ThrowsPlatformDependencyException()
    {
        if (!IsWindows) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();

        var act = () => backend.Initialize(window.Handle, 0, 64);

        act.Should().Throw<PlatformDependencyException>();
        backend.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public void Initialize_SecondCall_IsIdempotent()
    {
        if (!IsWindows) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();
        backend.Initialize(window.Handle, 64, 64);

        var act = () => backend.Initialize(window.Handle, 128, 96);

        act.Should().NotThrow();
        backend.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void Resize_AfterInit_Succeeds()
    {
        if (!IsWindows) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();
        backend.Initialize(window.Handle, 64, 64);

        var act = () => backend.Resize(128, 96);

        act.Should().NotThrow();
    }

    [Fact]
    public void Resize_WithInjectedResizeBuffersFailure_ThrowsGraphicsInitializationException()
    {
        if (!IsWindows) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        var hooks = new D3D11BackendTestHooks
        {
            ResizeBuffersOverride = (_, _) => unchecked((int)0x887A0001)
        };
        using var backend = new D3D11Backend(hooks);
        backend.Initialize(window.Handle, 64, 64);

        var act = () => backend.Resize(128, 96);

        act.Should().Throw<GraphicsInitializationException>()
            .Which.Operation.Should().Be("Resize");
    }

    [Fact]
    public void Resize_WithZeroDimensions_IsSilentlyIgnored()
    {
        if (!IsWindows) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();
        backend.Initialize(window.Handle, 64, 64);

        var act = () => backend.Resize(0, 0);

        act.Should().NotThrow();
    }

    [Fact]
    public void SetClearColor_DoesNotThrow()
    {
        if (!IsWindows) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();
        backend.Initialize(window.Handle, 64, 64);

        var act = () => backend.SetClearColor(new Vector4(0.5f, 0.5f, 0.5f, 1.0f));

        act.Should().NotThrow();
    }

    [Fact]
    public void MultipleFrameCycles_CompleteSuccessfully()
    {
        if (!IsWindows) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();
        backend.Initialize(window.Handle, 64, 64);

        var act = () =>
        {
            for (int i = 0; i < 5; i++)
            {
                backend.BeginFrame();
                backend.EndFrame();
            }
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void ResourceCreation_AfterResize_Succeeds()
    {
        if (!IsWindows) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();
        backend.Initialize(window.Handle, 64, 64);

        backend.Resize(128, 96);

        var factory = backend.GetResourceFactory();
        var vertices = new[]
        {
            new VertexPositionNormalColor(new Vector3(0, 0, 0), Vector3.UnitZ, RgbaFloat.White)
        };

        using var vb = factory.CreateVertexBuffer(vertices);

        vb.Should().NotBeNull();
    }

    [Fact]
    public void DrawIndexed_AfterResize_CompletesWithoutError()
    {
        if (!IsWindows) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();
        backend.Initialize(window.Handle, 64, 64);

        var factory = backend.GetResourceFactory();
        var executor = backend.GetCommandExecutor();

        var vertices = new[]
        {
            new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Red),
            new VertexPositionNormalColor(new Vector3(0f, 0.5f, 0f), Vector3.UnitZ, RgbaFloat.Green),
            new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Blue)
        };
        var indices = new uint[] { 0, 1, 2 };

        using var vb = factory.CreateVertexBuffer(vertices);
        using var ib = factory.CreateIndexBuffer(indices);
        using var pipeline = factory.CreatePipeline(VertexPositionNormalColor.SizeInBytes, hasNormals: true, hasColors: true);

        backend.Resize(128, 96);

        var act = () =>
        {
            backend.BeginFrame();
            executor.SetPipeline(pipeline);
            executor.SetVertexBuffer(vb, 0);
            executor.SetIndexBuffer(ib);
            executor.DrawIndexed(3);
            backend.EndFrame();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void MultipleResizeCycles_ThenDraw_CompletesWithoutError()
    {
        if (!IsWindows) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();
        backend.Initialize(window.Handle, 64, 64);

        for (var i = 0; i < 3; i++)
        {
            backend.Resize(128 + (i * 16), 96 + (i * 8));
            backend.BeginFrame();
            backend.EndFrame();
        }

        var factory = backend.GetResourceFactory();
        var executor = backend.GetCommandExecutor();
        var vertices = new[]
        {
            new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Red),
            new VertexPositionNormalColor(new Vector3(0f, 0.5f, 0f), Vector3.UnitZ, RgbaFloat.Green),
            new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Blue)
        };
        var indices = new uint[] { 0, 1, 2 };

        using var vb = factory.CreateVertexBuffer(vertices);
        using var ib = factory.CreateIndexBuffer(indices);
        using var pipeline = factory.CreatePipeline(VertexPositionNormalColor.SizeInBytes, hasNormals: true, hasColors: true);

        var act = () =>
        {
            backend.BeginFrame();
            executor.SetPipeline(pipeline);
            executor.SetVertexBuffer(vb, 0);
            executor.SetIndexBuffer(ib);
            executor.DrawIndexed(3);
            backend.EndFrame();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void UniformBuffer_UpdateAndBind_Succeeds()
    {
        if (!IsWindows) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();
        backend.Initialize(window.Handle, 64, 64);

        var factory = backend.GetResourceFactory();
        var executor = backend.GetCommandExecutor();

        using var ub = factory.CreateUniformBuffer(128);
        ub.SetData(Matrix4x4.Identity, 0);
        ub.SetData(Matrix4x4.Identity, 64);

        using var pipeline = factory.CreatePipeline(VertexPositionNormalColor.SizeInBytes, hasNormals: true, hasColors: true);

        var act = () =>
        {
            backend.BeginFrame();
            executor.SetPipeline(pipeline);
            executor.SetVertexBuffer(ub, 1);
            backend.EndFrame();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void Backend_Reinitialization_AfterDispose_Succeeds()
    {
        if (!IsWindows) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        var backend = new D3D11Backend();
        backend.Initialize(window.Handle, 64, 64);
        backend.IsInitialized.Should().BeTrue();
        backend.Dispose();
        backend.IsInitialized.Should().BeFalse();

        using var backend2 = new D3D11Backend();
        backend2.Initialize(window.Handle, 64, 64);
        backend2.IsInitialized.Should().BeTrue();
    }
}
