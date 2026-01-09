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

namespace Videra.Avalonia.Controls;

/// <summary>
/// 3D 渲染视图控件 - 使用跨平台软件后端渲染。
/// </summary>
public partial class VideraViewNew : Decorator
{
    private IGraphicsBackend? _backend;
    private bool _isInitialized;
    private bool _isReady;
    private uint _width;
    private uint _height;
    private WriteableBitmap? _bitmap;
    private bool _isSoftwareBackend;
    private DispatcherTimer? _renderTimer;
    private VideraNativeHost? _nativeHost;
    private Grid? _nativeContainer;
    private Border? _inputOverlay;
    private IntPtr _renderHandle;
    private int _renderTickCount;

    public event EventHandler? BackendReady;

    public static readonly StyledProperty<Color> BackgroundColorProperty =
        AvaloniaProperty.Register<VideraViewNew, Color>(nameof(BackgroundColor), Colors.Black);

    public static readonly StyledProperty<IEnumerable> ItemsProperty =
        AvaloniaProperty.Register<VideraViewNew, IEnumerable>(nameof(Items));

    public static readonly StyledProperty<bool> CameraInvertXProperty =
        AvaloniaProperty.Register<VideraViewNew, bool>(nameof(CameraInvertX));

    public static readonly StyledProperty<bool> CameraInvertYProperty =
        AvaloniaProperty.Register<VideraViewNew, bool>(nameof(CameraInvertY));

    public static readonly StyledProperty<bool> IsGridVisibleProperty =
        AvaloniaProperty.Register<VideraViewNew, bool>(nameof(IsGridVisible), true);

    public static readonly StyledProperty<float> GridHeightProperty =
        AvaloniaProperty.Register<VideraViewNew, float>(nameof(GridHeight), 0f);

    public static readonly StyledProperty<Color> GridColorProperty =
        AvaloniaProperty.Register<VideraViewNew, Color>(nameof(GridColor), Colors.Gray);

    public static readonly StyledProperty<GraphicsBackendPreference> PreferredBackendProperty =
        AvaloniaProperty.Register<VideraViewNew, GraphicsBackendPreference>(
            nameof(PreferredBackend),
            GraphicsBackendPreference.Auto);

    public VideraViewNew()
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
                Console.WriteLine("[VideraViewNew] OnSizeChanged: native backend without handle, ensuring host");
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
        Console.WriteLine($"[VideraViewNew] InitializeGraphicsDevice {widthPx}x{heightPx}, PreferredBackend={PreferredBackend}");
        _backend = GraphicsBackendFactory.CreateBackend(PreferredBackend);
        _isSoftwareBackend = _backend is ISoftwareBackend;
        Console.WriteLine($"[VideraViewNew] Backend={_backend.GetType().Name}, IsSoftware={_isSoftwareBackend}");
        if (_isSoftwareBackend)
        {
            _renderHandle = IntPtr.Zero;
            ReleaseNativeHost();
        }

        var topLevel = TopLevel.GetTopLevel(this);
        var handle = _isSoftwareBackend
            ? topLevel?.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero
            : _renderHandle;
        Console.WriteLine($"[VideraViewNew] Init handle=0x{handle.ToInt64():X}, TopLevel={(topLevel != null)}");
        //System.Diagnostics.Debug.WriteLine($"Current handle is {handle}");
        _backend.Initialize(handle, (int)widthPx, (int)heightPx);

        //_backend.Initialize(IntPtr.Zero, (int)widthPx, (int)heightPx);

        Engine.Initialize(_backend);
        Engine.Resize(widthPx, heightPx);

        var frameLogEnv = Environment.GetEnvironmentVariable("VIDERA_FRAMELOG");
        Engine.EnableFrameLogging = string.Equals(frameLogEnv, "1", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(frameLogEnv, "true", StringComparison.OrdinalIgnoreCase);
        if (Engine.EnableFrameLogging)
            Console.WriteLine("[VideraViewNew] Frame logging enabled");

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
        Console.WriteLine("[VideraViewNew] Render loop started");
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
            Console.WriteLine("[VideraViewNew] First render tick");
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
        Console.WriteLine("[VideraViewNew] OnViewAttached");
        if (WantsNativeBackend())
        {
            Console.WriteLine("[VideraViewNew] Wants native backend, ensuring native host");
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
        Console.WriteLine("[VideraViewNew] OnViewDetached");
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

        return OperatingSystem.IsWindows();
    }

    private void EnsureNativeHost()
    {
        if (_nativeHost != null)
            return;

        var host = new VideraNativeHost
        {
            IsHitTestVisible = false
        };
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
        _nativeContainer.Children.Add(host);
        _nativeContainer.Children.Add(overlay);
        _nativeHost = host;
        Child = _nativeContainer;
        Console.WriteLine("[VideraViewNew] Native host created");
    }

    private void ReleaseNativeHost()
    {
        if (_nativeHost == null)
            return;

        _nativeHost.HandleCreated -= OnNativeHandleCreated;
        _nativeHost.HandleDestroyed -= OnNativeHandleDestroyed;
        _nativeHost.NativePointer -= OnNativePointer;
        Child = null;
        _nativeHost = null;
        _nativeContainer = null;
        _inputOverlay = null;
        _renderHandle = IntPtr.Zero;
    }

    private void OnNativeHandleCreated(IntPtr handle)
    {
        _renderHandle = handle;
        Console.WriteLine($"[VideraViewNew] Native handle created: 0x{handle.ToInt64():X}");

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
