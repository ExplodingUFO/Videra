using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using Videra.Core.Exceptions;
using Videra.Core.NativeLibrary;

namespace Videra.Avalonia.Controls;

internal sealed partial class VideraLinuxNativeHost : NativeControlHost, IVideraNativeHost
{
    // Register DllImport resolver so "libX11.so.6" falls back to "libX11.so" and "libX11"
    static VideraLinuxNativeHost()
    {
        NativeLibraryHelper.RegisterDllImportResolver("libX11.so.6", "libX11.so", "libX11");
    }
    private IntPtr _display;
    private IntPtr _window;
    private bool _isDisposed;
    private readonly ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<VideraLinuxNativeHost>();

    public event Action<IntPtr>? HandleCreated;
    public event Action? HandleDestroyed;
    public event Action<NativePointerEvent>? NativePointer;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (!OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException("Linux native host is only supported on Linux.");

        _display = XOpenDisplay(IntPtr.Zero);
        if (_display == IntPtr.Zero)
            throw new PlatformDependencyException(
                "Failed to open X11 display. Ensure X11 is available.",
                "CreateNativeControl",
                "Linux");

        var screen = XDefaultScreen(_display);
        var rootWindow = XRootWindow(_display, screen);

        var scale = VisualRoot?.RenderScaling ?? 1.0;
        var width = Math.Max(1, (int)Math.Round(Bounds.Width * scale));
        var height = Math.Max(1, (int)Math.Round(Bounds.Height * scale));

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

        // Select input events
        const long eventMask =
            (1L << 2) |   // KeyPressMask
            (1L << 3) |   // KeyReleaseMask
            (1L << 4) |   // ButtonPressMask
            (1L << 5) |   // ButtonReleaseMask
            (1L << 6) |   // PointerMotionMask
            (1L << 15) |  // ExposureMask
            (1L << 17);   // StructureNotifyMask
        ObserveX11CallResult(XSelectInput(_display, _window, eventMask));

        // Reparent to Avalonia's window
        if (parent.Handle != IntPtr.Zero)
            ObserveX11CallResult(XReparentWindow(_display, _window, parent.Handle, 0, 0));

        ObserveX11CallResult(XMapWindow(_display, _window));
        ObserveX11CallResult(XFlush(_display));

        Log.CreatedX11Window(_logger, _window.ToInt64());
        HandleCreated?.Invoke(_window);

        return new PlatformHandle(_window, "XID");
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        if (_window != IntPtr.Zero)
        {
            ObserveX11CallResult(XDestroyWindow(_display, _window));
            _window = IntPtr.Zero;
            HandleDestroyed?.Invoke();
        }

        if (_display != IntPtr.Zero)
        {
            ObserveX11CallResult(XCloseDisplay(_display));
            _display = IntPtr.Zero;
        }

        base.DestroyNativeControlCore(control);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateNativeSize();
    }

    private void UpdateNativeSize()
    {
        if (_window == IntPtr.Zero || _display == IntPtr.Zero)
            return;

        var scale = VisualRoot?.RenderScaling ?? 1.0;
        var width = Math.Max(1, (uint)Math.Round(Bounds.Width * scale));
        var height = Math.Max(1, (uint)Math.Round(Bounds.Height * scale));

        ObserveX11CallResult(XResizeWindow(_display, _window, width, height));
        ObserveX11CallResult(XFlush(_display));
        Log.ResizedX11Window(_logger, width, height);
    }

    private static void ObserveX11CallResult(int result)
    {
        // Xlib reports protocol failures asynchronously through the connection error handler.
        // Capture the native return value explicitly so this ignore stays intentional.
        _ = result;
    }

    public IntPtr Display => _display;
    public IntPtr Window => _window;

    #region X11 Interop

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

    #endregion

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Created X11 window 0x{Window:X}")]
        public static partial void CreatedX11Window(ILogger logger, long window);

        [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Resize to {Width}x{Height}")]
        public static partial void ResizedX11Window(ILogger logger, uint width, uint height);
    }
}
