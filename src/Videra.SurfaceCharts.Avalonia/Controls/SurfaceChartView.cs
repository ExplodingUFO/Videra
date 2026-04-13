using Avalonia;
using Avalonia.Controls;
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

    /// <summary>
    /// Occurs when a tile request fails.
    /// </summary>
    public event EventHandler<SurfaceChartTileRequestFailedEventArgs>? TileRequestFailed;

    /// <summary>
    /// Gets the last tile request failure, if any.
    /// </summary>
    public SurfaceChartTileRequestFailedEventArgs? LastTileFailure { get; private set; }

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
        LastTileFailure = args;
        TileRequestFailed?.Invoke(this, args);
    }

    internal void ClearLastTileFailure()
    {
        LastTileFailure = null;
    }
}
