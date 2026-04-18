using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Videra.Avalonia.Controls.Interaction;
using Videra.Avalonia.Composition;
using Videra.Avalonia.Rendering;
using Videra.Avalonia.Runtime;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.RenderPipeline.Extensibility;
using Videra.Core.Graphics.Wireframe;
using Videra.Core.Inspection;
using Videra.Core.Styles.Parameters;
using Videra.Core.Styles.Presets;

namespace Videra.Avalonia.Controls;

/// <summary>
/// 3D 渲染视图控件 - 使用跨平台软件后端渲染。
/// </summary>
public partial class VideraView : Decorator
{
    private readonly VideraViewRuntime _runtime;
    private readonly INativeHostFactory _nativeHostFactory;
    private NativeControlHost? _nativeHost;
    private Grid? _nativeContainer;
    private Border? _inputOverlay;
    private VideraViewOverlayPresenter? _overlayPresenter;
    private readonly ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<VideraView>();
    private readonly Func<uint, uint, WriteableBitmap?>? _bitmapFactory;

    public event EventHandler? BackendReady;
    public event EventHandler<VideraBackendStatusChangedEventArgs>? BackendStatusChanged;
    public event EventHandler<VideraBackendFailureEventArgs>? InitializationFailed;
    public event EventHandler<SelectionRequestedEventArgs>? SelectionRequested;
    public event EventHandler<AnnotationRequestedEventArgs>? AnnotationRequested;

    public static readonly StyledProperty<Color> BackgroundColorProperty =
        AvaloniaProperty.Register<VideraView, Color>(nameof(BackgroundColor), Colors.Black);

    public static readonly StyledProperty<IEnumerable> ItemsProperty =
        AvaloniaProperty.Register<VideraView, IEnumerable>(nameof(Items));

    public static readonly StyledProperty<bool> CameraInvertXProperty =
        AvaloniaProperty.Register<VideraView, bool>(nameof(CameraInvertX));

    public static readonly StyledProperty<bool> CameraInvertYProperty =
        AvaloniaProperty.Register<VideraView, bool>(nameof(CameraInvertY));

    public static readonly StyledProperty<bool> IsGridVisibleProperty =
        AvaloniaProperty.Register<VideraView, bool>(nameof(IsGridVisible), true);

    public static readonly StyledProperty<float> GridHeightProperty =
        AvaloniaProperty.Register<VideraView, float>(nameof(GridHeight), 0f);

    public static readonly StyledProperty<Color> GridColorProperty =
        AvaloniaProperty.Register<VideraView, Color>(nameof(GridColor), Colors.Gray);

    public static readonly StyledProperty<GraphicsBackendPreference> PreferredBackendProperty =
        AvaloniaProperty.Register<VideraView, GraphicsBackendPreference>(
            nameof(PreferredBackend),
            GraphicsBackendPreference.Auto);

    public static readonly StyledProperty<RenderStylePreset> RenderStyleProperty =
        AvaloniaProperty.Register<VideraView, RenderStylePreset>(
            nameof(RenderStyle),
            defaultValue: RenderStylePreset.Realistic);

    public static readonly StyledProperty<RenderStyleParameters?> RenderStyleParametersProperty =
        AvaloniaProperty.Register<VideraView, RenderStyleParameters?>(
            nameof(RenderStyleParameters));

    public static readonly StyledProperty<WireframeMode> WireframeModeProperty =
        AvaloniaProperty.Register<VideraView, WireframeMode>(
            nameof(WireframeMode),
            defaultValue: WireframeMode.None);

    public static readonly StyledProperty<Color> WireframeColorProperty =
        AvaloniaProperty.Register<VideraView, Color>(
            nameof(WireframeColor),
            Colors.Black);

    public VideraView()
        : this(nativeHostFactory: null)
    {
    }

    internal VideraView(INativeHostFactory? nativeHostFactory, Func<uint, uint, WriteableBitmap?>? bitmapFactory = null)
    {
        AvaloniaGraphicsBackendResolver.EnsureRegistered();
        Engine = new VideraEngine();
        _nativeHostFactory = nativeHostFactory ?? new DefaultNativeHostFactory();
        _bitmapFactory = bitmapFactory;
        _runtime = new VideraViewRuntime(this, Engine, _logger, _bitmapFactory);
        Focusable = true;
        ClipToBounds = true;
    }

    /// <summary>
    /// Gets the engine that backs this view's public extensibility surface.
    /// </summary>
    public VideraEngine Engine { get; }

    /// <summary>
    /// Gets or sets the backend and diagnostics options used when initializing the view session.
    /// </summary>
    public VideraViewOptions Options
    {
        get => _runtime.Options;
        set => _runtime.Options = value;
    }

    /// <summary>
    /// Gets the public availability and fallback shell for the current session state.
    /// </summary>
    /// <remarks>
    /// The snapshot mirrors backend readiness, <see cref="VideraBackendDiagnostics.IsUsingSoftwareFallback" />,
    /// and <see cref="VideraBackendDiagnostics.FallbackReason" /> so callers can inspect unavailable or
    /// degraded backend resolution without reaching into internal session state.
    /// </remarks>
    public VideraBackendDiagnostics BackendDiagnostics => _runtime.BackendDiagnostics;

    /// <summary>
    /// Gets the public render capability snapshot exposed by the underlying engine.
    /// </summary>
    /// <remarks>
    /// This remains queryable before initialization and after disposal. In those states the snapshot reports
    /// <c>IsInitialized = false</c> while still exposing the stable capability flags and any last-known
    /// pipeline snapshot from a previously rendered frame.
    /// </remarks>
    public RenderCapabilitySnapshot RenderCapabilities => Engine.GetRenderCapabilities();

    /// <summary>
    /// Gets or sets the host-owned object selection state projected by the control.
    /// </summary>
    public VideraSelectionState SelectionState
    {
        get => _runtime.SelectionState;
        set => _runtime.SelectionState = value;
    }

    /// <summary>
    /// Gets or sets the host-owned annotations to display for the current scene.
    /// </summary>
    public IReadOnlyList<VideraAnnotation> Annotations
    {
        get => _runtime.Annotations;
        set => _runtime.Annotations = value;
    }

    /// <summary>
    /// Gets or sets the host-readable measurement set projected by the inspection workflow.
    /// </summary>
    public IReadOnlyList<VideraMeasurement> Measurements
    {
        get => _runtime.Measurements;
        set => _runtime.Measurements = value;
    }

    /// <summary>
    /// Gets or sets the viewer-first clipping planes applied to the active inspection scene.
    /// </summary>
    public IReadOnlyList<VideraClipPlane> ClippingPlanes
    {
        get => _runtime.ClippingPlanes;
        set => _runtime.ClippingPlanes = value;
    }

    /// <summary>
    /// Gets or sets the interaction mode the shell should interpret future input against.
    /// </summary>
    public VideraInteractionMode InteractionMode { get; set; } = VideraInteractionMode.Navigate;

    /// <summary>
    /// Gets or sets the host-supplied interaction options for controlled selection and annotation behavior.
    /// </summary>
    public VideraInteractionOptions InteractionOptions
    {
        get => _runtime.InteractionOptions;
        set => _runtime.InteractionOptions = value;
    }

    public IResourceFactory? GetResourceFactory() => _runtime.GetResourceFactory();

    public Color BackgroundColor
    {
        get => GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public IEnumerable Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public bool CameraInvertX
    {
        get => GetValue(CameraInvertXProperty);
        set => SetValue(CameraInvertXProperty, value);
    }

    public bool CameraInvertY
    {
        get => GetValue(CameraInvertYProperty);
        set => SetValue(CameraInvertYProperty, value);
    }

    public bool IsGridVisible
    {
        get => GetValue(IsGridVisibleProperty);
        set => SetValue(IsGridVisibleProperty, value);
    }

    public float GridHeight
    {
        get => GetValue(GridHeightProperty);
        set => SetValue(GridHeightProperty, value);
    }

    public Color GridColor
    {
        get => GetValue(GridColorProperty);
        set => SetValue(GridColorProperty, value);
    }

    public GraphicsBackendPreference PreferredBackend
    {
        get => GetValue(PreferredBackendProperty);
        set => SetValue(PreferredBackendProperty, value);
    }

    public RenderStylePreset RenderStyle
    {
        get => GetValue(RenderStyleProperty);
        set => SetValue(RenderStyleProperty, value);
    }

    public RenderStyleParameters? RenderStyleParameters
    {
        get => GetValue(RenderStyleParametersProperty);
        set => SetValue(RenderStyleParametersProperty, value);
    }

    public WireframeMode WireframeMode
    {
        get => GetValue(WireframeModeProperty);
        set => SetValue(WireframeModeProperty, value);
    }

    public Color WireframeColor
    {
        get => GetValue(WireframeColorProperty);
        set => SetValue(WireframeColorProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PreferredBackendProperty)
        {
            _runtime.OnPreferredBackendChanged();
            return;
        }

        if (change.Property == RenderStyleProperty)
        {
            Engine.StyleService.ApplyPreset(change.GetNewValue<RenderStylePreset>());
            _runtime.RenderSession.Invalidate(RenderInvalidationKinds.Style);
            return;
        }

        if (change.Property == RenderStyleParametersProperty)
        {
            var parameters = change.GetNewValue<RenderStyleParameters?>();
            if (parameters != null)
            {
                Engine.StyleService.UpdateParameters(parameters);
                _runtime.RenderSession.Invalidate(RenderInvalidationKinds.Style);
            }

            return;
        }

        if (change.Property == WireframeModeProperty)
        {
            Engine.Wireframe.Mode = change.GetNewValue<WireframeMode>();
            _runtime.RenderSession.Invalidate(RenderInvalidationKinds.Style);
            return;
        }

        if (change.Property == WireframeColorProperty)
        {
            var c = change.GetNewValue<Color>();
            Engine.Wireframe.LineColor = new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
            _runtime.RenderSession.Invalidate(RenderInvalidationKinds.Style);
            return;
        }

        if (change.Property == ItemsProperty)
        {
            var oldList = change.GetOldValue<IEnumerable>();
            var newList = change.GetNewValue<IEnumerable>();
            _runtime.UpdateItemsSubscription(oldList, newList);
            return;
        }

        if (!_runtime.RenderSession.IsReady)
            return;

        if (change.Property == BackgroundColorProperty)
        {
            var c = change.GetNewValue<Color>();
            Engine.BackgroundColor = new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
            _runtime.RenderSession.Invalidate(RenderInvalidationKinds.Style);
        }
        else if (change.Property == CameraInvertXProperty)
        {
            Engine.Camera.InvertX = change.GetNewValue<bool>();
            _runtime.RenderSession.Invalidate(RenderInvalidationKinds.Style);
        }
        else if (change.Property == CameraInvertYProperty)
        {
            Engine.Camera.InvertY = change.GetNewValue<bool>();
            _runtime.RenderSession.Invalidate(RenderInvalidationKinds.Style);
        }
        else if (change.Property == IsGridVisibleProperty ||
                 change.Property == GridHeightProperty ||
                 change.Property == GridColorProperty)
        {
            _runtime.ApplyGridSettings();
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        var scaling = VisualRoot?.RenderScaling ?? 1.0;
        var widthPx = (uint)Math.Max(64, Math.Round(e.NewSize.Width * scaling));
        var heightPx = (uint)Math.Max(64, Math.Round(e.NewSize.Height * scaling));

        Dispatcher.UIThread.Post(() =>
        {
            if (_runtime.WantsNativeBackend() && !_runtime.RenderSession.HandleState.IsBound)
            {
                Log.NativeBackendSizeChangedWithoutHandle(_logger);
                EnsureNativeHost();
                if (!_runtime.RenderSession.HandleState.IsBound)
                    return;
            }

            _runtime.SynchronizeSession(widthPx, heightPx, retryCount: 0, useBackendChangePath: false);
        }, DispatcherPriority.Background);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (!_runtime.RenderSession.IsInitialized)
        {
            context.FillRectangle(new SolidColorBrush(BackgroundColor), new Rect(Bounds.Size));
            return;
        }

        if (!_runtime.RenderSession.IsSoftwareBackend)
            return;

        if (_runtime.RenderSession.Bitmap == null)
        {
            context.FillRectangle(new SolidColorBrush(BackgroundColor), new Rect(Bounds.Size));
            return;
        }

        context.DrawImage(_runtime.RenderSession.Bitmap, new Rect(0, 0, Bounds.Width, Bounds.Height));
        RenderOverlay(context);
    }

    private void OnViewAttached()
    {
        _runtime.OnViewAttached();
    }

    private void OnViewDetached()
    {
        _runtime.OnViewDetached();
    }

    private void EnsureNativeHost()
    {
        if (_nativeHost != null)
            return;

        RuntimeTraceLog.Write("VideraView.EnsureNativeHost");
        var host = _nativeHostFactory.CreateHost();
        if (host == null)
        {
            Log.NativeHostUnavailable(_logger);
            return;
        }

        if (host is not NativeControlHost nativeHost)
        {
            throw new InvalidOperationException("Native host factory must return a NativeControlHost implementation.");
        }

        host.IsHitTestVisible = true;
        host.HandleCreated += OnNativeHandleCreated;
        host.HandleDestroyed += OnNativeHandleDestroyed;
        host.NativePointer += OnNativePointer;

        _nativeHost = nativeHost;
        _inputOverlay = new Border
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        _overlayPresenter = new VideraViewOverlayPresenter();
        _overlayPresenter.UpdateOverlayState(_runtime.OverlayState);
        _inputOverlay.Child = _overlayPresenter;
        _nativeContainer = new Grid();
        _nativeContainer.Children.Add(nativeHost);
        _nativeContainer.Children.Add(_inputOverlay);
        Child = _nativeContainer;
        Log.NativeHostCreated(_logger);
    }

    private void ReleaseNativeHost()
    {
        RuntimeTraceLog.Write("VideraView.ReleaseNativeHost");
        if (_nativeHost is IVideraNativeHost host)
        {
            host.HandleCreated -= OnNativeHandleCreated;
            host.HandleDestroyed -= OnNativeHandleDestroyed;
            host.NativePointer -= OnNativePointer;
        }

        _runtime.OnNativeHandleDestroyed();
        Child = null;
        _nativeHost = null;
        _nativeContainer = null;
        _inputOverlay = null;
        _overlayPresenter = null;
    }

    private void OnNativeHandleCreated(IntPtr handle)
    {
        RuntimeTraceLog.Write($"VideraView.OnNativeHandleCreated handle=0x{handle.ToInt64():X}");
        _runtime.OnNativeHandleCreated(handle);
    }

    private void OnNativeHandleDestroyed()
    {
        RuntimeTraceLog.Write("VideraView.OnNativeHandleDestroyed");
        _runtime.OnNativeHandleDestroyed();
    }

    private void RaiseSelectionRequested(SelectionRequestedEventArgs e)
    {
        SelectionRequested?.Invoke(this, e);
    }

    private void RaiseAnnotationRequested(AnnotationRequestedEventArgs e)
    {
        AnnotationRequested?.Invoke(this, e);
    }

    internal bool IsPreferredBackendOverrideSetForRuntime() => IsSet(PreferredBackendProperty);

    internal double ResolveRenderScalingForRuntime() => VisualRoot?.RenderScaling ?? 1.0;

    internal (string? ResolvedDisplayServer, bool FallbackUsed, string? FallbackReason) GetNativeHostDisplayServerDiagnosticsForRuntime()
    {
        if (_nativeHost is IVideraNativeHost host)
        {
            return (host.ResolvedDisplayServer, host.DisplayServerFallbackUsed, host.DisplayServerFallbackReason);
        }

        return (null, false, null);
    }

    internal void EnsureNativeHostForRuntime() => EnsureNativeHost();

    internal void ReleaseNativeHostForRuntime() => ReleaseNativeHost();

    internal void UpdateOverlayPresentationFromRuntime(VideraViewOverlayState overlayState)
    {
        _overlayPresenter?.UpdateOverlayState(overlayState);
    }

    internal void InvalidateVisualsFromRuntime()
    {
        InvalidateVisual();
        _overlayPresenter?.InvalidateVisual();
    }

    internal void PublishBackendDiagnosticsFromRuntime(VideraBackendDiagnostics diagnostics)
    {
        BackendStatusChanged?.Invoke(this, new VideraBackendStatusChangedEventArgs(diagnostics));
    }

    internal void RaiseBackendReadyFromRuntime()
    {
        BackendReady?.Invoke(this, EventArgs.Empty);
    }

    internal void RaiseInitializationFailedFromRuntime(VideraBackendDiagnostics diagnostics, Exception ex)
    {
        InitializationFailed?.Invoke(this, new VideraBackendFailureEventArgs(diagnostics, ex));
    }

    private void RefreshBackendDiagnostics(string? lastInitializationError) => _runtime.RefreshBackendDiagnostics(lastInitializationError);

    internal static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "OnSizeChanged: native backend without handle, ensuring host")]
        public static partial void NativeBackendSizeChangedWithoutHandle(ILogger logger);

        [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Render session init failed (Try #{RetryCount}): {Error}")]
        public static partial void RenderSessionInitFailed(ILogger logger, int retryCount, string error, Exception exception);

        [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "OnViewAttached")]
        public static partial void ViewAttached(ILogger logger);

        [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "OnViewDetached")]
        public static partial void ViewDetached(ILogger logger);

        [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "No native host available for this platform")]
        public static partial void NativeHostUnavailable(ILogger logger);

        [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Native host created")]
        public static partial void NativeHostCreated(ILogger logger);

        [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Native handle created: 0x{Handle:X}")]
        public static partial void NativeHandleCreated(ILogger logger, long handle);
    }
}
