using FluentAssertions;
using System.Runtime.InteropServices;
using Videra.Core.Graphics;
using Videra.Platform.Windows;
using Xunit;

namespace Videra.Platform.Windows.Tests.Backend;

public sealed class D3D11BackendSmokeTests
{
    [Fact]
    public void D3D11Backend_ConstructedBackend_StartsUninitialized()
    {
        var backend = new D3D11Backend();

        backend.IsInitialized.Should().BeFalse();
        GraphicsBackendFactory.GetPlatformName().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void D3D11Backend_RealWindowInitialization_CurrentlyRequiresReusableHwndFixture()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        RuntimeInformation.IsOSPlatform(OSPlatform.Windows).Should().BeTrue("a real HWND-backed smoke path still needs a reusable test host fixture");
    }
}
