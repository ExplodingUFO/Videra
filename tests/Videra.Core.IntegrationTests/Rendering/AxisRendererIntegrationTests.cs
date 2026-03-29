using FluentAssertions;
using Videra.Core.Cameras;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Software;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public class AxisRendererIntegrationTests
{
    [Fact]
    public void AxisRenderer_Initialize_CreatesBuffers()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 100, 100);
        var factory = backend.GetResourceFactory();

        using var axis = new AxisRenderer();
        axis.Initialize(factory);

        axis.IsVisible.Should().BeTrue();
    }

    [Fact]
    public void AxisRenderer_Draw_WhenVisible_CompletesWithoutError()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        var factory = backend.GetResourceFactory();
        var executor = backend.GetCommandExecutor();
        using var pipeline = factory.CreatePipeline(
            Videra.Core.Geometry.VertexPositionNormalColor.SizeInBytes,
            hasNormals: true, hasColors: true);

        using var axis = new AxisRenderer();
        axis.Initialize(factory);

        var camera = new OrbitCamera();
        camera.UpdateProjection(200, 200);

        var act = () =>
        {
            backend.BeginFrame();
            axis.Draw(executor, pipeline, camera, 200, 200, 1.0f);
            backend.EndFrame();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void AxisRenderer_Draw_WhenHidden_SkipsRendering()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 100, 100);
        var factory = backend.GetResourceFactory();

        using var axis = new AxisRenderer { IsVisible = false };
        axis.Initialize(factory);

        var camera = new OrbitCamera();

        // Should not throw even with null executor (early return)
        var act = () => axis.Draw(null, null, camera, 100, 100, 1.0f);
        act.Should().NotThrow();
    }

    [Fact]
    public void AxisRenderer_CustomColors_AreApplied()
    {
        using var axis = new AxisRenderer();
        axis.XColor = new Videra.Core.Geometry.RgbaFloat(1, 0, 0, 1);
        axis.YColor = new Videra.Core.Geometry.RgbaFloat(0, 1, 0, 1);
        axis.ZColor = new Videra.Core.Geometry.RgbaFloat(0, 0, 1, 1);

        axis.XColor.R.Should().Be(1f);
        axis.YColor.G.Should().Be(1f);
        axis.ZColor.B.Should().Be(1f);
    }

    [Fact]
    public void AxisRenderer_Dispose_CleansUpSafely()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 100, 100);
        var factory = backend.GetResourceFactory();

        var axis = new AxisRenderer();
        axis.Initialize(factory);
        axis.Dispose();

        // Double dispose should not throw
        var act = () => axis.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void AxisRenderer_Draw_MultipleFrames_CompletesSuccessfully()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        var factory = backend.GetResourceFactory();
        var executor = backend.GetCommandExecutor();
        using var pipeline = factory.CreatePipeline(
            Videra.Core.Geometry.VertexPositionNormalColor.SizeInBytes,
            hasNormals: true, hasColors: true);

        using var axis = new AxisRenderer();
        axis.Initialize(factory);

        var camera = new OrbitCamera();
        camera.UpdateProjection(200, 200);

        var act = () =>
        {
            for (int i = 0; i < 5; i++)
            {
                backend.BeginFrame();
                axis.Draw(executor, pipeline, camera, 200, 200, 1.0f);
                backend.EndFrame();
            }
        };

        act.Should().NotThrow();
    }
}
