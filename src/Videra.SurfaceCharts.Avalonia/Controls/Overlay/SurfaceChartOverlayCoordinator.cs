using Avalonia;
using Avalonia.Media;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal sealed class SurfaceChartOverlayCoordinator
{
    private readonly List<SurfaceProbeRequest> _pinnedProbeRequests = [];
    private Point? _probeScreenPosition;
    private Point? _pointerScreenPosition;
    private Size _viewSize;

    public SurfaceProbeOverlayState ProbeState { get; private set; } = SurfaceProbeOverlayState.Empty;

    public SurfaceAxisOverlayState AxisState { get; private set; } = SurfaceAxisOverlayState.Empty;

    public SurfaceLegendOverlayState LegendState { get; private set; } = SurfaceLegendOverlayState.Empty;

    public SurfaceCrosshairOverlayState CrosshairState { get; private set; } = SurfaceCrosshairOverlayState.Empty;

    public SurfaceChartToolbarOverlayState ToolbarState { get; private set; } = SurfaceChartToolbarOverlayState.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the coordinator is in snapshot mode.
    /// When true, interaction chrome (crosshair, hovered probe, toolbar) is suppressed during rendering.
    /// </summary>
    public bool IsSnapshotMode { get; set; }

    public void ResetForSourceChange()
    {
        _pinnedProbeRequests.Clear();
        _probeScreenPosition = null;
        _pointerScreenPosition = null;
        ProbeState = SurfaceProbeOverlayState.Empty;
        AxisState = SurfaceAxisOverlayState.Empty;
        LegendState = SurfaceLegendOverlayState.Empty;
        CrosshairState = SurfaceCrosshairOverlayState.Empty;
        ToolbarState = SurfaceChartToolbarOverlayState.Empty;
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
        _pointerScreenPosition = probeScreenPosition;
        return true;
    }

    /// <summary>
    /// Updates the pointer screen position for toolbar hover highlighting.
    /// </summary>
    public void UpdatePointerPosition(Point pointerScreenPosition)
    {
        _pointerScreenPosition = pointerScreenPosition;
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

    /// <summary>
    /// Lightweight crosshair position update — bypasses full overlay coordinator rebuild.
    /// Only updates crosshair state without rebuilding axis/legend/probe state.
    /// </summary>
    public void UpdateCrosshairPosition(
        Point probeScreenPosition,
        SurfaceChartProjection? projection,
        SurfaceChartOverlayOptions overlayOptions,
        SurfaceMetadata? metadata)
    {
        CrosshairState = SurfaceCrosshairOverlayPresenter.CreateState(
            probeScreenPosition,
            projection,
            overlayOptions,
            metadata);
    }

    public void Refresh(
        ISurfaceTileSource? source,
        IReadOnlyList<SurfaceTile> loadedTiles,
        SurfaceColorMap? colorMap,
        bool hasExplicitColorMap,
        SurfaceCameraFrame? cameraFrame,
        SurfaceChartProjection? chartProjection,
        SurfaceChartOverlayOptions overlayOptions,
        IReadOnlyList<Plot3DSeries>? series = null,
        bool canInteract = false)
    {
        ArgumentNullException.ThrowIfNull(loadedTiles);
        ArgumentNullException.ThrowIfNull(overlayOptions);

        AxisState = source is not null && loadedTiles.Count > 0
            ? SurfaceAxisOverlayPresenter.CreateState(source.Metadata, chartProjection, overlayOptions)
            : SurfaceAxisOverlayState.Empty;
        LegendState = series is not null && series.Count > 0
            ? SurfaceLegendOverlayPresenter.CreateState(series, chartProjection, overlayOptions)
            : SurfaceLegendOverlayState.Empty;
        ProbeState = SurfaceProbeOverlayPresenter.CreateState(
            source,
            cameraFrame,
            loadedTiles,
            _probeScreenPosition,
            _pinnedProbeRequests,
            overlayOptions,
            series);
        ToolbarState = SurfaceChartToolbarOverlayPresenter.CreateState(
            _viewSize,
            _pointerScreenPosition,
            canInteract);
    }

    public void Render(DrawingContext context, SurfaceChartProjection? chartProjection)
    {
        ArgumentNullException.ThrowIfNull(context);

        SurfaceAxisOverlayPresenter.Render(context, AxisState);
        SurfaceLegendOverlayPresenter.Render(context, LegendState);

        if (IsSnapshotMode)
        {
            // In snapshot mode, render only pinned probes (intentional data markers).
            // Skip hovered probe, crosshair, and toolbar (interaction chrome).
            SurfaceProbeOverlayPresenter.RenderPinnedOnly(context, ProbeState, _viewSize, chartProjection);
        }
        else
        {
            SurfaceProbeOverlayPresenter.Render(context, ProbeState, _viewSize, chartProjection);
            SurfaceCrosshairOverlayPresenter.Render(context, CrosshairState);
            SurfaceChartToolbarOverlayPresenter.Render(context, ToolbarState, _pointerScreenPosition);
        }
    }

    private static bool MatchesPinnedProbe(SurfaceProbeRequest request, SurfaceProbeInfo probe)
    {
        return Math.Abs(request.SampleX - probe.SampleX) <= double.Epsilon &&
               Math.Abs(request.SampleY - probe.SampleY) <= double.Epsilon;
    }
}
