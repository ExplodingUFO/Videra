using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using Videra.Core.Exceptions;

namespace Videra.SurfaceCharts.Avalonia.Controls;

internal sealed partial class SurfaceChartMacOSNativeHost : NativeControlHost, ISurfaceChartNativeHost
{
    private IntPtr _nsView;
    private bool _isDisposed;
    private readonly ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<SurfaceChartMacOSNativeHost>();

    public event Action<IntPtr>? HandleCreated;

    public event Action? HandleDestroyed;

    public IntPtr CurrentHandle => _nsView;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (!OperatingSystem.IsMacOS())
        {
            throw new PlatformNotSupportedException("macOS native chart host is only supported on macOS.");
        }

        var width = Math.Max(1, (int)Math.Round(Bounds.Width));
        var height = Math.Max(1, (int)Math.Round(Bounds.Height));

        _nsView = CreateNSView(width, height);
        Log.CreatedNsView(_logger, _nsView.ToInt64());
        HandleCreated?.Invoke(_nsView);
        return new PlatformHandle(_nsView, "NSView");
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (_nsView != IntPtr.Zero)
        {
            ObjCRuntime.SendMessageVoid(_nsView, ObjCRuntime.SEL("removeFromSuperview"));
            ObjCRuntime.SendMessageVoid(_nsView, ObjCRuntime.SEL("release"));
            _nsView = IntPtr.Zero;
            HandleDestroyed?.Invoke();
        }

        base.DestroyNativeControlCore(control);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (_nsView == IntPtr.Zero)
        {
            return;
        }

        var width = Math.Max(1, Bounds.Width);
        var height = Math.Max(1, Bounds.Height);
        ObjCRuntime.SetFrame(_nsView, 0, 0, width, height);
        Log.ResizedNsView(_logger, width, height);
    }

    private static IntPtr CreateNSView(int width, int height)
    {
        ObjCRuntime.EnsureAppKitReady();
        var view = ObjCRuntime.InitWithFrame(ObjCRuntime.Alloc("NSView"), 0, 0, width, height);
        return ObjCRuntime.RequireNonZeroHandle(view, "CreateNSView", "Failed to initialize NSView for chart Metal rendering.");
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Created chart NSView 0x{Handle:X}")]
        public static partial void CreatedNsView(ILogger logger, long handle);

        [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Resize chart NSView to {Width}x{Height}")]
        public static partial void ResizedNsView(ILogger logger, double width, double height);
    }

    private static class ObjCRuntime
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

        public static IntPtr SEL(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return RequireNonZeroHandle(
                RegisterSelector(name),
                "SEL",
                $"Failed to register Objective-C selector '{name}'.");
        }

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
                "Failed to initialize NSApplication before creating chart Cocoa views.");

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
            var frame = new CGRect
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
            };

            return RequireNonZeroHandle(
                SendMessageInitWithCGRect(receiver, SEL("initWithFrame:"), frame),
                "InitWithFrame",
                "Failed to initialize chart Objective-C object with frame.");
        }

        public static void SetFrame(IntPtr receiver, double x, double y, double width, double height)
        {
            var frame = new CGRect
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
            };

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
    private struct CGRect
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;
    }
}
