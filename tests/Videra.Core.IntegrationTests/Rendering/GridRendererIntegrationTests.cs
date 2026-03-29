using FluentAssertions;
using Videra.Core.Cameras;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Software;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public class GridRendererIntegrationTests
{
    [Fact]
    public void GridRenderer_Initialize_CreatesBuffers()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 100, 100);
        var factory = backend.GetResourceFactory();

        using var grid = new GridRenderer();
        grid.Initialize(factory);

        grid.IsVisible.Should().BeTrue();
    }

    [Fact]
    public void GridRenderer_Draw_WhenVisible_CompletesWithoutError()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        var factory = backend.GetResourceFactory();
        var executor = backend.GetCommandExecutor();
        using var pipeline = factory.CreatePipeline(
            Videra.Core.Geometry.VertexPositionNormalColor.SizeInBytes,
            hasNormals: true, hasColors: true);

        using var grid = new GridRenderer();
        grid.Initialize(factory);

        var camera = new OrbitCamera();
        camera.UpdateProjection(200, 200);

        var act = () =>
        {
            backend.BeginFrame();
            grid.Draw(executor, pipeline, camera, 200, 200);
            backend.EndFrame();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void GridRenderer_Draw_WhenHidden_SkipsRendering()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 100, 100);
        var factory = backend.GetResourceFactory();
        var executor = backend.GetCommandExecutor();

        using var grid = new GridRenderer { IsVisible = false };
        grid.Initialize(factory);

        var camera = new OrbitCamera();

        // Should not throw even with null pipeline (early return)
        var act = () => grid.Draw(executor, null, camera, 100, 100);
        act.Should().NotThrow();
    }

    [Fact]
    public void GridRenderer_Rebuild_UpdatesBuffers()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 100, 100);
        var factory = backend.GetResourceFactory();

        using var grid = new GridRenderer { Height = 0f };
        grid.Initialize(factory);

        grid.Height = 5f;
        grid.Rebuild();

        // Rebuild should not throw and should work
        var executor = backend.GetCommandExecutor();
        using var pipeline = factory.CreatePipeline(
            Videra.Core.Geometry.VertexPositionNormalColor.SizeInBytes,
            hasNormals: true, hasColors: true);

        var camera = new OrbitCamera();
        camera.UpdateProjection(100, 100);

        var act = () =>
        {
            backend.BeginFrame();
            grid.Draw(executor, pipeline, camera, 100, 100);
            backend.EndFrame();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void GridRenderer_Dispose_CleansUpSafely()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 100, 100);
        var factory = backend.GetResourceFactory();

        var grid = new GridRenderer();
        grid.Initialize(factory);
        grid.Dispose();

        // Double dispose should not throw
        var act = () => grid.Dispose();
        act.Should().NotThrow();
    }
}
