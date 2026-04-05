using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Videra.Core.Exceptions;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.macOS;

/// <summary>
/// macOS Metal graphics backend implementation.
/// Provides hardware-accelerated rendering using Apple's Metal API through Objective-C interop
/// via <c>ObjCRuntime</c>. Requires a valid NSView handle on macOS. Supports Retina display
/// scaling by querying the backing scale factor of the host window.
/// </summary>
public unsafe partial class MetalBackend : IGraphicsBackend
{
    private static readonly DepthBufferConfiguration DepthConfig = DepthBufferConfiguration.Default;

    internal const int MetalPixelFormatDepth32Float = 252;

    private IntPtr _device;
    private IntPtr _commandQueue;
    private IntPtr _metalLayer;
    private IntPtr _depthTexture;
    private IntPtr _depthStencilState;
    private IntPtr _nsView;
    private bool _ownsMetalLayer;

    private Vector4 _clearColor = new(0.1f, 0.1f, 0.15f, 1.0f);
    private int _width;
    private int _height;
    private double _scaleFactor = 1.0;

    private MetalResourceFactory _resourceFactory;
    private MetalCommandExecutor _commandExecutor;
    private bool _disposed;
    private readonly ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<MetalBackend>();

    /// <summary>
    /// Gets a value indicating whether the backend has been successfully initialized
    /// and is ready for rendering operations.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Initializes the Metal backend with the specified NSView handle and rendering dimensions.
    /// Queries the Retina backing scale factor, creates the default Metal device and command queue,
    /// creates or attaches a <c>CAMetalLayer</c> to the NSView, configures the layer's pixel format
    /// and drawable size, creates the depth-stencil state, and sets up the resource factory and command executor.
    /// </summary>
    /// <param name="windowHandle">
    /// A valid NSView handle for the target view. Must not be <see cref="IntPtr.Zero"/>.
    /// </param>
    /// <param name="width">The initial width of the rendering surface in pixels. Must be greater than zero.</param>
    /// <param name="height">The initial height of the rendering surface in pixels. Must be greater than zero.</param>
    /// <exception cref="PlatformDependencyException">
    /// Thrown when <paramref name="windowHandle"/> is <see cref="IntPtr.Zero"/>,
    /// when <paramref name="width"/> or <paramref name="height"/> is not positive,
    /// when the Metal device cannot be created, or when the Metal command queue cannot be created.
    /// </exception>
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
            _nsView = windowHandle;

            // Get Retina scale factor
            _scaleFactor = GetBackingScaleFactor(_nsView);
            Log.BackingScaleFactor(_logger, _scaleFactor);

            // Create Metal Device
            _device = ObjCRuntime.MTLCreateSystemDefaultDevice();
            if (_device == IntPtr.Zero)
                throw new PlatformDependencyException(
                    "Failed to create Metal device. Ensure Metal is supported on this hardware.",
                    "Initialize",
                    "macOS");

            // Create Command Queue
            _commandQueue = ObjCRuntime.SendMessage(_device, ObjCRuntime.SEL("newCommandQueue"));
            if (_commandQueue == IntPtr.Zero)
                throw new PlatformDependencyException(
                    "Failed to create Metal command queue.",
                    "Initialize",
                    "macOS");

            // Get or create CAMetalLayer
            _metalLayer = GetOrCreateMetalLayer(_nsView);

            // Configure MetalLayer
            ObjCRuntime.SendMessagePtrVoid(_metalLayer, ObjCRuntime.SEL("setDevice:"), _device);
            ObjCRuntime.SendMessageInt(_metalLayer, ObjCRuntime.SEL("setPixelFormat:"), 80); // MTLPixelFormatBGRA8Unorm
            ObjCRuntime.SendMessageBool(_metalLayer, ObjCRuntime.SEL("setFramebufferOnly:"), false);
            ObjCRuntime.SendMessageDoubleArg(_metalLayer, ObjCRuntime.SEL("setContentsScale:"), _scaleFactor);
            SetLayerDrawableSize(_metalLayer, width, height);

            CreateDepthTexture();

            Log.InitializedSurface(_logger, width, height, _scaleFactor);

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
        var existingLayer = ObjCRuntime.SendMessage(nsView, ObjCRuntime.SEL("layer"));
        if (IsCametalLayer(existingLayer))
        {
            _ownsMetalLayer = false;
            return existingLayer;
        }

        var newLayer = ObjCRuntime.AllocInit("CAMetalLayer");
        ObjCRuntime.SendMessageBool(nsView, ObjCRuntime.SEL("setWantsLayer:"), true);
        ObjCRuntime.SendMessagePtrVoid(nsView, ObjCRuntime.SEL("setLayer:"), newLayer);
        _ownsMetalLayer = true;
        return newLayer;
    }

    private static bool IsCametalLayer(IntPtr layer)
    {
        if (layer == IntPtr.Zero)
        {
            return false;
        }

        var cametalLayerClass = ObjCRuntime.objc_getClass("CAMetalLayer");
        return cametalLayerClass != IntPtr.Zero &&
               ObjCRuntime.SendMessagePtrBoolReturn(layer, ObjCRuntime.SEL("isKindOfClass:"), cametalLayerClass);
    }

    private void CreateDepthStencilState()
    {
        var descriptor = ObjCRuntime.AllocInit("MTLDepthStencilDescriptor");
        ObjCRuntime.SendMessageInt(descriptor, ObjCRuntime.SEL("setDepthCompareFunction:"), MapDepthComparison(DepthConfig.DepthComparison));
        ObjCRuntime.SendMessageBool(descriptor, ObjCRuntime.SEL("setDepthWriteEnabled:"), true);
        _depthStencilState = ObjCRuntime.SendMessagePtr(_device, ObjCRuntime.SEL("newDepthStencilStateWithDescriptor:"), descriptor);
        ObjCRuntime.SendMessageVoid(descriptor, ObjCRuntime.SEL("release"));
        if (_depthStencilState == IntPtr.Zero)
            throw new GraphicsInitializationException(
                "Failed to create Metal depth stencil state.",
                "CreateDepthStencilState");
    }

    private void CreateDepthTexture()
    {
        ReleaseDepthTexture();

        var descriptor = ObjCRuntime.AllocInit("MTLTextureDescriptor");
        try
        {
            ObjCRuntime.SendMessageInt(descriptor, ObjCRuntime.SEL("setTextureType:"), 2); // MTLTextureType2D
            ObjCRuntime.SendMessageInt(descriptor, ObjCRuntime.SEL("setPixelFormat:"), MetalPixelFormatDepth32Float);
            ObjCRuntime.SendMessageInt(descriptor, ObjCRuntime.SEL("setWidth:"), _width);
            ObjCRuntime.SendMessageInt(descriptor, ObjCRuntime.SEL("setHeight:"), _height);
            ObjCRuntime.SendMessageInt(descriptor, ObjCRuntime.SEL("setStorageMode:"), 2); // MTLStorageModePrivate
            ObjCRuntime.SendMessageInt(descriptor, ObjCRuntime.SEL("setUsage:"), 4); // MTLTextureUsageRenderTarget

            _depthTexture = ObjCRuntime.SendMessagePtr(_device, ObjCRuntime.SEL("newTextureWithDescriptor:"), descriptor);
            if (_depthTexture == IntPtr.Zero)
            {
                throw new ResourceCreationException(
                    "Failed to create Metal depth texture.",
                    "CreateDepthTexture");
            }
        }
        finally
        {
            ObjCRuntime.SendMessageVoid(descriptor, ObjCRuntime.SEL("release"));
        }
    }

    private void ReleaseDepthTexture()
    {
        if (_depthTexture == IntPtr.Zero)
        {
            return;
        }

        ObjCRuntime.SendMessageVoid(_depthTexture, ObjCRuntime.SEL("release"));
        _depthTexture = IntPtr.Zero;
    }

    /// <summary>
    /// Resizes the Metal layer's drawable size and updates the cached dimensions to match
    /// the new size. Re-queries the backing scale factor to account for display changes.
    /// If either dimension is not positive, the call is silently ignored.
    /// </summary>
    /// <param name="width">The new width of the rendering surface in pixels.</param>
    /// <param name="height">The new height of the rendering surface in pixels.</param>
    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0) return;
        _scaleFactor = GetBackingScaleFactor(_nsView);
        _width = width;
        _height = height;
        SetLayerDrawableSize(_metalLayer, width, height);
        CreateDepthTexture();
        Log.ResizedSurface(_logger, width, height, _scaleFactor);
    }

    /// <summary>
    /// Begins a new rendering frame by delegating to the command executor, which acquires
    /// the next drawable from the Metal layer, creates a command buffer, begins an encode pass,
    /// clears with the current clear color, and applies the depth-stencil state.
    /// </summary>
    public void BeginFrame() => _commandExecutor.BeginFrame(_metalLayer, _clearColor, _depthStencilState, _depthTexture);

    /// <summary>
    /// Ends the current rendering frame by delegating to the command executor, which commits
    /// the command buffer and presents the drawable to the screen.
    /// </summary>
    public void EndFrame() => _commandExecutor.EndFrame();

    /// <summary>
    /// Sets the color used to clear the render target at the beginning of each frame.
    /// </summary>
    /// <param name="color">The clear color as a <see cref="Vector4"/> with RGBA components in the range [0, 1].</param>
    public void SetClearColor(Vector4 color) => _clearColor = color;

    /// <summary>
    /// Gets the Metal resource factory used to create GPU resources such as buffers, textures, and shaders.
    /// </summary>
    /// <returns>The <see cref="IResourceFactory"/> implementation for this backend.</returns>
    public IResourceFactory GetResourceFactory() => _resourceFactory;

    /// <summary>
    /// Gets the Metal command executor used to issue rendering commands to the GPU.
    /// </summary>
    /// <returns>The <see cref="ICommandExecutor"/> implementation for this backend.</returns>
    public ICommandExecutor GetCommandExecutor() => _commandExecutor;

    /// <summary>
    /// Releases all Metal resources by sending Objective-C <c>release</c> messages to the
    /// depth-stencil state, command queue, and Metal device.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        IsInitialized = false;

        if (_depthStencilState != IntPtr.Zero)
        {
            ObjCRuntime.SendMessageVoid(_depthStencilState, ObjCRuntime.SEL("release"));
            _depthStencilState = IntPtr.Zero;
        }

        ReleaseDepthTexture();

        if (_metalLayer != IntPtr.Zero)
        {
            if (_nsView != IntPtr.Zero)
            {
                ObjCRuntime.SendMessagePtrVoid(_nsView, ObjCRuntime.SEL("setLayer:"), IntPtr.Zero);
            }

            if (_ownsMetalLayer)
            {
                ObjCRuntime.SendMessageVoid(_metalLayer, ObjCRuntime.SEL("release"));
            }

            _metalLayer = IntPtr.Zero;
            _ownsMetalLayer = false;
        }

        if (_commandQueue != IntPtr.Zero)
        {
            ObjCRuntime.SendMessageVoid(_commandQueue, ObjCRuntime.SEL("release"));
            _commandQueue = IntPtr.Zero;
        }

        if (_device != IntPtr.Zero)
        {
            ObjCRuntime.SendMessageVoid(_device, ObjCRuntime.SEL("release"));
            _device = IntPtr.Zero;
        }

        _nsView = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    private static void SetLayerDrawableSize(IntPtr layer, int width, int height)
        => ObjCRuntime.SendMessageCGSize(layer, ObjCRuntime.SEL("setDrawableSize:"), new CGSize { width = width, height = height });

    private double GetBackingScaleFactor(IntPtr nsView)
    {
        var window = ObjCRuntime.SendMessage(nsView, ObjCRuntime.SEL("window"));
        if (window == IntPtr.Zero)
        {
            Log.WindowMissingUsingRetinaDefault(_logger);
            return 2.0;
        }
        var scale = ObjCRuntime.SendMessageDouble(window, ObjCRuntime.SEL("backingScaleFactor"));
        return scale > 0 ? scale : 2.0;
    }

    private static int MapDepthComparison(DepthComparisonFunction comparison)
    {
        return (int)comparison;
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Backing scale factor: {ScaleFactor}")]
        public static partial void BackingScaleFactor(ILogger logger, double scaleFactor);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Initialize: pixel {Width}x{Height}, scale {ScaleFactor}")]
        public static partial void InitializedSurface(ILogger logger, int width, int height, double scaleFactor);

        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Resize: pixel {Width}x{Height}, scale {ScaleFactor}")]
        public static partial void ResizedSurface(ILogger logger, int width, int height, double scaleFactor);

        [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "No window found, using scale factor 2.0 (Retina default)")]
        public static partial void WindowMissingUsingRetinaDefault(ILogger logger);
    }
}
