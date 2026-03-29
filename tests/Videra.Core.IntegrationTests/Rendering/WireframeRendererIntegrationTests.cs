using FluentAssertions;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;
using Videra.Core.Graphics.Wireframe;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public class WireframeRendererIntegrationTests
{
    private static (SoftwareBackend backend, IResourceFactory factory, ICommandExecutor executor, IPipeline pipeline) CreateTestEnvironment()
    {
        var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        var factory = backend.GetResourceFactory();
        var executor = backend.GetCommandExecutor();
        var pipeline = factory.CreatePipeline(VertexPositionNormalColor.SizeInBytes, hasNormals: true, hasColors: true);
        return (backend, factory, executor, pipeline);
    }

    private static Object3D CreateTestObject(IResourceFactory factory)
    {
        var vertices = new[]
        {
            new VertexPositionNormalColor(new System.Numerics.Vector3(-0.5f, -0.5f, 0f), System.Numerics.Vector3.UnitZ, RgbaFloat.Red),
            new VertexPositionNormalColor(new System.Numerics.Vector3(0f, 0.5f, 0f), System.Numerics.Vector3.UnitZ, RgbaFloat.Green),
            new VertexPositionNormalColor(new System.Numerics.Vector3(0.5f, -0.5f, 0f), System.Numerics.Vector3.UnitZ, RgbaFloat.Blue)
        };
        var indices = new uint[] { 0, 1, 2 };
        var mesh = new MeshData { Vertices = vertices, Indices = indices, Topology = MeshTopology.Triangles };
        var obj = new Object3D { Name = "TestTriangle" };
        obj.Initialize(factory, mesh);
        return obj;
    }

    [Fact]
    public void WireframeRenderer_NoneMode_DoesNotRender()
    {
        var (backend, factory, executor, pipeline) = CreateTestEnvironment();
        using var renderer = new WireframeRenderer { Mode = WireframeMode.None };
        renderer.Initialize(factory);

        var camera = new OrbitCamera();
        camera.UpdateProjection(200, 200);
        var objects = new[] { CreateTestObject(factory) };

        var act = () => renderer.RenderWireframes(objects, executor, pipeline, camera, 200, 200);
        act.Should().NotThrow();
        backend.Dispose();
    }

    [Fact]
    public void WireframeRenderer_AllEdgesMode_RendersWireframe()
    {
        var (backend, factory, executor, pipeline) = CreateTestEnvironment();
        using var renderer = new WireframeRenderer { Mode = WireframeMode.AllEdges };
        renderer.Initialize(factory);

        var camera = new OrbitCamera();
        camera.UpdateProjection(200, 200);
        var obj = CreateTestObject(factory);

        backend.BeginFrame();
        var act = () => renderer.RenderWireframes(new[] { obj }, executor, pipeline, camera, 200, 200);
        act.Should().NotThrow();
        backend.EndFrame();
        obj.Dispose();
    }

    [Fact]
    public void WireframeRenderer_VisibleOnlyMode_RendersWireframe()
    {
        var (backend, factory, executor, pipeline) = CreateTestEnvironment();
        using var renderer = new WireframeRenderer { Mode = WireframeMode.VisibleOnly };
        renderer.Initialize(factory);

        var camera = new OrbitCamera();
        camera.UpdateProjection(200, 200);
        var obj = CreateTestObject(factory);

        backend.BeginFrame();
        var act = () => renderer.RenderWireframes(new[] { obj }, executor, pipeline, camera, 200, 200);
        act.Should().NotThrow();
        backend.EndFrame();
        obj.Dispose();
    }

    [Fact]
    public void WireframeRenderer_OverlayMode_RendersWireframe()
    {
        var (backend, factory, executor, pipeline) = CreateTestEnvironment();
        using var renderer = new WireframeRenderer { Mode = WireframeMode.Overlay };
        renderer.Initialize(factory);

        var camera = new OrbitCamera();
        camera.UpdateProjection(200, 200);
        var obj = CreateTestObject(factory);

        backend.BeginFrame();
        var act = () => renderer.RenderWireframes(new[] { obj }, executor, pipeline, camera, 200, 200);
        act.Should().NotThrow();
        backend.EndFrame();
        obj.Dispose();
    }

    [Fact]
    public void WireframeRenderer_WireframeOnlyMode_RendersAndSkipsSolid()
    {
        using var renderer = new WireframeRenderer { Mode = WireframeMode.WireframeOnly };
        renderer.ShouldRenderSolid().Should().BeFalse();

        renderer.Mode = WireframeMode.AllEdges;
        renderer.ShouldRenderSolid().Should().BeTrue();
    }

    [Fact]
    public void WireframeRenderer_NotInitialized_SkipsRendering()
    {
        var (backend, factory, executor, pipeline) = CreateTestEnvironment();
        var renderer = new WireframeRenderer { Mode = WireframeMode.AllEdges };
        // Not initialized - no factory set

        var camera = new OrbitCamera();
        var obj = CreateTestObject(factory);

        var act = () => renderer.RenderWireframes(new[] { obj }, executor, pipeline, camera, 200, 200);
        act.Should().NotThrow();
        obj.Dispose();
        renderer.Dispose();
    }

    [Fact]
    public void WireframeRenderer_Dispose_CleansUp()
    {
        var renderer = new WireframeRenderer();
        renderer.Initialize(new SoftwareResourceFactory());
        renderer.IsInitialized.Should().BeTrue();

        renderer.Dispose();
        renderer.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public void WireframeRenderer_EmptyObjectList_RendersNothing()
    {
        var (_, factory, executor, pipeline) = CreateTestEnvironment();
        using var renderer = new WireframeRenderer { Mode = WireframeMode.AllEdges };
        renderer.Initialize(factory);

        var camera = new OrbitCamera();

        var act = () => renderer.RenderWireframes(Array.Empty<Object3D>(), executor, pipeline, camera, 200, 200);
        act.Should().NotThrow();
    }
}
