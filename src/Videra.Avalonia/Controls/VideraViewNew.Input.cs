using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Videra.Avalonia.Controls;

/// <summary>
/// VideraViewNew 的输入处理部分。
/// </summary>
public partial class VideraViewNew
{
    private bool _isLeftButtonDown;
    private bool _isRightButtonDown;
    private Point _lastPos;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        OnViewAttached();

        var top = TopLevel.GetTopLevel(this);
        if (top != null)
        {
            top.AddHandler(PointerPressedEvent, OnTopPointerPressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            top.AddHandler(PointerReleasedEvent, OnTopPointerReleased, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            top.AddHandler(PointerMovedEvent, OnTopPointerMoved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            top.AddHandler(PointerWheelChangedEvent, OnTopPointerWheel, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        var top = TopLevel.GetTopLevel(this);
        if (top != null)
        {
            top.RemoveHandler(PointerPressedEvent, OnTopPointerPressed);
            top.RemoveHandler(PointerReleasedEvent, OnTopPointerReleased);
            top.RemoveHandler(PointerMovedEvent, OnTopPointerMoved);
            top.RemoveHandler(PointerWheelChangedEvent, OnTopPointerWheel);
        }

        OnViewDetached();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

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
        e.Pointer.Capture(this);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            _isLeftButtonDown = false;
            e.Handled = true;
        }
        else if (e.InitialPressMouseButton == MouseButton.Right)
        {
            _isRightButtonDown = false;
            e.Handled = true;
        }

        e.Pointer.Capture(null);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_isLeftButtonDown && !_isRightButtonDown)
            return;

        var pos = e.GetPosition(this);
        var dx = (float)(pos.X - _lastPos.X);
        var dy = (float)(pos.Y - _lastPos.Y);
        _lastPos = pos;

        ProcessMove(dx, dy, _isLeftButtonDown);
        e.Handled = true;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var delta = e.Delta.Y;
        Engine.Camera.Zoom((float)(delta * 0.5));
        e.Handled = true;
    }

    private void OnTopPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Handled || !IsPointerOverThis(e))
            return;

        OnPointerPressed(e);
    }

    private void OnTopPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Handled || !IsPointerOverThis(e))
            return;

        OnPointerReleased(e);
    }

    private void OnTopPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.Handled || !IsPointerOverThis(e))
            return;

        OnPointerMoved(e);
    }

    private void OnTopPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        if (e.Handled || !IsPointerOverThis(e))
            return;

        OnPointerWheelChanged(e);
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
            Engine.Camera.Rotate(dx * 0.5f, dy * 0.5f);
        else
            Engine.Camera.Pan(-dx * 0.01f, dy * 0.01f);
    }
}
