using System.Numerics;
using System.Runtime.InteropServices;
using FluentAssertions;
using Tests.Common.Platform;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Platform.macOS;
using Xunit;

namespace Videra.Platform.macOS.Tests.Backend;

/// <summary>
/// Granular lifecycle tests for MetalBackend running against a real NSView on macOS.
/// These tests exercise individual lifecycle transitions and edge cases.
/// They only execute on macOS hosts with the Metal framework available.
/// </summary>
public sealed class MetalBackendLifecycleTests
{
    private static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    [MacOSFact]
    public void Dispose_WithoutInitialize_DoesNotThrow()
    {
        if (!IsMacOS) return;

        var act = () =>
        {
            using var backend = new MetalBackend();
        };

        act.Should().NotThrow();
    }

    [MacOSFact]
    public void DoubleDispose_DoesNotThrow()
    {
        if (!IsMacOS) return;

        using var window = NativeHostTestHelpers.CreateHiddenNSViewWindow();
        using var backend = new MetalBackend();
        backend.Initialize(window.ViewHandle, 64, 64);

        backend.Dispose();

        var act = () => backend.Dispose();
        act.Should().NotThrow();
    }

    [MacOSFact]
    public void Initialize_WithRealNSView_SetsInitialized()
    {
        if (!IsMacOS) return;

        using var window = NativeHostTestHelpers.CreateHiddenNSViewWindow();
        using var backend = new MetalBackend();

        backend.Initialize(window.ViewHandle, 64, 64);

        backend.IsInitialized.Should().BeTrue();
        backend.GetResourceFactory().Should().NotBeNull();
        backend.GetCommandExecutor().Should().NotBeNull();
    }

    [MacOSFact]
    public void Initialize_SecondCall_IsIdempotent()
    {
        if (!IsMacOS) return;

        using var window = NativeHostTestHelpers.CreateHiddenNSViewWindow();
        using var backend = new MetalBackend();
        backend.Initialize(window.ViewHandle, 64, 64);

        var act = () => backend.Initialize(window.ViewHandle, 128, 96);

        act.Should().NotThrow();
        backend.IsInitialized.Should().BeTrue();
    }

    [MacOSFact]
    public void Resize_AfterInit_Succeeds()
    {
        if (!IsMacOS) return;

        using var window = NativeHostTestHelpers.CreateHiddenNSViewWindow();
        using var backend = new MetalBackend();
        backend.Initialize(window.ViewHandle, 64, 64);

        var act = () => backend.Resize(128, 96);

        act.Should().NotThrow();
    }

    [MacOSFact]
    public void MultipleFrameCycles_CompleteSuccessfully()
    {
        if (!IsMacOS) return;

        using var window = NativeHostTestHelpers.CreateHiddenNSViewWindow();
        using var backend = new MetalBackend();
        backend.Initialize(window.ViewHandle, 64, 64);

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

    [MacOSFact]
    public void ResourceCreation_AndDrawPath_Succeeds()
    {
        if (!IsMacOS) return;

        using var window = NativeHostTestHelpers.CreateHiddenNSViewWindow();
        using var backend = new MetalBackend();
        backend.Initialize(window.ViewHandle, 128, 96);

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

    [MacOSFact]
    public void Backend_Reinitialization_AfterDispose_Succeeds()
    {
        if (!IsMacOS) return;

        using var window = NativeHostTestHelpers.CreateHiddenNSViewWindow();
        var backend = new MetalBackend();
        backend.Initialize(window.ViewHandle, 64, 64);
        backend.IsInitialized.Should().BeTrue();
        backend.Dispose();
        backend.IsInitialized.Should().BeFalse();

        using var backend2 = new MetalBackend();
        backend2.Initialize(window.ViewHandle, 64, 64);
        backend2.IsInitialized.Should().BeTrue();

        var act = () =>
        {
            backend2.BeginFrame();
            backend2.EndFrame();
        };

        act.Should().NotThrow();
    }
}
