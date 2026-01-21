using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
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
    private IGraphicsBackend? _backend;
    private bool _isInitialized;
    private bool _isReady;
    private uint _width;
    private uint _height;
    private WriteableBitmap? _bitmap;
    private bool _isSoftwareBackend;
    private DispatcherTimer? _renderTimer;
    private NativeControlHost? _nativeHost;
    private Grid? _nativeContainer;
    private Border? _inputOverlay;
    private IntPtr _renderHandle;
    private int _renderTickCount;

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
    {
        Engine = new VideraEngine();
        Focusable = true;
        ClipToBounds = true;
    }

    public VideraEngine Engine { get; }

    public IResourceFactory? GetResourceFactory()
    {
        return _backend?.GetResourceFactory();
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

        if (!_isReady)
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

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        var scaling = VisualRoot?.RenderScaling ?? 1.0;
        var widthPx = (uint)Math.Max(64, Math.Round(e.NewSize.Width * scaling));
        var heightPx = (uint)Math.Max(64, Math.Round(e.NewSize.Height * scaling));
        Engine.RenderScale = (float)scaling;

        Dispatcher.UIThread.Post(() =>
        {
            if (WantsNativeBackend() && _renderHandle == IntPtr.Zero)
            {
                Console.WriteLine("[VideraView] OnSizeChanged: native backend without handle, ensuring host");
                EnsureNativeHost();
                if (_renderHandle == IntPtr.Zero)
                    return;
            }
            TryInitializeOrResize(widthPx, heightPx);
        }, DispatcherPriority.Background);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (!_isInitialized)
        {
            context.FillRectangle(new SolidColorBrush(BackgroundColor), new Rect(Bounds.Size));
            return;
        }

        if (!_isSoftwareBackend)
            return;

        if (_bitmap == null)
        {
            context.FillRectangle(new SolidColorBrush(BackgroundColor), new Rect(Bounds.Size));
            return;
        }

        context.DrawImage(_bitmap, new Rect(0, 0, Bounds.Width, Bounds.Height));
    }

    private void TryInitializeOrResize(uint widthPx, uint heightPx, int retryCount = 0)
    {
        if (widthPx == 0 || heightPx == 0)
            return;

        if (!_isInitialized)
        {
            try
            {
                InitializeGraphicsDevice(widthPx, heightPx);
                _isInitialized = true;
                _isReady = true;
                BackendReady?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Videra] Init Failed (Try #{retryCount + 1}): {ex.Message}");

                if (retryCount < 5)
                {
                    Task.Delay(100).ContinueWith(_ =>
                    {
                        Dispatcher.UIThread.Post(() => TryInitializeOrResize(widthPx, heightPx, retryCount + 1));
                    });
                }
            }
        }
        else
        {
            if (_backend != null && (widthPx != _width || heightPx != _height))
            {
                _backend.Resize((int)widthPx, (int)heightPx);
                Engine.Resize(widthPx, heightPx);
                EnsureBitmap(widthPx, heightPx);
                _width = widthPx;
                _height = heightPx;
            }
        }
    }

    private void InitializeGraphicsDevice(uint widthPx, uint heightPx)
    {
        Console.WriteLine($"[VideraView] InitializeGraphicsDevice {widthPx}x{heightPx}, PreferredBackend={PreferredBackend}");
        _backend = GraphicsBackendFactory.CreateBackend(PreferredBackend);
        _isSoftwareBackend = _backend is ISoftwareBackend;
        Console.WriteLine($"[VideraView] Backend={_backend.GetType().Name}, IsSoftware={_isSoftwareBackend}");
        if (_isSoftwareBackend)
        {
            _renderHandle = IntPtr.Zero;
            ReleaseNativeHost();
        }

        var topLevel = TopLevel.GetTopLevel(this);
        var handle = _isSoftwareBackend
            ? topLevel?.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero
            : _renderHandle;
        Console.WriteLine($"[VideraView] Init handle=0x{handle.ToInt64():X}, TopLevel={(topLevel != null)}");
        //System.Diagnostics.Debug.WriteLine($"Current handle is {handle}");
        _backend.Initialize(handle, (int)widthPx, (int)heightPx);

        //_backend.Initialize(IntPtr.Zero, (int)widthPx, (int)heightPx);

        Engine.Initialize(_backend);
        Engine.Resize(widthPx, heightPx);

        var frameLogEnv = Environment.GetEnvironmentVariable("VIDERA_FRAMELOG");
        Engine.EnableFrameLogging = string.Equals(frameLogEnv, "1", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(frameLogEnv, "true", StringComparison.OrdinalIgnoreCase);
        if (Engine.EnableFrameLogging)
            Console.WriteLine("[VideraView] Frame logging enabled");

        var color = BackgroundColor;
        Engine.BackgroundColor = new RgbaFloat(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        Engine.Camera.InvertX = CameraInvertX;
        Engine.Camera.InvertY = CameraInvertY;

        _width = widthPx;
        _height = heightPx;

        EnsureBitmap(widthPx, heightPx);
        ApplyGridSettings();
        UpdateItemsSubscription(null, Items);
        StartRenderLoop();
    }

    private void EnsureBitmap(uint widthPx, uint heightPx)
    {
        if (widthPx == 0 || heightPx == 0)
            return;

        if (!_isSoftwareBackend)
        {
            _bitmap?.Dispose();
            _bitmap = null;
            return;
        }

        if (_bitmap != null &&
            _bitmap.PixelSize.Width == widthPx &&
            _bitmap.PixelSize.Height == heightPx)
        {
            return;
        }

        _bitmap?.Dispose();
        _bitmap = new WriteableBitmap(
            new PixelSize((int)widthPx, (int)heightPx),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);
    }

    private void StartRenderLoop()
    {
        if (_renderTimer != null)
            return;

        _renderTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _renderTimer.Tick += OnRenderTick;
        _renderTimer.Start();
        Console.WriteLine("[VideraView] Render loop started");
    }

    private void StopRenderLoop()
    {
        if (_renderTimer == null)
        {
            return;
        }

        _renderTimer.Stop();
        _renderTimer.Tick -= OnRenderTick;
        _renderTimer = null;
    }

    private void OnRenderTick(object? sender, EventArgs e)
    {
        if (_renderTickCount == 0)
            Console.WriteLine("[VideraView] First render tick");
        _renderTickCount++;
        RenderFrame();
    }

    private void RenderFrame()
    {
        if (!_isInitialized || _backend == null || !Engine.IsInitialized)
            return;

        try
        {
            Engine.Draw();

            if (_isSoftwareBackend && _backend is ISoftwareBackend softwareBackend)
            {
                if (_bitmap != null)
                {
                    using var locked = _bitmap.Lock();
                    softwareBackend.CopyFrameTo(locked.Address, locked.RowBytes);
                }

                InvalidateVisual();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Videra] Render error: {ex.Message}");
            StopRenderLoop();
        }
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
        Console.WriteLine("[VideraView] OnViewAttached");
        if (WantsNativeBackend())
        {
            Console.WriteLine("[VideraView] Wants native backend, ensuring native host");
            EnsureNativeHost();
        }

        if (Bounds.Width <= 0 || Bounds.Height <= 0)
            return;

        var scaling = VisualRoot?.RenderScaling ?? 1.0;
        var widthPx = (uint)Math.Max(64, Math.Round(Bounds.Width * scaling));
        var heightPx = (uint)Math.Max(64, Math.Round(Bounds.Height * scaling));
        Engine.RenderScale = (float)scaling;

        if (WantsNativeBackend())
        {
            if (_renderHandle != IntPtr.Zero)
                TryInitializeOrResize(widthPx, heightPx);
        }
        else
        {
            _renderHandle = IntPtr.Zero;
            TryInitializeOrResize(widthPx, heightPx);
        }
    }

    private void OnViewDetached()
    {
        Console.WriteLine("[VideraView] OnViewDetached");
        StopRenderLoop();
        if (Items is INotifyCollectionChanged incc)
            incc.CollectionChanged -= OnCollectionChanged;
        Engine.Dispose();
        _backend = null;
        _isSoftwareBackend = false;
        ReleaseNativeHost();
        _bitmap?.Dispose();
        _bitmap = null;
        _isReady = false;
        _isInitialized = false;
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

        IVideraNativeHost host;
        if (OperatingSystem.IsWindows())
        {
            var winHost = new VideraNativeHost { IsHitTestVisible = true };
            host = winHost;
            _nativeHost = winHost;
        }
        else if (OperatingSystem.IsLinux())
        {
            var linuxHost = new VideraLinuxNativeHost { IsHitTestVisible = true };
            host = linuxHost;
            _nativeHost = linuxHost;
        }
        else if (OperatingSystem.IsMacOS())
        {
            var macHost = new VideraMacOSNativeHost { IsHitTestVisible = true };
            host = macHost;
            _nativeHost = macHost;
        }
        else
        {
            Console.WriteLine("[VideraView] No native host available for this platform");
            return;
        }

        host.HandleCreated += OnNativeHandleCreated;
        host.HandleDestroyed += OnNativeHandleDestroyed;
        host.NativePointer += OnNativePointer;
        var overlay = new Border
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = false
        };
        _inputOverlay = overlay;
        _nativeContainer = new Grid();
        _nativeContainer.Children.Add(_nativeHost);
        _nativeContainer.Children.Add(overlay);
        Child = _nativeContainer;
        Console.WriteLine("[VideraView] Native host created");
    }

    private void ReleaseNativeHost()
    {
        if (_nativeHost == null)
            return;

        if (_nativeHost is IVideraNativeHost host)
        {
            host.HandleCreated -= OnNativeHandleCreated;
            host.HandleDestroyed -= OnNativeHandleDestroyed;
            host.NativePointer -= OnNativePointer;
        }
        Child = null;
        _nativeHost = null;
        _nativeContainer = null;
        _inputOverlay = null;
        _renderHandle = IntPtr.Zero;
    }

    private void OnNativeHandleCreated(IntPtr handle)
    {
        _renderHandle = handle;
        Console.WriteLine($"[VideraView] Native handle created: 0x{handle.ToInt64():X}");

        var scaling = VisualRoot?.RenderScaling ?? 1.0;
        var widthPx = (uint)Math.Max(64, Math.Round(Bounds.Width * scaling));
        var heightPx = (uint)Math.Max(64, Math.Round(Bounds.Height * scaling));
        Engine.RenderScale = (float)scaling;
        TryInitializeOrResize(widthPx, heightPx);
    }

    private void OnNativeHandleDestroyed()
    {
        _renderHandle = IntPtr.Zero;
    }
}
