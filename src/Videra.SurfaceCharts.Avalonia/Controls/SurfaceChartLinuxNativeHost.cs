using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using Videra.Core.Exceptions;
using Videra.Core.NativeLibrary;
using Videra.Core.Platform.Linux;

namespace Videra.SurfaceCharts.Avalonia.Controls;

internal sealed partial class SurfaceChartLinuxNativeHost : NativeControlHost, ISurfaceChartNativeHost
{
    private readonly LinuxNativeHostFactory _nativeHostFactory;
    private ILinuxPlatformNativeHost? _selectedHost;
    private bool _isDisposed;

    public SurfaceChartLinuxNativeHost()
        : this(new LinuxNativeHostFactory())
    {
    }

    internal SurfaceChartLinuxNativeHost(LinuxNativeHostFactory nativeHostFactory)
    {
        _nativeHostFactory = nativeHostFactory ?? throw new ArgumentNullException(nameof(nativeHostFactory));
    }

    public event Action<IntPtr>? HandleCreated;

    public event Action? HandleDestroyed;

    public IntPtr CurrentHandle => _selectedHost?.Handle ?? IntPtr.Zero;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux native chart host is only supported on Linux.");
        }

        _isDisposed = false;
        _selectedHost = _nativeHostFactory.CreateHost();
        var handle = _selectedHost.Create(parent, Bounds.Size, VisualRoot?.RenderScaling ?? 1.0);
        HandleCreated?.Invoke(handle.Handle);
        return handle;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (_selectedHost is not null)
        {
            _selectedHost.Destroy();
            _selectedHost = null;
            HandleDestroyed?.Invoke();
        }

        base.DestroyNativeControlCore(control);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (_selectedHost is not null)
        {
            _selectedHost.Resize(Bounds.Size, VisualRoot?.RenderScaling ?? 1.0);
        }
    }

    internal sealed class LinuxNativeHostFactory
    {
        private const string XWaylandCompatibilityReason =
            "Avalonia Linux native hosting currently embeds through X11 handles; using an XWayland compatibility path for this Wayland session.";

        private const string WaylandOnlyUnsupportedReason =
            "Wayland was detected, but the current Avalonia Linux native-host stack cannot embed directly into a compositor-native Wayland surface. Enable XWayland for this session or validate on an X11 host.";

        private readonly LinuxDisplayServerDetector _detector;
        private readonly Func<ILinuxPlatformNativeHost> _x11HostFactory;

        public LinuxNativeHostFactory(
            LinuxDisplayServerDetector? detector = null,
            Func<ILinuxPlatformNativeHost>? x11HostFactory = null)
        {
            _detector = detector ?? new LinuxDisplayServerDetector();
            _x11HostFactory = x11HostFactory ?? (() => new X11NativeHost());
        }

        public ILinuxPlatformNativeHost CreateHost()
        {
            var candidates = _detector.DetectCandidates(
                Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"),
                Environment.GetEnvironmentVariable("DISPLAY"),
                Environment.GetEnvironmentVariable("XDG_SESSION_TYPE"));

            string? fallbackReason = null;

            foreach (var candidate in candidates)
            {
                switch (candidate.DisplayServer)
                {
                    case LinuxDisplayServerKind.Wayland:
                        fallbackReason = candidate.AllowsXWaylandFallback
                            ? XWaylandCompatibilityReason
                            : WaylandOnlyUnsupportedReason;
                        continue;
                    case LinuxDisplayServerKind.XWayland:
                    case LinuxDisplayServerKind.X11:
                        return _x11HostFactory();
                }
            }

            throw new PlatformDependencyException(
                fallbackReason ?? "No supported Linux display server is available for chart native rendering.",
                "CreateLinuxNativeHost",
                "Linux");
        }
    }

    internal interface ILinuxPlatformNativeHost
    {
        IntPtr Handle { get; }

        IPlatformHandle Create(IPlatformHandle parent, Size bounds, double renderScaling);

        void Resize(Size bounds, double renderScaling);

        void Destroy();
    }

    private sealed partial class X11NativeHost : ILinuxPlatformNativeHost
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
                0,
                0,
                (uint)width,
                (uint)height,
                0,
                0,
                0);

            if (_window == IntPtr.Zero)
            {
                ObserveX11CallResult(XCloseDisplay(_display));
                _display = IntPtr.Zero;
                throw new PlatformDependencyException(
                    "Failed to create X11 window for chart native rendering.",
                    "CreateNativeControl",
                    "Linux");
            }

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
            int x,
            int y,
            uint width,
            uint height,
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
        private static extern int XResizeWindow(IntPtr display, IntPtr window, uint width, uint height);

        [DllImport("libX11.so.6")]
        private static extern int XFlush(IntPtr display);

        private static partial class Log
        {
            [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Created chart X11 window 0x{Window:X}")]
            public static partial void CreatedX11Window(ILogger logger, long window);

            [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Resize chart X11 host to {Width}x{Height}")]
            public static partial void ResizedX11Window(ILogger logger, uint width, uint height);
        }
    }
}
