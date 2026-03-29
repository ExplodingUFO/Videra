using Silk.NET.Direct3D11;
using FluentAssertions;
using Videra.Core.Exceptions;
using Videra.Platform.Windows;
using Xunit;

namespace Videra.Platform.Windows.Tests.Backend;

public class D3D11UnsupportedOperationTests
{
    [Fact]
    public void CreateShader_ThrowsUnsupportedOperationException()
    {
        using var factory = new D3D11ResourceFactory(default, default, D3D11.GetApi());

        var act = () => factory.CreateShader(
            Videra.Core.Graphics.Abstractions.ShaderStage.Vertex,
            Array.Empty<byte>(),
            "main");

        act.Should().Throw<UnsupportedOperationException>()
            .Which.Platform.Should().Be("Windows");
    }

    [Fact]
    public void CreateResourceSet_ThrowsUnsupportedOperationException()
    {
        using var factory = new D3D11ResourceFactory(default, default, D3D11.GetApi());

        var act = () => factory.CreateResourceSet(
            new Videra.Core.Graphics.Abstractions.ResourceSetDescription());

        act.Should().Throw<UnsupportedOperationException>()
            .Which.Platform.Should().Be("Windows");
    }

    [Fact]
    public void SetResourceSet_ThrowsUnsupportedOperationException()
    {
        using var context = new ComPtr<ID3D11DeviceContext>();
        using var executor = new D3D11CommandExecutor(context);

        var act = () => executor.SetResourceSet(0, null!);

        act.Should().Throw<UnsupportedOperationException>()
            .Which.Platform.Should().Be("Windows");
    }
}
