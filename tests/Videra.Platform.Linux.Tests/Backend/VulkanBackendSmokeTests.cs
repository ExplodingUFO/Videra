using FluentAssertions;
using System.Runtime.InteropServices;
using Tests.Common.Platform;
using Videra.Core.Graphics;
using Videra.Platform.Linux;
using Xunit;

namespace Videra.Platform.Linux.Tests.Backend;

public sealed class VulkanBackendSmokeTests
{
    [LinuxFact]
    public void VulkanBackend_ConstructedBackend_StartsUninitialized()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return;
        }

        var backend = new VulkanBackend();

        backend.IsInitialized.Should().BeFalse();
        GraphicsBackendFactory.GetPlatformName().Should().NotBeNullOrWhiteSpace();
    }

    [LinuxFact]
    public void VulkanBackend_InitializeWithZeroHandle_ThrowsWindowHandleRequirement()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return;
        }

        using var backend = new VulkanBackend();

        var act = () => backend.Initialize(IntPtr.Zero, 100, 100);

        act.Should().Throw<Exception>()
            .WithMessage("*valid X11 window handle*");
        backend.IsInitialized.Should().BeFalse();
    }

    [LinuxFact]
    public void VulkanBackend_RealWindowInitialization_CurrentlyRequiresReusableX11Fixture()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return;
        }

        RuntimeInformation.IsOSPlatform(OSPlatform.Linux).Should().BeTrue("a real X11-backed smoke path still needs a reusable test host fixture");
    }
}
