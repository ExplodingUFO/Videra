using Avalonia;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceChartController
{
    private readonly SurfaceCameraController _cameraController;
    private readonly SurfaceTileCache _tileCache;
    private readonly SurfaceTileScheduler _tileScheduler;
    private readonly Action _invalidateScene;
    private CancellationTokenSource? _requestCts;
    private long _requestGeneration;
    private Size _viewSize;

    public SurfaceChartController(
        SurfaceCameraController cameraController,
        SurfaceTileCache tileCache,
        SurfaceTileScheduler tileScheduler,
        Action invalidateScene)
    {
        _cameraController = cameraController ?? throw new ArgumentNullException(nameof(cameraController));
        _tileCache = tileCache ?? throw new ArgumentNullException(nameof(tileCache));
        _tileScheduler = tileScheduler ?? throw new ArgumentNullException(nameof(tileScheduler));
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

        StartRequestPipeline(includeViewportRequest: _viewSize.Width > 0 && _viewSize.Height > 0, requestGeneration);
    }

    public void UpdateViewport(SurfaceViewport viewport)
    {
        _cameraController.UpdateViewport(viewport);
        var requestGeneration = SupersedeOutstandingRequests();
        _tileCache.PruneDetailTiles();
        _invalidateScene();
        StartRequestPipeline(includeViewportRequest: true, requestGeneration);
    }

    public void UpdateViewSize(Size viewSize)
    {
        _viewSize = viewSize;

        if (viewSize.Width > 0 && viewSize.Height > 0)
        {
            _tileCache.PruneDetailTiles();
            _invalidateScene();
        }

        var requestGeneration = SupersedeOutstandingRequests();
        StartRequestPipeline(includeViewportRequest: viewSize.Width > 0 && viewSize.Height > 0, requestGeneration);
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
}
