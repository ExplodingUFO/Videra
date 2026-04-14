using Avalonia;
using Avalonia.Input;
using Microsoft.Extensions.Logging;

namespace Videra.Avalonia.Controls.Interaction;

internal sealed partial class VideraInteractionController : IDisposable
{
    private const double SelectionDragThreshold = 4d;

    private static readonly bool InputLogEnabled =
        string.Equals(Environment.GetEnvironmentVariable("VIDERA_INPUTLOG"), "1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Environment.GetEnvironmentVariable("VIDERA_INPUTLOG"), "true", StringComparison.OrdinalIgnoreCase);

    private readonly IVideraInteractionHost _host;
    private readonly ILogger _logger;
    private readonly VideraSelectionIntentResolver _selectionIntentResolver = new();
    private readonly VideraAnnotationIntentResolver _annotationIntentResolver = new();
    private readonly VideraInteractionState _state = new();
    private bool _disposed;

    public VideraInteractionController(IVideraInteractionHost host, ILogger logger)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool HasActiveGesture => _state.HasActiveGesture;

    public bool IsLeftButtonDown => _state.IsLeftButtonDown;

    public bool IsRightButtonDown => _state.IsRightButtonDown;

    public void Dispose()
    {
        _disposed = true;
        Reset();
    }

    public void Reset()
    {
        _state.Reset();
    }

    public bool HandlePressed(VideraPointerGestureSnapshot snapshot, VideraPointerRoute route)
    {
        if (_disposed)
        {
            return false;
        }

        if (InputLogEnabled)
        {
            Log.PointerPressed(_logger, snapshot.Position, snapshot.IsLeftButtonPressed, snapshot.IsRightButtonPressed, route.ToString());
        }

        _host.FocusHost();
        _state.LastPosition = snapshot.Position;

        var handled = false;

        if (snapshot.IsLeftButtonPressed && !_state.IsLeftButtonDown)
        {
            _state.IsLeftButtonDown = true;
            _state.PointerDownPosition = snapshot.Position;
            _state.PointerDownModifiers = snapshot.Modifiers;
            _state.HasExceededClickThreshold = false;
            _state.IsSelectionBoxActive = false;
            handled = true;
        }

        if (snapshot.IsRightButtonPressed && !_state.IsRightButtonDown)
        {
            _state.IsRightButtonDown = true;
            handled = true;
        }

        return handled;
    }

    public bool HandleReleased(VideraPointerGestureSnapshot snapshot, VideraPointerRoute route)
    {
        if (_disposed)
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
                VideraSelectionRequest? request = null;
                if (_state.IsSelectionBoxActive)
                {
                    request = _selectionIntentResolver.CreateBoxRequest(
                        _host.Engine.Camera,
                        _host.GetInteractionViewportSize(),
                        _host.SceneObjects,
                        _state.PointerDownPosition,
                        snapshot.Position,
                        _state.PointerDownModifiers,
                        _host.InteractionOptions);
                }
                else if (!_state.HasExceededClickThreshold)
                {
                    request = _selectionIntentResolver.CreateClickRequest(
                        _host.Engine.Camera,
                        _host.GetInteractionViewportSize(),
                        _host.SceneObjects,
                        snapshot.Position,
                        snapshot.Modifiers,
                        _host.InteractionOptions);
                }

                if (request is not null)
                {
                    _host.RaiseSelectionRequested(new SelectionRequestedEventArgs(request));
                }
            }
            else if (_host.InteractionMode == VideraInteractionMode.Annotate &&
                     !_state.HasExceededClickThreshold &&
                     _annotationIntentResolver.TryResolveAnchor(
                         _host.Engine.Camera,
                         _host.GetInteractionViewportSize(),
                         _host.SceneObjects,
                         snapshot.Position,
                         out var anchor))
            {
                _host.RaiseAnnotationRequested(new AnnotationRequestedEventArgs(anchor));
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

    public bool HandleMoved(VideraPointerGestureSnapshot snapshot, VideraPointerRoute route)
    {
        if (_disposed || !_state.HasActiveGesture)
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

    public bool HandleWheel(VideraPointerGestureSnapshot snapshot, VideraPointerRoute route)
    {
        if (_disposed || !CanNavigateCamera() || Math.Abs(snapshot.WheelDeltaY) <= float.Epsilon)
        {
            return false;
        }

        if (InputLogEnabled)
        {
            Log.PointerWheelChanged(_logger, snapshot.WheelDeltaY, route.ToString());
        }

        _host.Engine.Camera.Zoom(snapshot.WheelDeltaY * 0.5f);
        return true;
    }

    private bool CanNavigateCamera()
    {
        return _host.InteractionOptions.AllowCameraNavigation;
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
