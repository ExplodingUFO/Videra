using Avalonia;
using Avalonia.Input;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceChartInteractionController
{
    private const double DragThreshold = 4d;
    private const double OrbitDegreesPerPixel = 0.5d;
    private const double WheelZoomStepFactor = 0.85d;
    private const double MinimumWindowSpan = 1d;
    private const double MinimumPitchDegrees = -80d;
    private const double MaximumPitchDegrees = 80d;

    private Point? _pressPosition;
    private Point? _lastPosition;
    private MouseButton? _activeButton;
    private SurfaceChartGestureMode _gestureMode;
    private bool _hasExceededDragThreshold;

    public bool HasActiveGesture => _activeButton is not null;

    /// <summary>
    /// Gets the currently active gesture mode, or <see cref="SurfaceChartGestureMode.None"/> if no gesture is active.
    /// </summary>
    public SurfaceChartGestureMode ActiveGestureMode => HasActiveGesture ? _gestureMode : SurfaceChartGestureMode.None;

    public Rect? ActiveSelectionRect { get; private set; }

    public bool HandlePointerPressed(PointerUpdateKind updateKind, Point position, KeyModifiers keyModifiers)
    {
        var gestureMode = updateKind switch
        {
            PointerUpdateKind.LeftButtonPressed => ResolveLeftButtonGestureMode(keyModifiers),
            PointerUpdateKind.RightButtonPressed => SurfaceChartGestureMode.Pan,
            _ => SurfaceChartGestureMode.None,
        };

        if (gestureMode == SurfaceChartGestureMode.None)
        {
            return false;
        }

        _activeButton = updateKind == PointerUpdateKind.RightButtonPressed ? MouseButton.Right : MouseButton.Left;
        _gestureMode = gestureMode;
        _pressPosition = position;
        _lastPosition = position;
        _hasExceededDragThreshold = false;
        ActiveSelectionRect = null;
        return true;
    }

    public bool HandlePointerMoved(Point position, SurfaceChartRuntime runtime)
    {
        if (!HasActiveGesture || _pressPosition is not Point pressPosition)
        {
            return false;
        }

        var previousPosition = _lastPosition ?? pressPosition;
        _lastPosition = position;

        var dragDistance = GetTravelDistance(pressPosition, position);
        if (!_hasExceededDragThreshold && dragDistance >= DragThreshold)
        {
            _hasExceededDragThreshold = true;
        }

        if (!_hasExceededDragThreshold)
        {
            return true;
        }

        return _gestureMode switch
        {
            SurfaceChartGestureMode.Orbit => ApplyOrbit(runtime, previousPosition, position),
            SurfaceChartGestureMode.Pan => ApplyPan(runtime, previousPosition, position),
            SurfaceChartGestureMode.FocusSelection => UpdateSelection(position, runtime),
            _ => true,
        };
    }

    public SurfaceChartPointerReleaseResult HandlePointerReleased(
        MouseButton mouseButton,
        Point position,
        KeyModifiers keyModifiers,
        SurfaceChartRuntime runtime)
    {
        if (!HasActiveGesture)
        {
            return default;
        }

        var handled = _activeButton == mouseButton;
        var shouldTogglePin = false;

        if (handled && _gestureMode == SurfaceChartGestureMode.PinToggle)
        {
            shouldTogglePin =
                mouseButton == MouseButton.Left &&
                keyModifiers.HasFlag(KeyModifiers.Shift) &&
                _pressPosition is Point pressPosition &&
                !_hasExceededDragThreshold &&
                GetTravelDistance(pressPosition, position) < DragThreshold;
        }
        else if (handled &&
                 _gestureMode == SurfaceChartGestureMode.FocusSelection &&
                 _hasExceededDragThreshold &&
                 ActiveSelectionRect is Rect selectionRect)
        {
            CompleteFocusSelection(runtime, selectionRect);
        }

        Reset();
        return new SurfaceChartPointerReleaseResult(handled, shouldTogglePin);
    }

    public static bool HandlePointerWheelChanged(
        Vector delta,
        SurfaceChartRuntime runtime,
        SurfaceProbeInfo? hoveredProbe)
    {
        if (Math.Abs(delta.Y) <= double.Epsilon)
        {
            return false;
        }

        return ApplyDolly(runtime, delta.Y, hoveredProbe);
    }

    public void Reset()
    {
        _pressPosition = null;
        _lastPosition = null;
        _activeButton = null;
        _gestureMode = SurfaceChartGestureMode.None;
        _hasExceededDragThreshold = false;
        ActiveSelectionRect = null;
    }

    private static SurfaceChartGestureMode ResolveLeftButtonGestureMode(KeyModifiers keyModifiers)
    {
        if (keyModifiers.HasFlag(KeyModifiers.Shift))
        {
            return SurfaceChartGestureMode.PinToggle;
        }

        if (keyModifiers.HasFlag(KeyModifiers.Control))
        {
            return SurfaceChartGestureMode.FocusSelection;
        }

        return SurfaceChartGestureMode.Orbit;
    }

    private static bool ApplyOrbit(SurfaceChartRuntime runtime, Point previousPosition, Point currentPosition)
    {
        if (!runtime.CanInteract)
        {
            return false;
        }

        runtime.BeginInteraction();
        var currentViewState = runtime.ViewState;
        var currentCamera = currentViewState.Camera;
        var deltaX = currentPosition.X - previousPosition.X;
        var deltaY = currentPosition.Y - previousPosition.Y;
        var nextCamera = new SurfaceCameraPose(
            currentCamera.Target,
            NormalizeDegrees(currentCamera.YawDegrees - (deltaX * OrbitDegreesPerPixel)),
            Math.Clamp(currentCamera.PitchDegrees + (deltaY * OrbitDegreesPerPixel), MinimumPitchDegrees, MaximumPitchDegrees),
            currentCamera.Distance,
            currentCamera.FieldOfViewDegrees);

        runtime.UpdateViewState(
            new SurfaceViewState(
                currentViewState.DataWindow,
                nextCamera,
                currentViewState.DisplaySpace));
        return true;
    }

    private static bool ApplyPan(SurfaceChartRuntime runtime, Point previousPosition, Point currentPosition)
    {
        if (!TryGetInteractionState(runtime, out var metadata, out var viewSize, out var currentViewState))
        {
            return false;
        }

        runtime.BeginInteraction();
        var currentWindow = currentViewState.DataWindow;
        var deltaX = currentPosition.X - previousPosition.X;
        var deltaY = currentPosition.Y - previousPosition.Y;
        var sampleDeltaX = (deltaX / viewSize.Width) * currentWindow.Height;
        var sampleDeltaY = (deltaY / viewSize.Height) * currentWindow.Width;
        var nextWindow = new SurfaceDataWindow(
                currentWindow.StartX + sampleDeltaY,
                currentWindow.StartY - sampleDeltaX,
                currentWindow.Width,
                currentWindow.Height)
            .ClampTo(metadata);
        var nextCamera = RecenterCamera(metadata, nextWindow, currentViewState.Camera, distanceScale: 1d);

        runtime.UpdateViewState(
            new SurfaceViewState(
                nextWindow,
                nextCamera,
                currentViewState.DisplaySpace));
        return true;
    }

    private static bool ApplyDolly(SurfaceChartRuntime runtime, double wheelDeltaY, SurfaceProbeInfo? hoveredProbe)
    {
        if (!TryGetInteractionState(runtime, out var metadata, out _, out var currentViewState))
        {
            return false;
        }

        runtime.BeginInteraction();
        var currentWindow = currentViewState.DataWindow;
        var zoomFactor = Math.Pow(WheelZoomStepFactor, wheelDeltaY);
        var nextWidth = Math.Clamp(currentWindow.Width * zoomFactor, MinimumWindowSpan, metadata.Width);
        var nextHeight = Math.Clamp(currentWindow.Height * zoomFactor, MinimumWindowSpan, metadata.Height);
        var anchorSampleX = hoveredProbe?.SampleX ?? (currentWindow.StartX + (currentWindow.Width * 0.5d));
        var anchorSampleY = hoveredProbe?.SampleY ?? (currentWindow.StartY + (currentWindow.Height * 0.5d));
        var anchorRatioX = Math.Clamp((anchorSampleX - currentWindow.StartX) / currentWindow.Width, 0d, 1d);
        var anchorRatioY = Math.Clamp((anchorSampleY - currentWindow.StartY) / currentWindow.Height, 0d, 1d);
        var nextWindow = new SurfaceDataWindow(
                anchorSampleX - (anchorRatioX * nextWidth),
                anchorSampleY - (anchorRatioY * nextHeight),
                nextWidth,
                nextHeight)
            .ClampTo(metadata);

        var widthScale = nextWindow.Width / currentWindow.Width;
        var heightScale = nextWindow.Height / currentWindow.Height;
        var nextCamera = RecenterCamera(metadata, nextWindow, currentViewState.Camera, (widthScale + heightScale) * 0.5d);

        runtime.UpdateViewState(
            new SurfaceViewState(
                nextWindow,
                nextCamera,
                currentViewState.DisplaySpace));
        return true;
    }

    private bool UpdateSelection(Point position, SurfaceChartRuntime runtime)
    {
        if (_pressPosition is not Point pressPosition)
        {
            return false;
        }

        runtime.BeginInteraction();
        ActiveSelectionRect = CreateSelectionRect(pressPosition, position);
        return true;
    }

    private static void CompleteFocusSelection(SurfaceChartRuntime runtime, Rect selectionRect)
    {
        if (!TryGetInteractionState(runtime, out var metadata, out var viewSize, out var currentViewState))
        {
            return;
        }

        var currentWindow = currentViewState.DataWindow;
        if (selectionRect.Width < DragThreshold || selectionRect.Height < DragThreshold)
        {
            return;
        }

        var nextWindow = CreateFocusedWindow(selectionRect, currentWindow, viewSize).ClampTo(metadata);
        var widthScale = nextWindow.Width / currentWindow.Width;
        var heightScale = nextWindow.Height / currentWindow.Height;
        var nextCamera = RecenterCamera(metadata, nextWindow, currentViewState.Camera, (widthScale + heightScale) * 0.5d);

        runtime.UpdateViewState(
            new SurfaceViewState(
                nextWindow,
                nextCamera,
                currentViewState.DisplaySpace));
    }

    private static SurfaceDataWindow CreateFocusedWindow(Rect selectionRect, SurfaceDataWindow currentWindow, Size viewSize)
    {
        var startRatioX = Math.Clamp(selectionRect.X / viewSize.Width, 0d, 1d);
        var startRatioY = Math.Clamp(selectionRect.Y / viewSize.Height, 0d, 1d);
        var widthRatio = Math.Clamp(selectionRect.Width / viewSize.Width, MinimumWindowSpan / currentWindow.Width, 1d);
        var heightRatio = Math.Clamp(selectionRect.Height / viewSize.Height, MinimumWindowSpan / currentWindow.Height, 1d);

        return new SurfaceDataWindow(
            currentWindow.StartX + (currentWindow.Width * startRatioX),
            currentWindow.StartY + (currentWindow.Height * startRatioY),
            Math.Max(currentWindow.Width * widthRatio, MinimumWindowSpan),
            Math.Max(currentWindow.Height * heightRatio, MinimumWindowSpan));
    }

    private static SurfaceCameraPose RecenterCamera(
        SurfaceMetadata metadata,
        SurfaceDataWindow dataWindow,
        SurfaceCameraPose currentCamera,
        double distanceScale)
    {
        var centeredCamera = SurfaceCameraPose.CreateDefault(metadata, dataWindow);
        return new SurfaceCameraPose(
            centeredCamera.Target,
            currentCamera.YawDegrees,
            currentCamera.PitchDegrees,
            Math.Max(currentCamera.Distance * distanceScale, MinimumWindowSpan),
            currentCamera.FieldOfViewDegrees);
    }

    private static bool TryGetInteractionState(
        SurfaceChartRuntime runtime,
        out SurfaceMetadata metadata,
        out Size viewSize,
        out SurfaceViewState currentViewState)
    {
        metadata = null!;
        viewSize = default;
        currentViewState = default;

        if (!runtime.CanInteract || runtime.Metadata is not SurfaceMetadata resolvedMetadata)
        {
            return false;
        }

        metadata = resolvedMetadata;
        viewSize = runtime.ViewSize;
        currentViewState = runtime.ViewState;
        return true;
    }

    private static Rect CreateSelectionRect(Point start, Point end)
    {
        return new Rect(
            Math.Min(start.X, end.X),
            Math.Min(start.Y, end.Y),
            Math.Abs(end.X - start.X),
            Math.Abs(end.Y - start.Y));
    }

    private static double GetTravelDistance(Point start, Point end)
    {
        var deltaX = end.X - start.X;
        var deltaY = end.Y - start.Y;
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
    }

    private static double NormalizeDegrees(double degrees)
    {
        var normalized = degrees % 360d;
        if (normalized <= -180d)
        {
            return normalized + 360d;
        }

        if (normalized > 180d)
        {
            return normalized - 360d;
        }

        return normalized;
    }
}

internal readonly record struct SurfaceChartPointerReleaseResult(bool Handled, bool TogglePinnedProbe);

internal enum SurfaceChartGestureMode
{
    None,
    PinToggle,
    Orbit,
    Pan,
    FocusSelection,
}
