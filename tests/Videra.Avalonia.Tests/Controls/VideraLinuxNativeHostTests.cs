using System.Reflection;
using Avalonia;
using Avalonia.Platform;
using FluentAssertions;
using Videra.Avalonia.Controls;
using Videra.Avalonia.Controls.Linux;
using Xunit;

namespace Videra.Avalonia.Tests.Controls;

public sealed class VideraLinuxNativeHostTests
{
    [Fact]
    public void DestroyNativeControlCore_RaisesHandleDestroyed_BeforeDestroyingSelectedHost()
    {
        var host = new VideraLinuxNativeHost(new LinuxNativeHostFactory());
        var selectedHost = new RecordingLinuxPlatformNativeHost();
        var handleDestroyedRaised = false;

        host.HandleDestroyed += () => handleDestroyedRaised = true;
        selectedHost.OnDestroy = () => handleDestroyedRaised.Should().BeTrue();

        WritePrivateField(host, "_selectedHost", selectedHost);
        WritePrivateField(host, "_selection", new LinuxNativeHostSelectionResult(selectedHost, "X11", false, null));
        WritePrivateField(host, "_isDisposed", false);

        InvokeDestroyNativeControlCore(host, new PlatformHandle(new IntPtr(0x1234), "XID"));

        selectedHost.DestroyCalls.Should().Be(1);
        handleDestroyedRaised.Should().BeTrue();
        host.ResolvedDisplayServer.Should().BeNull();
        host.DisplayServerFallbackUsed.Should().BeFalse();
        host.DisplayServerFallbackReason.Should().BeNull();
    }

    private static void InvokeDestroyNativeControlCore(VideraLinuxNativeHost host, IPlatformHandle handle)
    {
        var method = typeof(VideraLinuxNativeHost).GetMethod(
            "DestroyNativeControlCore",
            BindingFlags.Instance | BindingFlags.NonPublic);

        method.Should().NotBeNull();
        method!.Invoke(host, [handle]);
    }

    private static void WritePrivateField<T>(object target, string fieldName, T value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();
        field!.SetValue(target, value);
    }

    private sealed class RecordingLinuxPlatformNativeHost : ILinuxPlatformNativeHost
    {
        public IntPtr Handle => new(0x1234);

        public int DestroyCalls { get; private set; }

        public Action? OnDestroy { get; set; }

        public IPlatformHandle Create(IPlatformHandle parent, Size bounds, double renderScaling)
        {
            _ = parent;
            _ = bounds;
            _ = renderScaling;
            return new PlatformHandle(Handle, "XID");
        }

        public void Resize(Size bounds, double renderScaling)
        {
            _ = bounds;
            _ = renderScaling;
        }

        public void Destroy()
        {
            DestroyCalls++;
            OnDestroy?.Invoke();
        }
    }
}
