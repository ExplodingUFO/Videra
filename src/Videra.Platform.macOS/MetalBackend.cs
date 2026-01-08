using System.Numerics;
using System.Runtime.InteropServices;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.macOS;

/// <summary>
/// macOS Metal 图形后端实现
/// </summary>
public unsafe class MetalBackend : IGraphicsBackend
{
    private IntPtr _device;
    private IntPtr _commandQueue;
    private IntPtr _metalLayer;
    private IntPtr _depthStencilState;
    
    private Vector4 _clearColor = new(0.1f, 0.1f, 0.15f, 1.0f);
    private int _width;
    private int _height;
    
    private MetalResourceFactory _resourceFactory;
    private MetalCommandExecutor _commandExecutor;

    public bool IsInitialized { get; private set; }

    public void Initialize(IntPtr windowHandle, int width, int height)
    {
        if (IsInitialized) return;

        _width = width;
        _height = height;
        
        // 获取 NSView
        var nsView = windowHandle;
        
        // 创建 Metal Device
        _device = MTLCreateSystemDefaultDevice();
        if (_device == IntPtr.Zero)
            throw new Exception("Failed to create Metal device");

        // 创建 Command Queue
        _commandQueue = SendMessage(_device, SEL("newCommandQueue"));
        if (_commandQueue == IntPtr.Zero)
            throw new Exception("Failed to create Metal command queue");

        // 获取或创建 CAMetalLayer
        _metalLayer = GetOrCreateMetalLayer(nsView);
        
        // 配置 MetalLayer
        SetLayerDevice(_metalLayer, _device);
        SetLayerPixelFormat(_metalLayer, 80); // MTLPixelFormatBGRA8Unorm
        SetLayerFramebufferOnly(_metalLayer, false);
        SetLayerDrawableSize(_metalLayer, width, height);

        // 创建深度模板状态
        CreateDepthStencilState();
        
        // 创建工厂和命令执行器
        _resourceFactory = new MetalResourceFactory(_device);
        _commandExecutor = new MetalCommandExecutor(_commandQueue);

        IsInitialized = true;
    }

    private IntPtr GetOrCreateMetalLayer(IntPtr nsView)
    {
        // 创建新的 CAMetalLayer
        var newLayer = AllocInit("CAMetalLayer");
        
        // 启用 layer-backed view
        SendMessageWithBool(nsView, SEL("setWantsLayer:"), true);
        
        // 设置 Layer 到 View
        SendMessageWithPtr(nsView, SEL("setLayer:"), newLayer);
        
        return newLayer;
    }

    private void CreateDepthStencilState()
    {
        // 创建 MTLDepthStencilDescriptor
        var descriptor = AllocInit("MTLDepthStencilDescriptor");
        
        // 设置深度比较函数为 LessEqual
        SendMessageWithInt(descriptor, SEL("setDepthCompareFunction:"), 4); // MTLCompareFunctionLessEqual
        SendMessageWithBool(descriptor, SEL("setDepthWriteEnabled:"), true);
        
        // 创建 DepthStencilState
        _depthStencilState = SendMessageWithPtr(_device, SEL("newDepthStencilStateWithDescriptor:"), descriptor);
        
        // 释放 descriptor
        SendMessage(descriptor, SEL("release"));
    }

    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        _width = width;
        _height = height;
        
        SetLayerDrawableSize(_metalLayer, width, height);
    }

    public void BeginFrame()
    {
        // Metal 的帧渲染会在 CommandExecutor 中处理
        _commandExecutor.BeginFrame(_metalLayer, _clearColor, _depthStencilState);
    }

    public void EndFrame()
    {
        _commandExecutor.EndFrame();
    }

    public void SetClearColor(Vector4 color)
    {
        _clearColor = color;
    }

    public IResourceFactory GetResourceFactory() => _resourceFactory;

    public ICommandExecutor GetCommandExecutor() => _commandExecutor;

    public void Dispose()
    {
        if (_depthStencilState != IntPtr.Zero)
            SendMessage(_depthStencilState, SEL("release"));
        
        if (_commandQueue != IntPtr.Zero)
            SendMessage(_commandQueue, SEL("release"));
        
        if (_device != IntPtr.Zero)
            SendMessage(_device, SEL("release"));
    }

    #region Objective-C Interop

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendMessage(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendMessageWithPtr(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void SendMessageWithInt(IntPtr receiver, IntPtr selector, int arg);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void SendMessageWithBool(IntPtr receiver, IntPtr selector, bool arg);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern bool IsKindOfClass(IntPtr obj, IntPtr cls);

    // 辅助方法：将 selector 字符串转换为 SEL (IntPtr)
    private static IntPtr SEL(string name) => sel_registerName(name);

    [DllImport("/System/Library/Frameworks/Metal.framework/Metal")]
    private static extern IntPtr MTLCreateSystemDefaultDevice();

    private static IntPtr AllocInit(string className)
    {
        var cls = objc_getClass(className);
        var alloc = SendMessage(cls, SEL("alloc"));
        return SendMessage(alloc, SEL("init"));
    }

    private static void SetLayerDevice(IntPtr layer, IntPtr device)
    {
        SendMessageWithPtr(layer, SEL("setDevice:"), device);
    }

    private static void SetLayerPixelFormat(IntPtr layer, int format)
    {
        SendMessageWithInt(layer, SEL("setPixelFormat:"), format);
    }

    private static void SetLayerFramebufferOnly(IntPtr layer, bool value)
    {
        SendMessageWithBool(layer, SEL("setFramebufferOnly:"), value);
    }

    private static void SetLayerDrawableSize(IntPtr layer, int width, int height)
    {
        // 需要创建 CGSize 结构并传递
        var selector = SEL("setDrawableSize:");
        var size = new CGSize { width = width, height = height };
        objc_msgSend_CGSize(layer, selector, size);
    }

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_CGSize(IntPtr receiver, IntPtr selector, CGSize size);

    [StructLayout(LayoutKind.Sequential)]
    private struct CGSize
    {
        public double width;
        public double height;
    }

    #endregion
}
