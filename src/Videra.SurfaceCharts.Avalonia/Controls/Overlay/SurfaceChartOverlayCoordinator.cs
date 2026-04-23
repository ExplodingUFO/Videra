using Avalonia;
using Avalonia.Media;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal sealed class SurfaceChartOverlayCoordinator
{
    private readonly List<SurfaceProbeRequest> _pinnedProbeRequests = [];
    private Point? _probeScreenPosition;
    private Size _viewSize;

    public SurfaceProbeOverlayState ProbeState { get; private set; } = SurfaceProbeOverlayState.Empty;

    public SurfaceAxisOverlayState AxisState { get; private set; } = SurfaceAxisOverlayState.Empty;

    public SurfaceLegendOverlayState LegendState { get; private set; } = SurfaceLegendOverlayState.Empty;

    public void ResetForSourceChange()
    {
        _pinnedProbeRequests.Clear();
        _probeScreenPosition = null;
        ProbeState = SurfaceProbeOverlayState.Empty;
        AxisState = SurfaceAxisOverlayState.Empty;
        LegendState = SurfaceLegendOverlayState.Empty;
    }

    public void UpdateViewSize(Size viewSize)
    {
        _viewSize = viewSize;
    }

    public bool UpdateProbeScreenPosition(Point probeScreenPosition)
    {
        if (_probeScreenPosition.HasValue && _probeScreenPosition.Value == probeScreenPosition)
        {
            return false;
        }

        _probeScreenPosition = probeScreenPosition;
        return true;
    }

    public SurfaceProbeInfo? GetHoveredProbe() => ProbeState.HoveredProbe;

    public void TogglePinnedProbe(SurfaceProbeInfo probe)
    {
        var existingIndex = _pinnedProbeRequests.FindIndex(
            request => MatchesPinnedProbe(request, probe));

        if (existingIndex >= 0)
        {
            _pinnedProbeRequests.RemoveAt(existingIndex);
            return;
        }

        _pinnedProbeRequests.Add(new SurfaceProbeRequest(probe.SampleX, probe.SampleY));
    }

    public void Refresh(
        ISurfaceTileSource? source,
        IReadOnlyList<SurfaceTile> loadedTiles,
        SurfaceColorMap? colorMap,
        bool hasExplicitColorMap,
        SurfaceCameraFrame? cameraFrame,
        SurfaceChartProjection? chartProjection,
        SurfaceChartOverlayOptions overlayOptions)
    {
        ArgumentNullException.ThrowIfNull(loadedTiles);
        ArgumentNullException.ThrowIfNull(overlayOptions);

        AxisState = source is not null && loadedTiles.Count > 0
            ? SurfaceAxisOverlayPresenter.CreateState(source.Metadata, chartProjection, overlayOptions)
            : SurfaceAxisOverlayState.Empty;
        LegendState = loadedTiles.Count > 0
            ? SurfaceLegendOverlayPresenter.CreateState(source?.Metadata, colorMap, hasExplicitColorMap, chartProjection, overlayOptions)
            : SurfaceLegendOverlayState.Empty;
        ProbeState = SurfaceProbeOverlayPresenter.CreateState(
            source,
            cameraFrame,
            loadedTiles,
            _probeScreenPosition,
            _pinnedProbeRequests);
    }

    public void Render(DrawingContext context, SurfaceChartProjection? chartProjection)
    {
        ArgumentNullException.ThrowIfNull(context);

        SurfaceAxisOverlayPresenter.Render(context, AxisState);
        SurfaceLegendOverlayPresenter.Render(context, LegendState);
        SurfaceProbeOverlayPresenter.Render(context, ProbeState, _viewSize, chartProjection);
    }

    private static bool MatchesPinnedProbe(SurfaceProbeRequest request, SurfaceProbeInfo probe)
    {
        return Math.Abs(request.SampleX - probe.SampleX) <= double.Epsilon &&
               Math.Abs(request.SampleY - probe.SampleY) <= double.Epsilon;
    }
}
