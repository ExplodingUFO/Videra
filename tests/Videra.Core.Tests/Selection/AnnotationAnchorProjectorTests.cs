using System.Numerics;
using FluentAssertions;
using Moq;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Selection.Annotations;
using Xunit;

namespace Videra.Core.Tests.Selection;

public class AnnotationAnchorProjectorTests
{
    [Fact]
    public void Project_ObjectAnchor_ResolvesVisibleObjectById()
    {
        var viewportSize = new Vector2(800f, 600f);
        var camera = CreateCamera(viewportSize);
        var projector = new AnnotationAnchorProjector();
        var object3D = CreateObjectAtOrigin("Triangle");
        var anchor = AnnotationAnchorDescriptor.ForObject(object3D.Id);

        var result = projector.Project(anchor, camera, viewportSize, [object3D]);

        result.IsVisible.Should().BeTrue();
        result.ClipStatus.Should().Be(AnnotationProjectionClipStatus.Visible);
        result.ResolvedObjectId.Should().Be(object3D.Id);

        camera.TryProjectWorldPoint(object3D.WorldBounds!.Value.Center, viewportSize, out var expectedScreenPoint)
            .Should().BeTrue();
        result.ScreenPosition.Should().Be(expectedScreenPoint);
    }

    [Fact]
    public void Project_WorldPointAnchor_ProjectsToScreenCoordinates()
    {
        var viewportSize = new Vector2(1280f, 720f);
        var camera = CreateCamera(viewportSize);
        var projector = new AnnotationAnchorProjector();
        var worldPoint = new Vector3(0.5f, 0.5f, 0f);
        var anchor = AnnotationAnchorDescriptor.ForWorldPoint(worldPoint);

        var result = projector.Project(anchor, camera, viewportSize, []);

        result.IsVisible.Should().BeTrue();
        result.ClipStatus.Should().Be(AnnotationProjectionClipStatus.Visible);
        result.ResolvedObjectId.Should().BeNull();

        camera.TryProjectWorldPoint(worldPoint, viewportSize, out var expectedScreenPoint)
            .Should().BeTrue();
        result.ScreenPosition.Should().Be(expectedScreenPoint);
    }

    [Fact]
    public void Project_WorldPointAnchor_BehindCamera_ReturnsNonVisibleResult()
    {
        var viewportSize = new Vector2(800f, 600f);
        var camera = CreateCamera(viewportSize);
        var projector = new AnnotationAnchorProjector();
        var behindCameraPoint = camera.Position + Vector3.Normalize(camera.Position - camera.Target);
        var anchor = AnnotationAnchorDescriptor.ForWorldPoint(behindCameraPoint);

        var result = projector.Project(anchor, camera, viewportSize, []);

        result.IsVisible.Should().BeFalse();
        result.ClipStatus.Should().Be(AnnotationProjectionClipStatus.BehindCamera);
    }

    [Fact]
    public void Project_WorldPointAnchor_BeyondFarClip_ReturnsNonVisibleResult()
    {
        var viewportSize = new Vector2(800f, 600f);
        var camera = CreateCamera(viewportSize);
        var projector = new AnnotationAnchorProjector();
        var forward = Vector3.Normalize(camera.Target - camera.Position);
        var clippedPoint = camera.Position + forward * 5000f;
        var anchor = AnnotationAnchorDescriptor.ForWorldPoint(clippedPoint);

        var result = projector.Project(anchor, camera, viewportSize, []);

        result.IsVisible.Should().BeFalse();
        result.ClipStatus.Should().Be(AnnotationProjectionClipStatus.OutsideClipDepth);
    }

    [Fact]
    public void Project_MissingObjectAnchor_ReturnsNonVisibleResult()
    {
        var viewportSize = new Vector2(800f, 600f);
        var camera = CreateCamera(viewportSize);
        var projector = new AnnotationAnchorProjector();
        var anchor = AnnotationAnchorDescriptor.ForObject(Guid.NewGuid());
        var object3D = CreateObjectAtOrigin("Triangle");

        var result = projector.Project(anchor, camera, viewportSize, [object3D]);

        result.IsVisible.Should().BeFalse();
        result.ClipStatus.Should().Be(AnnotationProjectionClipStatus.MissingObject);
        result.ResolvedObjectId.Should().BeNull();
    }

    private static OrbitCamera CreateCamera(Vector2 viewportSize)
    {
        var camera = new OrbitCamera();
        camera.UpdateProjection((uint)viewportSize.X, (uint)viewportSize.Y);
        return camera;
    }

    private static Object3D CreateObjectAtOrigin(string name)
    {
        var mesh = new MeshData
        {
            Vertices = new[]
            {
                new VertexPositionNormalColor(new Vector3(0f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(1f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0f, 1f, 0f), Vector3.UnitZ, RgbaFloat.White)
            },
            Indices = new uint[] { 0, 1, 2 },
            Topology = MeshTopology.Triangles
        };

        var factory = CreateMockFactory();
        var object3D = new Object3D { Name = name };
        object3D.Initialize(factory.Object, mesh);
        return object3D;
    }

    private static Mock<IResourceFactory> CreateMockFactory()
    {
        var mockFactory = new Mock<IResourceFactory>();
        var mockBuffer = new Mock<IBuffer>();

        mockFactory.Setup(f => f.CreateVertexBuffer(It.IsAny<uint>())).Returns(mockBuffer.Object);
        mockFactory.Setup(f => f.CreateIndexBuffer(It.IsAny<uint>())).Returns(mockBuffer.Object);
        mockFactory.Setup(f => f.CreateUniformBuffer(It.IsAny<uint>())).Returns(mockBuffer.Object);

        return mockFactory;
    }
}
