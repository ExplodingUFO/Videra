using FluentAssertions;
using System.Runtime.InteropServices;
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

    private static Object3D CreateQuad(IResourceFactory factory, float z, RgbaFloat color, float halfExtent = 0.8f)
    {
        var vertices = new[]
        {
            new VertexPositionNormalColor(new System.Numerics.Vector3(-halfExtent, -halfExtent, z), System.Numerics.Vector3.UnitZ, color),
            new VertexPositionNormalColor(new System.Numerics.Vector3(halfExtent, -halfExtent, z), System.Numerics.Vector3.UnitZ, color),
            new VertexPositionNormalColor(new System.Numerics.Vector3(halfExtent, halfExtent, z), System.Numerics.Vector3.UnitZ, color),
            new VertexPositionNormalColor(new System.Numerics.Vector3(-halfExtent, halfExtent, z), System.Numerics.Vector3.UnitZ, color)
        };
        var indices = new uint[] { 0, 1, 2, 0, 2, 3 };
        var mesh = new MeshData { Vertices = vertices, Indices = indices, Topology = MeshTopology.Triangles };
        var obj = new Object3D { Name = $"Quad-{z}" };
        obj.Initialize(factory, mesh);
        return obj;
    }

    private static void DrawSolid(Object3D obj, ICommandExecutor executor, IPipeline pipeline)
    {
        executor.SetPipeline(pipeline);
        obj.UpdateUniforms(executor);
        executor.SetVertexBuffer(obj.VertexBuffer!, RenderBindingSlots.Vertex);
        executor.SetVertexBuffer(obj.WorldBuffer!, RenderBindingSlots.World);
        executor.SetIndexBuffer(obj.IndexBuffer!);
        executor.DrawIndexed(obj.IndexCount);
    }

    private static byte[] CaptureFrame(SoftwareBackend backend)
    {
        var bytes = new byte[backend.Width * backend.Height * 4];
        var handle = Marshal.AllocHGlobal(bytes.Length);

        try
        {
            backend.CopyFrameTo(handle, backend.Width * 4);
            Marshal.Copy(handle, bytes, 0, bytes.Length);
            return bytes;
        }
        finally
        {
            Marshal.FreeHGlobal(handle);
        }
    }

    private static (byte r, byte g, byte b, byte a) ReadPixel(byte[] frame, int width, int x, int y)
    {
        var offset = ((y * width) + x) * 4;
        return (frame[offset + 2], frame[offset + 1], frame[offset], frame[offset + 3]);
    }

    [Fact]
    public void WireframeRenderer_NoneMode_DoesNotRender()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        var factory = backend.GetResourceFactory();
        var executor = backend.GetCommandExecutor();
        var pipeline = factory.CreatePipeline(VertexPositionNormalColor.SizeInBytes, hasNormals: true, hasColors: true);
        using var renderer = new WireframeRenderer { Mode = WireframeMode.None };
        renderer.Initialize(factory);

        var camera = new OrbitCamera();
        camera.UpdateProjection(200, 200);
        using var obj = CreateTestObject(factory);
        obj.InitializeWireframe(factory);

        var act = () => renderer.RenderWireframes(new[] { obj }, executor, pipeline, camera, 200, 200);
        act.Should().NotThrow();
    }

    [Fact]
    public void WireframeRenderer_OverlayMode_DrawsOccludedLinesOverFrontGeometry()
    {
        var (backend, factory, executor, pipeline) = CreateTestEnvironment();
        using var renderer = new WireframeRenderer
        {
            Mode = WireframeMode.Overlay,
            LineColor = new RgbaFloat(0f, 0f, 0f, 1f)
        };
        renderer.Initialize(factory);

        using var occluder = CreateQuad(factory, 0.2f, RgbaFloat.White, 0.25f);
        using var wireframeTarget = CreateQuad(factory, 0.8f, RgbaFloat.White);
        wireframeTarget.InitializeWireframe(factory);

        backend.BeginFrame();
        executor.Clear(0f, 0f, 0f, 1f);
        DrawSolid(occluder, executor, pipeline);
        renderer.RenderWireframes(new[] { wireframeTarget }, executor, pipeline, new OrbitCamera(), 200, 200);
        backend.EndFrame();

        var frame = CaptureFrame(backend);
        ReadPixel(frame, backend.Width, 100, 100).Should().Be((0, 0, 0, 255));
        backend.Dispose();
    }

    [Fact]
    public void WireframeRenderer_VisibleOnlyMode_HidesOccludedLinesBehindFrontGeometry()
    {
        var (backend, factory, executor, pipeline) = CreateTestEnvironment();
        using var renderer = new WireframeRenderer { Mode = WireframeMode.VisibleOnly };
        renderer.Initialize(factory);

        using var occluder = CreateQuad(factory, 0.2f, RgbaFloat.White, 0.25f);
        using var wireframeTarget = CreateQuad(factory, 0.8f, RgbaFloat.White);
        wireframeTarget.InitializeWireframe(factory);

        backend.BeginFrame();
        executor.Clear(0f, 0f, 0f, 1f);
        DrawSolid(occluder, executor, pipeline);
        renderer.RenderWireframes(new[] { wireframeTarget }, executor, pipeline, new OrbitCamera(), 200, 200);
        backend.EndFrame();

        var frame = CaptureFrame(backend);
        ReadPixel(frame, backend.Width, 100, 100).Should().Be((255, 255, 255, 255));
        backend.Dispose();
    }

    [Fact]
    public void WireframeRenderer_AllEdgesMode_UsesHiddenLineColorForOccludedSegments()
    {
        var (backend, factory, executor, pipeline) = CreateTestEnvironment();
        using var renderer = new WireframeRenderer
        {
            Mode = WireframeMode.AllEdges,
            LineColor = new RgbaFloat(0f, 0f, 0f, 1f),
            HiddenLineColor = new RgbaFloat(0.5f, 0.5f, 0.5f, 0.3f)
        };
        renderer.Initialize(factory);

        using var occluder = CreateQuad(factory, 0.2f, RgbaFloat.White, 0.25f);
        using var wireframeTarget = CreateQuad(factory, 0.8f, RgbaFloat.White);
        wireframeTarget.InitializeWireframe(factory);

        backend.BeginFrame();
        executor.Clear(0f, 0f, 0f, 1f);
        DrawSolid(occluder, executor, pipeline);
        renderer.RenderWireframes(new[] { wireframeTarget }, executor, pipeline, new OrbitCamera(), 200, 200);
        backend.EndFrame();

        var frame = CaptureFrame(backend);
        ReadPixel(frame, backend.Width, 100, 100).Should().Be((217, 217, 217, 255));
        backend.Dispose();
    }

    [Fact]
    public void WireframeRenderer_WireframeOnlyMode_WritesDepthSoFartherGeometryCannotOverwriteVisibleEdges()
    {
        var (backend, factory, executor, pipeline) = CreateTestEnvironment();
        using var renderer = new WireframeRenderer
        {
            Mode = WireframeMode.WireframeOnly,
            LineColor = new RgbaFloat(0f, 0f, 0f, 1f)
        };
        renderer.Initialize(factory);

        using var wireframeTarget = CreateQuad(factory, 0.8f, RgbaFloat.White);
        using var fartherSolid = CreateQuad(factory, 0.95f, RgbaFloat.Red);
        wireframeTarget.InitializeWireframe(factory);

        backend.BeginFrame();
        executor.Clear(0f, 0f, 0f, 1f);
        renderer.RenderWireframes(new[] { wireframeTarget }, executor, pipeline, new OrbitCamera(), 200, 200);
        DrawSolid(fartherSolid, executor, pipeline);
        backend.EndFrame();

        var frame = CaptureFrame(backend);
        ReadPixel(frame, backend.Width, 60, 140).Should().Be((0, 0, 0, 255));
        renderer.ShouldRenderSolid().Should().BeFalse();
        backend.Dispose();
    }

    [Fact]
    public void WireframeRenderer_NotInitialized_SkipsRendering()
    {
        var (_, factory, executor, pipeline) = CreateTestEnvironment();
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
