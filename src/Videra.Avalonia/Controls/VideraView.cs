using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using Veldrid;
using Videra.Core.Graphics;
using PixelFormat = Veldrid.PixelFormat;

namespace Videra.Avalonia.Controls;

public partial class VideraView : NativeControlHost
{
    
    // 【修改 1】新增字段：暂存句柄
    private IntPtr _nativeHandle = IntPtr.Zero;
    
    // ==========================================
    // 1. 背景颜色属性 (支持绑定)
    // ==========================================
    public static readonly StyledProperty<Color> BackgroundColorProperty =
        AvaloniaProperty.Register<VideraView, Color>(nameof(BackgroundColor), Colors.Black);

    // ==========================================
    // 2. 模型列表源 (Items Source)
    // ==========================================
    public static readonly StyledProperty<IEnumerable> ItemsProperty =
        AvaloniaProperty.Register<VideraView, IEnumerable>(nameof(Items));

    // ==========================================
    // 3. 相机控制属性 (反转、速度)
    // ==========================================
    public static readonly StyledProperty<bool> CameraInvertXProperty =
        AvaloniaProperty.Register<VideraView, bool>(nameof(CameraInvertX));

    public static readonly StyledProperty<bool> CameraInvertYProperty =
        AvaloniaProperty.Register<VideraView, bool>(nameof(CameraInvertY));

    private bool _isReady;

    public VideraView()
    {
        Engine = new VideraEngine();
    }

    public VideraEngine Engine { get; }

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

    // ==========================================
    // 属性变更监听 (Property Changed)
    // ==========================================
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (!_isReady) return;
        if (change.Property == IsGridVisibleProperty)
        {
            Engine.Grid.IsVisible = change.GetNewValue<bool>();
        }
        else if (change.Property == GridHeightProperty)
        {
            Engine.Grid.Height = change.GetNewValue<float>();
        }
        else if (change.Property == GridColorProperty)
        {
            var c = change.GetNewValue<Color>();
            Engine.Grid.GridColor = new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }
        else if (change.Property == BackgroundColorProperty)
        {
            var c = change.GetNewValue<Color>();
            Engine.BackgroundColor = new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, 1f);
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
    // 列表同步逻辑 (核心)
    // ==========================================
    private void UpdateItemsSubscription(IEnumerable oldList, IEnumerable newList)
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

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // 处理集合变动：增删改
        if (e.Action == NotifyCollectionChangedAction.Add)
            foreach (Object3D item in e.NewItems)
                Engine.AddObject(item);
        else if (e.Action == NotifyCollectionChangedAction.Remove)
            foreach (Object3D item in e.OldItems)
                Engine.RemoveObject(item);
        else if (e.Action == NotifyCollectionChangedAction.Reset) Engine.ClearObjects();
    }

    // ... CreateNativeControlCore 等初始化代码保持不变 ...
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var handle = base.CreateNativeControlCore(parent);
        
        // 【修改 2】只保存句柄，绝对不要在这里初始化引擎！
        // 此时 macOS 的 View 大小可能还是 0，初始化会崩溃。
        _nativeHandle = handle.Handle;

        // 设置输入监听是安全的
        SetupInput(handle.Handle);

        // 直接返回，让 Avalonia 继续布局
        return handle;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        CleanupInput();
        Engine.Dispose();
        _nativeHandle = IntPtr.Zero; // 清空句柄
        _isReady = false;
        base.DestroyNativeControlCore(control);
    }

    private bool _isInitialized = false;

// 监听 Avalonia 的尺寸变化
    // 1. 修改 OnSizeChanged，降低优先级，并引入重试
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        var width = e.NewSize.Width;
        var height = e.NewSize.Height;
    
        // ❌ 不要乘以 scaling！macOS Metal 使用逻辑像素
        // var scaling = VisualRoot?.RenderScaling ?? 1.0;

        Dispatcher.UIThread.Post(() => 
        {
            // 直接使用逻辑像素
            TryInitializeOrResize((uint)width, (uint)height);
        }, DispatcherPriority.Background);
    }

// 2. 封装一个带重试机制的初始化/Resize 方法
    private void TryInitializeOrResize(uint width, uint height, int retryCount = 0)
    {
        if (_nativeHandle == IntPtr.Zero || width == 0 || height == 0) return;

        if (!_isInitialized)
        {
            try
            {
                Console.WriteLine($"[Videra] Attempting Init (Try #{retryCount + 1}): {width}x{height}");
                InitializeGraphicsDevice(width, height);
            
                // 如果执行到这里没有抛出异常，说明成功了
                _isInitialized = true;
                Console.WriteLine("[Videra] Init Success!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Videra] Init Failed (Try #{retryCount + 1}): {ex.Message}");
            
                // 核心修复：如果失败，且重试次数少于 5 次，则延迟 100ms 后重试
                if (retryCount < 5)
                {
                    Console.WriteLine("[Videra] Scheduling retry in 100ms...");
                    Task.Delay(100).ContinueWith(_ => 
                    {
                        Dispatcher.UIThread.Post(() => TryInitializeOrResize(width, height, retryCount + 1));
                    });
                }
                else
                {
                    Console.WriteLine("[Videra Critical] Gave up initializing after 5 attempts.");
                }
            }
        }
        else
        {
            // 已经初始化过，只做 Resize
            Engine.Resize(width, height);
        }
    }
    
    // 【修改 4】新增私有初始化方法
   private void InitializeGraphicsDevice(uint incomingWidth, uint incomingHeight)
{
    if (_nativeHandle == IntPtr.Zero) 
        throw new InvalidOperationException("Native Handle is null");

    uint actualWidth = incomingWidth;
    uint actualHeight = incomingHeight;
    
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        var frame = GetNSViewFrame(_nativeHandle);
        actualWidth = (uint)Math.Max(1, frame.width);
        actualHeight = (uint)Math.Max(1, frame.height);
        
        Console.WriteLine($"[Videra] NSView frame: {frame.width}x{frame.height} -> using {actualWidth}x{actualHeight}");
        
        // ⚠️ Metal 无法处理太小的尺寸，至少需要 64x64
        if (actualWidth < 64 || actualHeight < 64)
        {
            throw new InvalidOperationException($"NSView too small: {actualWidth}x{actualHeight}");
        }
    }
    else
    {
        actualWidth = Math.Max(64, incomingWidth);
        actualHeight = Math.Max(64, incomingHeight);
    }

    Console.WriteLine($"[Videra] Creating swapchain: {actualWidth}x{actualHeight}");

    var options = new GraphicsDeviceOptions
    {
        PreferStandardClipSpaceYDirection = true,
        PreferDepthRangeZeroToOne = true,
        ResourceBindingModel = ResourceBindingModel.Improved,
        Debug = false,  // ⚠️ 先关闭 Debug 模式，可能导致额外开销
        SwapchainDepthFormat = PixelFormat.D32_Float_S8_UInt 
    };

    SwapchainSource source;
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        source = SwapchainSource.CreateWin32(_nativeHandle, IntPtr.Zero);
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        source = SwapchainSource.CreateNSView(_nativeHandle);
    else
        source = SwapchainSource.CreateXlib(_nativeHandle, IntPtr.Zero);

    var scDesc = new SwapchainDescription(
        source,
        actualWidth, actualHeight,
        PixelFormat.D32_Float_S8_UInt, 
        true);

    Engine.Initialize(options, scDesc);
    
    // =========================================================
    // 【修复关键点】初始化后必须立即 Resize
    // 这会更新 Camera 的投影矩阵 (Projection Matrix)
    // =========================================================
    Engine.Resize(actualWidth, actualHeight);
    
    _isReady = true;
    _width = actualWidth;
    _height = actualHeight;
    
    SyncPropertiesToEngine();
    Dispatcher.UIThread.InvokeAsync(RenderLoop, DispatcherPriority.Background);
    
    Console.WriteLine($"[Videra] ✓ Initialized successfully at {actualWidth}x{actualHeight}");
}

// 添加 P/Invoke 获取 NSView 尺寸
[StructLayout(LayoutKind.Sequential)]
private struct CGRect
{
    public double x, y, width, height;
}

[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
private static extern CGRect objc_msgSend_CGRect(IntPtr receiver, IntPtr selector);

private static CGRect GetNSViewFrame(IntPtr nsView)
{
    var frameSelector = GetSelector("frame");
    return objc_msgSend_CGRect(nsView, frameSelector);
}

[DllImport("/usr/lib/libobjc.dylib")]
private static extern IntPtr sel_registerName(string name);

private static IntPtr GetSelector(string name) => sel_registerName(name);

// 添加字段记录实际尺寸
private uint _width, _height;

    private void SyncPropertiesToEngine()
    {
        // 同步背景色
        var c = BackgroundColor;
        Engine.BackgroundColor = new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, 1f);
        
        // 同步相机设置
        Engine.Camera.InvertX = CameraInvertX;
        Engine.Camera.InvertY = CameraInvertY;

        // 同步网格设置
        Engine.Grid.IsVisible = IsGridVisible;
        Engine.Grid.Height = GridHeight;
        var gc = GridColor;
        Engine.Grid.GridColor = new RgbaFloat(gc.R / 255f, gc.G / 255f, gc.B / 255f, gc.A / 255f);

        // 同步列表
        UpdateItemsSubscription(null, Items);
    }

    private async void RenderLoop()
    {
        while (true)
        {
            if (_isReady)
                try
                {
                    Engine.Draw();
                }
                catch
                {
                }

            await Task.Delay(16);
        }
    }

    #region 网格属性

    // --- Grid 属性 ---
    public static readonly StyledProperty<bool> IsGridVisibleProperty =
        AvaloniaProperty.Register<VideraView, bool>(nameof(IsGridVisible), true);

    public bool IsGridVisible
    {
        get => GetValue(IsGridVisibleProperty);
        set => SetValue(IsGridVisibleProperty, value);
    }

    public static readonly StyledProperty<float> GridHeightProperty =
        AvaloniaProperty.Register<VideraView, float>(nameof(GridHeight), 0f);

    public float GridHeight
    {
        get => GetValue(GridHeightProperty);
        set => SetValue(GridHeightProperty, value);
    }

    public static readonly StyledProperty<Color> GridColorProperty =
        AvaloniaProperty.Register<VideraView, Color>(nameof(GridColor), Colors.Gray);

    public Color GridColor
    {
        get => GetValue(GridColorProperty);
        set => SetValue(GridColorProperty, value);
    }

    #endregion
}