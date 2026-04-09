using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Videra.Avalonia.Controls.Interaction;
using Videra.Avalonia.Composition;
using Videra.Avalonia.Rendering;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.RenderPipeline.Extensibility;
using Videra.Core.Graphics.Wireframe;
using Videra.Core.Styles.Parameters;
using Videra.Core.Styles.Presets;

namespace Videra.Avalonia.Controls;

/// <summary>
/// 3D 渲染视图控件 - 使用跨平台软件后端渲染。
/// </summary>
public partial class VideraView : Decorator
{
    private RenderSession _renderSession;
    private VideraViewSessionBridge _sessionBridge;
    private VideraViewOptions _options = new();
    private VideraBackendDiagnostics _backendDiagnostics;
    private VideraSelectionState _selectionState = new();
    private IReadOnlyList<VideraAnnotation> _annotations = Array.Empty<VideraAnnotation>();
    private VideraViewOverlayState _overlayState = VideraViewOverlayState.Empty;
    private VideraInteractionOptions _interactionOptions = new();
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
        _renderSession = CreateRenderSession();
        _sessionBridge = CreateSessionBridge();
        InitializeInteractionController();
        SubscribeToOptions(_options);
        _backendDiagnostics = _sessionBridge.CreateDiagnosticsSnapshot(lastInitializationError: null);
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
        get => _options;
        set
        {
            UnsubscribeFromOptions(_options);
            _options = value ?? new VideraViewOptions();
            SubscribeToOptions(_options);
            RefreshBackendDiagnostics(lastInitializationError: _backendDiagnostics.LastInitializationError);
            SynchronizeSessionFromCurrentBounds(retryCount: 0, useBackendChangePath: true);
        }
    }

    /// <summary>
    /// Gets the public availability and fallback shell for the current session state.
    /// </summary>
    /// <remarks>
    /// The snapshot mirrors backend readiness, <see cref="VideraBackendDiagnostics.IsUsingSoftwareFallback" />,
    /// and <see cref="VideraBackendDiagnostics.FallbackReason" /> so callers can inspect unavailable or
    /// degraded backend resolution without reaching into internal session state.
    /// </remarks>
    public VideraBackendDiagnostics BackendDiagnostics => _backendDiagnostics;

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
        get => _selectionState;
        set
        {
            _selectionState = value ?? new VideraSelectionState();
            SynchronizeOverlayState();
        }
    }

    /// <summary>
    /// Gets or sets the host-owned annotations to display for the current scene.
    /// </summary>
    public IReadOnlyList<VideraAnnotation> Annotations
    {
        get => _annotations;
        set
        {
            _annotations = value ?? Array.Empty<VideraAnnotation>();
            SynchronizeOverlayState();
        }
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
        get => _interactionOptions;
        set => _interactionOptions = value ?? new VideraInteractionOptions();
    }

    public IResourceFactory? GetResourceFactory()
    {
        return _renderSession.ResourceFactory;
    }

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
            RefreshBackendDiagnostics(lastInitializationError: _backendDiagnostics.LastInitializationError);
            SynchronizeSessionFromCurrentBounds(retryCount: 0, useBackendChangePath: true);
            return;
        }

        if (change.Property == RenderStyleProperty)
        {
            Engine.StyleService.ApplyPreset(change.GetNewValue<RenderStylePreset>());
            return;
        }

        if (change.Property == RenderStyleParametersProperty)
        {
            var parameters = change.GetNewValue<RenderStyleParameters?>();
            if (parameters != null)
            {
                Engine.StyleService.UpdateParameters(parameters);
            }

            return;
        }

        if (change.Property == WireframeModeProperty)
        {
            Engine.Wireframe.Mode = change.GetNewValue<WireframeMode>();
            return;
        }

        if (change.Property == WireframeColorProperty)
        {
            var c = change.GetNewValue<Color>();
            Engine.Wireframe.LineColor = new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
            return;
        }

        if (!_renderSession.IsReady)
            return;

        if (change.Property == BackgroundColorProperty)
        {
            var c = change.GetNewValue<Color>();
            Engine.BackgroundColor = new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }
        else if (change.Property == CameraInvertXProperty)
        {
            Engine.Camera.InvertX = change.GetNewValue<bool>();
        }
        else if (change.Property == CameraInvertYProperty)
        {
            Engine.Camera.InvertY = change.GetNewValue<bool>();
        }
        else if (change.Property == ItemsProperty)
        {
            var oldList = change.GetOldValue<IEnumerable>();
            var newList = change.GetNewValue<IEnumerable>();
            UpdateItemsSubscription(oldList, newList);
        }
        else if (change.Property == IsGridVisibleProperty ||
                 change.Property == GridHeightProperty ||
                 change.Property == GridColorProperty)
        {
            ApplyGridSettings();
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
            if (_sessionBridge.WantsNativeBackend() && !_renderSession.HandleState.IsBound)
            {
                Log.NativeBackendSizeChangedWithoutHandle(_logger);
                EnsureNativeHost();
                if (!_renderSession.HandleState.IsBound)
                    return;
            }

            SynchronizeSession(widthPx, heightPx, retryCount: 0, useBackendChangePath: false);
        }, DispatcherPriority.Background);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (!_renderSession.IsInitialized)
        {
            context.FillRectangle(new SolidColorBrush(BackgroundColor), new Rect(Bounds.Size));
            return;
        }

        if (!_renderSession.IsSoftwareBackend)
            return;

        if (_renderSession.Bitmap == null)
        {
            context.FillRectangle(new SolidColorBrush(BackgroundColor), new Rect(Bounds.Size));
            return;
        }

        context.DrawImage(_renderSession.Bitmap, new Rect(0, 0, Bounds.Width, Bounds.Height));
        RenderOverlay(context);
    }

    private RenderSession CreateRenderSession()
    {
        var session = new RenderSession(
            Engine,
            beforeRender: PushOverlayRenderState,
            requestRender: OnRenderSessionFrameRequested,
            logger: _logger,
            renderLoopFactory: static () => new RenderSession.DispatcherRenderLoopDriver(),
            bitmapFactory: _bitmapFactory);
        session.BackendReady += OnRenderSessionBackendReady;
        return session;
    }

    private VideraViewSessionBridge CreateSessionBridge()
    {
        return new VideraViewSessionBridge(
            _renderSession,
            isPreferredBackendOverrideSet: () => IsSet(PreferredBackendProperty),
            preferredBackendValue: () => PreferredBackend,
            backendOptionsAccessor: () => _options.Backend,
            diagnosticsOptionsAccessor: () => _options.Diagnostics);
    }

    private void OnRenderSessionBackendReady(object? sender, EventArgs e)
    {
        SynchronizeOverlayState();
        RefreshBackendDiagnostics(lastInitializationError: null);
        BackendReady?.Invoke(this, EventArgs.Empty);
    }

    private void SynchronizeSessionFromCurrentBounds(int retryCount, bool useBackendChangePath)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        var scaling = VisualRoot?.RenderScaling ?? 1.0;
        var widthPx = (uint)Math.Max(64, Math.Round(Bounds.Width * scaling));
        var heightPx = (uint)Math.Max(64, Math.Round(Bounds.Height * scaling));
        SynchronizeSession(widthPx, heightPx, retryCount, useBackendChangePath);
    }

    private void SynchronizeSession(uint widthPx, uint heightPx, int retryCount, bool useBackendChangePath)
    {
        if (widthPx == 0 || heightPx == 0)
        {
            return;
        }

        try
        {
            var scaling = (float)(VisualRoot?.RenderScaling ?? 1.0);
            var becameReady = useBackendChangePath
                ? _sessionBridge.OnBackendOptionsChanged(widthPx, heightPx, scaling)
                : _sessionBridge.OnSizeChanged(widthPx, heightPx, scaling);
            RefreshBackendDiagnostics(lastInitializationError: null);

            if (becameReady)
            {
                ApplyViewState();
            }

            SynchronizeOverlayPresentation();
        }
        catch (Exception ex)
        {
            Log.RenderSessionInitFailed(_logger, retryCount + 1, ex.Message, ex);
            RefreshBackendDiagnostics(lastInitializationError: ex.Message);
            InitializationFailed?.Invoke(this, new VideraBackendFailureEventArgs(_backendDiagnostics, ex));

            if (retryCount < 5)
            {
                Task.Delay(100).ContinueWith(_ =>
                {
                    Dispatcher.UIThread.Post(() => SynchronizeSession(widthPx, heightPx, retryCount + 1, useBackendChangePath));
                });
            }
        }
    }

    private void ApplyViewState()
    {
        var color = BackgroundColor;
        Engine.BackgroundColor = new RgbaFloat(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        Engine.Camera.InvertX = CameraInvertX;
        Engine.Camera.InvertY = CameraInvertY;
        ApplyGridSettings();
        UpdateItemsSubscription(Items, Items);
        SynchronizeOverlayState();
    }

    private void UpdateItemsSubscription(IEnumerable? oldList, IEnumerable? newList)
    {
        if (oldList is INotifyCollectionChanged oldIncc)
            oldIncc.CollectionChanged -= OnCollectionChanged;

        if (newList is INotifyCollectionChanged newIncc)
            newIncc.CollectionChanged += OnCollectionChanged;

        Engine.ClearObjects();
        if (newList != null)
        {
            foreach (var item in newList)
                if (item is Object3D obj)
                    Engine.AddObject(obj);
        }

        SynchronizeOverlayPresentation();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            foreach (Object3D item in e.NewItems)
                Engine.AddObject(item);
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            foreach (Object3D item in e.OldItems)
                Engine.RemoveObject(item);
        else if (e.Action == NotifyCollectionChangedAction.Reset)
            Engine.ClearObjects();

        SynchronizeOverlayPresentation();
    }

    private void ApplyGridSettings()
    {
        Engine.Grid.IsVisible = IsGridVisible;
        Engine.Grid.Height = GridHeight;
        var c = GridColor;
        Engine.Grid.GridColor = new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        Engine.Grid.Rebuild(GetResourceFactory());
    }

    private void OnViewAttached()
    {
        Log.ViewAttached(_logger);
        var becameReady = _sessionBridge.OnViewAttached();
        RefreshBackendDiagnostics(lastInitializationError: null);

        if (becameReady)
        {
            ApplyViewState();
        }

        if (_sessionBridge.WantsNativeBackend())
        {
            EnsureNativeHost();
        }
        else
        {
            ReleaseNativeHost();
        }

        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var scaling = VisualRoot?.RenderScaling ?? 1.0;
        var widthPx = (uint)Math.Max(64, Math.Round(Bounds.Width * scaling));
        var heightPx = (uint)Math.Max(64, Math.Round(Bounds.Height * scaling));
        SynchronizeSession(widthPx, heightPx, retryCount: 0, useBackendChangePath: false);
    }

    private void OnViewDetached()
    {
        Log.ViewDetached(_logger);
        if (Items is INotifyCollectionChanged incc)
            incc.CollectionChanged -= OnCollectionChanged;

        ReleaseNativeHost();
        _sessionBridge.OnViewDetached();
        _renderSession = CreateRenderSession();
        _sessionBridge = CreateSessionBridge();
        RefreshBackendDiagnostics(lastInitializationError: null);
    }

    private void EnsureNativeHost()
    {
        if (_nativeHost != null)
            return;

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
        _overlayPresenter.UpdateOverlayState(_overlayState);
        _inputOverlay.Child = _overlayPresenter;
        _nativeContainer = new Grid();
        _nativeContainer.Children.Add(nativeHost);
        _nativeContainer.Children.Add(_inputOverlay);
        Child = _nativeContainer;
        Log.NativeHostCreated(_logger);
    }

    private void ReleaseNativeHost()
    {
        if (_nativeHost is IVideraNativeHost host)
        {
            host.HandleCreated -= OnNativeHandleCreated;
            host.HandleDestroyed -= OnNativeHandleDestroyed;
            host.NativePointer -= OnNativePointer;
        }

        _sessionBridge.OnNativeHandleDestroyed();
        Child = null;
        _nativeHost = null;
        _nativeContainer = null;
        _inputOverlay = null;
        _overlayPresenter = null;
        RefreshBackendDiagnostics(lastInitializationError: _backendDiagnostics.LastInitializationError);
    }

    private void OnNativeHandleCreated(IntPtr handle)
    {
        string? resolvedDisplayServer = null;
        var fallbackUsed = false;
        string? fallbackReason = null;
        if (_nativeHost is IVideraNativeHost host)
        {
            resolvedDisplayServer = host.ResolvedDisplayServer;
            fallbackUsed = host.DisplayServerFallbackUsed;
            fallbackReason = host.DisplayServerFallbackReason;
        }

        Log.NativeHandleCreated(_logger, handle.ToInt64());

        var scaling = VisualRoot?.RenderScaling ?? 1.0;
        var widthPx = (uint)Math.Max(64, Math.Round(Bounds.Width * scaling));
        var heightPx = (uint)Math.Max(64, Math.Round(Bounds.Height * scaling));
        var becameReady = _sessionBridge.OnNativeHandleCreated(
            handle,
            resolvedDisplayServer,
            fallbackUsed,
            fallbackReason,
            widthPx,
            heightPx,
            (float)scaling);
        RefreshBackendDiagnostics(lastInitializationError: null);
        if (becameReady)
        {
            ApplyViewState();
        }
    }

    private void OnNativeHandleDestroyed()
    {
        _sessionBridge.OnNativeHandleDestroyed();
        RefreshBackendDiagnostics(lastInitializationError: _backendDiagnostics.LastInitializationError);
    }

    private void SubscribeToOptions(VideraViewOptions options)
    {
        options.PropertyChanged += OnOptionsPropertyChanged;
        options.Backend.PropertyChanged += OnBackendOptionsPropertyChanged;
        options.Diagnostics.PropertyChanged += OnDiagnosticsOptionsPropertyChanged;
    }

    private void UnsubscribeFromOptions(VideraViewOptions options)
    {
        options.PropertyChanged -= OnOptionsPropertyChanged;
        options.Backend.PropertyChanged -= OnBackendOptionsPropertyChanged;
        options.Diagnostics.PropertyChanged -= OnDiagnosticsOptionsPropertyChanged;
    }

    private void OnOptionsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not VideraViewOptions options)
        {
            return;
        }

        if (e.PropertyName == nameof(VideraViewOptions.Backend))
        {
            RefreshBackendDiagnostics(lastInitializationError: _backendDiagnostics.LastInitializationError);
            SynchronizeSessionFromCurrentBounds(retryCount: 0, useBackendChangePath: true);
        }
        else if (e.PropertyName == nameof(VideraViewOptions.Diagnostics))
        {
            RefreshBackendDiagnostics(lastInitializationError: _backendDiagnostics.LastInitializationError);
        }

        if (options.Backend != null)
        {
            options.Backend.PropertyChanged -= OnBackendOptionsPropertyChanged;
            options.Backend.PropertyChanged += OnBackendOptionsPropertyChanged;
        }

        if (options.Diagnostics != null)
        {
            options.Diagnostics.PropertyChanged -= OnDiagnosticsOptionsPropertyChanged;
            options.Diagnostics.PropertyChanged += OnDiagnosticsOptionsPropertyChanged;
        }
    }

    private void OnBackendOptionsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _ = sender;
        _ = e;
        RefreshBackendDiagnostics(lastInitializationError: _backendDiagnostics.LastInitializationError);
        SynchronizeSessionFromCurrentBounds(retryCount: 0, useBackendChangePath: true);
    }

    private void OnDiagnosticsOptionsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _ = sender;
        _ = e;
        RefreshBackendDiagnostics(lastInitializationError: _backendDiagnostics.LastInitializationError);
    }

    private void RefreshBackendDiagnostics(string? lastInitializationError)
    {
        var next = _sessionBridge.CreateDiagnosticsSnapshot(lastInitializationError);
        _backendDiagnostics = next;
        BackendStatusChanged?.Invoke(this, new VideraBackendStatusChangedEventArgs(next));
    }

    private void RaiseSelectionRequested(SelectionRequestedEventArgs e)
    {
        SelectionRequested?.Invoke(this, e);
    }

    private void RaiseAnnotationRequested(AnnotationRequestedEventArgs e)
    {
        AnnotationRequested?.Invoke(this, e);
    }

    private void OnRenderSessionFrameRequested()
    {
        SynchronizeOverlayPresentation();
        InvalidateVisual();
        _overlayPresenter?.InvalidateVisual();
    }

    private static partial class Log
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
