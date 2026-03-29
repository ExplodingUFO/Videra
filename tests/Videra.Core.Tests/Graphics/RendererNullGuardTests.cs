using FluentAssertions;
using Moq;
using Videra.Core.Cameras;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Xunit;

namespace Videra.Core.Tests.Graphics;

public sealed class RendererNullGuardTests
{
    [Fact]
    public void AxisRenderer_Draw_NullCamera_ThrowsArgumentNullException()
    {
        var renderer = new AxisRenderer();
        var act = () => renderer.Draw(
            Mock.Of<ICommandExecutor>(), Mock.Of<IPipeline>(),
            null!, 64, 64, 1f);

        act.Should().Throw<ArgumentNullException>().WithParameterName("camera");
    }

    [Fact]
    public void GridRenderer_Draw_NullCamera_ThrowsArgumentNullException()
    {
        var renderer = new GridRenderer();
        var act = () => renderer.Draw(
            Mock.Of<ICommandExecutor>(), Mock.Of<IPipeline>(),
            null!, 64, 64);

        act.Should().Throw<ArgumentNullException>().WithParameterName("camera");
    }

    [Fact]
    public void Object3D_UpdateUniforms_NullExecutor_ThrowsArgumentNullException()
    {
        var obj = new Object3D();
        var act = () => obj.UpdateUniforms(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("executor");
    }

    [Fact]
    public void Object3D_InitializeWireframe_NullFactory_ThrowsArgumentNullException()
    {
        var obj = new Object3D();
        var act = () => obj.InitializeWireframe(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("factory");
    }
}
