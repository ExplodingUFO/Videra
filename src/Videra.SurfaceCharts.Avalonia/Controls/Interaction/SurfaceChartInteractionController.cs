using Avalonia;
using Avalonia.Input;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceChartInteractionController
{
    private const double OrbitDegreesPerPixel = 0.35;
    private const double PanViewportFractionPerPixel = 1.0;
    private const double DollyScalePerWheelStep = 0.9;
    private GestureMode _activeGesture;
    private Point _lastPosition;

    public bool HandlePointerPressed(PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var source = GetSourceVisual(e);
        var properties = e.GetCurrentPoint(source).Properties;
        if (properties.IsLeftButtonPressed)
        {
            _activeGesture = GestureMode.Orbit;
            _lastPosition = e.GetPosition(source);
            return true;
        }

        if (properties.IsRightButtonPressed)
        {
            _activeGesture = GestureMode.Pan;
            _lastPosition = e.GetPosition(source);
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
            _ => null,
        };
    }

    public bool HandlePointerReleased(PointerReleasedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (_activeGesture == GestureMode.None)
        {
            return false;
        }

        _activeGesture = GestureMode.None;
        return true;
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

    private static Visual GetSourceVisual(PointerEventArgs e)
    {
        return e.Source as Visual
            ?? throw new InvalidOperationException("Surface chart pointer events must originate from a visual.");
    }

    private enum GestureMode
    {
        None,
        Orbit,
        Pan,
    }
}
