using System.Numerics;
using System.Runtime.InteropServices;
using FluentAssertions;
using Tests.Common.Platform;
using Videra.Core.Exceptions;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Platform.Linux;
using Xunit;

namespace Videra.Platform.Linux.Tests.Backend;

/// <summary>
/// Granular lifecycle tests for VulkanBackend running against a real X11 window on Linux.
/// These tests exercise individual lifecycle transitions and edge cases.
/// They only execute on Linux hosts with X11 and Vulkan drivers available.
/// </summary>
public sealed class VulkanBackendLifecycleTests
{
    private static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    [LinuxFact]
    public void Dispose_WithoutInitialize_DoesNotThrow()
    {
        if (!IsLinux) return;

        var act = () =>
        {
            using var backend = new VulkanBackend();
        };

        act.Should().NotThrow();
    }

    [LinuxFact]
    public void DoubleDispose_DoesNotThrow()
    {
        if (!IsLinux) return;

        using var window = NativeHostTestHelpers.CreateHiddenX11Window();
        using var backend = new VulkanBackend();
        backend.Initialize(window.WindowHandle, 64, 64);

        backend.Dispose();

        var act = () => backend.Dispose();
        act.Should().NotThrow();
    }

    [LinuxFact]
    public void Initialize_WithZeroHandle_ThrowsPlatformDependencyException()
    {
        if (!IsLinux) return;

        using var backend = new VulkanBackend();

        var act = () => backend.Initialize(IntPtr.Zero, 64, 64);

        act.Should().Throw<PlatformDependencyException>()
            .Which.Operation.Should().Be("Initialize");
        backend.IsInitialized.Should().BeFalse();
    }

    [LinuxFact]
    public void Initialize_SecondCall_IsIdempotent()
    {
        if (!IsLinux) return;

        using var window = NativeHostTestHelpers.CreateHiddenX11Window();
        using var backend = new VulkanBackend();
        backend.Initialize(window.WindowHandle, 64, 64);

        var act = () => backend.Initialize(window.WindowHandle, 128, 96);

        act.Should().NotThrow();
        backend.IsInitialized.Should().BeTrue();
    }

    [LinuxFact]
    public void Initialize_WithRealX11Window_SetsInitialized()
    {
        if (!IsLinux) return;

        using var window = NativeHostTestHelpers.CreateHiddenX11Window();
        using var backend = new VulkanBackend();

        backend.Initialize(window.WindowHandle, 64, 64);

        backend.IsInitialized.Should().BeTrue();
        backend.GetResourceFactory().Should().NotBeNull();
        backend.GetCommandExecutor().Should().NotBeNull();
    }

    [LinuxFact]
    public void Resize_AfterInit_Succeeds()
    {
        if (!IsLinux) return;

        using var window = NativeHostTestHelpers.CreateHiddenX11Window();
        using var backend = new VulkanBackend();
        backend.Initialize(window.WindowHandle, 64, 64);

        var act = () => backend.Resize(128, 96);

        act.Should().NotThrow();
    }

    [LinuxFact]
    public void MultipleFrameCycles_CompleteSuccessfully()
    {
        if (!IsLinux) return;

        using var window = NativeHostTestHelpers.CreateHiddenX11Window();
        using var backend = new VulkanBackend();
        backend.Initialize(window.WindowHandle, 64, 64);

        var act = () =>
        {
            for (int i = 0; i < 3; i++)
            {
                backend.BeginFrame();
                backend.EndFrame();
            }
        };

        act.Should().NotThrow();
    }

    [LinuxFact]
    public void ResourceCreation_AndDrawPath_Succeeds()
    {
        if (!IsLinux) return;

        using var window = NativeHostTestHelpers.CreateHiddenX11Window();
        using var backend = new VulkanBackend();
        backend.Initialize(window.WindowHandle, 128, 96);

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
        using var cameraBuffer = factory.CreateUniformBuffer(128);
        using var worldBuffer = factory.CreateUniformBuffer(64);
        using var pipeline = factory.CreatePipeline(VertexPositionNormalColor.SizeInBytes, hasNormals: true, hasColors: true);

        cameraBuffer.SetData(Matrix4x4.Identity, 0);
        cameraBuffer.SetData(Matrix4x4.Identity, 64);
        worldBuffer.SetData(Matrix4x4.Identity, 0);

        var act = () =>
        {
            backend.BeginFrame();
            executor.SetPipeline(pipeline);
            executor.SetVertexBuffer(vb, RenderBindingSlots.Vertex);
            executor.SetVertexBuffer(cameraBuffer, RenderBindingSlots.Camera);
            executor.SetVertexBuffer(worldBuffer, RenderBindingSlots.World);
            executor.SetIndexBuffer(ib);
            executor.DrawIndexed(3);
            backend.EndFrame();
        };

        act.Should().NotThrow();
    }

    [LinuxFact]
    public void Backend_Reinitialization_AfterDispose_Succeeds()
    {
        if (!IsLinux) return;

        using var window = NativeHostTestHelpers.CreateHiddenX11Window();
        var backend = new VulkanBackend();
        backend.Initialize(window.WindowHandle, 64, 64);
        backend.IsInitialized.Should().BeTrue();
        backend.Dispose();
        backend.IsInitialized.Should().BeFalse();

        using var backend2 = new VulkanBackend();
        backend2.Initialize(window.WindowHandle, 64, 64);
        backend2.IsInitialized.Should().BeTrue();
    }
}
