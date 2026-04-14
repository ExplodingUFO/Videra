using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Avalonia control shell for surface-chart tile scheduling and render-scene composition.
/// </summary>
public partial class SurfaceChartView : Control
{
    private readonly SurfaceTileCache _tileCache;
    private readonly SurfaceCameraController _cameraController;
    private readonly SurfaceChartController _controller;

    internal event EventHandler<SurfaceChartTileRequestFailedEventArgs>? TileRequestFailed;

    internal SurfaceChartTileRequestFailedEventArgs? LastTileFailure { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceChartView"/> class.
    /// </summary>
    public SurfaceChartView()
    {
        _tileCache = new SurfaceTileCache();
        _cameraController = new SurfaceCameraController(Viewport);
        _controller = new SurfaceChartController(
            _cameraController,
            _tileCache,
            new SurfaceTileScheduler(_tileCache, NotifyTilesChanged, OnTileRequestFailed),
            ClearLastTileFailure,
            InvalidateRenderScene);

        ClipToBounds = true;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        _controller.UpdateViewSize(finalSize);
        UpdateOverlayViewSize(finalSize);
        return base.ArrangeOverride(finalSize);
    }

    private void OnTileRequestFailed(SurfaceTileKey tileKey, Exception exception)
    {
        var args = new SurfaceChartTileRequestFailedEventArgs(tileKey, exception);
        if (Dispatcher.UIThread.CheckAccess())
        {
            PublishTileRequestFailure(args);
            return;
        }

        Dispatcher.UIThread.Post(() => PublishTileRequestFailure(args));
    }

    internal void ClearLastTileFailure()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(ClearLastTileFailure);
            return;
        }

        LastTileFailure = null;
    }

    private void PublishTileRequestFailure(SurfaceChartTileRequestFailedEventArgs args)
    {
        LastTileFailure = args;
        TileRequestFailed?.Invoke(this, args);
    }
}
