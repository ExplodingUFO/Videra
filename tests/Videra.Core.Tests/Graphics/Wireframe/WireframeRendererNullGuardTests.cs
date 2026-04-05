using FluentAssertions;
using Moq;
using Videra.Core.Cameras;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Wireframe;
using Xunit;

namespace Videra.Core.Tests.Graphics.Wireframe;

public sealed class WireframeRendererNullGuardTests : IDisposable
{
    private readonly WireframeRenderer _renderer = new();

    [Fact]
    public void RenderWireframes_NullObjects_ThrowsArgumentNullException()
    {
        var act = () => _renderer.RenderWireframes(
            null!, Mock.Of<ICommandExecutor>(), Mock.Of<IPipeline>(),
            new OrbitCamera(), 64, 64);

        act.Should().Throw<ArgumentNullException>().WithParameterName("objects");
    }

    [Fact]
    public void RenderWireframes_NullExecutor_ThrowsArgumentNullException()
    {
        var act = () => _renderer.RenderWireframes(
            Array.Empty<Object3D>(), null!, Mock.Of<IPipeline>(),
            new OrbitCamera(), 64, 64);

        act.Should().Throw<ArgumentNullException>().WithParameterName("executor");
    }

    [Fact]
    public void RenderWireframes_NullPipeline_ThrowsArgumentNullException()
    {
        var act = () => _renderer.RenderWireframes(
            Array.Empty<Object3D>(), Mock.Of<ICommandExecutor>(), null!,
            new OrbitCamera(), 64, 64);

        act.Should().Throw<ArgumentNullException>().WithParameterName("pipeline");
    }

    [Fact]
    public void RenderWireframes_NullCamera_ThrowsArgumentNullException()
    {
        var act = () => _renderer.RenderWireframes(
            Array.Empty<Object3D>(), Mock.Of<ICommandExecutor>(), Mock.Of<IPipeline>(),
            null!, 64, 64);

        act.Should().Throw<ArgumentNullException>().WithParameterName("camera");
    }

    [Fact]
    public void Initialize_NullFactory_ThrowsArgumentNullException()
    {
        var act = () => _renderer.Initialize(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("factory");
    }

    public void Dispose()
    {
        _renderer.Dispose();
        GC.SuppressFinalize(this);
    }
}
