using Avalonia;
using Avalonia.Input;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceChartInteractionController
{
    private const double PinToggleTravelThreshold = 4d;
    private Point? _leftPressPosition;
    private bool _pinGestureActive;

    public static void HandlePointerMoved(Point position, Action<Point> updateProbeScreenPosition)
    {
        ArgumentNullException.ThrowIfNull(updateProbeScreenPosition);
        updateProbeScreenPosition(position);
    }

    public bool HandlePointerPressed(PointerUpdateKind updateKind, Point position, KeyModifiers keyModifiers)
    {
        if (updateKind != PointerUpdateKind.LeftButtonPressed)
        {
            return false;
        }

        _leftPressPosition = position;
        _pinGestureActive = keyModifiers.HasFlag(KeyModifiers.Shift);
        return _pinGestureActive;
    }

    public bool HandlePointerReleased(MouseButton mouseButton, Point position, KeyModifiers keyModifiers)
    {
        var shouldTogglePin = mouseButton == MouseButton.Left &&
                              _pinGestureActive &&
                              keyModifiers.HasFlag(KeyModifiers.Shift) &&
                              _leftPressPosition is Point pressPosition &&
                              GetTravelDistance(pressPosition, position) < PinToggleTravelThreshold;

        Reset();
        return shouldTogglePin;
    }

    public void Reset()
    {
        _leftPressPosition = null;
        _pinGestureActive = false;
    }

    private static double GetTravelDistance(Point start, Point end)
    {
        var deltaX = end.X - start.X;
        var deltaY = end.Y - start.Y;
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
    }
}
