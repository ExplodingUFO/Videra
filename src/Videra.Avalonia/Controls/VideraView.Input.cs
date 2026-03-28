using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.Logging;

namespace Videra.Avalonia.Controls;

/// <summary>
/// VideraView 的输入处理部分。
/// </summary>
public partial class VideraView
{
    private static readonly bool InputLogEnabled =
        string.Equals(Environment.GetEnvironmentVariable("VIDERA_INPUTLOG"), "1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Environment.GetEnvironmentVariable("VIDERA_INPUTLOG"), "true", StringComparison.OrdinalIgnoreCase);

    private bool _isLeftButtonDown;
    private bool _isRightButtonDown;
    private Point _lastPos;
    private TopLevel? _cachedTopLevel;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        OnViewAttached();

        _cachedTopLevel = TopLevel.GetTopLevel(this);
        if (_cachedTopLevel != null)
        {
            _cachedTopLevel.AddHandler(PointerPressedEvent, OnTopPointerPressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
            _cachedTopLevel.AddHandler(PointerReleasedEvent, OnTopPointerReleased, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
            _cachedTopLevel.AddHandler(PointerMovedEvent, OnTopPointerMoved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
            _cachedTopLevel.AddHandler(PointerWheelChangedEvent, OnTopPointerWheel, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_cachedTopLevel != null)
        {
            _cachedTopLevel.RemoveHandler(PointerPressedEvent, OnTopPointerPressed);
            _cachedTopLevel.RemoveHandler(PointerReleasedEvent, OnTopPointerReleased);
            _cachedTopLevel.RemoveHandler(PointerMovedEvent, OnTopPointerMoved);
            _cachedTopLevel.RemoveHandler(PointerWheelChangedEvent, OnTopPointerWheel);
            _cachedTopLevel = null;
        }

        OnViewDetached();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var props = e.GetCurrentPoint(this).Properties;
        _lastPos = e.GetPosition(this);

        if (InputLogEnabled)
            _logger.LogDebug("Pressed at {Position}, Left={IsLeft}, Right={IsRight}", _lastPos, props.IsLeftButtonPressed, props.IsRightButtonPressed);

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

        if (InputLogEnabled)
            _logger.LogDebug("Released at {Position} ({Button})", e.GetPosition(this), e.InitialPressMouseButton);

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

        if (InputLogEnabled && (_isLeftButtonDown || _isRightButtonDown))
            _logger.LogDebug("Moved to {Position}", e.GetPosition(this));

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

        if (InputLogEnabled)
            _logger.LogDebug("Wheel delta {Delta}", e.Delta);

        var delta = e.Delta.Y;
        Engine.Camera.Zoom((float)(delta * 0.5));
        e.Handled = true;
    }

    private void OnTopPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsPointerOverThis(e))
            return;

        OnPointerPressed(e);
    }

    private void OnTopPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!IsPointerOverThis(e))
            return;

        OnPointerReleased(e);
    }

    private void OnTopPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!IsPointerOverThis(e))
            return;

        OnPointerMoved(e);
    }

    private void OnTopPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        if (!IsPointerOverThis(e))
            return;

        OnPointerWheelChanged(e);
    }

    private void OnOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        OnPointerPressed(e);
    }

    private void OnOverlayPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        OnPointerReleased(e);
    }

    private void OnOverlayPointerMoved(object? sender, PointerEventArgs e)
    {
        OnPointerMoved(e);
    }

    private void OnOverlayPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        OnPointerWheelChanged(e);
    }

    private void OnNativePointer(NativePointerEvent e)
    {
        var scale = VisualRoot?.RenderScaling ?? 1.0;
        var pos = new Point(e.X / scale, e.Y / scale);

        switch (e.Kind)
        {
            case NativePointerKind.LeftDown:
                _isLeftButtonDown = true;
                _lastPos = pos;
                if (InputLogEnabled)
                    _logger.LogDebug("Native LeftDown at {Position}", pos);
                Focus();
                break;
            case NativePointerKind.LeftUp:
                _isLeftButtonDown = false;
                if (InputLogEnabled)
                    _logger.LogDebug("Native LeftUp at {Position}", pos);
                break;
            case NativePointerKind.RightDown:
                _isRightButtonDown = true;
                _lastPos = pos;
                if (InputLogEnabled)
                    _logger.LogDebug("Native RightDown at {Position}", pos);
                Focus();
                break;
            case NativePointerKind.RightUp:
                _isRightButtonDown = false;
                if (InputLogEnabled)
                    _logger.LogDebug("Native RightUp at {Position}", pos);
                break;
            case NativePointerKind.Move:
                if (!_isLeftButtonDown && !_isRightButtonDown)
                    return;

                var dx = (float)(pos.X - _lastPos.X);
                var dy = (float)(pos.Y - _lastPos.Y);
                _lastPos = pos;
                if (InputLogEnabled)
                    _logger.LogDebug("Native Move {Position} dx={Dx} dy={Dy}", pos, dx, dy);
                ProcessMove(dx, dy, _isLeftButtonDown);
                break;
            case NativePointerKind.Wheel:
                var normalized = e.WheelDelta / 120.0f;
                if (InputLogEnabled)
                    _logger.LogDebug("Native Wheel delta {Delta}", e.WheelDelta);
                if (Math.Abs(normalized) > float.Epsilon)
                    Engine.Camera.Zoom(normalized * 0.5f);
                break;
        }
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
