using Avalonia.Media;
using Avalonia.Threading;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

public partial class SurfaceChartView
{
    private readonly SurfaceChartRenderHost _renderHost = new();
    private SurfaceRenderScene? _renderScene;

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        base.Render(context);
        var projection = _chartProjection ?? CreateChartProjection();
        if (RenderSnapshot.ActiveBackend == SurfaceChartRenderBackendKind.Software)
        {
            SurfaceScenePainter.DrawScene(context, _renderHost.SoftwareScene, projection);
        }

        RenderOverlay(context);
    }

    private void NotifyTilesChanged()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            InvalidateRenderScene();
            return;
        }

        Dispatcher.UIThread.Post(InvalidateRenderScene);
    }

    private void InvalidateRenderScene()
    {
        SyncRenderHost();
        InvalidateVisual();
        InvalidateOverlay();
    }

    private void SyncRenderHost()
    {
        var source = Source;
        var tiles = _tileCache.GetLoadedTiles();
        var renderSize = _overlayViewSize.Width > 0d && _overlayViewSize.Height > 0d
            ? _overlayViewSize
            : Bounds.Size;
        var colorMap = source is null ? null : ColorMap ?? CreateFallbackColorMap(source.Metadata.ValueRange);

        _renderHost.UpdateInputs(
            new SurfaceChartRenderInputs
            {
                Metadata = source?.Metadata,
                LoadedTiles = tiles,
                ColorMap = colorMap,
                Viewport = Viewport,
                ProjectionSettings = _cameraController.ProjectionSettings,
                ViewWidth = renderSize.Width,
                ViewHeight = renderSize.Height,
                NativeHandle = IntPtr.Zero,
                HandleBound = false,
                RenderScale = (float)(VisualRoot?.RenderScaling ?? 1.0),
            });

        _renderScene = _renderHost.SoftwareScene;
    }

    private static SurfaceColorMap CreateFallbackColorMap(SurfaceValueRange range)
    {
        return new SurfaceColorMap(range, new SurfaceColorMapPalette(0xFF102030u, 0xFFE6EEF5u));
    }

    private SurfaceChartProjection? CreateChartProjection()
    {
        var source = Source;
        if (source is null || _overlayViewSize.Width <= 0d || _overlayViewSize.Height <= 0d)
        {
            return null;
        }

        var loadedTiles = _tileCache.GetLoadedTiles();
        if (loadedTiles.Count == 0)
        {
            return null;
        }

        var projection = SurfaceChartProjection.Create(
            _renderHost.SoftwareScene,
            _overlayViewSize,
            SurfaceChartProjection.CreateChartBoundsPoints(source.Metadata, source.Metadata.ValueRange),
            _cameraController.ProjectionSettings);
        _chartProjection = projection;
        return projection;
    }
}
