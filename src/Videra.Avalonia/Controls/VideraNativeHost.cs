using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;

namespace Videra.Avalonia.Controls;

internal sealed partial class VideraNativeHost : NativeControlHost, IVideraNativeHost
{
    private IntPtr _handle;
    private IntPtr _oldWndProc;
    private WndProcDelegate? _wndProc;
    private readonly ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<VideraNativeHost>();

    public event Action<IntPtr>? HandleCreated;
    public event Action? HandleDestroyed;
    public event Action<NativePointerEvent>? NativePointer;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Native GPU host is only implemented on Windows.");

        if (!string.Equals(parent.HandleDescriptor, "HWND", StringComparison.OrdinalIgnoreCase))
            throw new PlatformNotSupportedException($"Expected HWND parent, got '{parent.HandleDescriptor}'.");

        const int exStyle = 0;
        const int ssNotify = 0x0100;
        const int style = 0x40000000 | 0x10000000 | 0x04000000 | 0x02000000 | ssNotify; // WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN | SS_NOTIFY

        Log.CreatingChildWindow(_logger, parent.Handle.ToInt64());
        _handle = CreateWindowExW(
            exStyle,
            "STATIC",
            string.Empty,
            style,
            0,
            0,
            1,
            1,
            parent.Handle,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);

        if (_handle == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to create native child window.");

        HookWndProc();
        UpdateNativeSize();
        Log.CreatedChildWindow(_logger, _handle.ToInt64());
        HandleCreated?.Invoke(_handle);
        return new PlatformHandle(_handle, "HWND");
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (_handle != IntPtr.Zero)
        {
            UnhookWndProc();
            DestroyWindow(_handle);
            _handle = IntPtr.Zero;
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
        if (_handle == IntPtr.Zero)
            return;

        var scale = VisualRoot?.RenderScaling ?? 1.0;
        var width = Math.Max(1, (int)Math.Round(Bounds.Width * scale));
        var height = Math.Max(1, (int)Math.Round(Bounds.Height * scale));

        const uint flags = 0x0010 | 0x0004; // SWP_NOACTIVATE | SWP_NOZORDER
        SetWindowPos(_handle, IntPtr.Zero, 0, 0, width, height, flags);
        Log.ResizedChildWindow(_logger, width, height);
    }

    private void HookWndProc()
    {
        if (_handle == IntPtr.Zero || _oldWndProc != IntPtr.Zero)
            return;

        _wndProc = WndProc;
        _oldWndProc = SetWindowLongPtr(_handle, GwlWndProc, Marshal.GetFunctionPointerForDelegate(_wndProc));
        if (_oldWndProc == IntPtr.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            if (error != 0)
                Log.WndProcHookFailed(_logger, error);
        }
    }

    private void UnhookWndProc()
    {
        if (_handle == IntPtr.Zero || _oldWndProc == IntPtr.Zero)
            return;

        SetWindowLongPtr(_handle, GwlWndProc, _oldWndProc);
        _oldWndProc = IntPtr.Zero;
        _wndProc = null;
    }

    private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WmLButtonDown:
                SetFocus(hWnd);
                SetCapture(hWnd);
                RaisePointer(NativePointerKind.LeftDown, lParam, 0);
                return IntPtr.Zero;
            case WmLButtonUp:
                ReleaseCapture();
                RaisePointer(NativePointerKind.LeftUp, lParam, 0);
                return IntPtr.Zero;
            case WmRButtonDown:
                SetFocus(hWnd);
                SetCapture(hWnd);
                RaisePointer(NativePointerKind.RightDown, lParam, 0);
                return IntPtr.Zero;
            case WmRButtonUp:
                ReleaseCapture();
                RaisePointer(NativePointerKind.RightUp, lParam, 0);
                return IntPtr.Zero;
            case WmMouseMove:
                RaisePointer(NativePointerKind.Move, lParam, 0);
                return IntPtr.Zero;
            case WmMouseWheel:
                var delta = (short)((long)wParam >> 16);
                RaisePointer(NativePointerKind.Wheel, lParam, delta);
                return IntPtr.Zero;
        }

        return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
    }

    private const int GwlWndProc = -4;
    private const int WmMouseMove = 0x0200;
    private const int WmLButtonDown = 0x0201;
    private const int WmLButtonUp = 0x0202;
    private const int WmRButtonDown = 0x0204;
    private const int WmRButtonUp = 0x0205;
    private const int WmMouseWheel = 0x020A;

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowExW(
        int dwExStyle,
        string lpClassName,
        string lpWindowName,
        int dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr SetCapture(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SetFocus(IntPtr hWnd);

    private void RaisePointer(NativePointerKind kind, IntPtr lParam, int wheelDelta)
    {
        var x = (short)((long)lParam & 0xFFFF);
        var y = (short)(((long)lParam >> 16) & 0xFFFF);
        NativePointer?.Invoke(new NativePointerEvent(kind, x, y, wheelDelta));
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Creating child HWND under 0x{ParentHandle:X}")]
        public static partial void CreatingChildWindow(ILogger logger, long parentHandle);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Created HWND 0x{Handle:X}")]
        public static partial void CreatedChildWindow(ILogger logger, long handle);

        [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Resize HWND to {Width}x{Height}")]
        public static partial void ResizedChildWindow(ILogger logger, int width, int height);

        [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Failed to hook WndProc (err={Error})")]
        public static partial void WndProcHookFailed(ILogger logger, int error);
    }
}
