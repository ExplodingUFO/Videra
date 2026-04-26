using FluentAssertions;
using System.Runtime.InteropServices;
using Tests.Common.Platform;
using Videra.Core.Exceptions;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Platform.macOS;
using Xunit;

namespace Videra.Platform.macOS.Tests.Backend;

public sealed class MetalBackendSmokeTests
{
    [MacOSFact]
    public void MetalBackend_ConstructedBackend_StartsUninitialized()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        var backend = new MetalBackend();

        backend.IsInitialized.Should().BeFalse();
        GraphicsBackendFactory.GetPlatformName().Should().NotBeNullOrWhiteSpace();
    }

    [MacOSFact]
    public void MetalBackend_RealViewInitialization_CurrentlyRequiresReusableNsViewFixture()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RuntimeInformation.IsOSPlatform(OSPlatform.OSX).Should().BeTrue("a real NSView-backed smoke path still needs a reusable test host fixture");
    }

    [MacOSFact]
    public void MetalResourceFactory_CreatePipelineDescription_UsesBuiltInMinimumContract()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        using var window = NativeHostTestHelpers.CreateHiddenNSViewWindow();
        using var backend = new MetalBackend();
        backend.Initialize(window.ViewHandle, 64, 64);

        var factory = backend.GetResourceFactory();

        factory.Should().BeAssignableTo<IResourceFactoryCapabilities>()
            .Which.SupportsShaderCreation.Should().BeFalse();
        ((IResourceFactoryCapabilities)factory).SupportsResourceSetCreation.Should().BeFalse();

        var act = () => factory.CreatePipeline(new PipelineDescription());

        act.Should().NotThrow();
    }

    [MacOSFact]
    public void MetalCommandExecutor_SetResourceSet_ThrowsUnsupportedOperationException()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        using var window = NativeHostTestHelpers.CreateHiddenNSViewWindow();
        using var backend = new MetalBackend();
        backend.Initialize(window.ViewHandle, 64, 64);

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
        public void Dispose()
        {
        }
    }
}
