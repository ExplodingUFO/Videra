using Avalonia;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;

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
    private SurfaceTileRequestPlan? _currentPlan;

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
        _currentPlan = null;

        if (source is null)
        {
            return;
        }

        var plan = CreateCurrentRequestPlan();
        _currentPlan = plan;
        StartRequestPipeline(plan, requestGeneration);
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
        var effectiveOutputSize = GetEffectiveOutputSize();
        var metadata = _tileScheduler.Metadata;
        if (metadata is null || effectiveOutputSize.Width <= 0d || effectiveOutputSize.Height <= 0d)
        {
            return _tileScheduler.CreateRequestPlan(_cameraController.CurrentViewport, effectiveOutputSize);
        }

        var cameraFrame = SurfaceProjectionMath.CreateCameraFrame(
            metadata,
            _cameraController.CurrentViewState,
            effectiveOutputSize.Width,
            effectiveOutputSize.Height,
            1f);
        return _tileScheduler.CreateRequestPlan(
            _cameraController.CurrentViewState,
            cameraFrame,
            effectiveOutputSize,
            _interactionQuality);
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
        var plan = CreateCurrentRequestPlan();
        _clearFailureState();

        if (plan.IsEquivalentTo(_currentPlan))
        {
            _tileCache.PruneToKeys(plan.RetainedKeys);
            _invalidateScene();
            return;
        }

        var requestGeneration = SupersedeOutstandingRequests();
        if (_interactionQuality != SurfaceChartInteractionQuality.Interactive)
        {
            _tileCache.PruneToKeys(plan.RetainedKeys);
        }

        _invalidateScene();
        _currentPlan = plan;
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
