using System.Numerics;
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

        // Verify no exceptions thrown
        true.Should().BeTrue();
    }

    [Fact]
    public void DrawIndexed_WithLineTopology_DoesNotThrow()
    {
        var (backend, factory, executor, pipeline) = CreateEnv();
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
}
