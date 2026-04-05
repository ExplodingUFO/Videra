using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Videra.Avalonia.Composition;
using Videra.Avalonia.Rendering;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
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
    private readonly INativeHostFactory _nativeHostFactory;
    private NativeControlHost? _nativeHost;
    private Grid? _nativeContainer;
    private Border? _inputOverlay;
    private readonly ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<VideraView>();

    public event EventHandler? BackendReady;

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

    internal VideraView(INativeHostFactory? nativeHostFactory)
    {
        AvaloniaGraphicsBackendResolver.EnsureRegistered();
        Engine = new VideraEngine();
        _nativeHostFactory = nativeHostFactory ?? new DefaultNativeHostFactory();
        _renderSession = CreateRenderSession();
        Focusable = true;
        ClipToBounds = true;
    }

    public VideraEngine Engine { get; }

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
        else if (change.Property == RenderStyleProperty)
        {
            Engine.StyleService.ApplyPreset(change.GetNewValue<RenderStylePreset>());
        }
        else if (change.Property == RenderStyleParametersProperty)
        {
            var parameters = change.GetNewValue<RenderStyleParameters?>();
            if (parameters != null)
            {
                Engine.StyleService.UpdateParameters(parameters);
            }
        }
        else if (change.Property == WireframeModeProperty)
        {
            Engine.Wireframe.Mode = change.GetNewValue<WireframeMode>();
        }
        else if (change.Property == WireframeColorProperty)
        {
            var c = change.GetNewValue<Color>();
            Engine.Wireframe.LineColor = new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
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
            if (WantsNativeBackend() && !_renderSession.HandleState.IsBound)
            {
                Log.NativeBackendSizeChangedWithoutHandle(_logger);
                EnsureNativeHost();
                if (!_renderSession.HandleState.IsBound)
                    return;
            }

            ApplyRenderSessionSize(widthPx, heightPx, retryCount: 0);
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
    }

    private RenderSession CreateRenderSession()
    {
        var session = new RenderSession(
            Engine,
            requestRender: InvalidateVisual,
            logger: _logger,
            renderLoopFactory: static () => new RenderSession.DispatcherRenderLoopDriver());
        session.BackendReady += OnRenderSessionBackendReady;
        return session;
    }

    private void OnRenderSessionBackendReady(object? sender, EventArgs e)
    {
        BackendReady?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyRenderSessionSize(uint widthPx, uint heightPx, int retryCount)
    {
        if (widthPx == 0 || heightPx == 0)
            return;

        try
        {
            var wasReady = _renderSession.IsReady;
            _renderSession.Attach(PreferredBackend);
            _renderSession.Resize(widthPx, heightPx, (float)(VisualRoot?.RenderScaling ?? 1.0));

            if (!wasReady && _renderSession.IsReady)
            {
                ApplyViewState();
            }
        }
        catch (Exception ex)
        {
            Log.RenderSessionInitFailed(_logger, retryCount + 1, ex.Message, ex);

            if (retryCount < 5)
            {
                Task.Delay(100).ContinueWith(_ =>
                {
                    Dispatcher.UIThread.Post(() => ApplyRenderSessionSize(widthPx, heightPx, retryCount + 1));
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
        _renderSession.Attach(PreferredBackend);

        if (WantsNativeBackend())
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
        ApplyRenderSessionSize(widthPx, heightPx, retryCount: 0);
    }

    private void OnViewDetached()
    {
        Log.ViewDetached(_logger);
        if (Items is INotifyCollectionChanged incc)
            incc.CollectionChanged -= OnCollectionChanged;

        _renderSession.Dispose();
        _renderSession = CreateRenderSession();
        ReleaseNativeHost();
    }

    private bool WantsNativeBackend()
    {
        if (PreferredBackend == GraphicsBackendPreference.Software)
            return false;

        if (PreferredBackend == GraphicsBackendPreference.Auto)
        {
            var backendMode = Environment.GetEnvironmentVariable("VIDERA_BACKEND");
            if (string.Equals(backendMode, "software", StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS();
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

        _renderSession.BindHandle(IntPtr.Zero);
        Child = null;
        _nativeHost = null;
        _nativeContainer = null;
        _inputOverlay = null;
    }

    private void OnNativeHandleCreated(IntPtr handle)
    {
        _renderSession.BindHandle(handle);
        Log.NativeHandleCreated(_logger, handle.ToInt64());

        var scaling = VisualRoot?.RenderScaling ?? 1.0;
        var widthPx = (uint)Math.Max(64, Math.Round(Bounds.Width * scaling));
        var heightPx = (uint)Math.Max(64, Math.Round(Bounds.Height * scaling));
        ApplyRenderSessionSize(widthPx, heightPx, retryCount: 0);
    }

    private void OnNativeHandleDestroyed()
    {
        _renderSession.BindHandle(IntPtr.Zero);
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
