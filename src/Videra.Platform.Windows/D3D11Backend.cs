using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using Videra.Core.Exceptions;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Geometry;

namespace Videra.Platform.Windows;

/// <summary>
/// Windows Direct3D 11 graphics backend implementation.
/// Provides hardware-accelerated rendering using the Direct3D 11 API via Silk.NET bindings.
/// Requires a valid HWND window handle on the Windows operating system.
/// </summary>
public unsafe class D3D11Backend : IGraphicsBackend
{
    private ComPtr<ID3D11Device> _device;
    private ComPtr<ID3D11DeviceContext> _context;
    private ComPtr<IDXGISwapChain> _swapchain;
    private ComPtr<ID3D11RenderTargetView> _backBufferRTV;
    private ComPtr<ID3D11DepthStencilView> _depthStencilView;
    private ComPtr<ID3D11Texture2D> _depthStencilTexture;
    private ComPtr<ID3D11DepthStencilState> _depthStencilState;
    private ComPtr<ID3D11DepthStencilState> _depthTestOnlyState;     // 只测试不写入
    private ComPtr<ID3D11DepthStencilState> _depthDisabledState;     // 禁用深度
    
    private D3D11 _d3d11;
    private DXGI _dxgi;

    private static readonly DepthBufferConfiguration DepthConfig = new(
        DepthBufferFormat.Depth24UnormStencil8,
        clearDepthValue: 1.0f,
        clearStencilValue: 0,
        depthComparison: DepthComparisonFunction.LessEqual);

    private Vector4 _clearColor = new(0.1f, 0.1f, 0.15f, 1.0f);
    private int _width;
    private int _height;
    
    private D3D11ResourceFactory _resourceFactory;
    private D3D11CommandExecutor _commandExecutor;
    private readonly D3D11BackendTestHooks? _testHooks;
    private bool _disposed;

    /// <summary>
    /// Gets a value indicating whether the backend has been successfully initialized
    /// and is ready for rendering operations.
    /// </summary>
    public bool IsInitialized { get; private set; }

    public D3D11Backend() : this(null)
    {
    }

    internal D3D11Backend(D3D11BackendTestHooks? testHooks)
    {
        _testHooks = testHooks;
    }

    /// <summary>
    /// Initializes the Direct3D 11 backend with the specified window handle and rendering dimensions.
    /// Creates the D3D11 device, device context, swap chain, render target view, depth-stencil resources,
    /// resource factory, and command executor.
    /// </summary>
    /// <param name="windowHandle">
    /// A valid Win32 HWND handle for the target window. Must not be <see cref="IntPtr.Zero"/>.
    /// </param>
    /// <param name="width">The initial width of the rendering surface in pixels. Must be greater than zero.</param>
    /// <param name="height">The initial height of the rendering surface in pixels. Must be greater than zero.</param>
    /// <exception cref="PlatformDependencyException">
    /// Thrown when <paramref name="windowHandle"/> is <see cref="IntPtr.Zero"/> or
    /// when <paramref name="width"/> or <paramref name="height"/> is not positive.
    /// </exception>
    /// <exception cref="GraphicsInitializationException">
    /// Thrown when the D3D11 device, swap chain, render target view, depth-stencil texture,
    /// depth-stencil view, or depth-stencil state fails to be created.
    /// </exception>
    public void Initialize(IntPtr windowHandle, int width, int height)
    {
        if (IsInitialized) return;

        if (windowHandle == IntPtr.Zero)
            throw new PlatformDependencyException(
                "A valid window handle is required for D3D11 initialization.",
                "Initialize",
                "Windows");

        if (width <= 0 || height <= 0)
            throw new PlatformDependencyException(
                $"Invalid dimensions for D3D11 initialization: {width}x{height}. Both width and height must be positive.",
                "Initialize",
                "Windows");

        _width = width;
        _height = height;

        try
        {
            _d3d11 = D3D11.GetApi();
            _dxgi = DXGI.GetApi();

            // 创建 Device 和 DeviceContext
            CreateDeviceAndSwapchain(windowHandle);

            // 创建深度模板视图
            CreateDepthStencil();

            // 创建深度模板状态
            CreateDepthStencilState();

            // 设置工厂和命令执行器
            _resourceFactory = new D3D11ResourceFactory(_device, _context, _d3d11);
            _commandExecutor = new D3D11CommandExecutor(_context);
            _commandExecutor.UpdateRenderTargets(_backBufferRTV, _depthStencilView);
            _commandExecutor.InitializeDepthStates(_depthStencilState, _depthTestOnlyState, _depthDisabledState);
        }
        catch
        {
            Dispose();
            throw;
        }

        IsInitialized = true;
    }

    private void CreateDeviceAndSwapchain(IntPtr windowHandle)
    {
        // Swapchain 描述
        var swapchainDesc = new SwapChainDesc
        {
            BufferDesc = new ModeDesc
            {
                Width = (uint)_width,
                Height = (uint)_height,
                RefreshRate = new Rational(60, 1),
                Format = Format.FormatR8G8B8A8Unorm
            },
            SampleDesc = new SampleDesc(1, 0),
            BufferUsage = DXGI.UsageRenderTargetOutput,
            BufferCount = 1,
            OutputWindow = windowHandle,
            Windowed = true,
            SwapEffect = SwapEffect.Discard,
            Flags = 0
        };

        D3DFeatureLevel featureLevel;
        
        fixed (ID3D11Device** devicePtr = &_device.Handle)
        fixed (IDXGISwapChain** swapchainPtr = &_swapchain.Handle)
        fixed (ID3D11DeviceContext** contextPtr = &_context.Handle)
        {
            var result = _d3d11.CreateDeviceAndSwapChain(
                (IDXGIAdapter*)null,
                D3DDriverType.Hardware,
                nint.Zero,
                (uint)CreateDeviceFlag.BgraSupport,
                null,
                0,
                D3D11.SdkVersion,
                in swapchainDesc,
                swapchainPtr,
                devicePtr,
                &featureLevel,
                contextPtr
            );

            if (result != 0)
                throw new GraphicsInitializationException(
                    $"Failed to create D3D11 device and swapchain. HRESULT: 0x{result:X8}",
                    "CreateDeviceAndSwapchain",
                    result);
        }

        // 创建 BackBuffer RenderTargetView
        CreateBackBufferRTV();
    }

    private void CreateBackBufferRTV()
    {
        // 获取 BackBuffer
        ComPtr<ID3D11Texture2D> backBuffer = default;
        try
        {
            var result = _swapchain.Handle->GetBuffer<ID3D11Texture2D>(0, out backBuffer);
            if (result != 0)
                throw new GraphicsInitializationException(
                    $"Failed to get swapchain back buffer. HRESULT: 0x{result:X8}",
                    "CreateBackBufferRTV",
                    result);

            // 创建 RenderTargetView
            fixed (ID3D11RenderTargetView** rtvPtr = &_backBufferRTV.Handle)
            {
                result = _device.Handle->CreateRenderTargetView((ID3D11Resource*)backBuffer.Handle, null, rtvPtr);
                if (result != 0)
                    throw new GraphicsInitializationException(
                        $"Failed to create render target view. HRESULT: 0x{result:X8}",
                        "CreateBackBufferRTV",
                        result);
            }
        }
        finally
        {
            backBuffer.Dispose();
        }
    }

    private void CreateDepthStencil()
    {
        // 创建深度模板纹理
        var depthDesc = new Texture2DDesc
        {
            Width = (uint)_width,
            Height = (uint)_height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.FormatD24UnormS8Uint, // Use D24S8 for broad D3D11 hardware compatibility.
            SampleDesc = new SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)BindFlag.DepthStencil,
            CPUAccessFlags = 0,
            MiscFlags = 0
        };

        fixed (ID3D11Texture2D** texPtr = &_depthStencilTexture.Handle)
        {
            var result = _device.Handle->CreateTexture2D(in depthDesc, null, texPtr);
            if (result != 0)
                throw new GraphicsInitializationException(
                    $"Failed to create depth stencil texture. HRESULT: 0x{result:X8}",
                    "CreateDepthStencil",
                    result);
        }

        // 创建深度模板视图
        fixed (ID3D11DepthStencilView** dsvPtr = &_depthStencilView.Handle)
        {
            var result = _device.Handle->CreateDepthStencilView((ID3D11Resource*)_depthStencilTexture.Handle, null, dsvPtr);
            if (result != 0)
                throw new GraphicsInitializationException(
                    $"Failed to create depth stencil view. HRESULT: 0x{result:X8}",
                    "CreateDepthStencil",
                    result);
        }
    }

    private void CreateDepthStencilState()
    {
        var stencilOp = new DepthStencilopDesc
        {
            StencilFailOp = StencilOp.Keep,
            StencilDepthFailOp = StencilOp.Keep,
            StencilPassOp = StencilOp.Keep,
            StencilFunc = ComparisonFunc.Always
        };

        // 1. 默认状态：深度测试和写入都启用
        var depthStencilDesc = new DepthStencilDesc
        {
            DepthEnable = 1, // true
            DepthWriteMask = DepthWriteMask.All,
            DepthFunc = MapDepthComparison(DepthConfig.DepthComparison),
            StencilEnable = 0, // false
            StencilReadMask = 0xFF,
            StencilWriteMask = 0xFF,
            FrontFace = stencilOp,
            BackFace = stencilOp
        };

        fixed (ID3D11DepthStencilState** dssPtr = &_depthStencilState.Handle)
        {
            var result = _device.Handle->CreateDepthStencilState(in depthStencilDesc, dssPtr);
            if (result != 0)
                throw new GraphicsInitializationException(
                    $"Failed to create depth stencil state. HRESULT: 0x{result:X8}",
                    "CreateDepthStencilState",
                    result);
        }

        // 2. 只测试不写入状态
        var testOnlyDesc = new DepthStencilDesc
        {
            DepthEnable = 1,
            DepthWriteMask = DepthWriteMask.Zero, // 不写入深度
            DepthFunc = MapDepthComparison(DepthConfig.DepthComparison),
            StencilEnable = 0,
            StencilReadMask = 0xFF,
            StencilWriteMask = 0xFF,
            FrontFace = stencilOp,
            BackFace = stencilOp
        };

        fixed (ID3D11DepthStencilState** dssPtr = &_depthTestOnlyState.Handle)
        {
            var result = _device.Handle->CreateDepthStencilState(in testOnlyDesc, dssPtr);
            if (result != 0)
                throw new GraphicsInitializationException(
                    $"Failed to create depth test-only state. HRESULT: 0x{result:X8}",
                    "CreateDepthStencilState",
                    result);
        }

        // 3. 禁用深度测试状态
        var disabledDesc = new DepthStencilDesc
        {
            DepthEnable = 0, // 禁用深度测试
            DepthWriteMask = DepthWriteMask.Zero,
            DepthFunc = ComparisonFunc.Always,
            StencilEnable = 0,
            StencilReadMask = 0xFF,
            StencilWriteMask = 0xFF,
            FrontFace = stencilOp,
            BackFace = stencilOp
        };

        fixed (ID3D11DepthStencilState** dssPtr = &_depthDisabledState.Handle)
        {
            var result = _device.Handle->CreateDepthStencilState(in disabledDesc, dssPtr);
            if (result != 0)
                throw new GraphicsInitializationException(
                    $"Failed to create depth disabled state. HRESULT: 0x{result:X8}",
                    "CreateDepthStencilState",
                    result);
        }
    }

    private static ComparisonFunc MapDepthComparison(DepthComparisonFunction comparison)
    {
        return comparison switch
        {
            DepthComparisonFunction.Never => ComparisonFunc.Never,
            DepthComparisonFunction.Less => ComparisonFunc.Less,
            DepthComparisonFunction.Equal => ComparisonFunc.Equal,
            DepthComparisonFunction.LessEqual => ComparisonFunc.LessEqual,
            DepthComparisonFunction.Greater => ComparisonFunc.Greater,
            DepthComparisonFunction.NotEqual => ComparisonFunc.NotEqual,
            DepthComparisonFunction.GreaterEqual => ComparisonFunc.GreaterEqual,
            _ => ComparisonFunc.Always
        };
    }

    /// <summary>
    /// Resizes the swap chain and recreates dependent resources to match the new dimensions.
    /// If either dimension is not positive, the call is silently ignored.
    /// </summary>
    /// <param name="width">The new width of the rendering surface in pixels.</param>
    /// <param name="height">The new height of the rendering surface in pixels.</param>
    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        var previousWidth = _width;
        var previousHeight = _height;
        _context.Handle->OMSetRenderTargets(0, null, null);
        _backBufferRTV.Dispose();
        _depthStencilView.Dispose();
        _depthStencilTexture.Dispose();
        _backBufferRTV = default;
        _depthStencilView = default;
        _depthStencilTexture = default;

        var resizeResult = _testHooks?.ResizeBuffersOverride?.Invoke(width, height)
            ?? _swapchain.Handle->ResizeBuffers(0, (uint)width, (uint)height, Format.FormatUnknown, 0);
        if (resizeResult != 0)
        {
            _width = previousWidth;
            _height = previousHeight;
            CreateBackBufferRTV();
            CreateDepthStencil();
            _commandExecutor.UpdateRenderTargets(_backBufferRTV, _depthStencilView);
            throw new GraphicsInitializationException(
                $"Failed to resize swapchain buffers. HRESULT: 0x{resizeResult:X8}",
                "Resize",
                resizeResult);
        }

        _width = width;
        _height = height;

        // 重新创建资源
        CreateBackBufferRTV();
        CreateDepthStencil();
        _commandExecutor.UpdateRenderTargets(_backBufferRTV, _depthStencilView);
    }

    /// <summary>
    /// Begins a new rendering frame. Binds the back buffer render target and depth-stencil view,
    /// applies the default depth-stencil state, clears the render target with the current clear color,
    /// clears the depth-stencil buffer, and sets the viewport and scissor rect to the current dimensions.
    /// </summary>
    public void BeginFrame()
    {
        // 设置 RenderTarget 和 DepthStencil
        var rtv = _backBufferRTV.Handle;
        var dsv = _depthStencilView.Handle;
        _context.Handle->OMSetRenderTargets(1, &rtv, dsv);

        // 设置深度模板状态
        _context.Handle->OMSetDepthStencilState(_depthStencilState.Handle, 0);

        // 清屏
        var clearColor = _clearColor;
        _context.Handle->ClearRenderTargetView(_backBufferRTV.Handle, (float*)&clearColor);
        _context.Handle->ClearDepthStencilView(_depthStencilView.Handle,
            (uint)(ClearFlag.Depth | ClearFlag.Stencil), DepthConfig.ClearDepthValue, (byte)DepthConfig.ClearStencilValue);

        // 设置 Viewport
        var viewport = new Viewport(0, 0, _width, _height, 0f, 1f);
        _context.Handle->RSSetViewports(1, &viewport);

        // 设置 Scissor Rect
        var scissor = new Box2D<int>(0, 0, _width, _height);
        _context.Handle->RSSetScissorRects(1, &scissor);
    }

    /// <summary>
    /// Ends the current rendering frame by presenting the swap chain with vertical synchronization enabled.
    /// </summary>
    public void EndFrame()
    {
        // 呈现
        _swapchain.Handle->Present(1, 0);
    }

    /// <summary>
    /// Sets the color used to clear the render target at the beginning of each frame.
    /// </summary>
    /// <param name="color">The clear color as a <see cref="Vector4"/> with RGBA components in the range [0, 1].</param>
    public void SetClearColor(Vector4 color)
    {
        _clearColor = color;
    }

    /// <summary>
    /// Gets the Direct3D 11 resource factory used to create GPU resources such as buffers, textures, and shaders.
    /// </summary>
    /// <returns>The <see cref="IResourceFactory"/> implementation for this backend.</returns>
    public IResourceFactory GetResourceFactory() => _resourceFactory;

    /// <summary>
    /// Gets the Direct3D 11 command executor used to issue rendering commands to the GPU.
    /// </summary>
    /// <returns>The <see cref="ICommandExecutor"/> implementation for this backend.</returns>
    public ICommandExecutor GetCommandExecutor() => _commandExecutor;

    /// <summary>
    /// Releases all Direct3D 11 resources including the device, device context, swap chain,
    /// render target view, depth-stencil resources, and the underlying D3D11 and DXGI APIs.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        IsInitialized = false;

        _backBufferRTV.Dispose();
        _depthStencilView.Dispose();
        _depthStencilTexture.Dispose();
        _depthStencilState.Dispose();
        _depthTestOnlyState.Dispose();
        _depthDisabledState.Dispose();
        _swapchain.Dispose();
        _context.Dispose();
        _device.Dispose();

        _d3d11?.Dispose();
        _dxgi?.Dispose();
    }
}
