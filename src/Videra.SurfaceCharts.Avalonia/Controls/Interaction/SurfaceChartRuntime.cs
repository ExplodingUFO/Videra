using Avalonia;
using Avalonia.Threading;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceChartRuntime
{
    private static readonly TimeSpan InteractionSettleDelay = TimeSpan.FromMilliseconds(250);
    private readonly SurfaceCameraController _cameraController;
    private readonly SurfaceChartController _controller;
    private readonly Action _invalidateRenderScene;
    private readonly Action _invalidateOverlay;
    private readonly Action _clearFailureState;
    private readonly Action<SurfaceChartInteractionQuality> _publishInteractionQuality;
    private CancellationTokenSource? _interactionSettleCts;
    private long _interactionSettleGeneration;

    public SurfaceChartRuntime(
        SurfaceViewport initialViewport,
        SurfaceTileCache tileCache,
        Action notifyTilesChanged,
        Action<SurfaceTileKey, Exception> onTileRequestFailed,
        Action clearFailureState,
        Action<SurfaceChartInteractionQuality> publishInteractionQuality,
        Action invalidateRenderScene,
        Action invalidateOverlay)
    {
        ArgumentNullException.ThrowIfNull(tileCache);
        ArgumentNullException.ThrowIfNull(notifyTilesChanged);
        ArgumentNullException.ThrowIfNull(onTileRequestFailed);
        ArgumentNullException.ThrowIfNull(clearFailureState);
        ArgumentNullException.ThrowIfNull(publishInteractionQuality);
        ArgumentNullException.ThrowIfNull(invalidateRenderScene);
        ArgumentNullException.ThrowIfNull(invalidateOverlay);

        _clearFailureState = clearFailureState;
        _publishInteractionQuality = publishInteractionQuality;
        _invalidateRenderScene = invalidateRenderScene;
        _invalidateOverlay = invalidateOverlay;
        _cameraController = new SurfaceCameraController(initialViewport);
        _controller = new SurfaceChartController(
            _cameraController,
            tileCache,
            new SurfaceTileScheduler(tileCache, notifyTilesChanged, onTileRequestFailed),
            clearFailureState,
            invalidateRenderScene);
    }

    public ISurfaceTileSource? Source { get; private set; }

    public SurfaceMetadata? Metadata => Source?.Metadata;

    public SurfaceViewState ViewState => _cameraController.CurrentViewState;

    public Size ViewSize { get; private set; }

    public bool CanInteract => Metadata is not null && ViewSize.Width > 0d && ViewSize.Height > 0d;

    public SurfaceViewport CurrentViewport => _cameraController.CurrentViewport;

    public SurfaceChartProjectionSettings ProjectionSettings => _cameraController.ProjectionSettings;

    public SurfaceChartInteractionQuality InteractionQuality { get; private set; } = SurfaceChartInteractionQuality.Refine;

    public SurfaceCameraFrame? CreateCameraFrame(Size viewSize, float renderScale)
    {
        var metadata = Source?.Metadata;
        if (metadata is null || viewSize.Width <= 0d || viewSize.Height <= 0d)
        {
            return null;
        }

        return SurfaceProjectionMath.CreateCameraFrame(metadata, ViewState, viewSize.Width, viewSize.Height, renderScale);
    }

    public void UpdateSource(ISurfaceTileSource? source)
    {
        CancelInteractionSettle();
        SetInteractionQuality(SurfaceChartInteractionQuality.Refine);
        Source = source;
        _controller.UpdateSource(source);
        _invalidateOverlay();
    }

    public void UpdateViewSize(Size viewSize)
    {
        if (viewSize == ViewSize)
        {
            return;
        }

        ViewSize = viewSize;
        _controller.UpdateViewSize(viewSize);
    }

    public void UpdateViewState(SurfaceViewState viewState)
    {
        if (ViewState == viewState)
        {
            return;
        }

        _controller.UpdateViewState(viewState);
        _invalidateOverlay();
    }

    public void UpdateDataWindow(SurfaceDataWindow dataWindow)
    {
        if (ViewState.DataWindow == dataWindow)
        {
            return;
        }

        _controller.UpdateDataWindow(dataWindow);
        _invalidateOverlay();
    }

    public void UpdateViewport(SurfaceViewport viewport)
    {
        UpdateDataWindow(viewport.ToDataWindow());
    }

    public void UpdateProjectionSettings(SurfaceChartProjectionSettings projectionSettings)
    {
        if (ProjectionSettings == projectionSettings)
        {
            return;
        }

        _clearFailureState();
        _cameraController.UpdateProjectionSettings(projectionSettings);
        _invalidateRenderScene();
        _invalidateOverlay();
    }

    public void BeginInteraction()
    {
        SetInteractionQuality(SurfaceChartInteractionQuality.Interactive);
        ScheduleInteractionSettle();
    }

    public void FitToData()
    {
        var metadata = Source?.Metadata;
        if (metadata is null)
        {
            return;
        }

        UpdateViewState(
            SurfaceViewState.CreateDefault(
                metadata,
                new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height)));
    }

    public void ResetCamera()
    {
        var metadata = Source?.Metadata;
        if (metadata is null)
        {
            return;
        }

        _clearFailureState();
        _cameraController.UpdateViewState(
            new SurfaceViewState(
                ViewState.DataWindow,
                SurfaceCameraPose.CreateDefault(metadata, ViewState.DataWindow)));
        _invalidateRenderScene();
        _invalidateOverlay();
    }

    public void ZoomTo(SurfaceDataWindow dataWindow)
    {
        var metadata = Source?.Metadata;
        if (metadata is null)
        {
            return;
        }

        var nextWindow = dataWindow.ClampTo(metadata);
        UpdateDataWindow(nextWindow);
    }

    private void SetInteractionQuality(SurfaceChartInteractionQuality interactionQuality)
    {
        if (InteractionQuality == interactionQuality)
        {
            return;
        }

        InteractionQuality = interactionQuality;
        _controller.UpdateInteractionQuality(interactionQuality);
        PublishInteractionQuality(interactionQuality);
    }

    private void PublishInteractionQuality(SurfaceChartInteractionQuality interactionQuality)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            _publishInteractionQuality(interactionQuality);
            return;
        }

        Dispatcher.UIThread.Post(() => _publishInteractionQuality(interactionQuality));
    }

    private void ScheduleInteractionSettle()
    {
        var generation = Interlocked.Increment(ref _interactionSettleGeneration);
        CancelInteractionSettle();

        var cancellationTokenSource = new CancellationTokenSource();
        _interactionSettleCts = cancellationTokenSource;
        _ = RunInteractionSettleAsync(generation, cancellationTokenSource);
    }

    private async Task RunInteractionSettleAsync(long generation, CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            await Task.Delay(InteractionSettleDelay, cancellationTokenSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (cancellationTokenSource.IsCancellationRequested ||
                generation != Interlocked.Read(ref _interactionSettleGeneration))
            {
                return;
            }

            SetInteractionQuality(SurfaceChartInteractionQuality.Refine);
            if (ReferenceEquals(_interactionSettleCts, cancellationTokenSource))
            {
                _interactionSettleCts = null;
                cancellationTokenSource.Dispose();
            }
        });
    }

    private void CancelInteractionSettle()
    {
        if (_interactionSettleCts is null)
        {
            return;
        }

        _interactionSettleCts.Cancel();
        _interactionSettleCts.Dispose();
        _interactionSettleCts = null;
    }
}
