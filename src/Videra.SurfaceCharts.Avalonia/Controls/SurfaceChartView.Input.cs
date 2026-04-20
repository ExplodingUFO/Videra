using Avalonia.Input;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

public partial class SurfaceChartView
{
    private readonly SurfaceChartInteractionController _interactionController = new();

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerPressed(e);

        var pointerPosition = e.GetPosition(this);
        UpdateProbeScreenPosition(pointerPosition);

        if (_interactionController.HandlePointerPressed(
                e.GetCurrentPoint(this).Properties.PointerUpdateKind,
                pointerPosition,
                e.KeyModifiers))
        {
            e.Pointer.Capture(this);
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerMoved(e);

        var pointerPosition = e.GetPosition(this);
        UpdateProbeScreenPosition(pointerPosition);

        if (_interactionController.HandlePointerMoved(pointerPosition, _runtime))
        {
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerReleased(e);

        var pointerPosition = e.GetPosition(this);
        UpdateProbeScreenPosition(pointerPosition);

        var releaseResult = _interactionController.HandlePointerReleased(
            e.InitialPressMouseButton,
            pointerPosition,
            e.KeyModifiers,
            _runtime);

        if (releaseResult.TogglePinnedProbe &&
            GetHoveredProbe() is SurfaceProbeInfo hoveredProbe)
        {
            TogglePinnedProbe(hoveredProbe);
        }

        if (releaseResult.Handled)
        {
            InvalidateOverlay();

            if (!_interactionController.HasActiveGesture &&
                ReferenceEquals(e.Pointer.Captured, this))
            {
                e.Pointer.Capture(null);
            }

            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerWheelChanged(e);

        var pointerPosition = e.GetPosition(this);
        UpdateProbeScreenPosition(pointerPosition);

        if (SurfaceChartInteractionController.HandlePointerWheelChanged(e.Delta, _runtime, GetHoveredProbe()))
        {
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        _interactionController.Reset();
        InvalidateOverlay();
        base.OnPointerCaptureLost(e);
    }
}
