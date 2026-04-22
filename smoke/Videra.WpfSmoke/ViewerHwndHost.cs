using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Videra.WpfSmoke;

public sealed class ViewerHwndHost : HwndHost
{
    private IntPtr _handle;

    public IntPtr NativeHandle => _handle;

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        const int wsChild = 0x40000000;
        const int wsVisible = 0x10000000;
        const int wsClipChildren = 0x02000000;
        const int wsClipSiblings = 0x04000000;

        _handle = CreateWindowExW(
            0,
            "STATIC",
            string.Empty,
            wsChild | wsVisible | wsClipChildren | wsClipSiblings,
            0,
            0,
            1,
            1,
            hwndParent.Handle,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);

        if (_handle == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to create the WPF smoke child HWND.");
        }

        UpdateNativeSize(ActualWidth, ActualHeight);
        return new HandleRef(this, _handle);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        if (_handle != IntPtr.Zero)
        {
            DestroyWindow(_handle);
            _handle = IntPtr.Zero;
        }
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        ArgumentNullException.ThrowIfNull(sizeInfo);
        base.OnRenderSizeChanged(sizeInfo);
        UpdateNativeSize(sizeInfo.NewSize.Width, sizeInfo.NewSize.Height);
    }

    private void UpdateNativeSize(double width, double height)
    {
        if (_handle == IntPtr.Zero)
        {
            return;
        }

        var dpi = VisualTreeHelper.GetDpi(this);
        var scaledWidth = Math.Max(1, (int)Math.Round(width * dpi.DpiScaleX));
        var scaledHeight = Math.Max(1, (int)Math.Round(height * dpi.DpiScaleY));
        SetWindowPos(_handle, IntPtr.Zero, 0, 0, scaledWidth, scaledHeight, 0x0010 | 0x0004);
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
