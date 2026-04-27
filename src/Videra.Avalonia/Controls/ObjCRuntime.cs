using System.Runtime.InteropServices;
using Videra.Core.Exceptions;

namespace Videra.Avalonia.Controls;

internal static class ObjCRuntime
{
    private static IntPtr _appKitHandle;
    private static bool _appKitInitialized;

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern IntPtr GetClass(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public static extern IntPtr RegisterSelector(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageVoid(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern IntPtr SendMessageInitWithCGRect(IntPtr receiver, IntPtr selector, CGRect frame);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    public static extern void SendMessageCGRect(IntPtr receiver, IntPtr selector, CGRect rect);

    public static IntPtr SEL(string name) => RegisterSelector(name);

    public static void EnsureAppKitReady()
    {
        if (_appKitInitialized)
        {
            return;
        }

        const string appKitPath = "/System/Library/Frameworks/AppKit.framework/AppKit";
        if (_appKitHandle == IntPtr.Zero && !NativeLibrary.TryLoad(appKitPath, out _appKitHandle))
        {
            throw new PlatformDependencyException(
                $"Failed to load AppKit framework from '{appKitPath}'.",
                "EnsureAppKitReady",
                "macOS");
        }

        var nsApplicationClass = GetClass("NSApplication");
        if (nsApplicationClass == IntPtr.Zero)
        {
            throw new PlatformDependencyException(
                "Failed to resolve NSApplication after loading AppKit.",
                "EnsureAppKitReady",
                "macOS");
        }

        RequireNonZeroHandle(
            SendMessage(nsApplicationClass, SEL("sharedApplication")),
            "EnsureAppKitReady",
            "Failed to initialize NSApplication before creating Cocoa views.");

        _appKitInitialized = true;
    }

    public static IntPtr Alloc(string className)
    {
        var cls = GetClass(className);
        if (cls == IntPtr.Zero)
        {
            throw new PlatformDependencyException(
                $"Failed to resolve Objective-C class '{className}'.",
                "Alloc",
                "macOS");
        }

        return RequireNonZeroHandle(
            SendMessage(cls, SEL("alloc")),
            "Alloc",
            $"Failed to allocate Objective-C class '{className}'.");
    }

    public static IntPtr InitWithFrame(IntPtr receiver, double x, double y, double width, double height)
    {
        var frame = new CGRect { x = x, y = y, width = width, height = height };
        return RequireNonZeroHandle(
            SendMessageInitWithCGRect(receiver, SEL("initWithFrame:"), frame),
            "InitWithFrame",
            "Failed to initialize Objective-C object with frame.");
    }

    public static void SetFrame(IntPtr receiver, double x, double y, double width, double height)
    {
        var frame = new CGRect { x = x, y = y, width = width, height = height };
        SendMessageCGRect(receiver, SEL("setFrame:"), frame);
    }

    public static IntPtr RequireNonZeroHandle(IntPtr handle, string operation, string message)
    {
        if (handle == IntPtr.Zero)
        {
            throw new PlatformDependencyException(message, operation, "macOS");
        }

        return handle;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct CGRect
{
    public double x;
    public double y;
    public double width;
    public double height;
}
