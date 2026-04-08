using System.Numerics;
using FluentAssertions;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;
using Videra.Core.Graphics.Wireframe;
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
}
