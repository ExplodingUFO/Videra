using System.Numerics;
using Avalonia;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

public partial class SurfaceChartView
{
    private static readonly SurfaceViewState DefaultViewState = CreateCompatibilityViewState(new SurfaceViewport(0d, 0d, 1d, 1d));
    private bool _isSynchronizingViewStateProperties;
    private SurfaceChartInteractionQuality _interactionQuality = SurfaceChartInteractionQuality.Refine;

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
            defaultValue: DefaultViewState);

    /// <summary>
    /// Identifies the <see cref="ColorMap"/> property.
    /// </summary>
    public static readonly StyledProperty<SurfaceColorMap?> ColorMapProperty =
        AvaloniaProperty.Register<SurfaceChartView, SurfaceColorMap?>(nameof(ColorMap));

    /// <summary>
    /// Identifies the <see cref="OverlayOptions"/> property.
    /// </summary>
    public static readonly StyledProperty<SurfaceChartOverlayOptions> OverlayOptionsProperty =
        AvaloniaProperty.Register<SurfaceChartView, SurfaceChartOverlayOptions>(
            nameof(OverlayOptions),
            defaultValue: SurfaceChartOverlayOptions.Default);

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
        OverlayOptionsProperty.Changed.AddClassHandler<SurfaceChartView>(
            static (view, _) => view.InvalidateOverlay());
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
    /// Gets or sets the current surface viewport in sample space as a compatibility bridge for
    /// hosts that have not moved to the authoritative <see cref="ViewState"/> contract.
    /// </summary>
    public SurfaceViewport Viewport
    {
        get => GetValue(ViewportProperty);
        set => SetValue(ViewportProperty, value);
    }

    /// <summary>
    /// Gets or sets the authoritative persisted chart-view state.
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

    /// <summary>
    /// Gets or sets chart-local overlay layout and formatting options for formatter,
    /// title/unit override, minor ticks, grid plane, and axis-side selection.
    /// </summary>
    public SurfaceChartOverlayOptions OverlayOptions
    {
        get => GetValue(OverlayOptionsProperty);
        set => SetValue(OverlayOptionsProperty, value);
    }

    /// <summary>
    /// Gets the current diagnostic interaction-quality mode, exposing <c>Interactive</c>
    /// while input is active and <c>Refine</c> after it settles.
    /// </summary>
    public SurfaceChartInteractionQuality InteractionQuality => _interactionQuality;

    private void OnSourceChanged(ISurfaceTileSource? source)
    {
        _overlayCoordinator.ResetForSourceChange();
        _chartProjection = null;
        LastTileFailure = null;
        _runtime.UpdateSource(source);
    }

    private void OnViewportChanged(SurfaceViewport viewport)
    {
        if (_isSynchronizingViewStateProperties)
        {
            return;
        }

        var nextViewState = new SurfaceViewState(
            viewport.ToDataWindow(),
            _runtime.ViewState.Camera,
            _runtime.ViewState.DisplaySpace);
        ApplyViewState(nextViewState);
    }

    private void OnViewStateChanged(SurfaceViewState viewState)
    {
        if (_isSynchronizingViewStateProperties)
        {
            return;
        }

        ApplyViewState(viewState);
    }

    private void ApplyViewState(SurfaceViewState viewState)
    {
        _runtime.UpdateViewState(viewState);
        SynchronizeViewStateProperties(_runtime.ViewState);
    }

    private void SynchronizeViewStateProperties(SurfaceViewState viewState)
    {
        _isSynchronizingViewStateProperties = true;

        try
        {
            if (GetValue(ViewStateProperty) != viewState)
            {
                SetCurrentValue(ViewStateProperty, viewState);
            }

            var viewport = viewState.ToViewport();
            if (GetValue(ViewportProperty) != viewport)
            {
                SetCurrentValue(ViewportProperty, viewport);
            }
        }
        finally
        {
            _isSynchronizingViewStateProperties = false;
        }
    }

    private void UpdateInteractionQuality(SurfaceChartInteractionQuality interactionQuality)
    {
        if (_interactionQuality == interactionQuality)
        {
            return;
        }

        _interactionQuality = interactionQuality;
        InteractionQualityChanged?.Invoke(this, EventArgs.Empty);
    }

    private static SurfaceViewState CreateCompatibilityViewState(SurfaceViewport viewport)
    {
        var target = new Vector3(
            (float)(viewport.StartX + (viewport.Width * 0.5d)),
            0f,
            (float)(viewport.StartY + (viewport.Height * 0.5d)));
        var diagonal = Math.Sqrt((viewport.Width * viewport.Width) + (viewport.Height * viewport.Height));
        var camera = new SurfaceCameraPose(
            target,
            SurfaceCameraPose.DefaultYawDegrees,
            SurfaceCameraPose.DefaultPitchDegrees,
            Math.Max(diagonal, 1d),
            SurfaceCameraPose.DefaultFieldOfViewDegrees);

        return new SurfaceViewState(viewport.ToDataWindow(), camera);
    }
}
