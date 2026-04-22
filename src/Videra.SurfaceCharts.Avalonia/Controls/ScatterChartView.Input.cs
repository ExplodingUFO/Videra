using Avalonia;
using Avalonia.Input;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

public sealed partial class ScatterChartView
{
    private const double OrbitDegreesPerPixel = 0.35d;
    private const double ZoomExponentPerWheelDelta = 0.15d;
    private const double MinimumPitchDegrees = -89d;
    private const double MaximumPitchDegrees = 89d;

    private bool _isDragging;
    private Point _lastPointerPosition;

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerPressed(e);

        if (Source is null || Source.PointCount == 0)
        {
            return;
        }

        var point = e.GetCurrentPoint(this);
        if (point.Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            _lastPointerPosition = e.GetPosition(this);
            SetInteracting(true);
            e.Pointer.Capture(this);
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerMoved(e);

        if (!_isDragging || Source is null || Source.PointCount == 0)
        {
            return;
        }

        var nextPosition = e.GetPosition(this);
        var delta = nextPosition - _lastPointerPosition;
        _lastPointerPosition = nextPosition;
        Orbit(delta.X, delta.Y);
        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerReleased(e);

        if (_isDragging && e.InitialPressMouseButton == MouseButton.Left)
        {
            _isDragging = false;
            SetInteracting(false);
            if (ReferenceEquals(e.Pointer.Captured, this))
            {
                e.Pointer.Capture(null);
            }
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        _isDragging = false;
        SetInteracting(false);
        base.OnPointerCaptureLost(e);
    }

    /// <inheritdoc />
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerWheelChanged(e);

        if (Source is null || Source.PointCount == 0)
        {
            return;
        }

        Zoom(e.Delta.Y);
        e.Handled = true;
    }

    private void Orbit(double deltaX, double deltaY)
    {
        _camera = new SurfaceCameraPose(
            _camera.Target,
            NormalizeDegrees(_camera.YawDegrees - (deltaX * OrbitDegreesPerPixel)),
            Math.Clamp(_camera.PitchDegrees + (deltaY * OrbitDegreesPerPixel), MinimumPitchDegrees, MaximumPitchDegrees),
            _camera.Distance,
            _camera.FieldOfViewDegrees);
        UpdateRenderingStatus();
        InvalidateVisual();
    }

    private void Zoom(double wheelDeltaY)
    {
        var zoomFactor = Math.Exp(-wheelDeltaY * ZoomExponentPerWheelDelta);
        _camera = new SurfaceCameraPose(
            _camera.Target,
            _camera.YawDegrees,
            _camera.PitchDegrees,
            Math.Max(_camera.Distance * zoomFactor, 0.1d),
            _camera.FieldOfViewDegrees);
        UpdateRenderingStatus();
        InvalidateVisual();
    }

    private static double NormalizeDegrees(double degrees)
    {
        var normalized = degrees % 360d;
        if (normalized > 180d)
        {
            return normalized - 360d;
        }

        if (normalized < -180d)
        {
            return normalized + 360d;
        }

        return normalized;
    }
}
