using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using Veldrid;
using Videra.Core.Graphics;
using PixelFormat = Veldrid.PixelFormat;

namespace Videra.Avalonia.Controls;

public partial class VideraView : NativeControlHost
{
    private bool _isReady;

    public VideraView()
    {
        Engine = new VideraEngine();
    }

    // 对外暴露引擎，允许外部调用 LoadModel 等方法
    public VideraEngine Engine { get; }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var handle = base.CreateNativeControlCore(parent);

        var options = new GraphicsDeviceOptions
        {
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true,
            ResourceBindingModel = ResourceBindingModel.Improved,
            Debug = true
        };

        SwapchainSource source;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            source = SwapchainSource.CreateWin32(handle.Handle, IntPtr.Zero);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            source = SwapchainSource.CreateNSView(handle.Handle);
        else
            source = SwapchainSource.CreateXlib(handle.Handle, IntPtr.Zero);

        var scDesc = new SwapchainDescription(source, 800, 600, PixelFormat.R32_Float, true);

        // 初始化核心引擎
        Engine.Initialize(options, scDesc, Environment.OSVersion.Platform);

        // 设置跨平台输入 (代码在 VideraView.Input.cs 中)
        SetupInput(handle.Handle);

        _isReady = true;
        Dispatcher.UIThread.InvokeAsync(RenderLoop, DispatcherPriority.Background);
        return handle;
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        CleanupInput();
        Engine.Dispose();
        base.DestroyNativeControlCore(control);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (_isReady)
            Engine.Resize((uint)e.NewSize.Width, (uint)e.NewSize.Height);
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
}