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
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerMoved(e);
        SurfaceChartInteractionController.HandlePointerMoved(e.GetPosition(this), UpdateProbeScreenPosition);
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        base.OnPointerReleased(e);

        var pointerPosition = e.GetPosition(this);
        UpdateProbeScreenPosition(pointerPosition);

        if (_interactionController.HandlePointerReleased(e.InitialPressMouseButton, pointerPosition, e.KeyModifiers) &&
            GetHoveredProbe() is SurfaceProbeInfo hoveredProbe)
        {
            TogglePinnedProbe(hoveredProbe);
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        _interactionController.Reset();
        base.OnPointerCaptureLost(e);
    }
}
