using System.Numerics;
using Avalonia;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceChartRuntime
{
    private const double DefaultYawDegrees = 45.0;
    private const double DefaultPitchDegrees = 35.264;
    private const double DefaultFieldOfViewDegrees = 45.0;
    private static readonly SurfaceTileKey OverviewTileKey = new(0, 0, 0, 0);
    private readonly SurfaceCameraController _cameraController;
    private readonly SurfaceTileScheduler _tileScheduler;
    private readonly Action _clearFailureState;
    private readonly Action _invalidateScene;
    private readonly TimeSpan _refineIdleDelay;
    private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync;
    private CancellationTokenSource? _requestCts;
    private CancellationTokenSource? _qualityModeCts;
    private Task _pendingQualityTransition = Task.CompletedTask;
    private long _requestGeneration;
    private Size _viewSize;

    public SurfaceChartRuntime(
        SurfaceViewport initialViewport,
        Action tilesChanged,
        Action<SurfaceTileKey, Exception>? tileFailed,
        Action clearFailureState,
        Action invalidateScene)
        : this(
            new SurfaceCameraController(initialViewport),
            new SurfaceTileCache(),
            tilesChanged,
            tileFailed,
            clearFailureState,
            invalidateScene,
            TimeSpan.FromMilliseconds(150),
            DefaultDelayAsync)
    {
    }

    internal SurfaceChartRuntime(
        SurfaceCameraController cameraController,
        SurfaceTileCache tileCache,
        Action tilesChanged,
        Action<SurfaceTileKey, Exception>? tileFailed,
        Action clearFailureState,
        Action invalidateScene,
        TimeSpan? refineIdleDelay = null,
        Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
    {
        _cameraController = cameraController ?? throw new ArgumentNullException(nameof(cameraController));
        TileCache = tileCache ?? throw new ArgumentNullException(nameof(tileCache));
        _tileScheduler = new SurfaceTileScheduler(TileCache, tilesChanged, tileFailed);
        _clearFailureState = clearFailureState ?? throw new ArgumentNullException(nameof(clearFailureState));
        _invalidateScene = invalidateScene ?? throw new ArgumentNullException(nameof(invalidateScene));
        _refineIdleDelay = refineIdleDelay ?? TimeSpan.FromMilliseconds(150);
        _delayAsync = delayAsync ?? DefaultDelayAsync;
        CurrentViewState = CreateDefaultViewState(_cameraController.CurrentViewport.ToDataWindow());
    }

    internal SurfaceTileCache TileCache { get; }

    internal SurfaceViewState CurrentViewState { get; private set; }

    internal SurfaceInteractionQualityMode CurrentInteractionQualityMode { get; private set; } = SurfaceInteractionQualityMode.Refine;

    internal IReadOnlyList<SurfaceTile> GetLoadedTiles()
    {
        var loadedTiles = TileCache.GetLoadedTiles();
        if (CurrentInteractionQualityMode != SurfaceInteractionQualityMode.Interactive || loadedTiles.Count <= 1)
        {
            return loadedTiles;
        }

        var overviewTiles = loadedTiles.Where(static tile => tile.Key == OverviewTileKey).ToArray();
        return overviewTiles.Length > 0 ? overviewTiles : [loadedTiles[0]];
    }

    internal SurfaceViewState CreateFitToDataViewState(ISurfaceTileSource? source)
    {
        if (source is null)
        {
            return CurrentViewState;
        }

        return CreateDefaultViewState(new SurfaceDataWindow(0d, source.Metadata.Width, 0d, source.Metadata.Height));
    }

    internal SurfaceViewState CreateResetCameraViewState()
    {
        return CreateDefaultViewState(CurrentViewState.DataWindow);
    }

    internal SurfaceViewState CreateViewportBridgeViewState(SurfaceViewport viewport)
    {
        return CreateZoomedViewState(viewport.ToDataWindow());
    }

    internal SurfaceViewState CreateZoomedViewState(SurfaceDataWindow dataWindow)
    {
        ArgumentNullException.ThrowIfNull(dataWindow);
        return new SurfaceViewState(dataWindow, CreateWindowCameraPose(dataWindow, CurrentViewState.Camera));
    }

    public void UpdateSource(ISurfaceTileSource? source)
    {
        var requestGeneration = SupersedeOutstandingRequests();
        TileCache.Clear();
        _tileScheduler.UpdateSource(source);
        _invalidateScene();

        if (source is null)
        {
            return;
        }

        StartRequestPipeline(includeViewportRequest: ShouldRequestViewportDetails(), requestGeneration);
    }

    public void UpdateViewport(SurfaceViewport viewport)
    {
        UpdateViewState(CreateViewportBridgeViewState(viewport));
    }

    public void UpdateViewState(SurfaceViewState viewState)
    {
        ArgumentNullException.ThrowIfNull(viewState);

        var dataWindowChanged = CurrentViewState.DataWindow != viewState.DataWindow;
        CurrentViewState = viewState;
        _cameraController.UpdateViewport(SurfaceViewport.FromDataWindow(viewState.DataWindow));

        if (!dataWindowChanged)
        {
            _invalidateScene();
            return;
        }

        var requestGeneration = SupersedeOutstandingRequests();
        _clearFailureState();
        TileCache.PruneDetailTiles();
        _invalidateScene();
        StartRequestPipeline(includeViewportRequest: ShouldRequestViewportDetails(), requestGeneration);
    }

    public void UpdateViewSize(Size viewSize)
    {
        if (viewSize == _viewSize)
        {
            return;
        }

        _viewSize = viewSize;
        var requestGeneration = SupersedeOutstandingRequests();
        _clearFailureState();
        TileCache.PruneDetailTiles();
        _invalidateScene();

        StartRequestPipeline(includeViewportRequest: ShouldRequestViewportDetails(), requestGeneration);
    }

    public void EnterInteractiveMode()
    {
        CancelPendingQualityTransition();

        if (CurrentInteractionQualityMode == SurfaceInteractionQualityMode.Interactive)
        {
            return;
        }

        CurrentInteractionQualityMode = SurfaceInteractionQualityMode.Interactive;

        var requestGeneration = SupersedeOutstandingRequests();
        _clearFailureState();
        TileCache.PruneDetailTiles();
        _invalidateScene();
        StartRequestPipeline(includeViewportRequest: false, requestGeneration);
    }

    public void ScheduleRefineMode()
    {
        CancelPendingQualityTransition();

        if (CurrentInteractionQualityMode != SurfaceInteractionQualityMode.Interactive)
        {
            return;
        }

        var cancellationTokenSource = new CancellationTokenSource();
        _qualityModeCts = cancellationTokenSource;
        _pendingQualityTransition = ReturnToRefineModeAsync(cancellationTokenSource.Token);
    }

    internal Task WaitForPendingQualityTransitionAsync()
    {
        return _pendingQualityTransition;
    }

    private void StartRequestPipeline(bool includeViewportRequest, long requestGeneration)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        _requestCts = cancellationTokenSource;
        _ = RunRequestPipelineAsync(includeViewportRequest, requestGeneration, cancellationTokenSource.Token);
    }

    private long SupersedeOutstandingRequests()
    {
        var requestGeneration = ++_requestGeneration;
        _tileScheduler.SetActiveGeneration(requestGeneration);
        CancelOutstandingRequests();
        return requestGeneration;
    }

    private async Task RunRequestPipelineAsync(
        bool includeViewportRequest,
        long requestGeneration,
        CancellationToken cancellationToken)
    {
        try
        {
            await _tileScheduler.RequestOverviewAsync(requestGeneration, cancellationToken).ConfigureAwait(false);

            if (includeViewportRequest)
            {
                await _tileScheduler.RequestViewportAsync(
                        _cameraController.CurrentViewport,
                        _viewSize,
                        requestGeneration,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Newer requests replace older pipelines, so cancellation is expected.
        }
    }

    private void CancelOutstandingRequests()
    {
        if (_requestCts is null)
        {
            return;
        }

        _requestCts.Cancel();
        _requestCts.Dispose();
        _requestCts = null;
    }

    private async Task ReturnToRefineModeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _delayAsync(_refineIdleDelay, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        CurrentInteractionQualityMode = SurfaceInteractionQualityMode.Refine;
        _invalidateScene();

        var requestGeneration = SupersedeOutstandingRequests();
        _clearFailureState();
        StartRequestPipeline(includeViewportRequest: _viewSize.Width > 0 && _viewSize.Height > 0, requestGeneration);
    }

    private void CancelPendingQualityTransition()
    {
        if (_qualityModeCts is null)
        {
            return;
        }

        _qualityModeCts.Cancel();
        _qualityModeCts.Dispose();
        _qualityModeCts = null;
        _pendingQualityTransition = Task.CompletedTask;
    }

    private bool ShouldRequestViewportDetails()
    {
        return CurrentInteractionQualityMode == SurfaceInteractionQualityMode.Refine
            && _viewSize.Width > 0
            && _viewSize.Height > 0;
    }

    private static Task DefaultDelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        return Task.Delay(delay, cancellationToken);
    }

    internal static SurfaceViewState CreateDefaultViewState(SurfaceDataWindow dataWindow)
    {
        ArgumentNullException.ThrowIfNull(dataWindow);
        return new SurfaceViewState(dataWindow, CreateDefaultCameraPose(dataWindow));
    }

    internal static SurfaceCameraPose CreateDefaultCameraPose(SurfaceDataWindow dataWindow)
    {
        ArgumentNullException.ThrowIfNull(dataWindow);

        return new SurfaceCameraPose(
            new Vector3(
                (float)((dataWindow.XMin + dataWindow.XMax) / 2.0),
                (float)((dataWindow.YMin + dataWindow.YMax) / 2.0),
                0f),
            yaw: DefaultYawDegrees,
            pitch: DefaultPitchDegrees,
            distance: Math.Max(dataWindow.Width, dataWindow.Height) * 2.0,
            fieldOfView: DefaultFieldOfViewDegrees,
            projectionMode: SurfaceProjectionMode.Perspective);
    }

    private static SurfaceCameraPose CreateWindowCameraPose(SurfaceDataWindow dataWindow, SurfaceCameraPose currentCamera)
    {
        ArgumentNullException.ThrowIfNull(currentCamera);

        var defaultCamera = CreateDefaultCameraPose(dataWindow);
        return new SurfaceCameraPose(
            defaultCamera.Target,
            currentCamera.Yaw,
            currentCamera.Pitch,
            defaultCamera.Distance,
            currentCamera.FieldOfView,
            currentCamera.ProjectionMode);
    }
}
