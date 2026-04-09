using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Videra.Avalonia.Controls.Interaction;

internal sealed class VideraInteractionRouter : IDisposable
{
    private readonly IVideraInteractionHost _host;
    private readonly VideraInteractionController _controller;
    private TopLevel? _topLevel;
    private object? _lastRoutedToken;
    private string? _lastRoutedPhase;
    private bool _disposed;

    public VideraInteractionRouter(IVideraInteractionHost host, VideraInteractionController controller)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    public void Attach()
    {
        if (_disposed)
        {
            return;
        }

        var topLevel = _host.ResolveTopLevel();
        if (ReferenceEquals(_topLevel, topLevel))
        {
            return;
        }

        DetachTopLevelHandlers();
        _topLevel = topLevel;
        if (_topLevel is null)
        {
            return;
        }

        _topLevel.AddHandler(InputElement.PointerPressedEvent, OnTopPointerPressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        _topLevel.AddHandler(InputElement.PointerReleasedEvent, OnTopPointerReleased, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        _topLevel.AddHandler(InputElement.PointerMovedEvent, OnTopPointerMoved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        _topLevel.AddHandler(InputElement.PointerWheelChangedEvent, OnTopPointerWheel, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
    }

    public void Detach()
    {
        DetachTopLevelHandlers();
        _controller.Reset();
        ResetDeduplication();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Detach();
    }

    public void RoutePressed(PointerPressedEventArgs e, VideraPointerRoute route)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (_disposed || ShouldSkipDuplicate("Pressed", e))
        {
            return;
        }

        if (_controller.HandlePressed(CreateSnapshot(e), route))
        {
            e.Pointer.Capture(_host.PointerCaptureTarget);
            e.Handled = true;
        }
    }

    public void RouteReleased(PointerReleasedEventArgs e, VideraPointerRoute route)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (_disposed || ShouldSkipDuplicate("Released", e))
        {
            return;
        }

        if (_controller.HandleReleased(CreateSnapshot(e), route))
        {
            if (!_controller.HasActiveGesture && ReferenceEquals(e.Pointer.Captured, _host.PointerCaptureTarget))
            {
                e.Pointer.Capture(null);
            }

            e.Handled = true;
        }
    }

    public void RouteMoved(PointerEventArgs e, VideraPointerRoute route)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (_disposed || ShouldSkipDuplicate("Moved", e))
        {
            return;
        }

        if (_controller.HandleMoved(CreateSnapshot(e), route))
        {
            e.Handled = true;
        }
    }

    public void RouteWheel(PointerWheelEventArgs e, VideraPointerRoute route)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (_disposed || ShouldSkipDuplicate("Wheel", e))
        {
            return;
        }

        if (_controller.HandleWheel(CreateSnapshot(e), route))
        {
            e.Handled = true;
        }
    }

    public void RouteCaptureLost(PointerCaptureLostEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (_disposed)
        {
            return;
        }

        _controller.Reset();
        ResetDeduplication();
        e.Handled = true;
    }

    public void RouteNativePointer(NativePointerEvent e)
    {
        if (_disposed)
        {
            return;
        }

        var scale = _host.ResolveTopLevel()?.RenderScaling ?? 1.0;
        var snapshot = new VideraPointerGestureSnapshot(
            new Point(e.X / scale, e.Y / scale),
            RawInputModifiers.None,
            IsLeftButtonPressed: e.Kind is NativePointerKind.LeftDown || (_controller.IsLeftButtonDown && e.Kind == NativePointerKind.Move),
            IsRightButtonPressed: e.Kind is NativePointerKind.RightDown || (_controller.IsRightButtonDown && e.Kind == NativePointerKind.Move),
            InitialPressMouseButton: e.Kind == NativePointerKind.RightUp ? MouseButton.Right : MouseButton.Left,
            WheelDeltaY: e.Kind == NativePointerKind.Wheel ? e.WheelDelta / 120.0f : 0f);

        switch (e.Kind)
        {
            case NativePointerKind.LeftDown:
                _controller.HandlePressed(snapshot with { IsLeftButtonPressed = true, IsRightButtonPressed = false }, VideraPointerRoute.Native);
                break;
            case NativePointerKind.LeftUp:
                _controller.HandleReleased(snapshot with { InitialPressMouseButton = MouseButton.Left }, VideraPointerRoute.Native);
                break;
            case NativePointerKind.RightDown:
                _controller.HandlePressed(snapshot with { IsLeftButtonPressed = false, IsRightButtonPressed = true }, VideraPointerRoute.Native);
                break;
            case NativePointerKind.RightUp:
                _controller.HandleReleased(snapshot with { InitialPressMouseButton = MouseButton.Right }, VideraPointerRoute.Native);
                break;
            case NativePointerKind.Move:
                _controller.HandleMoved(snapshot, VideraPointerRoute.Native);
                break;
            case NativePointerKind.Wheel:
                _controller.HandleWheel(snapshot, VideraPointerRoute.Native);
                break;
        }
    }

    private bool ShouldHandleTopLevel(PointerEventArgs e)
    {
        var position = e.GetPosition((Visual)_host.PointerCaptureTarget);
        return _controller.HasActiveGesture || _host.IsPointWithinHost(position);
    }

    private void OnTopPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ShouldHandleTopLevel(e))
        {
            RoutePressed(e, VideraPointerRoute.TopLevel);
        }
    }

    private void OnTopPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (ShouldHandleTopLevel(e))
        {
            RouteReleased(e, VideraPointerRoute.TopLevel);
        }
    }

    private void OnTopPointerMoved(object? sender, PointerEventArgs e)
    {
        if (ShouldHandleTopLevel(e))
        {
            RouteMoved(e, VideraPointerRoute.TopLevel);
        }
    }

    private void OnTopPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        if (_host.IsPointWithinHost(e.GetPosition((Visual)_host.PointerCaptureTarget)))
        {
            RouteWheel(e, VideraPointerRoute.TopLevel);
        }
    }

    private void DetachTopLevelHandlers()
    {
        if (_topLevel is null)
        {
            return;
        }

        _topLevel.RemoveHandler(InputElement.PointerPressedEvent, OnTopPointerPressed);
        _topLevel.RemoveHandler(InputElement.PointerReleasedEvent, OnTopPointerReleased);
        _topLevel.RemoveHandler(InputElement.PointerMovedEvent, OnTopPointerMoved);
        _topLevel.RemoveHandler(InputElement.PointerWheelChangedEvent, OnTopPointerWheel);
        _topLevel = null;
    }

    private void ResetDeduplication()
    {
        _lastRoutedToken = null;
        _lastRoutedPhase = null;
    }

    private bool ShouldSkipDuplicate(string phase, object token)
    {
        if (ReferenceEquals(_lastRoutedToken, token) && string.Equals(_lastRoutedPhase, phase, StringComparison.Ordinal))
        {
            return true;
        }

        _lastRoutedToken = token;
        _lastRoutedPhase = phase;
        return false;
    }

    private VideraPointerGestureSnapshot CreateSnapshot(PointerPressedEventArgs e)
    {
        var target = (Visual)_host.PointerCaptureTarget;
        var point = e.GetCurrentPoint(target);
        return new VideraPointerGestureSnapshot(
            e.GetPosition(target),
            e.KeyModifiers.ToRawInputModifiers(),
            point.Properties.IsLeftButtonPressed,
            point.Properties.IsRightButtonPressed,
            MouseButton.Left,
            0f);
    }

    private VideraPointerGestureSnapshot CreateSnapshot(PointerReleasedEventArgs e)
    {
        var target = (Visual)_host.PointerCaptureTarget;
        return new VideraPointerGestureSnapshot(
            e.GetPosition(target),
            e.KeyModifiers.ToRawInputModifiers(),
            false,
            false,
            e.InitialPressMouseButton,
            0f);
    }

    private VideraPointerGestureSnapshot CreateSnapshot(PointerEventArgs e)
    {
        var target = (Visual)_host.PointerCaptureTarget;
        var point = e.GetCurrentPoint(target);
        return new VideraPointerGestureSnapshot(
            e.GetPosition(target),
            e.KeyModifiers.ToRawInputModifiers(),
            point.Properties.IsLeftButtonPressed,
            point.Properties.IsRightButtonPressed,
            MouseButton.Left,
            0f);
    }

    private VideraPointerGestureSnapshot CreateSnapshot(PointerWheelEventArgs e)
    {
        var target = (Visual)_host.PointerCaptureTarget;
        var point = e.GetCurrentPoint(target);
        return new VideraPointerGestureSnapshot(
            e.GetPosition(target),
            e.KeyModifiers.ToRawInputModifiers(),
            point.Properties.IsLeftButtonPressed,
            point.Properties.IsRightButtonPressed,
            MouseButton.Left,
            (float)e.Delta.Y);
    }
}
