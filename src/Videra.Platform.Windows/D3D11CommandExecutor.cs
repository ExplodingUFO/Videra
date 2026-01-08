using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.Windows;

internal unsafe class D3D11CommandExecutor : ICommandExecutor
{
    private readonly ComPtr<ID3D11DeviceContext> _context;
    private readonly D3D11 _d3d11;

    public D3D11CommandExecutor(ComPtr<ID3D11DeviceContext> context, D3D11 d3d11)
    {
        _context = context;
        _d3d11 = d3d11;
    }

    public void SetPipeline(IPipeline pipeline)
    {
        // TODO: Implement pipeline binding
        throw new NotImplementedException();
    }

    public void SetVertexBuffer(IBuffer buffer)
    {
        if (buffer is not D3D11Buffer d3dBuffer)
            throw new ArgumentException("Buffer must be a D3D11Buffer");

        var bufferPtr = d3dBuffer.NativeBuffer;
        uint stride = 40; // sizeof(VertexPositionNormalColor) = 40 bytes
        uint offset = 0;
        
        _context.Handle->IASetVertexBuffers(0, 1, &bufferPtr, &stride, &offset);
    }

    public void SetIndexBuffer(IBuffer buffer)
    {
        if (buffer is not D3D11Buffer d3dBuffer)
            throw new ArgumentException("Buffer must be a D3D11Buffer");

        _context.Handle->IASetIndexBuffer(d3dBuffer.NativeBuffer, Format.FormatR32Uint, 0);
    }

    public void SetResourceSet(uint slot, IResourceSet resourceSet)
    {
        // TODO: Implement resource set binding
        throw new NotImplementedException();
    }

    public void DrawIndexed(uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
    {
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
        // TODO: 需要获取RenderTargetView和DepthStencilView来清除
        // 这需要在Backend中保存这些视图的引用
        Console.WriteLine($"[D3D11] Clear called with color ({r}, {g}, {b}, {a})");
    }
}
