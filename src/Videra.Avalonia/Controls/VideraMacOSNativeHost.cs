using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using Videra.Core.Exceptions;
#if VIDERA_MACOS_BACKEND
using Videra.Platform.macOS;
#endif

namespace Videra.Avalonia.Controls;

internal sealed partial class VideraMacOSNativeHost : NativeControlHost, IVideraNativeHost
{
    private IntPtr _nsView;
    private bool _isDisposed;
    private readonly ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<VideraMacOSNativeHost>();

    public event Action<IntPtr>? HandleCreated;
    public event Action? HandleDestroyed;
    public event Action<NativePointerEvent>? NativePointer;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (!OperatingSystem.IsMacOS())
            throw new PlatformNotSupportedException("macOS native host is only supported on macOS.");

        var width = Math.Max(1, (int)Math.Round(Bounds.Width));
        var height = Math.Max(1, (int)Math.Round(Bounds.Height));

        // Create NSView for Metal rendering
        _nsView = CreateNSView(width, height);
        if (_nsView == IntPtr.Zero)
            throw new PlatformDependencyException(
                "Failed to create NSView for Metal rendering.",
                "CreateNativeControlCore",
                "macOS");

        Log.CreatedNsView(_logger, _nsView.ToInt64());
        HandleCreated?.Invoke(_nsView);

        return new PlatformHandle(_nsView, "NSView");
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        if (_nsView != IntPtr.Zero)
        {
#if VIDERA_MACOS_BACKEND
            ObjCRuntime.SendMessageVoid(_nsView, ObjCRuntime.SEL("removeFromSuperview"));
            ObjCRuntime.SendMessageVoid(_nsView, ObjCRuntime.SEL("release"));
#endif
            _nsView = IntPtr.Zero;
            HandleDestroyed?.Invoke();
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
        if (_nsView == IntPtr.Zero)
            return;

        var width = Math.Max(1, Bounds.Width);
        var height = Math.Max(1, Bounds.Height);

#if VIDERA_MACOS_BACKEND
        ObjCRuntime.SetFrame(_nsView, 0, 0, width, height);
#endif
        Log.ResizedNsView(_logger, width, height);
    }

    private static IntPtr CreateNSView(int width, int height)
    {
#if VIDERA_MACOS_BACKEND
        ObjCRuntime.EnsureAppKitReady();
        var view = ObjCRuntime.InitWithFrame(ObjCRuntime.Alloc("NSView"), 0, 0, width, height);
        return ObjCRuntime.RequireNonZeroHandle(view, "CreateNSView", "Failed to initialize NSView for Metal rendering.");
#else
        _ = width;
        _ = height;
        throw new PlatformNotSupportedException("macOS native host is only supported when the macOS backend is compiled.");
#endif
    }

    public IntPtr NSView => _nsView;

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Created NSView 0x{Handle:X}")]
        public static partial void CreatedNsView(ILogger logger, long handle);

        [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Resize to {Width}x{Height}")]
        public static partial void ResizedNsView(ILogger logger, double width, double height);
    }
}
