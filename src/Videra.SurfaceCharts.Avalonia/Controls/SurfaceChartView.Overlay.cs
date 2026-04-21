using Avalonia;
using Avalonia.Media;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

public partial class SurfaceChartView
{
    private static readonly IBrush SelectionFillBrush = new SolidColorBrush(Color.FromArgb(40, 0x4D, 0xA3, 0xFF));
    private static readonly Pen SelectionBorderPen = new(new SolidColorBrush(Color.FromArgb(220, 0x4D, 0xA3, 0xFF)), thickness: 1.5d);
    private readonly SurfaceChartOverlayCoordinator _overlayCoordinator = new();
    private SurfaceChartProjection? _chartProjection;
    private Size _overlayViewSize;

    internal void UpdateProbeScreenPosition(Point probeScreenPosition)
    {
        _overlayCoordinator.UpdateProbeScreenPosition(probeScreenPosition);
        InvalidateOverlay();
    }

    internal SurfaceProbeInfo? GetHoveredProbe()
    {
        return _overlayCoordinator.GetHoveredProbe();
    }

    internal void TogglePinnedProbe(SurfaceProbeInfo probe)
    {
        _overlayCoordinator.TogglePinnedProbe(probe);
        InvalidateOverlay();
    }

    internal void UpdateProjectionSettings(SurfaceChartProjectionSettings projectionSettings)
    {
        _runtime.UpdateProjectionSettings(projectionSettings);
    }

    private void UpdateOverlayViewSize(Size viewSize)
    {
        _overlayViewSize = viewSize;
        _overlayCoordinator.UpdateViewSize(viewSize);
        InvalidateOverlay();
    }

    private void InvalidateOverlay()
    {
        var loadedTiles = _runtime.GetLoadedTiles();
        var source = _runtime.Source;
        var activeColorMap = source is null ? null : ColorMap ?? CreateFallbackColorMap(source.Metadata.ValueRange);
        var cameraFrame = source is not null
            ? _runtime.CreateCameraFrame(_overlayViewSize, 1f)
            : null;

        _chartProjection = source is not null && loadedTiles.Count > 0 && cameraFrame is not null
            ? SurfaceChartProjection.Create(
                _renderScene,
                cameraFrame.Value,
                SurfaceChartProjection.CreateChartBoundsPoints(source.Metadata, source.Metadata.ValueRange))
            : null;
        _overlayCoordinator.Refresh(
            source,
            loadedTiles,
            activeColorMap,
            ColorMap is not null,
            cameraFrame,
            _chartProjection,
            OverlayOptions);
        _overlayLayer.InvalidateVisual();
    }

    private void RenderOverlay(DrawingContext context)
    {
        if (_interactionController.ActiveSelectionRect is Rect selectionRect &&
            selectionRect.Width > 0d &&
            selectionRect.Height > 0d)
        {
            context.DrawRectangle(SelectionFillBrush, SelectionBorderPen, selectionRect);
        }

        _overlayCoordinator.Render(context, _chartProjection);
    }
}
