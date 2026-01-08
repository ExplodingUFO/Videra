using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Videra.Avalonia.Controls;

/// <summary>
/// VideraViewNew 的输入处理部分
/// </summary>
public partial class VideraViewNew
{
    private void SetupInput(IntPtr handle)
    {
        // 使用 Avalonia 的输入事件系统
        var top = TopLevel.GetTopLevel(this);
        if (top != null)
        {
            // 添加隧道事件监听器
            top.AddHandler(PointerPressedEvent, OnAvaPress, RoutingStrategies.Tunnel);
            top.AddHandler(PointerReleasedEvent, OnAvaRelease, RoutingStrategies.Tunnel);
            top.AddHandler(PointerMovedEvent, OnAvaMove, RoutingStrategies.Tunnel);
            top.AddHandler(PointerWheelChangedEvent, OnAvaWheel, RoutingStrategies.Tunnel);
        }

        // 确保控件可以接收焦点
        Focusable = true;
    }

    private void CleanupInput()
    {
        var top = TopLevel.GetTopLevel(this);
        if (top != null)
        {
            top.RemoveHandler(PointerPressedEvent, OnAvaPress);
            top.RemoveHandler(PointerReleasedEvent, OnAvaRelease);
            top.RemoveHandler(PointerMovedEvent, OnAvaMove);
            top.RemoveHandler(PointerWheelChangedEvent, OnAvaWheel);
        }
    }

    private bool _isLeftButtonDown;
    private bool _isRightButtonDown;
    private Point _lastPos;

    private void OnAvaPress(object? sender, PointerPressedEventArgs e)
    {
        if (!IsPointerOverThis(e)) return;

        var props = e.GetCurrentPoint(this).Properties;
        _lastPos = e.GetPosition(this);

        if (props.IsLeftButtonPressed)
        {
            _isLeftButtonDown = true;
            e.Handled = true;
        }
        else if (props.IsRightButtonPressed)
        {
            _isRightButtonDown = true;
            e.Handled = true;
        }

        Focus();
    }

    private void OnAvaRelease(object? sender, PointerReleasedEventArgs e)
    {
        if (!IsPointerOverThis(e)) return;

        var props = e.GetCurrentPoint(this).Properties;

        if (props.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
        {
            _isLeftButtonDown = false;
            e.Handled = true;
        }
        else if (props.PointerUpdateKind == PointerUpdateKind.RightButtonReleased)
        {
            _isRightButtonDown = false;
            e.Handled = true;
        }
    }

    private void OnAvaMove(object? sender, PointerEventArgs e)
    {
        if (!_isLeftButtonDown && !_isRightButtonDown) return;
        if (!IsPointerOverThis(e)) return;

        var pos = e.GetPosition(this);
        var dx = (float)(pos.X - _lastPos.X);
        var dy = (float)(pos.Y - _lastPos.Y);
        _lastPos = pos;

        ProcessMove(dx, dy, _isLeftButtonDown);
        e.Handled = true;
    }

    private void OnAvaWheel(object? sender, PointerWheelEventArgs e)
    {
        if (!IsPointerOverThis(e)) return;

        var delta = e.Delta.Y;
        Engine.Camera.Zoom((float)(delta * 0.1));
        e.Handled = true;
    }

    private bool IsPointerOverThis(PointerEventArgs e)
    {
        var pos = e.GetPosition(this);
        return pos.X >= 0 && pos.X < Bounds.Width &&
               pos.Y >= 0 && pos.Y < Bounds.Height;
    }

    private void ProcessMove(float dx, float dy, bool isLeft)
    {
        if (isLeft)
        {
            // 左键：旋转
            Engine.Camera.Rotate(dx * 0.5f, dy * 0.5f);
        }
        else
        {
            // 右键：平移
            Engine.Camera.Pan(-dx * 0.01f, dy * 0.01f);
        }
    }
}
