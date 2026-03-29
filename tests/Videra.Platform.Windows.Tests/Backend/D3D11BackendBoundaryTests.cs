using System.Runtime.InteropServices;
using FluentAssertions;
using Tests.Common.Platform;
using Videra.Core.Exceptions;
using Videra.Platform.Windows;
using Xunit;

namespace Videra.Platform.Windows.Tests.Backend;

public sealed class D3D11BackendBoundaryTests
{
    [Fact]
    public void Initialize_CalledWithZeroHandle_ThrowsPlatformDependencyException()
    {
        using var backend = new D3D11Backend();

        var act = () => backend.Initialize(IntPtr.Zero, 64, 64);

        act.Should().Throw<PlatformDependencyException>()
            .Which.Platform.Should().Be("Windows");
    }

    [Fact]
    public void Initialize_CalledWithZeroHandle_ExceptionContainsOperation()
    {
        using var backend = new D3D11Backend();

        var act = () => backend.Initialize(IntPtr.Zero, 64, 64);

        var ex = act.Should().Throw<PlatformDependencyException>().Which;
        ex.Operation.Should().Be("Initialize");
        ex.Message.Should().Contain("window handle");
    }

    [Fact]
    public void Initialize_CalledWithZeroWidth_ThrowsPlatformDependencyException()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();

        var act = () => backend.Initialize(window.Handle, 0, 64);

        act.Should().Throw<PlatformDependencyException>()
            .Which.Operation.Should().Be("Initialize");
    }

    [Fact]
    public void Initialize_CalledWithZeroHeight_ThrowsPlatformDependencyException()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();

        var act = () => backend.Initialize(window.Handle, 64, 0);

        act.Should().Throw<PlatformDependencyException>()
            .Which.Operation.Should().Be("Initialize");
    }

    [Fact]
    public void Initialize_CalledWithNegativeWidth_ThrowsPlatformDependencyException()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();

        var act = () => backend.Initialize(window.Handle, -1, 64);

        act.Should().Throw<PlatformDependencyException>();
    }

    [Fact]
    public void Initialize_CalledWithNegativeHeight_ThrowsPlatformDependencyException()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        using var window = NativeHostTestHelpers.CreateHiddenWin32Window();
        using var backend = new D3D11Backend();

        var act = () => backend.Initialize(window.Handle, 64, -1);

        act.Should().Throw<PlatformDependencyException>();
    }
}
