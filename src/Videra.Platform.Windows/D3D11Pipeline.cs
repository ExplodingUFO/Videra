using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.Windows;

internal sealed class D3D11Pipeline : IPipeline
{
    public ComPtr<ID3D11VertexShader> VertexShader { get; }
    public ComPtr<ID3D11PixelShader> PixelShader { get; }
    public ComPtr<ID3D11InputLayout> InputLayout { get; }
    public ComPtr<ID3D11RasterizerState> RasterizerState { get; }

    public D3D11Pipeline(
        ComPtr<ID3D11VertexShader> vertexShader,
        ComPtr<ID3D11PixelShader> pixelShader,
        ComPtr<ID3D11InputLayout> inputLayout,
        ComPtr<ID3D11RasterizerState> rasterizerState)
    {
        VertexShader = vertexShader;
        PixelShader = pixelShader;
        InputLayout = inputLayout;
        RasterizerState = rasterizerState;
    }

    public void Dispose()
    {
        VertexShader.Dispose();
        PixelShader.Dispose();
        InputLayout.Dispose();
        RasterizerState.Dispose();
    }
}
