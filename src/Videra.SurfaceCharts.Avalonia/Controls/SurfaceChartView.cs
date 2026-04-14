using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Avalonia control shell for host-driven surface-chart tile scheduling and render-scene composition.
/// </summary>
/// <remarks>
/// The current alpha surface expects hosts to provide the tile source, viewport updates, and any
/// higher-level zoom/pan/orbit UI. Full built-in chart interaction and axis presentation are not
/// complete yet.
/// </remarks>
public partial class SurfaceChartView : Control
{
    private readonly SurfaceChartRuntime _runtime;
    private bool _synchronizingViewportFromViewState;
    private bool _synchronizingViewStateFromViewport;

    internal event EventHandler<SurfaceChartTileRequestFailedEventArgs>? TileRequestFailed;

    internal SurfaceChartTileRequestFailedEventArgs? LastTileFailure { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceChartView"/> class.
    /// </summary>
    public SurfaceChartView()
    {
        _runtime = new SurfaceChartRuntime(
            Viewport,
            NotifyTilesChanged,
            OnTileRequestFailed,
            ClearLastTileFailure,
            InvalidateRenderScene);

        ClipToBounds = true;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        _runtime.UpdateViewSize(finalSize);
        UpdateOverlayViewSize(finalSize);
        return base.ArrangeOverride(finalSize);
    }

    /// <summary>
    /// Resets the chart view to show the full dataset when a source is available.
    /// </summary>
    public void FitToData()
    {
        ApplyViewState(_runtime.CreateFitToDataViewState(Source));
    }

    /// <summary>
    /// Resets the persisted camera pose for the current data window.
    /// </summary>
    public void ResetCamera()
    {
        ApplyViewState(_runtime.CreateResetCameraViewState());
    }

    /// <summary>
    /// Updates the chart to the requested sample-space window.
    /// </summary>
    public void ZoomTo(double xMin, double xMax, double yMin, double yMax)
    {
        ApplyViewState(_runtime.CreateZoomedViewState(new SurfaceDataWindow(xMin, xMax, yMin, yMax)));
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

    private void ApplyViewState(SurfaceViewState viewState)
    {
        ArgumentNullException.ThrowIfNull(viewState);

        if (ViewState == viewState)
        {
            return;
        }

        SetCurrentValue(ViewStateProperty, viewState);
    }
}
