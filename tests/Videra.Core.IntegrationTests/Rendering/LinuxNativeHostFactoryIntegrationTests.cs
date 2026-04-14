using Avalonia;
using Avalonia.Platform;
using FluentAssertions;
using Tests.Common.Platform;
using Videra.Avalonia.Controls.Linux;
using Videra.Core.Exceptions;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

[Collection(ProcessEnvironmentCollection.Name)]
public sealed class LinuxNativeHostFactoryIntegrationTests
{
    [Fact]
    public void WaylandSession_WithX11Fallback_ResolvesXWaylandCompatibilityPath()
    {
        using var environment = LinuxDisplayEnvironmentScope.Override(
            waylandDisplay: "wayland-0",
            x11Display: ":99",
            sessionType: "wayland");

        var factory = new LinuxNativeHostFactory(
            x11HostFactory: static () => new FakeLinuxPlatformNativeHost());

        var selection = factory.CreateHost();

        selection.ResolvedDisplayServer.Should().Be("XWayland");
        selection.FallbackUsed.Should().BeTrue();
        selection.FallbackReason.Should().Contain("XWayland");
    }

    [Fact]
    public void WaylandOnlySession_ThrowsHelpfulCompatibilityMessage()
    {
        using var environment = LinuxDisplayEnvironmentScope.Override(
            waylandDisplay: "wayland-0",
            x11Display: null,
            sessionType: "wayland");

        var factory = new LinuxNativeHostFactory(
            x11HostFactory: static () => new FakeLinuxPlatformNativeHost());

        var act = () => factory.CreateHost();

        act.Should().Throw<PlatformDependencyException>()
            .WithMessage("*Wayland*XWayland*");
    }

    [LinuxNativeFact]
    public void NativeValidationScenario_ResolvesExpectedDisplayServer()
    {
        var expected = Environment.GetEnvironmentVariable("VIDERA_EXPECT_LINUX_DISPLAY_SERVER");
        expected.Should().NotBeNullOrWhiteSpace();

        var factory = new LinuxNativeHostFactory(
            x11HostFactory: static () => new FakeLinuxPlatformNativeHost());

        var selection = factory.CreateHost();

        selection.ResolvedDisplayServer.Should().Be(expected);

        if (string.Equals(expected, "XWayland", StringComparison.Ordinal))
        {
            Environment.GetEnvironmentVariable("WAYLAND_DISPLAY").Should().NotBeNullOrWhiteSpace();
            selection.FallbackUsed.Should().BeTrue();
            selection.FallbackReason.Should().Contain("XWayland");
        }
        else
        {
            selection.FallbackUsed.Should().BeFalse();
            selection.FallbackReason.Should().BeNull();
        }
    }

    private sealed class FakeLinuxPlatformNativeHost : ILinuxPlatformNativeHost
    {
        public IntPtr Handle => IntPtr.Zero;

        public IPlatformHandle Create(IPlatformHandle parent, Size bounds, double renderScaling)
        {
            return new PlatformHandle(IntPtr.Zero, "XID");
        }

        public void Resize(Size bounds, double renderScaling)
        {
        }

        public void Destroy()
        {
        }
    }

    private sealed class LinuxDisplayEnvironmentScope : IDisposable
    {
        private readonly string? _previousWaylandDisplay;
        private readonly string? _previousX11Display;
        private readonly string? _previousSessionType;
        private readonly bool _hadWaylandDisplay;
        private readonly bool _hadX11Display;
        private readonly bool _hadSessionType;

        private LinuxDisplayEnvironmentScope(string? waylandDisplay, string? x11Display, string? sessionType)
        {
            _hadWaylandDisplay = TryRead("WAYLAND_DISPLAY", out _previousWaylandDisplay);
            _hadX11Display = TryRead("DISPLAY", out _previousX11Display);
            _hadSessionType = TryRead("XDG_SESSION_TYPE", out _previousSessionType);

            Write("WAYLAND_DISPLAY", waylandDisplay);
            Write("DISPLAY", x11Display);
            Write("XDG_SESSION_TYPE", sessionType);
        }

        public static LinuxDisplayEnvironmentScope Override(string? waylandDisplay, string? x11Display, string? sessionType)
            => new(waylandDisplay, x11Display, sessionType);

        public void Dispose()
        {
            Restore("WAYLAND_DISPLAY", _hadWaylandDisplay, _previousWaylandDisplay);
            Restore("DISPLAY", _hadX11Display, _previousX11Display);
            Restore("XDG_SESSION_TYPE", _hadSessionType, _previousSessionType);
        }

        private static bool TryRead(string name, out string? value)
        {
            value = Environment.GetEnvironmentVariable(name);
            return Environment.GetEnvironmentVariables().Contains(name);
        }

        private static void Write(string name, string? value)
        {
            Environment.SetEnvironmentVariable(name, value);
        }

        private static void Restore(string name, bool hadValue, string? value)
        {
            if (hadValue)
            {
                Environment.SetEnvironmentVariable(name, value);
            }
            else
            {
                Environment.SetEnvironmentVariable(name, null);
            }
        }
    }
}
