using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Videra.Core.Exceptions;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.macOS;

/// <summary>
/// macOS Metal graphics backend implementation
/// </summary>
public unsafe class MetalBackend : IGraphicsBackend
{
    private IntPtr _device;
    private IntPtr _commandQueue;
    private IntPtr _metalLayer;
    private IntPtr _depthStencilState;
    private IntPtr _nsView;

    private Vector4 _clearColor = new(0.1f, 0.1f, 0.15f, 1.0f);
    private int _width;
    private int _height;
    private double _scaleFactor = 1.0;

    private MetalResourceFactory _resourceFactory;
    private MetalCommandExecutor _commandExecutor;
    private bool _disposed;
    private readonly ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<MetalBackend>();

    public bool IsInitialized { get; private set; }

    public void Initialize(IntPtr windowHandle, int width, int height)
    {
        if (IsInitialized) return;

        if (windowHandle == IntPtr.Zero)
            throw new PlatformDependencyException(
                "A valid NSView handle is required for Metal initialization.",
                "Initialize",
                "macOS");

        if (width <= 0 || height <= 0)
            throw new PlatformDependencyException(
                $"Invalid dimensions for Metal initialization: {width}x{height}. Both width and height must be positive.",
                "Initialize",
                "macOS");

        _width = width;
        _height = height;

        try
        {
            // Get NSView
            _nsView = windowHandle;

            // Get Retina scale factor
            _scaleFactor = GetBackingScaleFactor(_nsView);
            _logger.LogInformation("Backing scale factor: {ScaleFactor}", _scaleFactor);

            // Create Metal Device
            _device = MTLCreateSystemDefaultDevice();
            if (_device == IntPtr.Zero)
                throw new PlatformDependencyException(
                    "Failed to create Metal device. Ensure Metal is supported on this hardware.",
                    "Initialize",
                    "macOS");

            // Create Command Queue
            _commandQueue = SendMessage(_device, SEL("newCommandQueue"));
            if (_commandQueue == IntPtr.Zero)
                throw new PlatformDependencyException(
                    "Failed to create Metal command queue.",
                    "Initialize",
                    "macOS");

            // Get or create CAMetalLayer
            _metalLayer = GetOrCreateMetalLayer(_nsView);

            // Configure MetalLayer
            SetLayerDevice(_metalLayer, _device);
            SetLayerPixelFormat(_metalLayer, 80); // MTLPixelFormatBGRA8Unorm
            SetLayerFramebufferOnly(_metalLayer, false);

            // Set contentsScale for Retina support (draw dimensions are already in pixels, no secondary scaling)
            SetLayerContentsScale(_metalLayer, _scaleFactor);

            // Set drawable size (using pixel dimensions directly)
            SetLayerDrawableSize(_metalLayer, width, height);
            _logger.LogInformation("Initialize: pixel {Width}x{Height}, scale {ScaleFactor}", width, height, _scaleFactor);

            // Create depth stencil state
            CreateDepthStencilState();

            // Create factory and command executor
            _resourceFactory = new MetalResourceFactory(_device);
            _commandExecutor = new MetalCommandExecutor(_commandQueue);
        }
        catch
        {
            Dispose();
            throw;
        }

        IsInitialized = true;
    }

    private IntPtr GetOrCreateMetalLayer(IntPtr nsView)
    {
        // Create new CAMetalLayer
        var newLayer = AllocInit("CAMetalLayer");

        // Enable layer-backed view
        SendMessageWithBool(nsView, SEL("setWantsLayer:"), true);

        // Set Layer to View
        SendMessageWithPtr(nsView, SEL("setLayer:"), newLayer);

        return newLayer;
    }

    private void CreateDepthStencilState()
    {
        // Create MTLDepthStencilDescriptor
        var descriptor = AllocInit("MTLDepthStencilDescriptor");

        // Set depth comparison function to LessEqual
        SendMessageWithInt(descriptor, SEL("setDepthCompareFunction:"), 4); // MTLCompareFunctionLessEqual
        SendMessageWithBool(descriptor, SEL("setDepthWriteEnabled:"), true);

        // Create DepthStencilState
        _depthStencilState = SendMessageWithPtr(_device, SEL("newDepthStencilStateWithDescriptor:"), descriptor);

        // Release descriptor
        SendMessage(descriptor, SEL("release"));
    }

    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        // Re-acquire scale factor (window may have moved to different DPI display); drawable size stays in pixels
        _scaleFactor = GetBackingScaleFactor(_nsView);

        _width = width;
        _height = height;

        SetLayerDrawableSize(_metalLayer, width, height);
        _logger.LogInformation("Resize: pixel {Width}x{Height}, scale {ScaleFactor}", width, height, _scaleFactor);
    }

    public void BeginFrame()
    {
        // Metal frame rendering is handled in CommandExecutor
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
        if (_disposed) return;
        _disposed = true;
        IsInitialized = false;

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

    // Helper: convert selector string to SEL (IntPtr)
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
        var selector = SEL("setDrawableSize:");
        var size = new CGSize { width = width, height = height };
        objc_msgSend_CGSize(layer, selector, size);
    }

    private static void SetLayerContentsScale(IntPtr layer, double scale)
    {
        objc_msgSend_double(layer, SEL("setContentsScale:"), scale);
    }

    private double GetBackingScaleFactor(IntPtr nsView)
    {
        // Get the window from NSView
        var window = SendMessage(nsView, SEL("window"));
        if (window == IntPtr.Zero)
        {
            _logger.LogDebug("No window found, using scale factor 2.0 (Retina default)");
            return 2.0; // macOS Retina default
        }

        // Get window backingScaleFactor
        var scale = objc_msgSend_double_ret(window, SEL("backingScaleFactor"));
        return scale > 0 ? scale : 2.0;
    }

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_CGSize(IntPtr receiver, IntPtr selector, CGSize size);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_double(IntPtr receiver, IntPtr selector, double arg);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern double objc_msgSend_double_ret(IntPtr receiver, IntPtr selector);

    [StructLayout(LayoutKind.Sequential)]
    private struct CGSize
    {
        public double width;
        public double height;
    }

    #endregion
}
