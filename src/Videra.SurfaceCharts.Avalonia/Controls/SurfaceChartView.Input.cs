using Avalonia.Input;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;

namespace Videra.SurfaceCharts.Avalonia.Controls;

public partial class SurfaceChartView
{
    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerPressed(e);
        UpdateProbeScreenPosition(e.GetPosition(this));

        if (_interactionController.HandlePointerPressed(e))
        {
            e.Pointer.Capture(this);
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerReleased(e);
        UpdateProbeScreenPosition(e.GetPosition(this));

        var releaseResult = _interactionController.HandlePointerReleased(e, ViewState, Bounds.Size);
        if (releaseResult.ViewState is not null)
        {
            ApplyViewState(releaseResult.ViewState);
        }

        if (releaseResult.Handled)
        {
            if (ReferenceEquals(e.Pointer.Captured, this))
            {
                e.Pointer.Capture(null);
            }

            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerMoved(e);
        UpdateProbeScreenPosition(e.GetPosition(this));

        var updatedViewState = _interactionController.HandlePointerMoved(e, ViewState, Bounds.Size);
        if (updatedViewState is not null)
        {
            ApplyViewState(updatedViewState);
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerWheelChanged(e);
        UpdateProbeScreenPosition(e.GetPosition(this));

        var updatedViewState = SurfaceChartInteractionController.HandlePointerWheel(e, ViewState);
        if (updatedViewState is not null)
        {
            ApplyViewState(updatedViewState);
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        base.OnPointerCaptureLost(e);
        _interactionController.Reset();
    }
}
