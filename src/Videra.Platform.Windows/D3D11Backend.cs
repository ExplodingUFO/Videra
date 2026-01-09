using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Geometry;

namespace Videra.Platform.Windows;

/// <summary>
/// Windows Direct3D 11 图形后端实现
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
    
    private D3D11 _d3d11;
    private DXGI _dxgi;
    
    private Vector4 _clearColor = new(0.1f, 0.1f, 0.15f, 1.0f);
    private int _width;
    private int _height;
    
    private D3D11ResourceFactory _resourceFactory;
    private D3D11CommandExecutor _commandExecutor;

    public bool IsInitialized { get; private set; }

    public void Initialize(IntPtr windowHandle, int width, int height)
    {
        if (IsInitialized) return;

        _width = width;
        _height = height;
        
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
            BufferCount = 2,
            OutputWindow = windowHandle,
            Windowed = true,
            SwapEffect = SwapEffect.FlipDiscard,
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
                throw new Exception($"Failed to create D3D11 device and swapchain. HRESULT: 0x{result:X8}");
        }

        // 创建 BackBuffer RenderTargetView
        CreateBackBufferRTV();
    }

    private void CreateBackBufferRTV()
    {
        // 获取 BackBuffer
        ComPtr<ID3D11Texture2D> backBuffer = default;
        
        var result = _swapchain.Handle->GetBuffer(0, out backBuffer.Handle);
        if (result != 0)
            throw new Exception($"Failed to get swapchain back buffer. HRESULT: 0x{result:X8}");

        // 创建 RenderTargetView
        fixed (ID3D11RenderTargetView** rtvPtr = &_backBufferRTV.Handle)
        {
            result = _device.Handle->CreateRenderTargetView((ID3D11Resource*)backBuffer.Handle, null, rtvPtr);
            if (result != 0)
                throw new Exception($"Failed to create render target view. HRESULT: 0x{result:X8}");
        }

        backBuffer.Dispose();
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
            Format = Format.FormatD24UnormS8Uint,
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
                throw new Exception($"Failed to create depth stencil texture. HRESULT: 0x{result:X8}");
        }

        // 创建深度模板视图
        fixed (ID3D11DepthStencilView** dsvPtr = &_depthStencilView.Handle)
        {
            var result = _device.Handle->CreateDepthStencilView((ID3D11Resource*)_depthStencilTexture.Handle, null, dsvPtr);
            if (result != 0)
                throw new Exception($"Failed to create depth stencil view. HRESULT: 0x{result:X8}");
        }
    }

    private void CreateDepthStencilState()
    {
        var depthStencilDesc = new DepthStencilDesc
        {
            DepthEnable = 1, // true
            DepthWriteMask = DepthWriteMask.All,
            DepthFunc = ComparisonFunc.LessEqual,
            StencilEnable = 0, // false
            StencilReadMask = 0xFF,
            StencilWriteMask = 0xFF,
            FrontFace = new DepthStencilopDesc
            {
                StencilFailOp = StencilOp.Keep,
                StencilDepthFailOp = StencilOp.Keep,
                StencilPassOp = StencilOp.Keep,
                StencilFunc = ComparisonFunc.Always
            },
            BackFace = new DepthStencilopDesc
            {
                StencilFailOp = StencilOp.Keep,
                StencilDepthFailOp = StencilOp.Keep,
                StencilPassOp = StencilOp.Keep,
                StencilFunc = ComparisonFunc.Always
            }
        };

        fixed (ID3D11DepthStencilState** dssPtr = &_depthStencilState.Handle)
        {
            var result = _device.Handle->CreateDepthStencilState(in depthStencilDesc, dssPtr);
            if (result != 0)
                throw new Exception($"Failed to create depth stencil state. HRESULT: 0x{result:X8}");
        }
    }

    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        _width = width;
        _height = height;

        // 释放旧资源
        _backBufferRTV.Dispose();
        _depthStencilView.Dispose();
        _depthStencilTexture.Dispose();

        // 调整 Swapchain 大小
        _swapchain.Handle->ResizeBuffers(0, (uint)width, (uint)height, Format.FormatUnknown, 0);

        // 重新创建资源
        CreateBackBufferRTV();
        CreateDepthStencil();
        _commandExecutor.UpdateRenderTargets(_backBufferRTV, _depthStencilView);
    }

    public void BeginFrame()
    {
        // 设置 RenderTarget 和 DepthStencil
        var rtv = _backBufferRTV.Handle;
        var dsv = _depthStencilView.Handle;
        _context.Handle->OMSetRenderTargets(1, &rtv, dsv);

        // 设置深度模板状态
        _context.Handle->OMSetDepthStencilState(_depthStencilState.Handle, 0);

        // 清屏
        _context.Handle->ClearRenderTargetView(_backBufferRTV.Handle, (float*)&_clearColor);
        _context.Handle->ClearDepthStencilView(_depthStencilView.Handle, 
            (uint)(ClearFlag.Depth | ClearFlag.Stencil), 1.0f, 0);

        // 设置 Viewport
        var viewport = new Viewport(0, 0, _width, _height, 0f, 1f);
        _context.Handle->RSSetViewports(1, &viewport);

        // 设置 Scissor Rect
        var scissor = new Box2D<int>(0, 0, _width, _height);
        _context.Handle->RSSetScissorRects(1, &scissor);
    }

    public void EndFrame()
    {
        // 呈现
        _swapchain.Handle->Present(1, 0);
    }

    public void SetClearColor(Vector4 color)
    {
        _clearColor = color;
    }

    public IResourceFactory GetResourceFactory() => _resourceFactory;

    public ICommandExecutor GetCommandExecutor() => _commandExecutor;

    public void Dispose()
    {
        _backBufferRTV.Dispose();
        _depthStencilView.Dispose();
        _depthStencilTexture.Dispose();
        _depthStencilState.Dispose();
        _swapchain.Dispose();
        _context.Dispose();
        _device.Dispose();
        
        _d3d11?.Dispose();
        _dxgi?.Dispose();
    }
}
