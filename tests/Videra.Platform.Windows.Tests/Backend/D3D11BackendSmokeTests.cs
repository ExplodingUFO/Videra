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

public sealed class D3D11BackendSmokeTests
{
    [WindowsFact]
    public void D3D11Backend_ConstructedBackend_StartsUninitialized()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        using var backend = new D3D11Backend();

        backend.IsInitialized.Should().BeFalse();
    }

    [WindowsFact]
    public void D3D11Backend_InitializeWithRealHwnd_RunsLifecycleOnWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();

        backend.Initialize(window.Handle, 64, 64);

        backend.IsInitialized.Should().BeTrue();
        backend.GetResourceFactory().Should().NotBeNull();
        backend.GetCommandExecutor().Should().NotBeNull();

        var act = () =>
        {
            backend.Resize(96, 80);
            backend.BeginFrame();
            backend.EndFrame();
        };

        act.Should().NotThrow();
    }

    [WindowsFact]
    public void D3D11Backend_RealHwnd_AllowsResourceCreationAndDrawLifecycle_OnWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();

        backend.Initialize(window.Handle, 128, 96);

        var factory = backend.GetResourceFactory();
        var executor = backend.GetCommandExecutor();

        var vertices = new[]
        {
            new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Red),
            new VertexPositionNormalColor(new Vector3(0f, 0.5f, 0f), Vector3.UnitZ, RgbaFloat.Green),
            new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Blue)
        };
        var indices = new uint[] { 0, 1, 2 };

        using var vertexBuffer = factory.CreateVertexBuffer(vertices);
        using var indexBuffer = factory.CreateIndexBuffer(indices);
        using var cameraBuffer = factory.CreateUniformBuffer(128);
        using var worldBuffer = factory.CreateUniformBuffer(64);
        using var pipeline = factory.CreatePipeline(VertexPositionNormalColor.SizeInBytes, hasNormals: true, hasColors: true);

        cameraBuffer.SetData(Matrix4x4.Identity, 0);
        cameraBuffer.SetData(Matrix4x4.Identity, 64);
        worldBuffer.SetData(Matrix4x4.Identity, 0);

        var act = () =>
        {
            backend.BeginFrame();
            executor.Clear(0.05f, 0.1f, 0.15f, 1f);
            executor.SetViewport(0, 0, 128, 96);
            executor.SetPipeline(pipeline);
            executor.SetVertexBuffer(vertexBuffer, 0);
            executor.SetVertexBuffer(cameraBuffer, 1);
            executor.SetVertexBuffer(worldBuffer, 2);
            executor.SetIndexBuffer(indexBuffer);
            executor.DrawIndexed(3u);
            backend.EndFrame();
        };

        act.Should().NotThrow();
    }

    [WindowsFact]
    public void D3D11ResourceFactory_CreateShader_ThrowsUnsupportedOperationException()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();
        backend.Initialize(window.Handle, 64, 64);

        var factory = backend.GetResourceFactory();

        factory.Should().BeAssignableTo<IResourceFactoryCapabilities>()
            .Which.SupportsShaderCreation.Should().BeFalse();
        ((IResourceFactoryCapabilities)factory).SupportsResourceSetCreation.Should().BeFalse();

        var act = () => factory.CreateShader(ShaderStage.Vertex, Array.Empty<byte>(), "main");

        act.Should().Throw<UnsupportedOperationException>()
            .Which.Operation.Should().Be("CreateShader");
    }

    [WindowsFact]
    public void D3D11ResourceFactory_CreateResourceSet_ThrowsUnsupportedOperationException()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();
        backend.Initialize(window.Handle, 64, 64);

        var factory = backend.GetResourceFactory();
        var desc = new ResourceSetDescription();

        factory.Should().BeAssignableTo<IResourceFactoryCapabilities>()
            .Which.SupportsResourceSetCreation.Should().BeFalse();

        var act = () => factory.CreateResourceSet(desc);

        act.Should().Throw<UnsupportedOperationException>()
            .Which.Operation.Should().Be("CreateResourceSet");
    }

    [WindowsFact]
    public void D3D11CommandExecutor_SetResourceSet_ThrowsUnsupportedOperationException()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();
        backend.Initialize(window.Handle, 64, 64);

        var executor = backend.GetCommandExecutor();
        var mockResourceSet = new MockResourceSet();

        executor.Should().BeAssignableTo<ICommandExecutorCapabilities>()
            .Which.SupportsResourceSetBinding.Should().BeFalse();

        var act = () => executor.SetResourceSet(0, mockResourceSet);

        act.Should().Throw<UnsupportedOperationException>()
            .Which.Operation.Should().Be("SetResourceSet");
    }

    private sealed class MockResourceSet : IResourceSet
    {
        public void Dispose() { }
    }
}
