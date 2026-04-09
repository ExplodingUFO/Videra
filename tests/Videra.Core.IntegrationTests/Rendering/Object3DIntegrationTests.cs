using System.Numerics;
using FluentAssertions;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;
using Videra.Core.Selection.Annotations;
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
}
