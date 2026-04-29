using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Avalonia control shell for built-in surface-chart interaction, tile scheduling, and render-scene composition.
/// </summary>
/// <remarks>
/// Hosts still own the tile source and persisted <see cref="ViewState"/>, but the control now ships
/// built-in orbit, pan, dolly, and focus interaction on top of the chart-local runtime contract.
/// </remarks>
public partial class VideraChartView : Decorator
{
    private readonly SurfaceChartRuntime _runtime;
    private readonly SurfaceChartRenderHost _renderHost;
    private readonly ISurfaceChartNativeHostFactory _nativeHostFactory;
    private readonly Grid _hostContainer;
    private readonly SurfaceChartOverlayLayer _overlayLayer;
    private readonly ContourSceneCache _contourSceneCache = new();
    private ISurfaceChartNativeHost? _nativeHost;

    internal event EventHandler<SurfaceChartTileRequestFailedEventArgs>? TileRequestFailed;

    /// <summary>
    /// Raised when the chart-local rendering backend, fallback state, or native-surface usage changes.
    /// Diagnostic fields include <c>ActiveBackend</c>, <c>IsReady</c>, <c>IsFallback</c>,
    /// <c>FallbackReason</c>, <c>UsesNativeSurface</c>, <c>ResidentTileCount</c>,
    /// <c>VisibleTileCount</c>, and <c>ResidentTileBytes</c>.
    /// </summary>
    public event EventHandler? RenderStatusChanged;

    /// <summary>
    /// Raised when the current diagnostic interaction-quality mode changes between
    /// <c>Interactive</c> and <c>Refine</c>.
    /// </summary>
    public event EventHandler? InteractionQualityChanged;

    internal SurfaceChartTileRequestFailedEventArgs? LastTileFailure { get; private set; }

    internal SurfaceChartRenderSnapshot RenderSnapshot => _renderHost.Snapshot;

    /// <summary>
    /// Gets the latest chart-local rendering status projected from the render host.
    /// Hosts can inspect <c>ActiveBackend</c>, <c>IsReady</c>, <c>IsFallback</c>,
    /// <c>FallbackReason</c>, <c>UsesNativeSurface</c>, <c>ResidentTileCount</c>,
    /// <c>VisibleTileCount</c>, and <c>ResidentTileBytes</c>.
    /// </summary>
    public SurfaceChartRenderingStatus RenderingStatus { get; private set; }

    /// <summary>
    /// Gets the latest Plot-owned scatter rendering and streaming diagnostics.
    /// </summary>
    public ScatterChartRenderingStatus ScatterRenderingStatus { get; private set; }

    /// <summary>
    /// Gets the latest Plot-owned bar chart rendering diagnostics.
    /// </summary>
    public BarChartRenderingStatus BarRenderingStatus { get; private set; }

    /// <summary>
    /// Gets the latest Plot-owned contour rendering diagnostics.
    /// </summary>
    public ContourChartRenderingStatus ContourRenderingStatus { get; private set; }

    /// <summary>
    /// Gets the chart plot model used to add surface, waterfall, and scatter series.
    /// </summary>
    public Plot3D Plot { get; }

    /// <summary>
    /// Gets the latest plot revision rendered or requested through <see cref="Refresh"/>.
    /// </summary>
    public int LastRefreshRevision { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VideraChartView"/> class.
    /// </summary>
    public VideraChartView()
        : this(renderHost: null, nativeHostFactory: null)
    {
    }

    internal VideraChartView(
        SurfaceChartRenderHost? renderHost,
        ISurfaceChartNativeHostFactory? nativeHostFactory)
    {
        _renderHost = renderHost ?? new SurfaceChartRenderHost();
        _nativeHostFactory = nativeHostFactory ?? new DefaultSurfaceChartNativeHostFactory();
        Plot = new Plot3D(OnPlotChanged);
        Plot.SetRenderOffscreen(RenderOffscreenAsync);
        RenderingStatus = _renderHost.RenderingStatus;
        _runtime = new SurfaceChartRuntime(
            ViewState,
            NotifyTilesChanged,
            OnTileRequestFailed,
            ClearLastTileFailure,
            UpdateInteractionQuality,
            InvalidateRenderScene,
            InvalidateOverlay);
        ScatterRenderingStatus = CreateScatterRenderingStatus();
        ContourRenderingStatus = CreateContourRenderingStatus();
        BarRenderingStatus = CreateBarRenderingStatus();

        _overlayLayer = new SurfaceChartOverlayLayer(this)
        {
            IsHitTestVisible = true,
        };
        _hostContainer = new Grid();
        _hostContainer.Children.Add(_overlayLayer);
        Child = _hostContainer;

        ClipToBounds = true;
        SynchronizeViewStateProperties(_runtime.ViewState);
        UpdateInteractionQuality(_runtime.InteractionQuality);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        _runtime.UpdateViewSize(finalSize);
        UpdateOverlayViewSize(finalSize);
        UpdateScatterRenderingStatus();
        UpdateBarRenderingStatus();
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

    /// <summary>
    /// Resets the active data window to the full bounds of the current source.
    /// </summary>
    public void FitToData()
    {
        _runtime.FitToData();
        SynchronizeViewStateProperties(_runtime.ViewState);
    }

    /// <summary>
    /// Restores the default camera pose for the current data window.
    /// </summary>
    public void ResetCamera()
    {
        _runtime.ResetCamera();
        SynchronizeViewStateProperties(_runtime.ViewState);
    }

    /// <summary>
    /// Updates the active data window through the public chart-view contract.
    /// </summary>
    /// <param name="dataWindow">The target data window.</param>
    public void ZoomTo(SurfaceDataWindow dataWindow)
    {
        _runtime.ZoomTo(dataWindow);
        SynchronizeViewStateProperties(_runtime.ViewState);
    }

    /// <summary>
    /// Requests a visual refresh for the current plot model.
    /// </summary>
    public void Refresh()
    {
        LastRefreshRevision = Plot.Revision;
        UpdateScatterRenderingStatus();
        UpdateContourRenderingStatus();
        InvalidateRenderScene();
    }

    private void OnPlotChanged()
    {
        var activeSurfaceSeries = Plot.ActiveSurfaceSeries;
        var activeSource = activeSurfaceSeries?.SurfaceSource;
        if (!ReferenceEquals(_runtime.Source, activeSource))
        {
            OnSourceChanged(activeSource);
        }

        Refresh();
    }

    private void UpdateScatterRenderingStatus()
    {
        var nextStatus = CreateScatterRenderingStatus();
        if (ScatterRenderingStatus == nextStatus)
        {
            return;
        }

        ScatterRenderingStatus = nextStatus;
    }

    private void UpdateContourRenderingStatus()
    {
        var nextStatus = CreateContourRenderingStatus();
        if (ContourRenderingStatus == nextStatus)
        {
            return;
        }

        ContourRenderingStatus = nextStatus;
    }

    private ScatterChartRenderingStatus CreateScatterRenderingStatus()
    {
        var scatterData = Plot.ActiveScatterSeries?.ScatterData;
        var hasScatterData = scatterData is not null;
        return new ScatterChartRenderingStatus
        {
            HasSource = hasScatterData,
            IsReady = hasScatterData,
            BackendKind = SurfaceChartRenderBackendKind.Software,
            IsInteracting = _interactionController.HasActiveGesture,
            InteractionQuality = InteractionQuality,
            SeriesCount = scatterData?.SeriesCount ?? 0,
            PointCount = scatterData?.PointCount ?? 0,
            ColumnarSeriesCount = scatterData?.ColumnarSeriesCount ?? 0,
            ColumnarPointCount = scatterData?.ColumnarPointCount ?? 0,
            PickablePointCount = scatterData?.PickablePointCount ?? 0,
            StreamingAppendBatchCount = scatterData?.StreamingAppendBatchCount ?? 0,
            StreamingReplaceBatchCount = scatterData?.StreamingReplaceBatchCount ?? 0,
            StreamingDroppedPointCount = scatterData?.StreamingDroppedPointCount ?? 0,
            LastStreamingDroppedPointCount = scatterData?.LastStreamingDroppedPointCount ?? 0,
            ConfiguredFifoCapacity = scatterData?.ConfiguredFifoCapacity ?? 0,
            ViewSize = _runtime.ViewSize,
            CameraTarget = ViewState.Camera.Target,
            CameraDistance = ViewState.Camera.Distance,
        };
    }

    private void UpdateBarRenderingStatus()
    {
        var nextStatus = CreateBarRenderingStatus();
        if (BarRenderingStatus == nextStatus)
        {
            return;
        }

        BarRenderingStatus = nextStatus;
    }

    private BarChartRenderingStatus CreateBarRenderingStatus()
    {
        var barData = Plot.ActiveBarSeries?.BarData;
        var hasBarData = barData is not null;
        return new BarChartRenderingStatus
        {
            HasSource = hasBarData,
            IsReady = hasBarData,
            BackendKind = SurfaceChartRenderBackendKind.Software,
            IsInteracting = _interactionController.HasActiveGesture,
            SeriesCount = barData?.SeriesCount ?? 0,
            CategoryCount = barData?.CategoryCount ?? 0,
            BarCount = barData is not null ? barData.SeriesCount * barData.CategoryCount : 0,
            Layout = barData?.Layout ?? BarChartLayout.Grouped,
            ViewSize = _runtime.ViewSize,
        };
    }

    private ContourChartRenderingStatus CreateContourRenderingStatus()
    {
        var contourData = Plot.ActiveContourSeries?.ContourData;
        var hasContourData = contourData is not null;

        if (!hasContourData)
        {
            return new ContourChartRenderingStatus
            {
                HasSource = false,
                IsReady = false,
            };
        }

        var scene = _contourSceneCache.GetOrCompute(contourData!, Plot.Revision);
        var totalSegments = 0;
        foreach (var line in scene.Lines)
        {
            totalSegments += line.Segments.Count;
        }

        return new ContourChartRenderingStatus
        {
            HasSource = true,
            IsReady = scene.Lines.Count > 0,
            LevelCount = contourData!.LevelCount,
            ExtractedLineCount = scene.Lines.Count,
            TotalSegmentCount = totalSegments,
        };
    }
}
