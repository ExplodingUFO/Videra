using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Videra.Avalonia.Interop;

namespace Videra.Avalonia.Controls;

public partial class VideraView
{
    // Win32 相关
    private IntPtr _hwnd;
    private bool _isDragging;
    private Point _lastPos;
    private IntPtr _oldWndProc;
    private Win32.WndProcDelegate _wndProcDelegate;

    private void SetupInput(IntPtr hwnd)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _hwnd = hwnd;
            _wndProcDelegate = WndProc; // 保持引用
            _oldWndProc = Win32.SetWindowLongPtr(_hwnd, Win32.GWLP_WNDPROC,
                Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));
        }
        else
        {
            IsHitTestVisible = false; // Mac/Linux 穿透策略
        }
    }

    private void CleanupInput()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && _oldWndProc != IntPtr.Zero)
            Win32.SetWindowLongPtr(_hwnd, Win32.GWLP_WNDPROC, _oldWndProc);
    }

    // Windows 消息处理
    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case Win32.WM_LBUTTONDOWN:
            case Win32.WM_RBUTTONDOWN:
                _isDragging = true;
                _lastPos = new Point((short)((int)lParam & 0xFFFF), (short)(((int)lParam >> 16) & 0xFFFF));
                break;
            case Win32.WM_LBUTTONUP:
            case Win32.WM_RBUTTONUP:
                _isDragging = false;
                break;
            case Win32.WM_MOUSEMOVE:
                if (_isDragging)
                {
                    var x = (short)((int)lParam & 0xFFFF);
                    var y = (short)(((int)lParam >> 16) & 0xFFFF);
                    var current = new Point(x, y);
                    // 转发到 ProcessMove
                    var isLeft = (wParam.ToInt64() & 0x0001) != 0;
                    ProcessMove((float)(current.X - _lastPos.X), (float)(current.Y - _lastPos.Y), isLeft);
                    _lastPos = current;
                }

                break;
            case Win32.WM_MOUSEWHEEL:
                var delta = (short)((long)wParam >> 16);
                Engine.Camera.Zoom(delta / 120.0f);
                break;
        }

        return Win32.CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
    }

    // 统一处理移动逻辑
    private void ProcessMove(float dx, float dy, bool isLeft)
    {
        if (isLeft) Engine.Camera.Rotate(dx, dy);
        else Engine.Camera.Pan(-dx, dy);
    }

    // Mac/Linux Avalonia Tunnel
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var top = TopLevel.GetTopLevel(this);
            top?.AddHandler(PointerPressedEvent, OnAvaPress, RoutingStrategies.Tunnel);
            top?.AddHandler(PointerMovedEvent, OnAvaMove, RoutingStrategies.Tunnel);
            top?.AddHandler(PointerReleasedEvent, OnAvaRelease, RoutingStrategies.Tunnel);
            top?.AddHandler(PointerWheelChangedEvent, OnAvaWheel, RoutingStrategies.Tunnel);
        }
    }

    // 省略 Detached 处理... (同之前逻辑)

    private void OnAvaPress(object s, PointerPressedEventArgs e)
    {
        var p = e.GetPosition(this);
        if (new Rect(Bounds.Size).Contains(p))
        {
            _isDragging = true;
            _lastPos = p;
            e.Handled = true;
        }
    }

    private void OnAvaMove(object s, PointerEventArgs e)
    {
        if (!_isDragging) return;
        var p = e.GetPosition(this);
        var props = e.GetCurrentPoint(this).Properties;
        ProcessMove((float)(p.X - _lastPos.X), (float)(p.Y - _lastPos.Y), props.IsLeftButtonPressed);
        _lastPos = p;
        e.Handled = true;
    }

    private void OnAvaRelease(object s, PointerReleasedEventArgs e)
    {
        _isDragging = false;
    }

    private void OnAvaWheel(object s, PointerWheelEventArgs e)
    {
        if (new Rect(Bounds.Size).Contains(e.GetPosition(this)))
        {
            Engine.Camera.Zoom((float)e.Delta.Y);
            e.Handled = true;
        }
    }
}