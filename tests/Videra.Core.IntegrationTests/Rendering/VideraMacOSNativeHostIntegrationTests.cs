using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using FluentAssertions;
using Tests.Common.Platform;
using Videra.Avalonia.Controls;

namespace Videra.Core.IntegrationTests.Rendering;

public sealed class VideraMacOSNativeHostIntegrationTests
{
    [MacOSFact]
    public void CreateNSView_ReturnsValidNsViewHandle()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        var createNsView = typeof(VideraMacOSNativeHost).GetMethod(
            "CreateNSView",
            BindingFlags.Static | BindingFlags.NonPublic);

        createNsView.Should().NotBeNull();

        var nsView = (IntPtr)createNsView!.Invoke(null, [64, 64])!;

        try
        {
            Marshal.PtrToStringAnsi(object_getClassName(nsView)).Should().Be("NSView");
        }
        finally
        {
            objc_msgSend(nsView, sel_registerName("release"));
        }
    }

    [SuppressMessage("Interoperability", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "Objective-C selector names are UTF-8 C strings on Darwin and this test-only interop matches the production macOS backend.")]
    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName", CharSet = CharSet.Ansi)]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "object_getClassName")]
    private static extern IntPtr object_getClassName(IntPtr obj);
}
