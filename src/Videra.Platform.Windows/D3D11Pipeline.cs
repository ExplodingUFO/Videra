using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.Windows;

internal sealed class D3D11Pipeline : IPipeline
{
    private bool _disposed;

    public ComPtr<ID3D11VertexShader> VertexShader { get; }
    public ComPtr<ID3D11PixelShader> PixelShader { get; }
    public ComPtr<ID3D11InputLayout> InputLayout { get; }
    public ComPtr<ID3D11RasterizerState> RasterizerState { get; }
    public ComPtr<ID3D11BlendState> BlendState { get; }

    public D3D11Pipeline(
        ComPtr<ID3D11VertexShader> vertexShader,
        ComPtr<ID3D11PixelShader> pixelShader,
        ComPtr<ID3D11InputLayout> inputLayout,
        ComPtr<ID3D11RasterizerState> rasterizerState,
        ComPtr<ID3D11BlendState> blendState)
    {
        VertexShader = vertexShader;
        PixelShader = pixelShader;
        InputLayout = inputLayout;
        RasterizerState = rasterizerState;
        BlendState = blendState;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        VertexShader.Dispose();
        PixelShader.Dispose();
        InputLayout.Dispose();
        RasterizerState.Dispose();
        BlendState.Dispose();
    }
}
