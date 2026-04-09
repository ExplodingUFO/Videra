using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.Logging;
using Videra.Core.Selection;
using Videra.Core.Selection.Annotations;

namespace Videra.Avalonia.Controls.Interaction;

internal sealed partial class VideraInteractionController : IDisposable
{
    private const double SelectionDragThreshold = 4d;

    private static readonly bool InputLogEnabled =
        string.Equals(Environment.GetEnvironmentVariable("VIDERA_INPUTLOG"), "1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Environment.GetEnvironmentVariable("VIDERA_INPUTLOG"), "true", StringComparison.OrdinalIgnoreCase);

    private readonly IVideraInteractionHost _host;
    private readonly ILogger _logger;
    private readonly SceneHitTestService _hitTestService = new();
    private readonly SceneBoxSelectionService _boxSelectionService = new();
    private readonly VideraInteractionState _state = new();
    private TopLevel? _topLevel;
    private bool _disposed;

    public VideraInteractionController(IVideraInteractionHost host, ILogger logger)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        if (_topLevel == null)
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
        _state.Reset();
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

    public void HandlePressed(PointerPressedEventArgs e, VideraPointerRoute route)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (_disposed)
        {
            return;
        }

        var handled = HandlePressed(CreateSnapshot(e), route, e);
        if (!handled)
        {
            return;
        }

        e.Pointer.Capture(_host.PointerCaptureTarget);
        e.Handled = true;
    }

    public void HandleReleased(PointerReleasedEventArgs e, VideraPointerRoute route)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (_disposed)
        {
            return;
        }

        var handled = HandleReleased(CreateSnapshot(e), route, e);
        if (!handled)
        {
            return;
        }

        e.Pointer.Capture(null);
        e.Handled = true;
    }

    public void HandleMoved(PointerEventArgs e, VideraPointerRoute route)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (_disposed)
        {
            return;
        }

        if (HandleMoved(CreateSnapshot(e), route, e))
        {
            e.Handled = true;
        }
    }

    public void HandleWheel(PointerWheelEventArgs e, VideraPointerRoute route)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (_disposed)
        {
            return;
        }

        if (HandleWheel(CreateSnapshot(e), route, e))
        {
            e.Handled = true;
        }
    }

    public void HandleNativePointer(NativePointerEvent e)
    {
        if (_disposed)
        {
            return;
        }

        var scale = _host.ResolveTopLevel()?.RenderScaling ?? 1.0;
        var snapshot = new VideraPointerGestureSnapshot(
            new Point(e.X / scale, e.Y / scale),
            RawInputModifiers.None,
            IsLeftButtonPressed: e.Kind is NativePointerKind.LeftDown || (_state.IsLeftButtonDown && e.Kind == NativePointerKind.Move),
            IsRightButtonPressed: e.Kind is NativePointerKind.RightDown || (_state.IsRightButtonDown && e.Kind == NativePointerKind.Move),
            InitialPressMouseButton: e.Kind switch
            {
                NativePointerKind.RightUp => MouseButton.Right,
                _ => MouseButton.Left
            },
            WheelDeltaY: e.Kind == NativePointerKind.Wheel ? e.WheelDelta / 120.0f : 0f);

        switch (e.Kind)
        {
            case NativePointerKind.LeftDown:
                HandlePressed(snapshot, VideraPointerRoute.Native, token: null);
                break;
            case NativePointerKind.LeftUp:
                HandleReleased(snapshot, VideraPointerRoute.Native, token: null);
                break;
            case NativePointerKind.RightDown:
                HandlePressed(snapshot with { IsLeftButtonPressed = false, IsRightButtonPressed = true }, VideraPointerRoute.Native, token: null);
                break;
            case NativePointerKind.RightUp:
                HandleReleased(snapshot with { InitialPressMouseButton = MouseButton.Right }, VideraPointerRoute.Native, token: null);
                break;
            case NativePointerKind.Move:
                HandleMoved(snapshot, VideraPointerRoute.Native, token: null);
                break;
            case NativePointerKind.Wheel:
                HandleWheel(snapshot, VideraPointerRoute.Native, token: null);
                break;
        }
    }

    private bool HandlePressed(VideraPointerGestureSnapshot snapshot, VideraPointerRoute route, object? token)
    {
        if (_disposed)
        {
            return false;
        }

        if (ShouldSkipDuplicate("Pressed", token))
        {
            return false;
        }

        if (InputLogEnabled)
        {
            Log.PointerPressed(_logger, snapshot.Position, snapshot.IsLeftButtonPressed, snapshot.IsRightButtonPressed, route.ToString());
        }

        _host.FocusHost();
        _state.LastPosition = snapshot.Position;

        if (snapshot.IsLeftButtonPressed)
        {
            _state.IsLeftButtonDown = true;
            _state.PointerDownPosition = snapshot.Position;
            _state.PointerDownModifiers = snapshot.Modifiers;
            _state.HasExceededClickThreshold = false;
            _state.IsSelectionBoxActive = false;
            return true;
        }

        if (snapshot.IsRightButtonPressed)
        {
            _state.IsRightButtonDown = true;
            _state.PointerDownPosition = snapshot.Position;
            _state.PointerDownModifiers = snapshot.Modifiers;
            return true;
        }

        return false;
    }

    private bool HandleReleased(VideraPointerGestureSnapshot snapshot, VideraPointerRoute route, object? token)
    {
        if (_disposed)
        {
            return false;
        }

        if (ShouldSkipDuplicate("Released", token))
        {
            return false;
        }

        if (InputLogEnabled)
        {
            Log.PointerReleased(_logger, snapshot.Position, snapshot.InitialPressMouseButton, route.ToString());
        }

        var handled = false;

        if (snapshot.InitialPressMouseButton == MouseButton.Left && _state.IsLeftButtonDown)
        {
            handled = true;
            if (_host.InteractionMode == VideraInteractionMode.Select)
            {
                if (_state.IsSelectionBoxActive)
                {
                    RaiseBoxSelectionRequest(snapshot.Position, _state.PointerDownModifiers);
                }
                else if (!_state.HasExceededClickThreshold)
                {
                    RaiseSelectionRequest(snapshot.Position, snapshot.Modifiers);
                }
            }
            else if (_host.InteractionMode == VideraInteractionMode.Annotate && !_state.HasExceededClickThreshold)
            {
                RaiseAnnotationRequest(snapshot.Position);
            }

            _state.IsLeftButtonDown = false;
            _state.HasExceededClickThreshold = false;
            _state.IsSelectionBoxActive = false;
        }
        else if (snapshot.InitialPressMouseButton == MouseButton.Right && _state.IsRightButtonDown)
        {
            handled = true;
            _state.IsRightButtonDown = false;
        }

        if (!_state.HasActiveGesture)
        {
            _state.PointerDownModifiers = RawInputModifiers.None;
        }

        return handled;
    }

    private bool HandleMoved(VideraPointerGestureSnapshot snapshot, VideraPointerRoute route, object? token)
    {
        if (_disposed)
        {
            return false;
        }

        if (ShouldSkipDuplicate("Moved", token))
        {
            return false;
        }

        if (!_state.HasActiveGesture)
        {
            return false;
        }

        if (InputLogEnabled)
        {
            Log.PointerMoved(_logger, snapshot.Position, route.ToString());
        }

        var dx = (float)(snapshot.Position.X - _state.LastPosition.X);
        var dy = (float)(snapshot.Position.Y - _state.LastPosition.Y);
        _state.LastPosition = snapshot.Position;

        if (_state.IsLeftButtonDown)
        {
            var dragDistance = Distance(snapshot.Position, _state.PointerDownPosition);
            if (dragDistance >= SelectionDragThreshold)
            {
                _state.HasExceededClickThreshold = true;
                if (_host.InteractionMode == VideraInteractionMode.Select)
                {
                    _state.IsSelectionBoxActive = true;
                }
            }

            if (_host.InteractionMode == VideraInteractionMode.Navigate && CanNavigateCamera())
            {
                _host.Engine.Camera.Rotate(dx * 0.5f, dy * 0.5f);
            }

            return true;
        }

        if (_state.IsRightButtonDown && CanNavigateCamera())
        {
            _host.Engine.Camera.Pan(-dx * 0.01f, dy * 0.01f);
            return true;
        }

        return false;
    }

    private bool HandleWheel(VideraPointerGestureSnapshot snapshot, VideraPointerRoute route, object? token)
    {
        if (_disposed)
        {
            return false;
        }

        if (ShouldSkipDuplicate("Wheel", token))
        {
            return false;
        }

        if (!CanNavigateCamera())
        {
            return false;
        }

        if (InputLogEnabled)
        {
            Log.PointerWheelChanged(_logger, snapshot.WheelDeltaY, route.ToString());
        }

        if (Math.Abs(snapshot.WheelDeltaY) <= float.Epsilon)
        {
            return false;
        }

        _host.Engine.Camera.Zoom(snapshot.WheelDeltaY * 0.5f);
        return true;
    }

    private void RaiseSelectionRequest(Point position, RawInputModifiers modifiers)
    {
        var hit = HitTest(position).PrimaryHit;
        var selection = CreateSelectionForHit(hit, modifiers);
        if (selection == null)
        {
            return;
        }

        _host.RaiseSelectionRequested(new SelectionRequestedEventArgs(selection));
    }

    private void RaiseBoxSelectionRequest(Point position, RawInputModifiers modifiers)
    {
        var viewport = _host.GetInteractionViewportSize();
        var result = _boxSelectionService.Select(new SceneBoxSelectionQuery(
            _host.Engine.Camera,
            viewport,
            ToVector2(_state.PointerDownPosition),
            ToVector2(position),
            _host.SceneObjects));

        if (result.ObjectIds.Count == 0 && HasAdditiveModifier(modifiers))
        {
            return;
        }

        var objectIds = HasAdditiveModifier(modifiers)
            ? MergeCurrentSelection(result.ObjectIds)
            : result.ObjectIds;
        var selection = new VideraSelectionState
        {
            ObjectIds = objectIds,
            PrimaryObjectId = objectIds.Count > 0 ? objectIds[0] : null
        };
        _host.RaiseSelectionRequested(new SelectionRequestedEventArgs(selection));
    }

    private void RaiseAnnotationRequest(Point position)
    {
        var hit = HitTest(position).PrimaryHit;
        if (hit != null)
        {
            _host.RaiseAnnotationRequested(new AnnotationRequestedEventArgs(AnnotationAnchorDescriptor.ForObject(hit.ObjectId)));
            return;
        }

        if (TryCreateWorldPointAnchor(position, out var anchor))
        {
            _host.RaiseAnnotationRequested(new AnnotationRequestedEventArgs(anchor));
        }
    }

    private SceneHitTestResult HitTest(Point position)
    {
        return _hitTestService.HitTest(new SceneHitTestRequest(
            _host.Engine.Camera,
            _host.GetInteractionViewportSize(),
            ToVector2(position),
            _host.SceneObjects));
    }

    private VideraSelectionState? CreateSelectionForHit(SceneHitTestResult.SceneHit? hit, RawInputModifiers modifiers)
    {
        if (hit == null)
        {
            if (HasAdditiveModifier(modifiers))
            {
                return null;
            }

            return new VideraSelectionState();
        }

        if (!HasAdditiveModifier(modifiers))
        {
            return new VideraSelectionState
            {
                ObjectIds = [hit.ObjectId],
                PrimaryObjectId = hit.ObjectId
            };
        }

        var nextIds = new List<Guid>(_host.SelectionState.ObjectIds);
        if (!nextIds.Remove(hit.ObjectId))
        {
            nextIds.Add(hit.ObjectId);
        }

        return new VideraSelectionState
        {
            ObjectIds = nextIds,
            PrimaryObjectId = nextIds.Count > 0 ? nextIds[^1] : null
        };
    }

    private IReadOnlyList<Guid> MergeCurrentSelection(IReadOnlyList<Guid> objectIds)
    {
        var merged = new List<Guid>(_host.SelectionState.ObjectIds);
        foreach (var objectId in objectIds)
        {
            if (!merged.Contains(objectId))
            {
                merged.Add(objectId);
            }
        }

        return merged;
    }

    private bool TryCreateWorldPointAnchor(Point position, out AnnotationAnchorDescriptor anchor)
    {
        if (!_host.Engine.Camera.TryCreatePickingRay(ToVector2(position), _host.GetInteractionViewportSize(), out var origin, out var direction))
        {
            anchor = default;
            return false;
        }

        var planeNormal = Vector3.Normalize(_host.Engine.Camera.Target - _host.Engine.Camera.Position);
        var denominator = Vector3.Dot(direction, planeNormal);
        if (Math.Abs(denominator) <= 1e-5f)
        {
            anchor = default;
            return false;
        }

        var distance = Vector3.Dot(_host.Engine.Camera.Target - origin, planeNormal) / denominator;
        if (distance <= 0f)
        {
            anchor = default;
            return false;
        }

        anchor = AnnotationAnchorDescriptor.ForWorldPoint(origin + direction * distance);
        return true;
    }

    private bool CanNavigateCamera()
    {
        return _host.InteractionOptions.AllowCameraNavigation;
    }

    private bool ShouldSkipDuplicate(string phase, object? token)
    {
        if (token == null)
        {
            return false;
        }

        if (ReferenceEquals(_state.LastRoutedToken, token) && string.Equals(_state.LastRoutedPhase, phase, StringComparison.Ordinal))
        {
            return true;
        }

        _state.LastRoutedToken = token;
        _state.LastRoutedPhase = phase;
        return false;
    }

    private bool ShouldHandleTopLevel(PointerEventArgs e)
    {
        var position = e.GetPosition((Visual)_host.PointerCaptureTarget);
        return _state.HasActiveGesture || _host.IsPointWithinHost(position);
    }

    private void OnTopPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ShouldHandleTopLevel(e))
        {
            HandlePressed(e, VideraPointerRoute.TopLevel);
        }
    }

    private void OnTopPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (ShouldHandleTopLevel(e))
        {
            HandleReleased(e, VideraPointerRoute.TopLevel);
        }
    }

    private void OnTopPointerMoved(object? sender, PointerEventArgs e)
    {
        if (ShouldHandleTopLevel(e))
        {
            HandleMoved(e, VideraPointerRoute.TopLevel);
        }
    }

    private void OnTopPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        if (_host.IsPointWithinHost(e.GetPosition((Visual)_host.PointerCaptureTarget)))
        {
            HandleWheel(e, VideraPointerRoute.TopLevel);
        }
    }

    private void DetachTopLevelHandlers()
    {
        if (_topLevel == null)
        {
            return;
        }

        _topLevel.RemoveHandler(InputElement.PointerPressedEvent, OnTopPointerPressed);
        _topLevel.RemoveHandler(InputElement.PointerReleasedEvent, OnTopPointerReleased);
        _topLevel.RemoveHandler(InputElement.PointerMovedEvent, OnTopPointerMoved);
        _topLevel.RemoveHandler(InputElement.PointerWheelChangedEvent, OnTopPointerWheel);
        _topLevel = null;
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

    private static bool HasAdditiveModifier(RawInputModifiers modifiers)
    {
        return (modifiers & RawInputModifiers.Control) != 0 || (modifiers & RawInputModifiers.Shift) != 0;
    }

    private static Vector2 ToVector2(Point point)
    {
        return new Vector2((float)point.X, (float)point.Y);
    }

    private static double Distance(Point left, Point right)
    {
        var dx = left.X - right.X;
        var dy = left.Y - right.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 201, Level = LogLevel.Debug, Message = "Pressed at {Position}, Left={IsLeft}, Right={IsRight}, Route={Route}")]
        public static partial void PointerPressed(ILogger logger, Point position, bool isLeft, bool isRight, string route);

        [LoggerMessage(EventId = 202, Level = LogLevel.Debug, Message = "Released at {Position} ({Button}) via {Route}")]
        public static partial void PointerReleased(ILogger logger, Point position, MouseButton button, string route);

        [LoggerMessage(EventId = 203, Level = LogLevel.Debug, Message = "Moved to {Position} via {Route}")]
        public static partial void PointerMoved(ILogger logger, Point position, string route);

        [LoggerMessage(EventId = 204, Level = LogLevel.Debug, Message = "Wheel delta {Delta} via {Route}")]
        public static partial void PointerWheelChanged(ILogger logger, float delta, string route);
    }
}

internal static class VideraInteractionModifierExtensions
{
    public static RawInputModifiers ToRawInputModifiers(this KeyModifiers modifiers)
    {
        var raw = RawInputModifiers.None;
        if (modifiers.HasFlag(KeyModifiers.Shift))
        {
            raw |= RawInputModifiers.Shift;
        }

        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            raw |= RawInputModifiers.Control;
        }

        if (modifiers.HasFlag(KeyModifiers.Alt))
        {
            raw |= RawInputModifiers.Alt;
        }

        if (modifiers.HasFlag(KeyModifiers.Meta))
        {
            raw |= RawInputModifiers.Meta;
        }

        return raw;
    }
}
