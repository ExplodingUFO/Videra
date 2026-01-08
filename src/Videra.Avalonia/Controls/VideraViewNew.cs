using System.Collections;
using System.Collections.Specialized;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Avalonia.Controls;

/// <summary>
/// 3D 渲染视图控件 - 使用 Silk.NET 多平台后端
/// </summary>
public partial class VideraViewNew : NativeControlHost
{
    private IntPtr _nativeHandle = IntPtr.Zero;
    private IGraphicsBackend? _backend;
    private bool _isInitialized;
    private bool _isReady;
    private uint _width;
    private uint _height;
    private DispatcherTimer? _renderTimer;
    private const int TargetFPS = 60;
    
    // ==========================================
    // 1. 背景颜色属性
    // ==========================================
    public static readonly StyledProperty<Color> BackgroundColorProperty =
        AvaloniaProperty.Register<VideraViewNew, Color>(nameof(BackgroundColor), Colors.Black);

    // ==========================================
    // 2. 模型列表源
    // ==========================================
    public static readonly StyledProperty<IEnumerable> ItemsProperty =
        AvaloniaProperty.Register<VideraViewNew, IEnumerable>(nameof(Items));

    // ==========================================
    // 3. 相机控制属性
    // ==========================================
    public static readonly StyledProperty<bool> CameraInvertXProperty =
        AvaloniaProperty.Register<VideraViewNew, bool>(nameof(CameraInvertX));

    public static readonly StyledProperty<bool> CameraInvertYProperty =
        AvaloniaProperty.Register<VideraViewNew, bool>(nameof(CameraInvertY));

    // ==========================================
    // 4. 网格属性
    // ==========================================
    public static readonly StyledProperty<bool> IsGridVisibleProperty =
        AvaloniaProperty.Register<VideraViewNew, bool>(nameof(IsGridVisible), true);

    public static readonly StyledProperty<float> GridHeightProperty =
        AvaloniaProperty.Register<VideraViewNew, float>(nameof(GridHeight), 0f);

    public static readonly StyledProperty<Color> GridColorProperty =
        AvaloniaProperty.Register<VideraViewNew, Color>(nameof(GridColor), Colors.Gray);

    public VideraViewNew()
    {
        Engine = new VideraEngine();
    }

    public VideraEngine Engine { get; }

    /// <summary>
    /// 获取资源工厂，用于模型导入
    /// </summary>
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

    // ==========================================
    // 属性变更监听
    // ==========================================
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (!_isReady) return;

        if (change.Property == BackgroundColorProperty)
        {
            var c = change.GetNewValue<Color>();
            var rgbaColor = new Core.Geometry.RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
            Engine.BackgroundColor = rgbaColor;
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
    }

    // ==========================================
    // 列表同步逻辑
    // ==========================================
    private void UpdateItemsSubscription(IEnumerable? oldList, IEnumerable? newList)
    {
        if (oldList is INotifyCollectionChanged oldIncc)
            oldIncc.CollectionChanged -= OnCollectionChanged;

        if (newList is INotifyCollectionChanged newIncc)
            newIncc.CollectionChanged += OnCollectionChanged;

        // 初始全量加载
        Engine.ClearObjects();
        if (newList != null)
            foreach (var item in newList)
                if (item is Object3D obj)
                    Engine.AddObject(obj);
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

    // ==========================================
    // 原生控件生命周期
    // ==========================================
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var handle = base.CreateNativeControlCore(parent);
        _nativeHandle = handle.Handle;

        // 设置输入监听
        SetupInput(handle.Handle);

        return handle;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        StopRenderLoop();
        CleanupInput();
        _backend?.Dispose();
        _backend = null;
        Engine.Dispose();
        _nativeHandle = IntPtr.Zero;
        _isReady = false;
        _isInitialized = false;
        base.DestroyNativeControlCore(control);
    }

    // ==========================================
    // 尺寸变化处理
    // ==========================================
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        var scaling = VisualRoot?.RenderScaling ?? 1.0;
        var widthPx = (uint)Math.Max(64, Math.Round(e.NewSize.Width * scaling));
        var heightPx = (uint)Math.Max(64, Math.Round(e.NewSize.Height * scaling));
        
        Console.WriteLine($"[VideraViewNew] OnSizeChanged: logical {e.NewSize.Width}x{e.NewSize.Height}, scale {scaling}, pixel {widthPx}x{heightPx}");

        Dispatcher.UIThread.Post(() =>
        {
            TryInitializeOrResize(widthPx, heightPx);
        }, DispatcherPriority.Background);
    }

    private void TryInitializeOrResize(uint widthPx, uint heightPx, int retryCount = 0)
    {
        if (_nativeHandle == IntPtr.Zero || widthPx == 0 || heightPx == 0) return;

        if (!_isInitialized)
        {
            try
            {
                Console.WriteLine($"[Videra] Attempting Init (Try #{retryCount + 1}): {widthPx}x{heightPx} (pixel)");
                Console.WriteLine($"[Videra] Platform: {GraphicsBackendFactory.GetPlatformName()}");
                
                InitializeGraphicsDevice(widthPx, heightPx);
                _isInitialized = true;
                _isReady = true;
                
                Console.WriteLine("[Videra] Init Success!");
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
                else
                {
                    Console.WriteLine("[Videra] Gave up initializing after 5 attempts.");
                }
            }
        }
        else
        {
            // 已初始化，执行 Resize
            if (_backend != null && (widthPx != _width || heightPx != _height))
            {
                _backend.Resize((int)widthPx, (int)heightPx);
                Engine.Resize(widthPx, heightPx);
                _width = widthPx;
                _height = heightPx;
            }
        }
    }

    private void InitializeGraphicsDevice(uint widthPx, uint heightPx)
    {
        if (_nativeHandle == IntPtr.Zero)
            throw new InvalidOperationException("Native Handle is null");

        uint actualWidth = Math.Max(64, widthPx);
        uint actualHeight = Math.Max(64, heightPx);

        // 创建平台特定的后端（传入像素尺寸）
        _backend = GraphicsBackendFactory.CreateBackend();
        _backend.Initialize(_nativeHandle, (int)actualWidth, (int)actualHeight);

        // 初始化Engine（使用像素尺寸确保视口与 drawable 匹配）
        Engine.Initialize(_backend);
        Engine.Resize(actualWidth, actualHeight);

        // 设置背景色
        var color = BackgroundColor;
        var rgbaColor = new Core.Geometry.RgbaFloat(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        Engine.BackgroundColor = rgbaColor;

        _width = actualWidth;
        _height = actualHeight;

        // 启动渲染循环
        StartRenderLoop();
    }

    // ==========================================
    // 渲染循环
    // ==========================================
    private void StartRenderLoop()
    {
        if (_renderTimer != null)
            return;

        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / TargetFPS)
        };
        _renderTimer.Tick += (s, e) => RenderFrame();
        _renderTimer.Start();
        
        Console.WriteLine($"[Videra] Render loop started at {TargetFPS} FPS");
    }

    private void StopRenderLoop()
    {
        if (_renderTimer != null)
        {
            _renderTimer.Stop();
            _renderTimer = null;
            Console.WriteLine("[Videra] Render loop stopped");
        }
    }

    private void RenderFrame()
    {
        if (!_isInitialized || _backend == null || !Engine.IsInitialized)
            return;

        try
        {
            Engine.Draw();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Videra] Render error: {ex.Message}");
            // 发生错误时停止渲染循环，避免持续报错
            StopRenderLoop();
        }
    }

    // ==========================================
    // macOS NSView Frame 获取
    // ==========================================
    [StructLayout(LayoutKind.Sequential)]
    private struct CGRect
    {
        public double x, y, width, height;
    }

    private static CGRect GetNSViewFrame(IntPtr nsView)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return default;

        var frameSelector = sel_registerName("frame");
        return objc_msgSend_CGRect(nsView, frameSelector);
    }

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern CGRect objc_msgSend_CGRect(IntPtr receiver, IntPtr selector);
}
