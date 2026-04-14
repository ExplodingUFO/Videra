using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Avalonia control shell for host-driven surface-chart tile scheduling and render-scene composition.
/// </summary>
/// <remarks>
/// The current alpha surface expects hosts to provide the tile source, viewport updates, and any
/// higher-level zoom/pan/orbit UI. Full built-in chart interaction and axis presentation are not
/// complete yet.
/// </remarks>
public partial class SurfaceChartView : Decorator
{
    private readonly SurfaceTileCache _tileCache;
    private readonly SurfaceCameraController _cameraController;
    private readonly SurfaceChartController _controller;
    private readonly SurfaceChartRenderHost _renderHost;
    private readonly ISurfaceChartNativeHostFactory _nativeHostFactory;
    private readonly Grid _hostContainer;
    private readonly SurfaceChartOverlayLayer _overlayLayer;
    private ISurfaceChartNativeHost? _nativeHost;

    internal event EventHandler<SurfaceChartTileRequestFailedEventArgs>? TileRequestFailed;

    /// <summary>
    /// Raised when the chart-local rendering backend, fallback state, or native-surface usage changes.
    /// </summary>
    public event EventHandler? RenderStatusChanged;

    internal SurfaceChartTileRequestFailedEventArgs? LastTileFailure { get; private set; }

    internal SurfaceChartRenderSnapshot RenderSnapshot => _renderHost.Snapshot;

    /// <summary>
    /// Gets the latest chart-local rendering status projected from the render host.
    /// </summary>
    public SurfaceChartRenderingStatus RenderingStatus { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceChartView"/> class.
    /// </summary>
    public SurfaceChartView()
        : this(renderHost: null, nativeHostFactory: null)
    {
    }

    internal SurfaceChartView(
        SurfaceChartRenderHost? renderHost,
        ISurfaceChartNativeHostFactory? nativeHostFactory)
    {
        _renderHost = renderHost ?? new SurfaceChartRenderHost();
        _nativeHostFactory = nativeHostFactory ?? new DefaultSurfaceChartNativeHostFactory();
        RenderingStatus = _renderHost.RenderingStatus;
        _tileCache = new SurfaceTileCache();
        _cameraController = new SurfaceCameraController(Viewport);
        _controller = new SurfaceChartController(
            _cameraController,
            _tileCache,
            new SurfaceTileScheduler(_tileCache, NotifyTilesChanged, OnTileRequestFailed),
            ClearLastTileFailure,
            InvalidateRenderScene);

        _overlayLayer = new SurfaceChartOverlayLayer(this)
        {
            IsHitTestVisible = true,
        };
        _hostContainer = new Grid();
        _hostContainer.Children.Add(_overlayLayer);
        Child = _hostContainer;

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
