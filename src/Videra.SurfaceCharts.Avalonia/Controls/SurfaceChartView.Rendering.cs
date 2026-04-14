using Avalonia.Media;
using Avalonia.Threading;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

public partial class SurfaceChartView
{
    private readonly SurfaceRenderer _renderer = new();
    private SurfaceRenderScene? _renderScene;

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        base.Render(context);
        var projection = _chartProjection ?? CreateChartProjection();
        SurfaceScenePainter.DrawScene(context, _renderScene, projection);
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
        RebuildRenderSceneIfPossible();
        InvalidateVisual();
        InvalidateOverlay();
    }

    private void RebuildRenderSceneIfPossible()
    {
        var source = Source;
        if (source is null)
        {
            _renderScene = null;
            return;
        }

        var tiles = _tileCache.GetLoadedTiles();
        if (tiles.Count == 0)
        {
            _renderScene = null;
            return;
        }

        var colorMap = ColorMap ?? CreateFallbackColorMap(source.Metadata.ValueRange);
        _renderScene = _renderer.BuildScene(source.Metadata, tiles, colorMap);
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
            _renderScene,
            _overlayViewSize,
            SurfaceChartProjection.CreateChartBoundsPoints(source.Metadata, source.Metadata.ValueRange),
            _cameraController.ProjectionSettings);
        _chartProjection = projection;
        return projection;
    }
}
