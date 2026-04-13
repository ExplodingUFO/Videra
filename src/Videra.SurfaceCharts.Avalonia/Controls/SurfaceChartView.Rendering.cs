using Avalonia.Media;
using Avalonia.Threading;
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
        SurfaceScenePainter.DrawScene(context, _renderScene, Bounds.Size);
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
}
