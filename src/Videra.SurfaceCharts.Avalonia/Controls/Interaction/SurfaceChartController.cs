using Avalonia;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceChartController
{
    private const double InteractiveOutputScale = 2d;
    private readonly SurfaceCameraController _cameraController;
    private readonly SurfaceTileCache _tileCache;
    private readonly SurfaceTileScheduler _tileScheduler;
    private readonly Action _clearFailureState;
    private readonly Action _invalidateScene;
    private CancellationTokenSource? _requestCts;
    private long _requestGeneration;
    private Size _viewSize;
    private SurfaceChartInteractionQuality _interactionQuality = SurfaceChartInteractionQuality.Refine;

    public SurfaceChartController(
        SurfaceCameraController cameraController,
        SurfaceTileCache tileCache,
        SurfaceTileScheduler tileScheduler,
        Action clearFailureState,
        Action invalidateScene)
    {
        _cameraController = cameraController ?? throw new ArgumentNullException(nameof(cameraController));
        _tileCache = tileCache ?? throw new ArgumentNullException(nameof(tileCache));
        _tileScheduler = tileScheduler ?? throw new ArgumentNullException(nameof(tileScheduler));
        _clearFailureState = clearFailureState ?? throw new ArgumentNullException(nameof(clearFailureState));
        _invalidateScene = invalidateScene ?? throw new ArgumentNullException(nameof(invalidateScene));
    }

    public void UpdateSource(ISurfaceTileSource? source)
    {
        var requestGeneration = SupersedeOutstandingRequests();
        _tileCache.Clear();
        _tileScheduler.UpdateSource(source);
        _invalidateScene();

        if (source is null)
        {
            return;
        }

        StartRequestPipeline(CreateCurrentRequestPlan(), requestGeneration);
    }

    public void UpdateViewport(SurfaceViewport viewport)
    {
        UpdateDataWindow(viewport.ToDataWindow());
    }

    public void UpdateViewState(SurfaceViewState viewState)
    {
        var currentViewState = _cameraController.CurrentViewState;
        if (currentViewState == viewState)
        {
            return;
        }

        _cameraController.UpdateViewState(viewState);

        if (currentViewState.DataWindow == viewState.DataWindow)
        {
            _clearFailureState();
            _invalidateScene();
            return;
        }

        RefreshRequestPipeline();
    }

    public void UpdateDataWindow(SurfaceDataWindow dataWindow)
    {
        _cameraController.UpdateDataWindow(dataWindow);
        RefreshRequestPipeline();
    }

    public void UpdateViewSize(Size viewSize)
    {
        if (viewSize == _viewSize)
        {
            return;
        }

        _viewSize = viewSize;
        RefreshRequestPipeline();
    }

    public void UpdateInteractionQuality(SurfaceChartInteractionQuality interactionQuality)
    {
        if (_interactionQuality == interactionQuality)
        {
            return;
        }

        _interactionQuality = interactionQuality;
        RefreshRequestPipeline();
    }

    private SurfaceTileRequestPlan CreateCurrentRequestPlan()
    {
        return _tileScheduler.CreateRequestPlan(_cameraController.CurrentViewport, GetEffectiveOutputSize());
    }

    private Size GetEffectiveOutputSize()
    {
        if (_interactionQuality != SurfaceChartInteractionQuality.Interactive)
        {
            return _viewSize;
        }

        return new Size(
            Math.Ceiling(_viewSize.Width * InteractiveOutputScale),
            Math.Ceiling(_viewSize.Height * InteractiveOutputScale));
    }

    private void RefreshRequestPipeline()
    {
        var requestGeneration = SupersedeOutstandingRequests();
        _clearFailureState();
        var plan = CreateCurrentRequestPlan();
        _tileCache.PruneToKeys(plan.RetainedKeys);
        _invalidateScene();
        StartRequestPipeline(plan, requestGeneration);
    }

    private void StartRequestPipeline(SurfaceTileRequestPlan plan, long requestGeneration)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        _requestCts = cancellationTokenSource;
        _ = RunRequestPipelineAsync(plan, requestGeneration, cancellationTokenSource.Token);
    }

    private long SupersedeOutstandingRequests()
    {
        var requestGeneration = ++_requestGeneration;
        _tileScheduler.SetActiveGeneration(requestGeneration);
        CancelOutstandingRequests();
        return requestGeneration;
    }

    private async Task RunRequestPipelineAsync(
        SurfaceTileRequestPlan plan,
        long requestGeneration,
        CancellationToken cancellationToken)
    {
        try
        {
            await _tileScheduler.RequestPlanAsync(plan, requestGeneration, cancellationToken).ConfigureAwait(false);
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
}
