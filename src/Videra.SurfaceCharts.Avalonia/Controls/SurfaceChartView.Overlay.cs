using Avalonia;
using Avalonia.Media;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

public partial class SurfaceChartView
{
    private SurfaceProbeOverlayState _overlayState = SurfaceProbeOverlayState.Empty;
    private SurfaceAxisOverlayState _axisOverlayState = SurfaceAxisOverlayState.Empty;
    private SurfaceLegendOverlayState _legendOverlayState = SurfaceLegendOverlayState.Empty;
    private SurfaceChartProjection? _chartProjection;
    private readonly SurfaceProbeService _probeService = new();
    private readonly List<SurfaceProbeRequest> _pinnedProbeRequests = [];
    private Size _overlayViewSize;
    private Point? _probeScreenPosition;

    internal void UpdateProbeScreenPosition(Point probeScreenPosition)
    {
        _probeScreenPosition = probeScreenPosition;
        InvalidateOverlay();
    }

    internal SurfaceProbeInfo? GetHoveredProbe()
    {
        return _overlayState.HoveredProbe;
    }

    internal void TogglePinnedProbe(SurfaceProbeInfo probe)
    {
        var existingIndex = _pinnedProbeRequests.FindIndex(
            request => MatchesPinnedProbe(request, probe));

        if (existingIndex >= 0)
        {
            _pinnedProbeRequests.RemoveAt(existingIndex);
        }
        else
        {
            _pinnedProbeRequests.Add(new SurfaceProbeRequest(probe.SampleX, probe.SampleY));
        }

        InvalidateOverlay();
    }

    internal void UpdateProjectionSettings(SurfaceChartProjectionSettings projectionSettings)
    {
        _cameraController.UpdateProjectionSettings(projectionSettings);
        InvalidateRenderScene();
    }

    private void UpdateOverlayViewSize(Size viewSize)
    {
        _overlayViewSize = viewSize;
        InvalidateOverlay();
    }

    private void InvalidateOverlay()
    {
        var loadedTiles = _tileCache.GetLoadedTiles();
        var source = Source;
        var activeColorMap = source is null ? null : ColorMap ?? CreateFallbackColorMap(source.Metadata.ValueRange);

        _chartProjection = source is not null && loadedTiles.Count > 0
            ? SurfaceChartProjection.Create(
                _renderScene,
                _overlayViewSize,
                SurfaceChartProjection.CreateChartBoundsPoints(source.Metadata, source.Metadata.ValueRange),
                _cameraController.ProjectionSettings)
            : null;
        _axisOverlayState = source is not null && loadedTiles.Count > 0
            ? SurfaceAxisOverlayPresenter.CreateState(source.Metadata, _chartProjection)
            : SurfaceAxisOverlayState.Empty;
        _legendOverlayState = loadedTiles.Count > 0
            ? SurfaceLegendOverlayPresenter.CreateState(source?.Metadata, activeColorMap, ColorMap is not null, _chartProjection)
            : SurfaceLegendOverlayState.Empty;
        _overlayState = SurfaceProbeOverlayPresenter.CreateState(
            source,
            Viewport,
            _overlayViewSize,
            loadedTiles,
            _probeScreenPosition,
            _pinnedProbeRequests,
            _probeService);
        InvalidateVisual();
    }

    private void RenderOverlay(DrawingContext context)
    {
        SurfaceAxisOverlayPresenter.Render(context, _axisOverlayState);
        SurfaceLegendOverlayPresenter.Render(context, _legendOverlayState);
        SurfaceProbeOverlayPresenter.Render(context, _overlayState, _overlayViewSize, _chartProjection);
    }

    private static bool MatchesPinnedProbe(SurfaceProbeRequest request, SurfaceProbeInfo probe)
    {
        return Math.Abs(request.SampleX - probe.SampleX) <= double.Epsilon &&
               Math.Abs(request.SampleY - probe.SampleY) <= double.Epsilon;
    }
}
