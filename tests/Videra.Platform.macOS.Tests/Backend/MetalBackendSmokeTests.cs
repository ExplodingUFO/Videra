using FluentAssertions;
using System.Runtime.InteropServices;
using Videra.Core.Graphics;
using Videra.Platform.macOS;
using Xunit;

namespace Videra.Platform.macOS.Tests.Backend;

public sealed class MetalBackendSmokeTests
{
    [Fact]
    public void MetalBackend_ConstructedBackend_StartsUninitialized()
    {
        var backend = new MetalBackend();

        backend.IsInitialized.Should().BeFalse();
        GraphicsBackendFactory.GetPlatformName().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void MetalBackend_RealViewInitialization_CurrentlyRequiresReusableNsViewFixture()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RuntimeInformation.IsOSPlatform(OSPlatform.OSX).Should().BeTrue("a real NSView-backed smoke path still needs a reusable test host fixture");
    }
}
