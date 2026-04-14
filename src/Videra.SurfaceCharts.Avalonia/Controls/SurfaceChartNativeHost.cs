using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;

namespace Videra.SurfaceCharts.Avalonia.Controls;

internal sealed partial class SurfaceChartNativeHost : NativeControlHost, ISurfaceChartNativeHost
{
    private IntPtr _handle;
    private readonly ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<SurfaceChartNativeHost>();

    public event Action<IntPtr>? HandleCreated;

    public event Action? HandleDestroyed;

    public IntPtr CurrentHandle => _handle;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Windows native chart host is only available on Windows.");
        }

        if (!string.Equals(parent.HandleDescriptor, "HWND", StringComparison.OrdinalIgnoreCase))
        {
            throw new PlatformNotSupportedException($"Expected HWND parent, got '{parent.HandleDescriptor}'.");
        }

        const int exStyle = 0;
        const int style = 0x40000000 | 0x10000000 | 0x04000000 | 0x02000000; // WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN

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
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to create native chart host window.");
        }

        UpdateNativeSize();
        HandleCreated?.Invoke(_handle);
        Log.CreatedChildWindow(_logger, _handle.ToInt64());
        return new PlatformHandle(_handle, "HWND");
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (_handle != IntPtr.Zero)
        {
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
        {
            return;
        }

        var scale = VisualRoot?.RenderScaling ?? 1.0;
        var width = Math.Max(1, (int)Math.Round(Bounds.Width * scale));
        var height = Math.Max(1, (int)Math.Round(Bounds.Height * scale));

        const uint flags = 0x0010 | 0x0004; // SWP_NOACTIVATE | SWP_NOZORDER
        SetWindowPos(_handle, IntPtr.Zero, 0, 0, width, height, flags);
        Log.ResizedChildWindow(_logger, width, height);
    }

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

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Creating chart child HWND under 0x{ParentHandle:X}")]
        public static partial void CreatingChildWindow(ILogger logger, long parentHandle);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Created chart HWND 0x{Handle:X}")]
        public static partial void CreatedChildWindow(ILogger logger, long handle);

        [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "Resize chart HWND to {Width}x{Height}")]
        public static partial void ResizedChildWindow(ILogger logger, int width, int height);
    }
}
