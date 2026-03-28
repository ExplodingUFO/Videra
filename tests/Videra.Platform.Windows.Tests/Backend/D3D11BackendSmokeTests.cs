using FluentAssertions;
using System.Runtime.InteropServices;
using Tests.Common.Platform;
using Videra.Platform.Windows;
using Xunit;

namespace Videra.Platform.Windows.Tests.Backend;

public sealed class D3D11BackendSmokeTests
{
    [Fact]
    public void D3D11Backend_ConstructedBackend_StartsUninitialized()
    {
        using var backend = new D3D11Backend();

        backend.IsInitialized.Should().BeFalse();
    }

    [Fact]
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
}
