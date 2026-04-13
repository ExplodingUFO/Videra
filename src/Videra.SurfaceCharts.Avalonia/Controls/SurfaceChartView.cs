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
    /// Initializes a new instance of the <see cref="SurfaceChartView"/> class.
    /// </summary>
    public SurfaceChartView()
    {
        _tileCache = new SurfaceTileCache();
        _cameraController = new SurfaceCameraController(Viewport);
        _controller = new SurfaceChartController(
            _cameraController,
            _tileCache,
            new SurfaceTileScheduler(_tileCache, NotifyTilesChanged),
            InvalidateRenderScene);

        ClipToBounds = true;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        _controller.UpdateViewSize(finalSize);
        return base.ArrangeOverride(finalSize);
    }
}
