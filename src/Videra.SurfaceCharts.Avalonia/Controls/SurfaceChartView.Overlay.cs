using Avalonia;
using Avalonia.Media;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;

namespace Videra.SurfaceCharts.Avalonia.Controls;

public partial class SurfaceChartView
{
    private SurfaceProbeOverlayState _overlayState = SurfaceProbeOverlayState.Empty;
    private Size _overlayViewSize;
    private Point? _probeScreenPosition;

    internal void UpdateProbeScreenPosition(Point probeScreenPosition)
    {
        _probeScreenPosition = probeScreenPosition;
        InvalidateOverlay();
    }

    private void UpdateOverlayViewSize(Size viewSize)
    {
        _overlayViewSize = viewSize;
        InvalidateOverlay();
    }

    private void InvalidateOverlay()
    {
        _overlayState = SurfaceProbeOverlayPresenter.CreateState(
            Source,
            Viewport,
            _overlayViewSize,
            _runtime.GetLoadedTiles(),
            _probeScreenPosition);
        InvalidateVisual();
    }

    private void RenderOverlay(DrawingContext context)
    {
        SurfaceProbeOverlayPresenter.Render(context, _overlayState, _overlayViewSize);
    }
}
