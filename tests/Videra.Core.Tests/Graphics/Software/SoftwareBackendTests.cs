using System.Numerics;
using FluentAssertions;
using Videra.Core.Graphics.Software;
using Xunit;

namespace Videra.Core.Tests.Graphics.Software;

public class SoftwareBackendTests
{
    [Fact]
    public void Initialize_SetsIsInitialized()
    {
        using var backend = new SoftwareBackend();
        backend.IsInitialized.Should().BeFalse();

        backend.Initialize(IntPtr.Zero, 100, 100);

        backend.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void Initialize_SetsWidthAndHeight()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 150);

        backend.Width.Should().Be(200);
        backend.Height.Should().Be(150);
    }

    [Fact]
    public void Resize_UpdatesDimensions()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 100, 100);

        backend.Resize(200, 150);

        backend.Width.Should().Be(200);
        backend.Height.Should().Be(150);
    }

    [Fact]
    public void GetResourceFactory_ReturnsNonNull()
    {
        using var backend = new SoftwareBackend();
        var factory = backend.GetResourceFactory();

        factory.Should().NotBeNull();
    }

    [Fact]
    public void GetCommandExecutor_ReturnsNonNull()
    {
        using var backend = new SoftwareBackend();
        var executor = backend.GetCommandExecutor();

        executor.Should().NotBeNull();
    }

    [Fact]
    public void SetClearColor_DoesNotThrow()
    {
        using var backend = new SoftwareBackend();
        var act = () => backend.SetClearColor(new Vector4(0.5f, 0.5f, 0.5f, 1f));
        act.Should().NotThrow();
    }

    [Fact]
    public void BeginFrame_EndFrame_DoesNotThrow()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 50, 50);

        var act = () =>
        {
            backend.BeginFrame();
            backend.EndFrame();
        };
        act.Should().NotThrow();
    }

    [Fact]
    public void CopyFrameTo_ZeroDestination_DoesNotThrow()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 10, 10);

        var act = () => backend.CopyFrameTo(IntPtr.Zero, 40);
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var backend = new SoftwareBackend();
        var act = () =>
        {
            backend.Dispose();
            backend.Dispose();
        };
        act.Should().NotThrow();
    }
}
