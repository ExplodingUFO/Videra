using Avalonia;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

public partial class SurfaceChartView
{
    /// <summary>
    /// Identifies the <see cref="Source"/> property.
    /// </summary>
    public static readonly StyledProperty<ISurfaceTileSource?> SourceProperty =
        AvaloniaProperty.Register<SurfaceChartView, ISurfaceTileSource?>(nameof(Source));

    /// <summary>
    /// Identifies the <see cref="Viewport"/> property.
    /// </summary>
    public static readonly StyledProperty<SurfaceViewport> ViewportProperty =
        AvaloniaProperty.Register<SurfaceChartView, SurfaceViewport>(
            nameof(Viewport),
            defaultValue: new SurfaceViewport(0, 0, 1, 1));

    /// <summary>
    /// Identifies the <see cref="ViewState"/> property.
    /// </summary>
    public static readonly StyledProperty<SurfaceViewState> ViewStateProperty =
        AvaloniaProperty.Register<SurfaceChartView, SurfaceViewState>(
            nameof(ViewState),
            defaultValue: SurfaceChartRuntime.CreateDefaultViewState(new SurfaceDataWindow(0d, 1d, 0d, 1d)));

    /// <summary>
    /// Identifies the <see cref="ColorMap"/> property.
    /// </summary>
    public static readonly StyledProperty<SurfaceColorMap?> ColorMapProperty =
        AvaloniaProperty.Register<SurfaceChartView, SurfaceColorMap?>(nameof(ColorMap));

    static SurfaceChartView()
    {
        SourceProperty.Changed.AddClassHandler<SurfaceChartView>(
            static (view, args) => view.OnSourceChanged((ISurfaceTileSource?)args.NewValue));
        ViewportProperty.Changed.AddClassHandler<SurfaceChartView>(
            static (view, args) => view.OnViewportChanged((SurfaceViewport)args.NewValue!));
        ViewStateProperty.Changed.AddClassHandler<SurfaceChartView>(
            static (view, args) => view.OnViewStateChanged((SurfaceViewState)args.NewValue!));
        ColorMapProperty.Changed.AddClassHandler<SurfaceChartView>(
            static (view, _) => view.InvalidateRenderScene());
    }

    /// <summary>
    /// Gets or sets the source used to resolve surface tiles.
    /// </summary>
    public ISurfaceTileSource? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the current surface viewport in sample space.
    /// </summary>
    public SurfaceViewport Viewport
    {
        get => GetValue(ViewportProperty);
        set => SetValue(ViewportProperty, value);
    }

    /// <summary>
    /// Gets or sets the persisted chart view state. This is the primary public view contract.
    /// </summary>
    public SurfaceViewState ViewState
    {
        get => GetValue(ViewStateProperty);
        set => SetValue(ViewStateProperty, value);
    }

    /// <summary>
    /// Gets or sets the optional color map used when building the render scene.
    /// </summary>
    public SurfaceColorMap? ColorMap
    {
        get => GetValue(ColorMapProperty);
        set => SetValue(ColorMapProperty, value);
    }

    private void OnSourceChanged(ISurfaceTileSource? source)
    {
        LastTileFailure = null;
        _runtime.UpdateSource(source);
    }

    private void OnViewportChanged(SurfaceViewport viewport)
    {
        if (_synchronizingViewportFromViewState)
        {
            return;
        }

        var bridgedViewState = _runtime.CreateViewportBridgeViewState(viewport);
        if (ViewState == bridgedViewState)
        {
            return;
        }

        _synchronizingViewStateFromViewport = true;
        try
        {
            SetCurrentValue(ViewStateProperty, bridgedViewState);
        }
        finally
        {
            _synchronizingViewStateFromViewport = false;
        }
    }

    private void OnViewStateChanged(SurfaceViewState viewState)
    {
        ArgumentNullException.ThrowIfNull(viewState);

        var bridgedViewport = SurfaceViewport.FromDataWindow(viewState.DataWindow);
        if (!_synchronizingViewStateFromViewport && Viewport != bridgedViewport)
        {
            _synchronizingViewportFromViewState = true;
            try
            {
                SetCurrentValue(ViewportProperty, bridgedViewport);
            }
            finally
            {
                _synchronizingViewportFromViewState = false;
            }
        }

        _runtime.UpdateViewState(viewState);
    }
}
