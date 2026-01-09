using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace Videra.Avalonia.Controls;

internal sealed class VideraNativeHost : NativeControlHost
{
    private IntPtr _handle;

    public event Action<IntPtr>? HandleCreated;
    public event Action? HandleDestroyed;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Native GPU host is only implemented on Windows.");

        if (!string.Equals(parent.HandleDescriptor, "HWND", StringComparison.OrdinalIgnoreCase))
            throw new PlatformNotSupportedException($"Expected HWND parent, got '{parent.HandleDescriptor}'.");

        const int exStyle = 0;
        const int style = 0x40000000 | 0x10000000 | 0x04000000 | 0x02000000 | 0x08000000; // WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN | WS_DISABLED

        Console.WriteLine($"[VideraNativeHost] Creating child HWND under 0x{parent.Handle.ToInt64():X}");
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

        UpdateNativeSize();
        Console.WriteLine($"[VideraNativeHost] Created HWND 0x{_handle.ToInt64():X}");
        HandleCreated?.Invoke(_handle);
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
            return;

        var scale = VisualRoot?.RenderScaling ?? 1.0;
        var width = Math.Max(1, (int)Math.Round(Bounds.Width * scale));
        var height = Math.Max(1, (int)Math.Round(Bounds.Height * scale));

        const uint flags = 0x0010 | 0x0004; // SWP_NOACTIVATE | SWP_NOZORDER
        SetWindowPos(_handle, IntPtr.Zero, 0, 0, width, height, flags);
        Console.WriteLine($"[VideraNativeHost] Resize HWND to {width}x{height}");
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
}
