using Avalonia;
using Avalonia.Media;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

public partial class VideraChartView
{
    private static readonly IBrush SelectionFillBrush = new SolidColorBrush(Color.FromArgb(40, 0x4D, 0xA3, 0xFF));
    private static readonly Pen SelectionBorderPen = new(new SolidColorBrush(Color.FromArgb(220, 0x4D, 0xA3, 0xFF)), thickness: 1.5d);
    private readonly SurfaceChartOverlayCoordinator _overlayCoordinator = new();
    private SurfaceChartProjection? _chartProjection;
    private Size _overlayViewSize;

    // Cached projection inputs to avoid rebuilding projection on every probe move.
    private SurfaceRenderScene? _cachedProjectionScene;
    private Size _cachedProjectionViewSize;
    private SurfaceCameraFrame? _cachedProjectionCameraFrame;
    private ISurfaceTileSource? _cachedProjectionSource;

    internal bool UpdateProbeScreenPosition(Point probeScreenPosition)
    {
        if (!_overlayCoordinator.UpdateProbeScreenPosition(probeScreenPosition))
        {
            return false;
        }

        // Lightweight crosshair update — bypasses full overlay coordinator rebuild.
        // The crosshair state is updated directly without rebuilding axis/legend/probe state.
        _overlayCoordinator.UpdateCrosshairPosition(
            probeScreenPosition,
            _chartProjection,
            Plot.OverlayOptions,
            _runtime.Source?.Metadata);

        // Full rebuild for probe resolution (axis/legend/probe state).
        // The crosshair state is already updated above and will be rendered correctly.
        InvalidateOverlay();
        return true;
    }

    internal SurfaceProbeInfo? GetHoveredProbe()
    {
        return _overlayCoordinator.GetHoveredProbe();
    }

    /// <summary>
    /// Resolves a chart-local probe at the supplied screen position without changing hover or pinned probe state.
    /// </summary>
    /// <param name="screenPosition">The pointer position in chart-view screen coordinates.</param>
    /// <param name="probe">The resolved probe when available.</param>
    /// <returns><c>true</c> when a probe was resolved; otherwise, <c>false</c>.</returns>
    public bool TryResolveProbe(Point screenPosition, out SurfaceProbeInfo probe)
    {
        probe = default;

        var source = _runtime.Source;
        if (source is null || _overlayViewSize.Width <= 0d || _overlayViewSize.Height <= 0d)
        {
            return false;
        }

        var cameraFrame = _runtime.CreateCameraFrame(_overlayViewSize, 1f);
        if (cameraFrame is null)
        {
            return false;
        }

        var resolvedProbe = SurfaceProbeService.ResolveFromScreenPosition(
            source.Metadata,
            cameraFrame.Value,
            _runtime.GetLoadedTiles(),
            screenPosition);
        if (resolvedProbe is not SurfaceProbeInfo resolved)
        {
            return false;
        }

        probe = resolved;
        return true;
    }

    /// <summary>
    /// Creates a host-owned click selection report at the supplied screen position.
    /// </summary>
    public bool TryCreateSelectionReport(Point screenPosition, out SurfaceChartSelectionReport selection)
    {
        return TryCreateSelectionReport(screenPosition, screenPosition, out selection);
    }

    /// <summary>
    /// Creates a host-owned rectangle selection report between two screen positions.
    /// </summary>
    public bool TryCreateSelectionReport(
        Point screenStart,
        Point screenEnd,
        out SurfaceChartSelectionReport selection)
    {
        selection = null!;

        if (!TryMapScreenPointToSample(screenStart, out var start) ||
            !TryMapScreenPointToSample(screenEnd, out var end) ||
            _runtime.Metadata is not SurfaceMetadata metadata)
        {
            return false;
        }

        var isRectangle = Math.Abs(screenEnd.X - screenStart.X) > double.Epsilon &&
            Math.Abs(screenEnd.Y - screenStart.Y) > double.Epsilon;
        SurfaceDataWindow? dataWindow = null;
        if (isRectangle)
        {
            dataWindow = new SurfaceDataWindow(
                Math.Min(start.SampleX, end.SampleX),
                Math.Min(start.SampleY, end.SampleY),
                Math.Abs(end.SampleX - start.SampleX),
                Math.Abs(end.SampleY - start.SampleY)).ClampTo(metadata);
        }

        selection = new SurfaceChartSelectionReport(
            isRectangle ? SurfaceChartSelectionKind.Rectangle : SurfaceChartSelectionKind.Click,
            screenStart,
            screenEnd,
            start.SampleX,
            start.SampleY,
            end.SampleX,
            end.SampleY,
            metadata.MapHorizontalCoordinate(start.SampleX),
            metadata.MapVerticalCoordinate(start.SampleY),
            metadata.MapHorizontalCoordinate(end.SampleX),
            metadata.MapVerticalCoordinate(end.SampleY),
            dataWindow);
        return true;
    }

    /// <summary>
    /// Creates a host-owned annotation anchor from a resolved probe at the supplied screen position.
    /// </summary>
    public bool TryCreateProbeAnnotationAnchor(
        Point screenPosition,
        out SurfaceChartAnnotationAnchor anchor,
        string? label = null)
    {
        anchor = default;

        if (!TryResolveProbe(screenPosition, out var probe))
        {
            return false;
        }

        anchor = SurfaceChartAnnotationAnchor.FromProbe(probe, label);
        return true;
    }

    /// <summary>
    /// Creates host-owned annotation anchors from a selection report between two screen positions.
    /// </summary>
    public bool TryCreateSelectionAnnotationAnchors(
        Point screenStart,
        Point screenEnd,
        out SurfaceChartAnnotationAnchor startAnchor,
        out SurfaceChartAnnotationAnchor endAnchor)
    {
        startAnchor = default;
        endAnchor = default;

        if (!TryCreateSelectionReport(screenStart, screenEnd, out var selection))
        {
            return false;
        }

        startAnchor = SurfaceChartAnnotationAnchor.FromSelectionStart(selection);
        endAnchor = SurfaceChartAnnotationAnchor.FromSelectionEnd(selection);
        return true;
    }

    /// <summary>
    /// Creates a host-owned rectangle measurement report between two screen positions.
    /// </summary>
    public bool TryCreateSelectionMeasurementReport(
        Point screenStart,
        Point screenEnd,
        out SurfaceChartMeasurementReport measurement)
    {
        measurement = null!;

        if (!TryCreateSelectionReport(screenStart, screenEnd, out var selection))
        {
            return false;
        }

        measurement = SurfaceChartMeasurementReport.FromSelection(selection);
        return true;
    }

    /// <summary>
    /// Creates a bounded marker overlay recipe at the supplied screen position.
    /// </summary>
    public bool TryCreateDraggableMarkerOverlay(
        Point screenPosition,
        out SurfaceChartDraggableMarkerOverlay marker)
    {
        marker = default;

        if (!TryMapScreenPointToSample(screenPosition, out var sample) ||
            _runtime.Metadata is not SurfaceMetadata metadata)
        {
            return false;
        }

        marker = SurfaceChartDraggableOverlayRecipes.CreateMarker(metadata, sample.SampleX, sample.SampleY);
        return true;
    }

    /// <summary>
    /// Creates a bounded range overlay recipe between two screen positions.
    /// </summary>
    public bool TryCreateDraggableRangeOverlay(
        Point screenStart,
        Point screenEnd,
        out SurfaceChartDraggableRangeOverlay range)
    {
        range = default;

        if (!TryCreateSelectionReport(screenStart, screenEnd, out var selection) ||
            selection.DataWindow is not SurfaceDataWindow dataWindow ||
            _runtime.Metadata is not SurfaceMetadata metadata)
        {
            return false;
        }

        range = SurfaceChartDraggableOverlayRecipes.CreateRange(metadata, dataWindow);
        return true;
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
        var activeColorMap = source is null ? null : Plot.ColorMap ?? CreateDefaultColorMap(source.Metadata.ValueRange);
        var cameraFrame = source is not null
            ? _runtime.CreateCameraFrame(_overlayViewSize, 1f)
            : null;

        // Rebuild projection only when its dependencies change; probe moves alone skip this work.
        var projectionChanged = _chartProjection is null
            || _cachedProjectionScene != _renderScene
            || _cachedProjectionViewSize != _overlayViewSize
            || _cachedProjectionCameraFrame != cameraFrame
            || _cachedProjectionSource != source;

        if (projectionChanged)
        {
            _chartProjection = source is not null && loadedTiles.Count > 0 && cameraFrame is not null
                ? SurfaceChartProjection.Create(
                    _renderScene,
                    cameraFrame.Value,
                    SurfaceChartProjection.CreateChartBoundsPoints(source.Metadata, source.Metadata.ValueRange))
                : null;
            _cachedProjectionScene = _renderScene;
            _cachedProjectionViewSize = _overlayViewSize;
            _cachedProjectionCameraFrame = cameraFrame;
            _cachedProjectionSource = source;
        }

        _overlayCoordinator.Refresh(
            source,
            loadedTiles,
            activeColorMap,
            Plot.ColorMap is not null,
            cameraFrame,
            _chartProjection,
            Plot.OverlayOptions,
            Plot.Series,
            _runtime.CanInteract,
            Plot.TextAnnotations,
            Plot.ArrowAnnotations);
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

    /// <summary>
    /// Sets snapshot mode on the overlay coordinator to suppress interaction chrome during snapshot capture.
    /// </summary>
    internal void SetSnapshotMode(bool isSnapshotMode)
    {
        _overlayCoordinator.IsSnapshotMode = isSnapshotMode;
    }

    private bool TryMapScreenPointToSample(Point screenPosition, out SurfaceChartScreenSample sample)
    {
        sample = default;

        if (_runtime.Metadata is not SurfaceMetadata metadata ||
            _overlayViewSize.Width <= 0d ||
            _overlayViewSize.Height <= 0d)
        {
            return false;
        }

        var currentWindow = _runtime.ViewState.DataWindow.ClampTo(metadata);
        var normalizedX = Math.Clamp(screenPosition.X / _overlayViewSize.Width, 0d, 1d);
        var normalizedY = Math.Clamp(screenPosition.Y / _overlayViewSize.Height, 0d, 1d);
        sample = new SurfaceChartScreenSample(
            Math.Clamp(currentWindow.StartX + (normalizedX * currentWindow.Width), 0d, metadata.Width - 1d),
            Math.Clamp(currentWindow.StartY + (normalizedY * currentWindow.Height), 0d, metadata.Height - 1d));
        return true;
    }

    private readonly record struct SurfaceChartScreenSample(double SampleX, double SampleY);
}
