using System.Numerics;
using Avalonia;
using Avalonia.Input;
using Microsoft.Extensions.Logging;
using Videra.Avalonia.Rendering;
using Videra.Core.Interaction;
using Videra.Core.Picking;
using Videra.Core.Inspection;
using Videra.Core.Selection;

namespace Videra.Avalonia.Controls.Interaction;

internal sealed partial class VideraInteractionController : IDisposable
{
    private const double SelectionDragThreshold = 4d;

    private static readonly bool InputLogEnabled =
        string.Equals(Environment.GetEnvironmentVariable("VIDERA_INPUTLOG"), "1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Environment.GetEnvironmentVariable("VIDERA_INPUTLOG"), "true", StringComparison.OrdinalIgnoreCase);

    private readonly IVideraInteractionHost _host;
    private readonly ILogger _logger;
    private readonly OrbitCameraManipulator _cameraManipulator = new();
    private readonly PickingService _pickingService = new();
    private readonly SelectionBoxService _selectionBoxService = new();
    private readonly VideraInteractionState _state = new();
    private VideraMeasurementAnchor? _pendingMeasurementAnchor;
    private InteractiveFrameLease? _interactiveFrameLease;
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
        _interactiveFrameLease?.Dispose();
        _interactiveFrameLease = null;
        _pendingMeasurementAnchor = null;
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
        ResetMeasurementIntentIfInactive();
        _state.LastPosition = snapshot.Position;

        var handled = false;

        if (snapshot.IsLeftButtonPressed && !_state.IsLeftButtonDown)
        {
            _state.IsLeftButtonDown = true;
            _state.PointerDownPosition = snapshot.Position;
            _state.PointerDownModifiers = snapshot.Modifiers;
            _state.HasExceededClickThreshold = false;
            _state.IsSelectionBoxActive = false;
            AcquireInteractiveLeaseIfNeeded();
            handled = true;
        }

        if (snapshot.IsRightButtonPressed && !_state.IsRightButtonDown)
        {
            _state.IsRightButtonDown = true;
            AcquireInteractiveLeaseIfNeeded();
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
                    var result = _selectionBoxService.Select(
                        _host.Engine.Camera,
                        _host.GetInteractionViewportSize(),
                        ToVector2(_state.PointerDownPosition),
                        ToVector2(snapshot.Position),
                        _host.SceneObjects);
                    request = CreateBoxRequest(result, _state.PointerDownModifiers);
                }
                else if (!_state.HasExceededClickThreshold)
                {
                    request = CreateClickRequest(snapshot.Position, snapshot.Modifiers);
                }

                if (request is not null)
                {
                    _host.RaiseSelectionRequested(new SelectionRequestedEventArgs(request));
                }
            }
            else if (_host.InteractionMode == VideraInteractionMode.Annotate &&
                     !_state.HasExceededClickThreshold &&
                     _pickingService.TryResolveAnnotationAnchor(
                         _host.Engine.Camera,
                         _host.GetInteractionViewportSize(),
                         ToVector2(snapshot.Position),
                         _host.SceneObjects,
                         out var anchor))
            {
                _host.RaiseAnnotationRequested(new AnnotationRequestedEventArgs(anchor));
            }
            else if (_host.InteractionMode == VideraInteractionMode.Measure &&
                     !_state.HasExceededClickThreshold &&
                     _pickingService.TryResolveMeasurementAnchor(
                         _host.Engine.Camera,
                         _host.GetInteractionViewportSize(),
                         ToVector2(snapshot.Position),
                         _host.SceneObjects,
                         _host.InteractionOptions.MeasurementSnapMode,
                         _pendingMeasurementAnchor,
                         out var measurementAnchor))
            {
                if (_pendingMeasurementAnchor is VideraMeasurementAnchor startAnchor)
                {
                    _host.Measurements = _host.Measurements
                        .Append(new VideraMeasurement
                        {
                            Start = startAnchor,
                            End = measurementAnchor
                        })
                        .ToArray();
                    _pendingMeasurementAnchor = null;
                }
                else
                {
                    _pendingMeasurementAnchor = measurementAnchor;
                }
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
            _interactiveFrameLease?.Dispose();
            _interactiveFrameLease = null;
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
                _cameraManipulator.Rotate(_host.Engine.Camera, dx, dy);
                _host.InvalidateRender(RenderInvalidationKinds.Interaction);
            }

            return true;
        }

        if (_state.IsRightButtonDown && CanNavigateCamera())
        {
            _cameraManipulator.Pan(_host.Engine.Camera, dx, dy);
            _host.InvalidateRender(RenderInvalidationKinds.Interaction);
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

        _cameraManipulator.Zoom(_host.Engine.Camera, snapshot.WheelDeltaY);
        _host.InvalidateRender(RenderInvalidationKinds.Interaction);
        return true;
    }

    private bool CanNavigateCamera()
    {
        return _host.InteractionOptions.AllowCameraNavigation;
    }

    private void AcquireInteractiveLeaseIfNeeded()
    {
        if (!CanNavigateCamera() || _interactiveFrameLease is not null)
        {
            return;
        }

        _interactiveFrameLease = _host.BeginInteractiveFrameLease();
    }

    private void ResetMeasurementIntentIfInactive()
    {
        if (_host.InteractionMode != VideraInteractionMode.Measure)
        {
            _pendingMeasurementAnchor = null;
        }
    }

    private VideraSelectionRequest? CreateClickRequest(Point position, RawInputModifiers modifiers)
    {
        var hit = _pickingService.HitTest(
            _host.Engine.Camera,
            _host.GetInteractionViewportSize(),
            ToVector2(position),
            _host.SceneObjects).PrimaryHit;

        if (hit is null)
        {
            if (HasAdditiveModifier(modifiers) || _host.InteractionOptions.EmptySpaceSelectionBehavior == VideraEmptySpaceSelectionBehavior.PreserveSelection)
            {
                return null;
            }

            return new VideraSelectionRequest(
                VideraSelectionOperation.Replace,
                Array.Empty<Guid>(),
                primaryObjectId: null,
                _host.InteractionOptions.EmptySpaceSelectionBehavior);
        }

        return new VideraSelectionRequest(
            HasAdditiveModifier(modifiers) ? VideraSelectionOperation.Toggle : VideraSelectionOperation.Replace,
            [hit.ObjectId],
            hit.ObjectId,
            _host.InteractionOptions.EmptySpaceSelectionBehavior);
    }

    private VideraSelectionRequest? CreateBoxRequest(SceneBoxSelectionResult result, RawInputModifiers modifiers)
    {
        if (result.ObjectIds.Count == 0)
        {
            if (HasAdditiveModifier(modifiers) || _host.InteractionOptions.EmptySpaceSelectionBehavior == VideraEmptySpaceSelectionBehavior.PreserveSelection)
            {
                return null;
            }

            return new VideraSelectionRequest(
                VideraSelectionOperation.Replace,
                Array.Empty<Guid>(),
                primaryObjectId: null,
                _host.InteractionOptions.EmptySpaceSelectionBehavior);
        }

        return new VideraSelectionRequest(
            HasAdditiveModifier(modifiers) ? VideraSelectionOperation.Add : VideraSelectionOperation.Replace,
            result.ObjectIds,
            result.ObjectIds[0],
            _host.InteractionOptions.EmptySpaceSelectionBehavior);
    }

    private static bool HasAdditiveModifier(RawInputModifiers modifiers)
    {
        return (modifiers & RawInputModifiers.Control) != 0 || (modifiers & RawInputModifiers.Shift) != 0;
    }

    private static Vector2 ToVector2(Point point) => new((float)point.X, (float)point.Y);

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
