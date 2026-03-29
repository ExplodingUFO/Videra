using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using Videra.Core.Exceptions;
using Videra.Core.Exceptions;

namespace Videra.Avalonia.Controls;

internal sealed class VideraMacOSNativeHost : NativeControlHost, IVideraNativeHost
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

        var scale = VisualRoot?.RenderScaling ?? 1.0;
        var width = Math.Max(1, (int)Math.Round(Bounds.Width * scale));
        var height = Math.Max(1, (int)Math.Round(Bounds.Height * scale));

        // Create NSView for Metal rendering
        _nsView = CreateNSView(width, height);
        if (_nsView == IntPtr.Zero)
            throw new PlatformDependencyException(
                "Failed to create NSView for Metal rendering.",
                "CreateNativeControlCore",
                "macOS");

        _logger.LogInformation("Created NSView 0x{Handle:X}", _nsView.ToInt64());
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
            SendMessage(_nsView, SEL("removeFromSuperview"));
            SendMessage(_nsView, SEL("release"));
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

        var scale = VisualRoot?.RenderScaling ?? 1.0;
        var width = Math.Max(1, Bounds.Width * scale);
        var height = Math.Max(1, Bounds.Height * scale);

        var frame = new CGRect { x = 0, y = 0, width = width, height = height };
        objc_msgSend_CGRect(_nsView, SEL("setFrame:"), frame);
        _logger.LogDebug("Resize to {Width}x{Height}", width, height);
    }

    private static IntPtr CreateNSView(int width, int height)
    {
        var nsViewClass = objc_getClass("NSView");
        var alloc = SendMessage(nsViewClass, SEL("alloc"));

        var frame = new CGRect { x = 0, y = 0, width = width, height = height };
        var view = objc_msgSend_initWithFrame(alloc, SEL("initWithFrame:"), frame);

        // Enable layer backing for Metal
        SendMessageWithBool(view, SEL("setWantsLayer:"), true);

        return view;
    }

    public IntPtr NSView => _nsView;

    #region Objective-C Interop

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void SendMessageWithBool(IntPtr receiver, IntPtr selector, bool arg);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_initWithFrame(IntPtr receiver, IntPtr selector, CGRect frame);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_CGRect(IntPtr receiver, IntPtr selector, CGRect rect);

    private static IntPtr SEL(string name) => sel_registerName(name);

    [StructLayout(LayoutKind.Sequential)]
    private struct CGRect
    {
        public double x;
        public double y;
        public double width;
        public double height;
    }

    #endregion
}
