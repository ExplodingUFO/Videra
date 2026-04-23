using System.Numerics;
using System.Runtime.InteropServices;
using FluentAssertions;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;
using Videra.Core.Graphics.RenderPipeline;
using Videra.Core.Graphics.Wireframe;
using Videra.Core.Scene;
using Videra.Core.Styles.Presets;
using Xunit;

namespace Videra.Core.IntegrationTests.Rendering;

public class VideraEngineIntegrationTests
{
    [Fact]
    public void VideraEngine_Initialize_WithSoftwareBackend_SetsInitialized()
    {
        using var backend = new SoftwareBackend();
        using var engine = new VideraEngine();

        engine.Initialize(backend);

        engine.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void VideraEngine_Draw_WithoutInitialize_ReturnsSilently()
    {
        using var engine = new VideraEngine();

        // Should not throw
        var act = () => engine.Draw();
        act.Should().NotThrow();
    }

    [Fact]
    public void VideraEngine_Resize_AfterInit_Succeeds()
    {
        using var backend = new SoftwareBackend();
        using var engine = new VideraEngine();
        engine.Initialize(backend);

        var act = () => engine.Resize(200, 150);
        act.Should().NotThrow();
    }

    [Fact]
    public void VideraEngine_AddObject_AndDraw_Completes()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);

        var factory = backend.GetResourceFactory();
        var cube = DemoMeshFactory.CreateTestCube(factory);

        engine.AddObject(cube);

        var act = () => engine.Draw();
        act.Should().NotThrow();
    }

    [Fact]
    public void VideraEngine_RemoveObject_DisposesResources()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);

        var factory = backend.GetResourceFactory();
        var cube = DemoMeshFactory.CreateTestCube(factory);

        engine.AddObject(cube);
        engine.RemoveObject(cube);

        // Engine should still draw without error
        var act = () => engine.Draw();
        act.Should().NotThrow();
    }

    [Fact]
    public void VideraEngine_ClearObjects_RemovesAllObjects()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);

        var factory = backend.GetResourceFactory();
        engine.AddObject(DemoMeshFactory.CreateTestCube(factory));

        engine.ClearObjects();

        var act = () => engine.Draw();
        act.Should().NotThrow();
    }

    [Fact]
    public void VideraEngine_MultipleDrawCycles_CompleteSuccessfully()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);

        var factory = backend.GetResourceFactory();
        engine.AddObject(DemoMeshFactory.CreateTestCube(factory));

        var act = () =>
        {
            for (int i = 0; i < 5; i++)
            {
                engine.Draw();
            }
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void VideraEngine_WireframeMode_OverlayRenders()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);

        var factory = backend.GetResourceFactory();
        engine.AddObject(DemoMeshFactory.CreateTestCube(factory));
        engine.Wireframe.Mode = WireframeMode.AllEdges;

        var act = () => engine.Draw();
        act.Should().NotThrow();
    }

    [Fact]
    public void VideraEngine_WireframePreset_WithoutExplicitOverride_UsesWireframeOnlyPassSelection()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine
        {
            BackgroundColor = RgbaFloat.Blue
        };
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.Grid.IsVisible = false;
        engine.ShowAxis = false;

        var quad = DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory());
        engine.AddObject(quad);
        engine.StyleService.ApplyPreset(RenderStylePreset.Wireframe);

        engine.Draw();

        var frame = DemoMeshFactory.CaptureFrame(backend);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.White).Should().Be(0);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.Black).Should().BeGreaterThan(0);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.Blue).Should().BeGreaterThan(0);
    }

    [Fact]
    public void VideraEngine_ExplicitWireframeMode_OverridesStyleDrivenWireframeDefault()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine
        {
            BackgroundColor = RgbaFloat.Blue
        };
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.Grid.IsVisible = false;
        engine.ShowAxis = false;

        var quad = DemoMeshFactory.CreateWhiteQuad(backend.GetResourceFactory());
        engine.AddObject(quad);
        engine.StyleService.ApplyPreset(RenderStylePreset.Wireframe);
        engine.Wireframe.Mode = WireframeMode.Overlay;

        engine.Draw();

        var frame = DemoMeshFactory.CaptureFrame(backend);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.White).Should().BeGreaterThan(0);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.Black).Should().BeGreaterThan(0);
    }

    [Fact]
    public void VideraEngine_MaskedObject_BelowCutoff_DoesNotRenderOnSoftwareBackend()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine
        {
            BackgroundColor = RgbaFloat.Blue
        };
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.Grid.IsVisible = false;
        engine.ShowAxis = false;

        var maskedQuad = DemoMeshFactory.CreateMaskedQuad(alpha: 0.25f, cutoff: 0.5f);
        engine.AddObject(maskedQuad);

        engine.Draw();

        var frame = DemoMeshFactory.CaptureFrame(backend);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.White).Should().Be(0);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.Blue).Should().BeGreaterThan(0);
    }

    [Fact]
    public void VideraEngine_MaskedObject_AboveCutoff_RendersAsOpaqueOnSoftwareBackend()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine
        {
            BackgroundColor = RgbaFloat.Blue
        };
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.Grid.IsVisible = false;
        engine.ShowAxis = false;

        var maskedQuad = DemoMeshFactory.CreateMaskedQuad(alpha: 0.75f, cutoff: 0.5f);
        engine.AddObject(maskedQuad);

        engine.Draw();

        var frame = DemoMeshFactory.CaptureFrame(backend);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.White).Should().BeGreaterThan(0);
        DemoMeshFactory.CountPixels(frame, DemoMeshFactory.PixelColor.Blue).Should().BeGreaterThan(0);
    }

    [Fact]
    public void VideraEngine_BlendedObjects_RenderInDeterministicBackToFrontOrder_OnSoftwareBackend()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine
        {
            BackgroundColor = RgbaFloat.Black
        };
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.Grid.IsVisible = false;
        engine.ShowAxis = false;
        engine.Camera.SetOrbit(Vector3.Zero, radius: 5f, yaw: 0f, pitch: 0f);
        engine.Camera.UpdateProjection(200, 200);

        var nearBlue = DemoMeshFactory.CreateBlendedQuad(new RgbaFloat(0f, 0f, 1f, 0.5f), new Vector3(0f, 0f, 0.5f));
        var farRed = DemoMeshFactory.CreateBlendedQuad(new RgbaFloat(1f, 0f, 0f, 0.5f), new Vector3(0f, 0f, -0.5f));

        engine.AddObject(nearBlue);
        engine.AddObject(farRed);

        engine.Draw();

        var frame = DemoMeshFactory.CaptureFrame(backend);
        var pixel = DemoMeshFactory.ReadPixel(frame, backend.Width, backend.Width / 2, backend.Height / 2);
        engine.LastPipelineSnapshot.Should().NotBeNull();
        engine.LastPipelineSnapshot!.TransparentObjectCount.Should().Be(2);
        pixel.B.Should().BeGreaterThan(pixel.R);
        pixel.A.Should().BeGreaterThan((byte)0);
    }

    [Fact]
    public void VideraEngine_MixedOpaqueAndMaskSegments_RenderThroughOpaquePass_OnSoftwareBackend()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine
        {
            BackgroundColor = RgbaFloat.Black
        };
        engine.Initialize(backend);
        engine.Resize(200, 200);
        engine.Grid.IsVisible = false;
        engine.ShowAxis = false;
        engine.Camera.SetOrbit(Vector3.Zero, radius: 5f, yaw: 0f, pitch: 0f);
        engine.Camera.UpdateProjection(200, 200);

        var opaqueMaterial = new MaterialInstance(
            MaterialInstanceId.New(),
            "opaque",
            new RgbaFloat(1f, 0f, 0f, 1f),
            alpha: new MaterialAlphaSettings(MaterialAlphaMode.Opaque, 0f, false));
        var maskedMaterial = new MaterialInstance(
            MaterialInstanceId.New(),
            "mask",
            new RgbaFloat(0f, 0f, 1f, 0.5f),
            alpha: new MaterialAlphaSettings(MaterialAlphaMode.Mask, 0.5f, true));
        var opaqueMesh = new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, 0f), Vector3.UnitZ, new RgbaFloat(1f, 0f, 0f, 1f)),
                new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, 0f), Vector3.UnitZ, new RgbaFloat(1f, 0f, 0f, 1f)),
                new VertexPositionNormalColor(new Vector3(0f, 0.5f, 0f), Vector3.UnitZ, new RgbaFloat(1f, 0f, 0f, 1f))
            ],
            Indices = [0u, 1u, 2u],
            Topology = MeshTopology.Triangles
        };
        var maskedMesh = new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, 0.1f), Vector3.UnitZ, new RgbaFloat(0f, 0f, 1f, 0.5f)),
                new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, 0.1f), Vector3.UnitZ, new RgbaFloat(0f, 0f, 1f, 0.5f)),
                new VertexPositionNormalColor(new Vector3(0f, 0.5f, 0.1f), Vector3.UnitZ, new RgbaFloat(0f, 0f, 1f, 0.5f))
            ],
            Indices = [0u, 1u, 2u],
            Topology = MeshTopology.Triangles
        };
        var opaquePrimitive = new MeshPrimitive(MeshPrimitiveId.New(), "mixed#primitive0", opaqueMesh, opaqueMaterial.Id);
        var maskPrimitive = new MeshPrimitive(MeshPrimitiveId.New(), "mixed#primitive1", maskedMesh, maskedMaterial.Id);
        var rootNode = new SceneNode(SceneNodeId.New(), "mixed", Matrix4x4.Identity, parentId: null, [opaquePrimitive.Id, maskPrimitive.Id]);
        var asset = new ImportedSceneAsset(
            "mixed.gltf",
            "mixed.gltf",
            [rootNode],
            [opaquePrimitive, maskPrimitive],
            [opaqueMaterial, maskedMaterial]);

        engine.AddObject(SceneObjectFactory.CreateDeferred(asset));
        engine.Draw();

        engine.LastPipelineSnapshot.Should().NotBeNull();
        engine.LastPipelineSnapshot!.ActiveFeatures.Should().Be(RenderFeatureSet.Opaque);
    }

    [Fact]
    public void VideraEngine_ShowAxis_RendersAxisOverlay()
    {
        using var backend = new SoftwareBackend();
        backend.Initialize(IntPtr.Zero, 200, 200);
        using var engine = new VideraEngine();
        engine.Initialize(backend);
        engine.ShowAxis = true;

        var act = () => engine.Draw();
        act.Should().NotThrow();
    }

    [Fact]
    public void VideraEngine_Dispose_CleansUp()
    {
        using var backend = new SoftwareBackend();
        var engine = new VideraEngine();
        engine.Initialize(backend);

        var act = () => engine.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void VideraEngine_AfterDispose_PublicEntryPoints_AreNoOpsAndDoNotReinitialize()
    {
        using var firstBackend = new SoftwareBackend();
        var engine = new VideraEngine();
        engine.Initialize(firstBackend);
        engine.Dispose();

        using var secondBackend = new SoftwareBackend();
        var obj = new Object3D { Name = "AfterDisposeObject" };

        var act = () =>
        {
            engine.Dispose();
            engine.Draw();
            engine.Resize(320, 240);
            engine.AddObject(obj);
            engine.RemoveObject(obj);
            engine.ClearObjects();
            engine.Initialize(secondBackend);
            engine.Draw();
        };

        act.Should().NotThrow();
        engine.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public void VideraEngine_StyleService_AppliesPreset()
    {
        using var backend = new SoftwareBackend();
        using var engine = new VideraEngine();
        engine.Initialize(backend);

        var act = () => engine.StyleService.ApplyPreset(RenderStylePreset.Cartoon);
        act.Should().NotThrow();
    }

    [Fact]
    public void VideraEngine_Initialize_NullBackend_ThrowsArgumentNullException()
    {
        using var engine = new VideraEngine();
        var act = () => engine.Initialize(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void VideraEngine_AddObject_NullObject_ThrowsArgumentNullException()
    {
        using var engine = new VideraEngine();
        var act = () => engine.AddObject(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void VideraEngine_RemoveObject_NullObject_ThrowsArgumentNullException()
    {
        using var engine = new VideraEngine();
        var act = () => engine.RemoveObject(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}

/// <summary>
/// Lightweight test cube factory that doesn't depend on Demo project.
/// </summary>
internal static class DemoMeshFactory
{
    public static Object3D CreateTestCube(Videra.Core.Graphics.Abstractions.IResourceFactory factory, float size = 1f)
    {
        var half = size * 0.5f;
        var vertices = new[]
        {
            new VertexPositionNormalColor(new Vector3(-half, -half, -half), Vector3.UnitZ, RgbaFloat.Red),
            new VertexPositionNormalColor(new Vector3(half, -half, -half), Vector3.UnitZ, RgbaFloat.Green),
            new VertexPositionNormalColor(new Vector3(half, half, -half), Vector3.UnitZ, RgbaFloat.Blue),
            new VertexPositionNormalColor(new Vector3(-half, half, -half), Vector3.UnitZ, RgbaFloat.White),
        };
        var indices = new uint[] { 0, 1, 2, 0, 2, 3 };

        var mesh = new MeshData
        {
            Vertices = vertices,
            Indices = indices,
            Topology = MeshTopology.Triangles
        };

        var cube = new Object3D { Name = "TestCube" };
        cube.Initialize(factory, mesh);
        return cube;
    }

    public static Object3D CreateWhiteQuad(IResourceFactory factory, float halfExtent = 0.8f)
    {
        var vertices = new[]
        {
            new VertexPositionNormalColor(new Vector3(-halfExtent, -halfExtent, 0f), Vector3.UnitZ, RgbaFloat.White),
            new VertexPositionNormalColor(new Vector3(halfExtent, -halfExtent, 0f), Vector3.UnitZ, RgbaFloat.White),
            new VertexPositionNormalColor(new Vector3(halfExtent, halfExtent, 0f), Vector3.UnitZ, RgbaFloat.White),
            new VertexPositionNormalColor(new Vector3(-halfExtent, halfExtent, 0f), Vector3.UnitZ, RgbaFloat.White),
        };
        var indices = new uint[] { 0, 1, 2, 0, 2, 3 };

        var mesh = new MeshData
        {
            Vertices = vertices,
            Indices = indices,
            Topology = MeshTopology.Triangles
        };

        var quad = new Object3D { Name = "WhiteQuad" };
        quad.Initialize(factory, mesh);
        return quad;
    }

    public static Object3D CreateMaskedQuad(float alpha, float cutoff, float halfExtent = 0.8f)
    {
        var color = new RgbaFloat(1f, 1f, 1f, alpha);
        var vertices = new[]
        {
            new VertexPositionNormalColor(new Vector3(-halfExtent, -halfExtent, 0f), Vector3.UnitZ, color),
            new VertexPositionNormalColor(new Vector3(halfExtent, -halfExtent, 0f), Vector3.UnitZ, color),
            new VertexPositionNormalColor(new Vector3(halfExtent, halfExtent, 0f), Vector3.UnitZ, color),
            new VertexPositionNormalColor(new Vector3(-halfExtent, halfExtent, 0f), Vector3.UnitZ, color),
        };
        var indices = new uint[] { 0, 1, 2, 0, 2, 3 };
        var mesh = new MeshData
        {
            Vertices = vertices,
            Indices = indices,
            Topology = MeshTopology.Triangles
        };
        var material = new MaterialInstance(
            MaterialInstanceId.New(),
            "MaskedQuad",
            RgbaFloat.White,
            alpha: new MaterialAlphaSettings(MaterialAlphaMode.Mask, cutoff, true));
        var primitive = new MeshPrimitive(MeshPrimitiveId.New(), "MaskedQuadPrimitive", mesh, material.Id);
        var node = new SceneNode(SceneNodeId.New(), "MaskedQuadNode", Matrix4x4.Identity, parentId: null, [primitive.Id]);
        var asset = new ImportedSceneAsset("masked-quad.gltf", "masked-quad.gltf", [node], [primitive], [material]);
        return SceneObjectFactory.CreateDeferred(asset);
    }

    public static Object3D CreateBlendedQuad(RgbaFloat color, Vector3 position, float halfExtent = 0.8f)
    {
        var vertices = new[]
        {
            new VertexPositionNormalColor(new Vector3(-halfExtent, -halfExtent, 0f), Vector3.UnitZ, color),
            new VertexPositionNormalColor(new Vector3(halfExtent, -halfExtent, 0f), Vector3.UnitZ, color),
            new VertexPositionNormalColor(new Vector3(halfExtent, halfExtent, 0f), Vector3.UnitZ, color),
            new VertexPositionNormalColor(new Vector3(-halfExtent, halfExtent, 0f), Vector3.UnitZ, color),
        };
        var indices = new uint[] { 0, 1, 2, 0, 2, 3 };
        var mesh = new MeshData
        {
            Vertices = vertices,
            Indices = indices,
            Topology = MeshTopology.Triangles
        };
        var material = new MaterialInstance(
            MaterialInstanceId.New(),
            "BlendedQuad",
            color,
            alpha: new MaterialAlphaSettings(MaterialAlphaMode.Blend, 0.5f, true));
        var primitive = new MeshPrimitive(MeshPrimitiveId.New(), "BlendedQuadPrimitive", mesh, material.Id);
        var node = new SceneNode(SceneNodeId.New(), "BlendedQuadNode", Matrix4x4.CreateTranslation(position), parentId: null, [primitive.Id]);
        var asset = new ImportedSceneAsset("blended-quad.gltf", "blended-quad.gltf", [node], [primitive], [material]);
        return SceneObjectFactory.CreateDeferred(asset);
    }

    public static byte[] CaptureFrame(SoftwareBackend backend)
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

    public static int CountPixels(byte[] frame, PixelColor color)
    {
        var count = 0;

        for (var i = 0; i < frame.Length; i += 4)
        {
            var pixel = (r: frame[i + 2], g: frame[i + 1], b: frame[i], a: frame[i + 3]);
            if (pixel == color.Value)
            {
                count++;
            }
        }

        return count;
    }

    public static PixelColor ReadPixel(byte[] frame, int width, int x, int y)
    {
        var offset = ((y * width) + x) * 4;
        return new PixelColor(frame[offset + 2], frame[offset + 1], frame[offset], frame[offset + 3]);
    }

    public readonly record struct PixelColor(byte R, byte G, byte B, byte A)
    {
        public static PixelColor White => new(255, 255, 255, 255);
        public static PixelColor Black => new(0, 0, 0, 255);
        public static PixelColor Blue => new(0, 0, 255, 255);
        public static PixelColor Red => new(255, 0, 0, 255);
        public static PixelColor Green => new(0, 255, 0, 255);

        public (byte r, byte g, byte b, byte a) Value => (R, G, B, A);
    }
}
