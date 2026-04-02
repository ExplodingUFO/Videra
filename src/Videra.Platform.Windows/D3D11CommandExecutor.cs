using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Videra.Core.Exceptions;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.Windows;

internal unsafe class D3D11CommandExecutor : ICommandExecutor
{
    private static readonly DepthBufferConfiguration DepthConfig = new(
        DepthBufferFormat.Depth24UnormStencil8,
        clearDepthValue: 1.0f,
        clearStencilValue: 0,
        depthComparison: DepthComparisonFunction.LessEqual);

    private const D3DPrimitiveTopology TopologyPointList = (D3DPrimitiveTopology)1;
    private const D3DPrimitiveTopology TopologyLineList = (D3DPrimitiveTopology)2;
    private const D3DPrimitiveTopology TopologyTriangleList = (D3DPrimitiveTopology)4;

    private readonly ComPtr<ID3D11DeviceContext> _context;
    private ID3D11RenderTargetView* _renderTargetView;
    private ID3D11DepthStencilView* _depthStencilView;

    // 深度状态
    private ComPtr<ID3D11DepthStencilState> _depthTestWriteState;    // 默认状态
    private ComPtr<ID3D11DepthStencilState> _depthTestOnlyState;     // 只测试不写入
    private ComPtr<ID3D11DepthStencilState> _depthDisabledState;     // 禁用深度

    public D3D11CommandExecutor(ComPtr<ID3D11DeviceContext> context)
    {
        _context = context;
    }

    public void UpdateRenderTargets(ComPtr<ID3D11RenderTargetView> rtv, ComPtr<ID3D11DepthStencilView> dsv)
    {
        _renderTargetView = rtv.Handle;
        _depthStencilView = dsv.Handle;
    }

    /// <summary>
    /// 初始化深度状态（由Backend调用）
    /// </summary>
    public void InitializeDepthStates(
        ComPtr<ID3D11DepthStencilState> depthTestWrite,
        ComPtr<ID3D11DepthStencilState> depthTestOnly,
        ComPtr<ID3D11DepthStencilState> depthDisabled)
    {
        _depthTestWriteState = depthTestWrite;
        _depthTestOnlyState = depthTestOnly;
        _depthDisabledState = depthDisabled;
    }

    public void SetPipeline(IPipeline pipeline)
    {
        if (pipeline is not D3D11Pipeline d3dPipeline)
            throw new ArgumentException("Pipeline must be a D3D11Pipeline");

        _context.Handle->IASetInputLayout(d3dPipeline.InputLayout.Handle);
        _context.Handle->VSSetShader(d3dPipeline.VertexShader.Handle, null, 0);
        _context.Handle->PSSetShader(d3dPipeline.PixelShader.Handle, null, 0);
        _context.Handle->RSSetState(d3dPipeline.RasterizerState.Handle);
        _context.Handle->IASetPrimitiveTopology(TopologyTriangleList);
    }

    public void SetVertexBuffer(IBuffer buffer, uint index = 0)
    {
        if (buffer is not D3D11Buffer d3dBuffer)
            throw new ArgumentException("Buffer must be a D3D11Buffer");

        if (index == 0)
        {
            var bufferPtr = d3dBuffer.NativeBuffer;
            uint stride = VertexPositionNormalColor.SizeInBytes;
            uint offset = 0;
            _context.Handle->IASetVertexBuffers(0, 1, &bufferPtr, &stride, &offset);
            return;
        }

        // Constant buffers need to be bound to both VS and PS
        var constantBuffer = d3dBuffer.NativeBuffer;
        _context.Handle->VSSetConstantBuffers(index, 1, &constantBuffer);
        _context.Handle->PSSetConstantBuffers(index, 1, &constantBuffer);
    }

    public void SetIndexBuffer(IBuffer buffer)
    {
        if (buffer is not D3D11Buffer d3dBuffer)
            throw new ArgumentException("Buffer must be a D3D11Buffer");

        _context.Handle->IASetIndexBuffer(d3dBuffer.NativeBuffer, Format.FormatR32Uint, 0);
    }

    public void SetResourceSet(uint slot, IResourceSet resourceSet)
    {
        throw new UnsupportedOperationException(
            "Resource sets are not supported on the D3D11 backend. Constant buffers are bound directly through SetVertexBuffer.",
            "SetResourceSet",
            "Windows");
    }

    public void DrawIndexed(uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
    {
        _context.Handle->DrawIndexedInstanced(indexCount, instanceCount, firstIndex, vertexOffset, firstInstance);
    }

    public void DrawIndexed(uint primitiveType, uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
    {
        var topology = MapTopology(primitiveType);
        _context.Handle->IASetPrimitiveTopology(topology);
        _context.Handle->DrawIndexedInstanced(indexCount, instanceCount, firstIndex, vertexOffset, firstInstance);
    }

    public void Draw(uint vertexCount, uint instanceCount = 1, uint firstVertex = 0, uint firstInstance = 0)
    {
        _context.Handle->DrawInstanced(vertexCount, instanceCount, firstVertex, firstInstance);
    }

    public void SetViewport(float x, float y, float width, float height, float minDepth, float maxDepth)
    {
        var viewport = new Viewport(x, y, width, height, minDepth, maxDepth);
        _context.Handle->RSSetViewports(1, &viewport);
    }

    public void SetScissorRect(int x, int y, int width, int height)
    {
        var scissor = new Silk.NET.Maths.Box2D<int>(x, y, x + width, y + height);
        _context.Handle->RSSetScissorRects(1, &scissor);
    }

    public void Clear(float r, float g, float b, float a)
    {
        if (_renderTargetView == null || _depthStencilView == null)
            return;

        var color = stackalloc float[4] { r, g, b, a };
        _context.Handle->ClearRenderTargetView(_renderTargetView, color);
        _context.Handle->ClearDepthStencilView(_depthStencilView, (uint)ClearFlag.Depth, DepthConfig.ClearDepthValue, (byte)DepthConfig.ClearStencilValue);
    }

    private static D3DPrimitiveTopology MapTopology(uint primitiveType)
    {
        return primitiveType switch
        {
            1 => TopologyLineList,
            2 => TopologyPointList,
            _ => TopologyTriangleList
        };
    }

    public void SetDepthState(bool testEnabled, bool writeEnabled)
    {
        if (!testEnabled && _depthDisabledState.Handle != null)
        {
            _context.Handle->OMSetDepthStencilState(_depthDisabledState.Handle, 0);
        }
        else if (testEnabled && !writeEnabled && _depthTestOnlyState.Handle != null)
        {
            _context.Handle->OMSetDepthStencilState(_depthTestOnlyState.Handle, 0);
        }
        else if (_depthTestWriteState.Handle != null)
        {
            _context.Handle->OMSetDepthStencilState(_depthTestWriteState.Handle, 0);
        }
    }

    public void ResetDepthState()
    {
        if (_depthTestWriteState.Handle != null)
        {
            _context.Handle->OMSetDepthStencilState(_depthTestWriteState.Handle, 0);
        }
    }
}
