using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using Videra.Core.Graphics;
using Veldrid;

namespace Videra.Avalonia.Controls;

public partial class VideraView : NativeControlHost
{
    private readonly VideraEngine _engine;
    private bool _isReady;

    public VideraEngine Engine => _engine;

    public VideraView()
    {
        _engine = new VideraEngine();
    }

    // ==========================================
    // 1. 背景颜色属性 (支持绑定)
    // ==========================================
    public static readonly StyledProperty<Color> BackgroundColorProperty =
        AvaloniaProperty.Register<VideraView, Color>(nameof(BackgroundColor), Colors.Black);

    public Color BackgroundColor
    {
        get => GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    // ==========================================
    // 2. 模型列表源 (Items Source)
    // ==========================================
    public static readonly StyledProperty<IEnumerable> ItemsProperty =
        AvaloniaProperty.Register<VideraView, IEnumerable>(nameof(Items));

    public IEnumerable Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    // ==========================================
    // 3. 相机控制属性 (反转、速度)
    // ==========================================
    public static readonly StyledProperty<bool> CameraInvertXProperty =
        AvaloniaProperty.Register<VideraView, bool>(nameof(CameraInvertX), false);

    public bool CameraInvertX
    {
        get => GetValue(CameraInvertXProperty);
        set => SetValue(CameraInvertXProperty, value);
    }

    public static readonly StyledProperty<bool> CameraInvertYProperty =
        AvaloniaProperty.Register<VideraView, bool>(nameof(CameraInvertY), false);

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

        if (change.Property == BackgroundColorProperty)
        {
            var c = change.GetNewValue<Color>();
            _engine.BackgroundColor = new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, 1f);
        }
        else if (change.Property == CameraInvertXProperty)
        {
            _engine.Camera.InvertX = change.GetNewValue<bool>();
        }
        else if (change.Property == CameraInvertYProperty)
        {
            _engine.Camera.InvertY = change.GetNewValue<bool>();
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
        _engine.ClearObjects();
        if (newList != null)
        {
            foreach (var item in newList)
            {
                if (item is Object3D obj) _engine.AddObject(obj);
            }
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // 处理集合变动：增删改
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (Object3D item in e.NewItems) _engine.AddObject(item);
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (Object3D item in e.OldItems) _engine.RemoveObject(item);
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            _engine.ClearObjects();
        }
    }

    // ... CreateNativeControlCore 等初始化代码保持不变 ...
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        // (保持之前的初始化逻辑不变)
        var handle = base.CreateNativeControlCore(parent);

        // ... (GraphicsDeviceOptions, Swapchain 初始化) ...
        var options = new GraphicsDeviceOptions { PreferStandardClipSpaceYDirection = true, PreferDepthRangeZeroToOne = true, ResourceBindingModel = ResourceBindingModel.Improved, Debug = true };

        SwapchainSource source;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) source = SwapchainSource.CreateWin32(handle.Handle, IntPtr.Zero);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) source = SwapchainSource.CreateNSView(handle.Handle);
        else source = SwapchainSource.CreateXlib(handle.Handle, IntPtr.Zero);

        var scDesc = new SwapchainDescription(source, 800, 600, Veldrid.PixelFormat.R32_Float, true);

        _engine.Initialize(options, scDesc, Environment.OSVersion.Platform);
        SetupInput(handle.Handle);

        _isReady = true;

        // 【关键】初始化时同步一次当前的属性值到 Engine
        var c = BackgroundColor;
        _engine.BackgroundColor = new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, 1f);
        _engine.Camera.InvertX = CameraInvertX;
        _engine.Camera.InvertY = CameraInvertY;
        UpdateItemsSubscription(null, Items); // 同步初始列表

        Dispatcher.UIThread.InvokeAsync(RenderLoop, DispatcherPriority.Background);
        return handle;
    }

    // ... Destroy, Resize, RenderLoop, Dispose 保持不变 ...
    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        CleanupInput();
        _engine.Dispose();
        base.DestroyNativeControlCore(control);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (_isReady) _engine.Resize((uint)e.NewSize.Width, (uint)e.NewSize.Height);
    }

    private async void RenderLoop()
    {
        while (true)
        {
            if (_isReady) try { _engine.Draw(); } catch { }
            await Task.Delay(16);
        }
    }
}