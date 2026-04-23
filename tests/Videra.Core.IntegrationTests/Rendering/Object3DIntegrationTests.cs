using System.Numerics;
using System.Runtime.InteropServices;
using FluentAssertions;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;
using Videra.Core.Selection.Annotations;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public class Object3DIntegrationTests
{
    [Fact]
    public void Object3D_InitializeUpdateWireframeAndDispose_UsesRealSoftwareResources()
    {
        var mesh = new MeshData
        {
            Vertices = new[]
            {
                new VertexPositionNormalColor(new Vector3(0f, 0.5f, 0f), Vector3.UnitZ, RgbaFloat.Red),
                new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Green),
                new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Blue)
            },
            Indices = new uint[] { 0, 1, 2 },
            Topology = MeshTopology.Triangles
        };

        var factory = new SoftwareResourceFactory();
        using var object3D = new Object3D { Name = "Triangle" };
        var executor = new SoftwareCommandExecutor(new SoftwareFrameBuffer());

        object3D.Initialize(factory, mesh);

        object3D.VertexBuffer.Should().BeOfType<SoftwareBuffer>();
        object3D.IndexBuffer.Should().BeOfType<SoftwareBuffer>();
        object3D.WorldBuffer.Should().BeOfType<SoftwareBuffer>();
        object3D.IndexCount.Should().Be(3);
        object3D.Topology.Should().Be(MeshTopology.Triangles);

        object3D.Position = new Vector3(1f, 2f, 3f);
        object3D.Rotation = new Vector3(0.1f, 0.2f, 0.3f);
        object3D.Scale = new Vector3(2f, 2f, 2f);

        var act = () => object3D.UpdateUniforms(executor);
        act.Should().NotThrow();

        object3D.InitializeWireframe(factory);

        object3D.LineIndexBuffer.Should().BeOfType<SoftwareBuffer>();
        object3D.LineVertexBuffer.Should().BeOfType<SoftwareBuffer>();
        object3D.LineIndexCount.Should().BeGreaterThan(0);

        var recolor = () => object3D.UpdateWireframeColor(new RgbaFloat(1f, 1f, 0f, 1f));
        recolor.Should().NotThrow();
    }

    [Fact]
    public void Object3D_AnnotationAnchorProjection_ResolvesRealObjectIdentity()
    {
        var mesh = new MeshData
        {
            Vertices = new[]
            {
                new VertexPositionNormalColor(new Vector3(0f, 0.5f, 0f), Vector3.UnitZ, RgbaFloat.Red),
                new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Green),
                new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Blue)
            },
            Indices = new uint[] { 0, 1, 2 },
            Topology = MeshTopology.Triangles
        };

        var factory = new SoftwareResourceFactory();
        using var object3D = new Object3D { Name = "Triangle" };
        object3D.Initialize(factory, mesh);

        var camera = new OrbitCamera();
        camera.UpdateProjection(1280, 720);

        var projector = new AnnotationAnchorProjector();
        var anchor = AnnotationAnchorDescriptor.ForObject(object3D.Id);

        var result = projector.Project(anchor, camera, new Vector2(1280f, 720f), [object3D]);

        result.IsVisible.Should().BeTrue();
        result.ResolvedObjectId.Should().Be(object3D.Id);
        result.ClipStatus.Should().Be(AnnotationProjectionClipStatus.Visible);
        object3D.WorldBounds.Should().NotBeNull();
        camera.TryProjectWorldPoint(object3D.WorldBounds!.Value.Center, new Vector2(1280f, 720f), out var expectedScreenPoint)
            .Should().BeTrue();
        result.ScreenPosition.Should().Be(expectedScreenPoint);
    }

    [Fact]
    public void SceneObjectFactory_CreateDeferredRuntimeObjects_UsesRequestedBaseColorTextureCoordinateSet()
    {
        using var backend = CreateSoftwareBackend();
        using var engine = CreateEngine(backend);
        var asset = CreateTexturedQuadAsset(
            "TexCoordSet",
            coordinateSet: 1,
            transform: MaterialTextureTransform.Identity,
            [
                new MeshTextureCoordinateSet(0, [new Vector2(0.25f, 0.5f), new Vector2(0.25f, 0.5f), new Vector2(0.25f, 0.5f), new Vector2(0.25f, 0.5f)]),
                new MeshTextureCoordinateSet(1, [new Vector2(0.75f, 0.5f), new Vector2(0.75f, 0.5f), new Vector2(0.75f, 0.5f), new Vector2(0.75f, 0.5f)])
            ]);

        var runtimeObject = SceneObjectFactory.CreateDeferredRuntimeObjects(asset).Should().ContainSingle().Subject;

        engine.AddObject(runtimeObject);
        engine.Draw();

        ReadCenterPixel(backend).Should().Be(new PixelColor(0, 0, 255, 255));
        runtimeObject.MeshPayload.Should().NotBeNull();
        runtimeObject.MeshPayload!.Vertices.Should().OnlyContain(vertex => vertex.Color == RgbaFloat.Blue);
    }

    [Fact]
    public void SceneObjectFactory_CreateDeferredRuntimeObjects_UsesBaseColorTextureTransform()
    {
        using var backend = CreateSoftwareBackend();
        using var engine = CreateEngine(backend);
        var asset = CreateTexturedQuadAsset(
            "TextureTransform",
            coordinateSet: 0,
            transform: new MaterialTextureTransform(new Vector2(0.5f, 0f), Vector2.One, 0f),
            [
                new MeshTextureCoordinateSet(0, [new Vector2(0.25f, 0.5f), new Vector2(0.25f, 0.5f), new Vector2(0.25f, 0.5f), new Vector2(0.25f, 0.5f)])
            ]);

        var runtimeObject = SceneObjectFactory.CreateDeferredRuntimeObjects(asset).Should().ContainSingle().Subject;

        engine.AddObject(runtimeObject);
        engine.Draw();

        ReadCenterPixel(backend).Should().Be(new PixelColor(0, 0, 255, 255));
        runtimeObject.MeshPayload.Should().NotBeNull();
        runtimeObject.MeshPayload!.Vertices.Should().OnlyContain(vertex => vertex.Color == RgbaFloat.Blue);
    }

    private static SoftwareBackend CreateSoftwareBackend()
    {
        var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        return backend;
    }

    private static VideraEngine CreateEngine(SoftwareBackend backend)
    {
        var engine = new VideraEngine
        {
            BackgroundColor = RgbaFloat.Black
        };

        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.Grid.IsVisible = false;
        engine.ShowAxis = false;
        engine.Camera.SetOrbit(Vector3.Zero, radius: 5f, yaw: 0f, pitch: 0f);
        engine.Camera.UpdateProjection(200, 200);
        return engine;
    }

    private static ImportedSceneAsset CreateTexturedQuadAsset(
        string name,
        int coordinateSet,
        MaterialTextureTransform transform,
        MeshTextureCoordinateSet[] textureCoordinateSets)
    {
        var texture = CreateColorTexture();
        var sampler = CreateClampNearestSampler();
        var baseColorTexture = new MaterialTextureBinding(
            texture.Id,
            sampler.Id,
            coordinateSet,
            TextureColorSpace.Srgb,
            transform);
        var material = new MaterialInstance(
            MaterialInstanceId.New(),
            $"{name}#material0",
            RgbaFloat.White,
            baseColorTexture);
        var mesh = new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(-0.8f, -0.8f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0.8f, -0.8f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0.8f, 0.8f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(-0.8f, 0.8f, 0f), Vector3.UnitZ, RgbaFloat.White)
            ],
            Indices = [0u, 1u, 2u, 0u, 2u, 3u],
            TextureCoordinateSets = textureCoordinateSets,
            Topology = MeshTopology.Triangles
        };
        var primitive = new MeshPrimitive(MeshPrimitiveId.New(), $"{name}#primitive0", mesh, material.Id);
        var node = new SceneNode(SceneNodeId.New(), name, Matrix4x4.Identity, parentId: null, [primitive.Id]);

        return new ImportedSceneAsset(
            $"{name}.gltf",
            name,
            [node],
            [primitive],
            [material],
            [texture],
            [sampler]);
    }

    private static Texture2D CreateColorTexture()
    {
        var contentBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAIAAAABCAYAAAD0In+KAAAADklEQVR4nGP4z8AAQv8BD/kD/YURmXYAAAAASUVORK5CYII=");
        return new Texture2D(
            Texture2DId.New(),
            "RedBlueTexture",
            2,
            1,
            TextureImageFormat.Png,
            contentBytes,
            [
                255, 0, 0, 255,
                0, 0, 255, 255
            ]);
    }

    private static Sampler CreateClampNearestSampler()
    {
        return new Sampler(
            SamplerId.New(),
            "ClampNearest",
            TextureFilter.Nearest,
            TextureFilter.Nearest,
            TextureWrapMode.ClampToEdge,
            TextureWrapMode.ClampToEdge);
    }

    private static PixelColor ReadCenterPixel(SoftwareBackend backend)
    {
        var frame = new byte[backend.Width * backend.Height * 4];
        var handle = Marshal.AllocHGlobal(frame.Length);

        try
        {
            backend.CopyFrameTo(handle, backend.Width * 4);
            Marshal.Copy(handle, frame, 0, frame.Length);
        }
        finally
        {
            Marshal.FreeHGlobal(handle);
        }

        var x = backend.Width / 2;
        var y = backend.Height / 2;
        var offset = ((y * backend.Width) + x) * 4;
        return new PixelColor(frame[offset + 2], frame[offset + 1], frame[offset + 0], frame[offset + 3]);
    }

    private readonly record struct PixelColor(byte R, byte G, byte B, byte A);
}
