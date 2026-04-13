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
        CancelOutstandingRequests();
        _tileCache.Clear();
        _tileScheduler.UpdateSource(source);
        _invalidateScene();

        if (source is null)
        {
            return;
        }

        StartRequestPipeline(includeViewportRequest: _viewSize.Width > 0 && _viewSize.Height > 0);
    }

    public void UpdateViewport(SurfaceViewport viewport)
    {
        _cameraController.UpdateViewport(viewport);
        StartRequestPipeline(includeViewportRequest: true);
    }

    public void UpdateViewSize(Size viewSize)
    {
        _viewSize = viewSize;
        StartRequestPipeline(includeViewportRequest: viewSize.Width > 0 && viewSize.Height > 0);
    }

    private void StartRequestPipeline(bool includeViewportRequest)
    {
        CancelOutstandingRequests();

        var cancellationTokenSource = new CancellationTokenSource();
        _requestCts = cancellationTokenSource;
        _ = RunRequestPipelineAsync(includeViewportRequest, cancellationTokenSource.Token);
    }

    private async Task RunRequestPipelineAsync(bool includeViewportRequest, CancellationToken cancellationToken)
    {
        try
        {
            await _tileScheduler.RequestOverviewAsync(cancellationToken).ConfigureAwait(false);

            if (includeViewportRequest)
            {
                await _tileScheduler.RequestViewportAsync(_cameraController.CurrentViewport, _viewSize, cancellationToken)
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
