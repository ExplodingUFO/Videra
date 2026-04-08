using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using Videra.Core.Exceptions;
using Videra.Core.NativeLibrary;

namespace Videra.Avalonia.Controls.Linux;

internal sealed partial class X11NativeHost : ILinuxPlatformNativeHost
{
    private IntPtr _display;
    private IntPtr _window;
    private readonly ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<X11NativeHost>();

    static X11NativeHost()
    {
        NativeLibraryHelper.RegisterDllImportResolver("libX11.so.6", "libX11.so", "libX11");
    }

    public IntPtr Handle => _window;

    public IPlatformHandle Create(IPlatformHandle parent, Size bounds, double renderScaling)
    {
        _display = XOpenDisplay(IntPtr.Zero);
        if (_display == IntPtr.Zero)
        {
            throw new PlatformDependencyException(
                "Failed to open X11 display. Ensure X11 is available.",
                "CreateNativeControl",
                "Linux");
        }

        var screen = XDefaultScreen(_display);
        var rootWindow = XRootWindow(_display, screen);
        var width = Math.Max(1, (int)Math.Round(bounds.Width * renderScaling));
        var height = Math.Max(1, (int)Math.Round(bounds.Height * renderScaling));

        _window = XCreateSimpleWindow(
            _display,
            rootWindow,
            0, 0,
            (uint)width, (uint)height,
            0,
            0,
            0);

        if (_window == IntPtr.Zero)
        {
            ObserveX11CallResult(XCloseDisplay(_display));
            _display = IntPtr.Zero;
            throw new PlatformDependencyException(
                "Failed to create X11 window for native rendering.",
                "CreateNativeControl",
                "Linux");
        }

        const long eventMask =
            (1L << 2) |
            (1L << 3) |
            (1L << 4) |
            (1L << 5) |
            (1L << 6) |
            (1L << 15) |
            (1L << 17);
        ObserveX11CallResult(XSelectInput(_display, _window, eventMask));

        if (parent.Handle != IntPtr.Zero)
        {
            ObserveX11CallResult(XReparentWindow(_display, _window, parent.Handle, 0, 0));
        }

        ObserveX11CallResult(XMapWindow(_display, _window));
        ObserveX11CallResult(XFlush(_display));
        Log.CreatedX11Window(_logger, _window.ToInt64());

        return new PlatformHandle(_window, "XID");
    }

    public void Resize(Size bounds, double renderScaling)
    {
        if (_window == IntPtr.Zero || _display == IntPtr.Zero)
        {
            return;
        }

        var width = Math.Max(1, (uint)Math.Round(bounds.Width * renderScaling));
        var height = Math.Max(1, (uint)Math.Round(bounds.Height * renderScaling));

        ObserveX11CallResult(XResizeWindow(_display, _window, width, height));
        ObserveX11CallResult(XFlush(_display));
        Log.ResizedX11Window(_logger, width, height);
    }

    public void Destroy()
    {
        if (_window != IntPtr.Zero)
        {
            ObserveX11CallResult(XDestroyWindow(_display, _window));
            _window = IntPtr.Zero;
        }

        if (_display != IntPtr.Zero)
        {
            ObserveX11CallResult(XCloseDisplay(_display));
            _display = IntPtr.Zero;
        }
    }

    private static void ObserveX11CallResult(int result)
    {
        _ = result;
    }

    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XDefaultScreen(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XRootWindow(IntPtr display, int screen);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XCreateSimpleWindow(
        IntPtr display,
        IntPtr parent,
        int x, int y,
        uint width, uint height,
        uint borderWidth,
        ulong border,
        ulong background);

    [DllImport("libX11.so.6")]
    private static extern int XDestroyWindow(IntPtr display, IntPtr window);

    [DllImport("libX11.so.6")]
    private static extern int XMapWindow(IntPtr display, IntPtr window);

    [DllImport("libX11.so.6")]
    private static extern int XReparentWindow(IntPtr display, IntPtr window, IntPtr parent, int x, int y);

    [DllImport("libX11.so.6")]
    private static extern int XSelectInput(IntPtr display, IntPtr window, long eventMask);

    [DllImport("libX11.so.6")]
    private static extern int XResizeWindow(IntPtr display, IntPtr window, uint width, uint height);

    [DllImport("libX11.so.6")]
    private static extern int XFlush(IntPtr display);

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Created X11 window 0x{Window:X}")]
        public static partial void CreatedX11Window(ILogger logger, long window);

        [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Resize to {Width}x{Height}")]
        public static partial void ResizedX11Window(ILogger logger, uint width, uint height);
    }
}
