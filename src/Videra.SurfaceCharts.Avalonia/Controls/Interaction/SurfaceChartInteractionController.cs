using Avalonia;
using Avalonia.Input;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceChartInteractionController
{
    private const double OrbitDegreesPerPixel = 0.35;
    private const double PanViewportFractionPerPixel = 1.0;
    private const double DollyScalePerWheelStep = 0.9;
    private const double MinimumFocusSelectionPixels = 4.0;
    private GestureMode _activeGesture;
    private Point _gestureStartPosition;
    private Point _lastPosition;

    public bool HandlePointerPressed(PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var source = GetSourceVisual(e);
        var properties = e.GetCurrentPoint(source).Properties;
        if (properties.IsLeftButtonPressed && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            _activeGesture = GestureMode.BoxZoom;
            _gestureStartPosition = e.GetPosition(source);
            _lastPosition = _gestureStartPosition;
            return true;
        }

        if (properties.IsLeftButtonPressed)
        {
            _activeGesture = GestureMode.Orbit;
            _gestureStartPosition = e.GetPosition(source);
            _lastPosition = _gestureStartPosition;
            return true;
        }

        if (properties.IsRightButtonPressed)
        {
            _activeGesture = GestureMode.Pan;
            _gestureStartPosition = e.GetPosition(source);
            _lastPosition = _gestureStartPosition;
            return true;
        }

        return false;
    }

    public SurfaceViewState? HandlePointerMoved(PointerEventArgs e, SurfaceViewState currentViewState, Size viewSize)
    {
        ArgumentNullException.ThrowIfNull(e);
        ArgumentNullException.ThrowIfNull(currentViewState);

        if (_activeGesture == GestureMode.None)
        {
            return null;
        }

        var position = e.GetPosition(GetSourceVisual(e));
        var delta = position - _lastPosition;
        _lastPosition = position;

        return _activeGesture switch
        {
            GestureMode.Orbit => CreateOrbitedViewState(currentViewState, delta),
            GestureMode.Pan => CreatePannedViewState(currentViewState, delta, viewSize),
            GestureMode.BoxZoom => null,
            _ => null,
        };
    }

    public SurfaceInteractionReleaseResult HandlePointerReleased(
        PointerReleasedEventArgs e,
        SurfaceViewState currentViewState,
        Size viewSize)
    {
        ArgumentNullException.ThrowIfNull(e);
        ArgumentNullException.ThrowIfNull(currentViewState);

        if (_activeGesture == GestureMode.None)
        {
            return SurfaceInteractionReleaseResult.NotHandled;
        }

        SurfaceViewState? updatedViewState = null;
        if (_activeGesture == GestureMode.BoxZoom)
        {
            updatedViewState = CreateBoxZoomViewState(currentViewState, _gestureStartPosition, e.GetPosition(GetSourceVisual(e)), viewSize);
        }

        _activeGesture = GestureMode.None;
        return new SurfaceInteractionReleaseResult(
            Handled: true,
            ViewState: updatedViewState);
    }

    public static SurfaceViewState? HandlePointerWheel(PointerWheelEventArgs e, SurfaceViewState currentViewState)
    {
        ArgumentNullException.ThrowIfNull(e);
        ArgumentNullException.ThrowIfNull(currentViewState);

        if (Math.Abs(e.Delta.Y) <= double.Epsilon)
        {
            return null;
        }

        var scale = Math.Pow(DollyScalePerWheelStep, e.Delta.Y);
        var currentCamera = currentViewState.Camera;
        var newDistance = Math.Max(1d, currentCamera.Distance * scale);

        var updatedCamera = new SurfaceCameraPose(
            currentCamera.Target,
            currentCamera.Yaw,
            currentCamera.Pitch,
            newDistance,
            currentCamera.FieldOfView,
            currentCamera.ProjectionMode);

        return new SurfaceViewState(currentViewState.DataWindow, updatedCamera);
    }

    public void Reset()
    {
        _activeGesture = GestureMode.None;
    }

    private static SurfaceViewState CreateOrbitedViewState(SurfaceViewState currentViewState, Vector delta)
    {
        var currentCamera = currentViewState.Camera;
        var updatedCamera = new SurfaceCameraPose(
            currentCamera.Target,
            currentCamera.Yaw + (delta.X * OrbitDegreesPerPixel),
            Math.Clamp(currentCamera.Pitch - (delta.Y * OrbitDegreesPerPixel), -89d, 89d),
            currentCamera.Distance,
            currentCamera.FieldOfView,
            currentCamera.ProjectionMode);

        return new SurfaceViewState(currentViewState.DataWindow, updatedCamera);
    }

    private static SurfaceViewState CreatePannedViewState(SurfaceViewState currentViewState, Vector delta, Size viewSize)
    {
        if (viewSize.Width <= 0d || viewSize.Height <= 0d)
        {
            return currentViewState;
        }

        var currentWindow = currentViewState.DataWindow;
        var deltaX = -(delta.X / viewSize.Width) * currentWindow.Width * PanViewportFractionPerPixel;
        var deltaY = -(delta.Y / viewSize.Height) * currentWindow.Height * PanViewportFractionPerPixel;
        var updatedWindow = new SurfaceDataWindow(
            currentWindow.XMin + deltaX,
            currentWindow.XMax + deltaX,
            currentWindow.YMin + deltaY,
            currentWindow.YMax + deltaY);

        return new SurfaceViewState(updatedWindow, currentViewState.Camera);
    }

    private static SurfaceViewState? CreateBoxZoomViewState(
        SurfaceViewState currentViewState,
        Point selectionStart,
        Point selectionEnd,
        Size viewSize)
    {
        if (viewSize.Width <= 0d || viewSize.Height <= 0d)
        {
            return null;
        }

        var selectionWidth = Math.Abs(selectionEnd.X - selectionStart.X);
        var selectionHeight = Math.Abs(selectionEnd.Y - selectionStart.Y);
        if (selectionWidth < MinimumFocusSelectionPixels || selectionHeight < MinimumFocusSelectionPixels)
        {
            return null;
        }

        var currentWindow = currentViewState.DataWindow;
        var minX = Math.Clamp(Math.Min(selectionStart.X, selectionEnd.X), 0d, viewSize.Width);
        var maxX = Math.Clamp(Math.Max(selectionStart.X, selectionEnd.X), 0d, viewSize.Width);
        var minY = Math.Clamp(Math.Min(selectionStart.Y, selectionEnd.Y), 0d, viewSize.Height);
        var maxY = Math.Clamp(Math.Max(selectionStart.Y, selectionEnd.Y), 0d, viewSize.Height);

        var focusedWindow = new SurfaceDataWindow(
            currentWindow.XMin + ((minX / viewSize.Width) * currentWindow.Width),
            currentWindow.XMin + ((maxX / viewSize.Width) * currentWindow.Width),
            currentWindow.YMin + ((minY / viewSize.Height) * currentWindow.Height),
            currentWindow.YMin + ((maxY / viewSize.Height) * currentWindow.Height));

        return new SurfaceViewState(
            focusedWindow,
            CreateFocusedCameraPose(focusedWindow, currentViewState.Camera));
    }

    private static SurfaceCameraPose CreateFocusedCameraPose(SurfaceDataWindow focusedWindow, SurfaceCameraPose currentCamera)
    {
        var defaultCamera = SurfaceChartRuntime.CreateDefaultCameraPose(focusedWindow);
        return new SurfaceCameraPose(
            defaultCamera.Target,
            currentCamera.Yaw,
            currentCamera.Pitch,
            defaultCamera.Distance,
            currentCamera.FieldOfView,
            currentCamera.ProjectionMode);
    }

    private static Visual GetSourceVisual(PointerEventArgs e)
    {
        return e.Source as Visual
            ?? throw new InvalidOperationException("Surface chart pointer events must originate from a visual.");
    }

    internal readonly record struct SurfaceInteractionReleaseResult(bool Handled, SurfaceViewState? ViewState)
    {
        public static SurfaceInteractionReleaseResult NotHandled => new(false, null);
    }

    private enum GestureMode
    {
        None,
        Orbit,
        Pan,
        BoxZoom,
    }
}
