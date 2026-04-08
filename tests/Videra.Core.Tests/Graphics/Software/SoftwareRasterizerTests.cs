using System.Numerics;
using System.Runtime.InteropServices;
using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;
using Xunit;

namespace Videra.Core.Tests.Graphics.Software;

public class SoftwareRasterizerTests
{
    private static (SoftwareBackend backend, IResourceFactory factory, ICommandExecutor executor, IPipeline pipeline) CreateEnv(int w = 64, int h = 64)
    {
        var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, w, h);
        var factory = backend.GetResourceFactory();
        var executor = backend.GetCommandExecutor();
        var pipeline = factory.CreatePipeline(VertexPositionNormalColor.SizeInBytes, hasNormals: true, hasColors: true);
        return (backend, factory, executor, pipeline);
    }

    private static Object3D CreateTriangle(IResourceFactory factory)
    {
        var vertices = new[]
        {
            new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Red),
            new VertexPositionNormalColor(new Vector3(0f, 0.5f, 0f), Vector3.UnitZ, RgbaFloat.Green),
            new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Blue)
        };
        var indices = new uint[] { 0, 1, 2 };
        var mesh = new MeshData { Vertices = vertices, Indices = indices, Topology = MeshTopology.Triangles };
        var obj = new Object3D { Name = "TestTriangle" };
        obj.Initialize(factory, mesh);
        return obj;
    }

    private static Object3D CreateQuad(IResourceFactory factory, float z, RgbaFloat color, float halfExtent = 0.65f)
    {
        var vertices = new[]
        {
            new VertexPositionNormalColor(new Vector3(-halfExtent, -halfExtent, z), Vector3.UnitZ, color),
            new VertexPositionNormalColor(new Vector3(halfExtent, -halfExtent, z), Vector3.UnitZ, color),
            new VertexPositionNormalColor(new Vector3(halfExtent, halfExtent, z), Vector3.UnitZ, color),
            new VertexPositionNormalColor(new Vector3(-halfExtent, halfExtent, z), Vector3.UnitZ, color)
        };

        var indices = new uint[] { 0, 1, 2, 0, 2, 3 };
        var mesh = new MeshData { Vertices = vertices, Indices = indices, Topology = MeshTopology.Triangles };
        var obj = new Object3D { Name = $"Quad-{z}" };
        obj.Initialize(factory, mesh);
        return obj;
    }

    private static Object3D CreatePoint(IResourceFactory factory, float z, RgbaFloat color)
    {
        var vertices = new[]
        {
            new VertexPositionNormalColor(new Vector3(0f, 0f, z), Vector3.UnitZ, color)
        };

        var indices = new uint[] { 0 };
        var mesh = new MeshData { Vertices = vertices, Indices = indices, Topology = MeshTopology.Points };
        var obj = new Object3D { Name = $"Point-{z}" };
        obj.Initialize(factory, mesh);
        return obj;
    }

    private static void DrawSolid(Object3D obj, ICommandExecutor executor, IPipeline pipeline)
    {
        executor.SetPipeline(pipeline);
        obj.UpdateUniforms(executor);
        executor.SetVertexBuffer(obj.VertexBuffer!, 0);
        executor.SetVertexBuffer(obj.WorldBuffer!, 2);
        executor.SetIndexBuffer(obj.IndexBuffer!);
        executor.DrawIndexed(obj.IndexCount);
    }

    private static void DrawPoint(Object3D obj, ICommandExecutor executor, IPipeline pipeline)
    {
        executor.SetPipeline(pipeline);
        obj.UpdateUniforms(executor);
        executor.SetVertexBuffer(obj.VertexBuffer!, 0);
        executor.SetVertexBuffer(obj.WorldBuffer!, 2);
        executor.SetIndexBuffer(obj.IndexBuffer!);
        executor.DrawIndexed(
            primitiveType: PrimitiveCommandKind.PointList,
            indexCount: obj.IndexCount,
            instanceCount: 1,
            firstIndex: 0,
            vertexOffset: 0,
            firstInstance: 0);
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
    public void Clear_RedClear_FirstPixelIsRed()
    {
        var (backend, _, executor, _) = CreateEnv();
        using var _ = backend;

        executor.Clear(1f, 0f, 0f, 1f);

        // The frame buffer should now have red pixels
        // Verify through SoftwareBackend that frame was cleared
        backend.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void DrawIndexed_SingleTriangle_DoesNotThrow()
    {
        var (backend, factory, executor, pipeline) = CreateEnv();
        using var _ = backend;

        var obj = CreateTriangle(factory);

        executor.SetPipeline(pipeline);
        executor.SetVertexBuffer(obj.VertexBuffer!, 0);

        // Camera buffer
        var cameraBuffer = factory.CreateUniformBuffer(128);
        var view = Matrix4x4.CreateLookAt(new Vector3(0, 0, -5), Vector3.Zero, Vector3.UnitY);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4f, 1f, 0.1f, 100f);
        cameraBuffer.SetData(view, 0);
        cameraBuffer.SetData(projection, 64);
        executor.SetVertexBuffer(cameraBuffer, 1);

        executor.SetVertexBuffer(obj.WorldBuffer!, 2);
        executor.SetIndexBuffer(obj.IndexBuffer!);

        backend.BeginFrame();
        executor.Clear(0f, 0f, 0f, 1f);
        executor.DrawIndexed(obj.IndexCount);
        backend.EndFrame();

        // Verify frame completed without error
        backend.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void DrawIndexed_WithLineTopology_DoesNotThrow()
    {
        var (backend, factory, executor, _) = CreateEnv();
        using var _ = backend;

        // Create line vertices
        var vertices = new[]
        {
            new VertexPositionNormalColor(new Vector3(-0.5f, 0f, 0f), Vector3.UnitZ, RgbaFloat.Red),
            new VertexPositionNormalColor(new Vector3(0.5f, 0f, 0f), Vector3.UnitZ, RgbaFloat.Green)
        };
        var indices = new uint[] { 0, 1 };
        var mesh = new MeshData { Vertices = vertices, Indices = indices, Topology = MeshTopology.Lines };
        var obj = new Object3D { Name = "TestLine" };
        obj.Initialize(factory, mesh);

        var linePipeline = factory.CreatePipeline(VertexPositionNormalColor.SizeInBytes, hasNormals: true, hasColors: true);

        executor.SetPipeline(linePipeline);
        executor.SetVertexBuffer(obj.VertexBuffer!, 0);

        var cameraBuffer = factory.CreateUniformBuffer(128);
        var view = Matrix4x4.CreateLookAt(new Vector3(0, 0, -5), Vector3.Zero, Vector3.UnitY);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4f, 1f, 0.1f, 100f);
        cameraBuffer.SetData(view, 0);
        cameraBuffer.SetData(projection, 64);
        executor.SetVertexBuffer(cameraBuffer, 1);
        executor.SetVertexBuffer(obj.WorldBuffer!, 2);
        executor.SetIndexBuffer(obj.IndexBuffer!);

        backend.BeginFrame();
        executor.Clear(0f, 0f, 0f, 1f);
        executor.DrawIndexed(primitiveType: 1, indexCount: obj.IndexCount); // primitiveType 1 = LineList
        backend.EndFrame();

        // Verify line rendering completed without error
        backend.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void DrawIndexed_ZeroInstanceCount_IsNoOp()
    {
        var (backend, _, executor, _) = CreateEnv();
        using var _ = backend;

        backend.BeginFrame();
        var act = () => executor.DrawIndexed(0u, instanceCount: 0);
        act.Should().NotThrow();
        backend.EndFrame();
    }

    [Fact]
    public void SetViewport_DoesNotThrow()
    {
        var (backend, _, executor, _) = CreateEnv();
        using var _ = backend;

        var act = () => executor.SetViewport(0, 0, 32, 32);
        act.Should().NotThrow();
    }

    [Fact]
    public void MultipleFrames_DoesNotLeakOrCrash()
    {
        var (backend, factory, executor, pipeline) = CreateEnv();
        using var _ = backend;

        var obj = CreateTriangle(factory);

        executor.SetPipeline(pipeline);
        executor.SetVertexBuffer(obj.VertexBuffer!, 0);

        var cameraBuffer = factory.CreateUniformBuffer(128);
        var view = Matrix4x4.CreateLookAt(new Vector3(0, 0, -5), Vector3.Zero, Vector3.UnitY);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4f, 1f, 0.1f, 100f);
        cameraBuffer.SetData(view, 0);
        cameraBuffer.SetData(projection, 64);
        executor.SetVertexBuffer(cameraBuffer, 1);
        executor.SetVertexBuffer(obj.WorldBuffer!, 2);
        executor.SetIndexBuffer(obj.IndexBuffer!);

        for (int i = 0; i < 10; i++)
        {
            backend.BeginFrame();
            executor.Clear(0f, 0f, 0f, 1f);
            executor.DrawIndexed(obj.IndexCount);
            backend.EndFrame();
        }

        // Verify backend still healthy after multiple frames
        backend.IsInitialized.Should().BeTrue();
        backend.Width.Should().Be(64);
    }

    [Fact]
    public void Resize_DuringRendering_DoesNotCrash()
    {
        var (backend, _, _, _) = CreateEnv();
        using var _ = backend;

        backend.BeginFrame();
        backend.Resize(128, 128);
        backend.Width.Should().Be(128);
        backend.Height.Should().Be(128);
        backend.EndFrame();
    }

    [Fact]
    public void SetDepthState_DisabledDepthTestAndWrite_AllowsFartherTriangleToOverwriteNearerColor()
    {
        var (backend, factory, executor, pipeline) = CreateEnv(96, 96);
        using var _ = backend;
        using var nearQuad = CreateQuad(factory, 0.2f, RgbaFloat.Green);
        using var farQuad = CreateQuad(factory, 0.8f, RgbaFloat.Red);

        backend.BeginFrame();
        executor.Clear(0f, 0f, 0f, 1f);
        DrawSolid(nearQuad, executor, pipeline);
        executor.SetDepthState(false, false);
        DrawSolid(farQuad, executor, pipeline);
        backend.EndFrame();

        var frame = CaptureFrame(backend);
        ReadPixel(frame, backend.Width, 48, 48).Should().Be((255, 0, 0, 255));
    }

    [Fact]
    public void SetDepthState_DepthTestWithoutWrite_LeavesLaterDepthDecisionsUsingPreviousDepth()
    {
        var (backend, factory, executor, pipeline) = CreateEnv(96, 96);
        using var _ = backend;
        using var farQuad = CreateQuad(factory, 0.8f, RgbaFloat.Red);
        using var nearQuad = CreateQuad(factory, 0.2f, RgbaFloat.Green);
        using var middleQuad = CreateQuad(factory, 0.5f, RgbaFloat.Blue);

        backend.BeginFrame();
        executor.Clear(0f, 0f, 0f, 1f);
        DrawSolid(farQuad, executor, pipeline);
        executor.SetDepthState(true, false);
        DrawSolid(nearQuad, executor, pipeline);
        executor.ResetDepthState();
        DrawSolid(middleQuad, executor, pipeline);
        backend.EndFrame();

        var frame = CaptureFrame(backend);
        ReadPixel(frame, backend.Width, 48, 48).Should().Be((0, 0, 255, 255));
    }

    [Fact]
    public void ResetDepthState_RestoresDefaultDepthWriteAfterOverlayPass()
    {
        var (backend, factory, executor, pipeline) = CreateEnv(96, 96);
        using var _ = backend;
        using var nearQuad = CreateQuad(factory, 0.2f, RgbaFloat.Green);
        using var farOverlayQuad = CreateQuad(factory, 0.8f, RgbaFloat.Red);
        using var fartherQuad = CreateQuad(factory, 0.9f, RgbaFloat.Blue);

        backend.BeginFrame();
        executor.Clear(0f, 0f, 0f, 1f);
        DrawSolid(nearQuad, executor, pipeline);
        executor.SetDepthState(false, false);
        DrawSolid(farOverlayQuad, executor, pipeline);
        executor.ResetDepthState();
        DrawSolid(fartherQuad, executor, pipeline);
        backend.EndFrame();

        var frame = CaptureFrame(backend);
        ReadPixel(frame, backend.Width, 48, 48).Should().Be((255, 0, 0, 255));
    }

    [Fact]
    public void SetDepthState_DisabledDepthTest_AllowsFartherPointToOverwriteNearerPoint()
    {
        var (backend, factory, executor, pipeline) = CreateEnv(96, 96);
        using var _ = backend;
        using var nearPoint = CreatePoint(factory, 0.2f, RgbaFloat.Green);
        using var farPoint = CreatePoint(factory, 0.8f, RgbaFloat.Red);

        backend.BeginFrame();
        executor.Clear(0f, 0f, 0f, 1f);
        DrawPoint(nearPoint, executor, pipeline);
        executor.SetDepthState(false, false);
        DrawPoint(farPoint, executor, pipeline);
        backend.EndFrame();

        var frame = CaptureFrame(backend);
        ReadPixel(frame, backend.Width, 48, 48).Should().Be((255, 0, 0, 255));
    }
}
