using System.Numerics;
using FluentAssertions;
using Moq;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Selection;
using Xunit;

namespace Videra.Core.Tests.Selection;

public class SceneHitTestServiceTests
{
    [Fact]
    public void HitTest_WhenObjectsOverlapInScreenSpace_ReturnsNearestObjectFirst()
    {
        var front = CreateBoundsObject(new Vector3(-0.5f), new Vector3(0.5f));
        var back = CreateBoundsObject(new Vector3(-0.5f, -0.5f, -3.5f), new Vector3(0.5f, 0.5f, -2.5f));
        var camera = CreateCamera();
        var request = new SceneHitTestRequest(
            camera,
            new Vector2(800f, 600f),
            new Vector2(400f, 300f),
            [front, back]);

        var result = new SceneHitTestService().HitTest(request);

        result.PrimaryHit.Should().NotBeNull();
        result.PrimaryHit!.ObjectId.Should().Be(front.Id);
        result.Hits.Select(hit => hit.ObjectId).Should().ContainInOrder(front.Id, back.Id);
    }

    [Fact]
    public void HitTest_WhenSceneIsEmpty_ReturnsEmptyStructuredResult()
    {
        var camera = CreateCamera();
        var request = new SceneHitTestRequest(
            camera,
            new Vector2(800f, 600f),
            new Vector2(400f, 300f),
            []);

        var result = new SceneHitTestService().HitTest(request);

        result.Should().NotBeNull();
        result.PrimaryHit.Should().BeNull();
        result.Hits.Should().NotBeNull().And.BeEmpty();
    }

    private static OrbitCamera CreateCamera()
    {
        var camera = new OrbitCamera();
        camera.SetOrbit(Vector3.Zero, 10f, 0f, 0f);
        camera.UpdateProjection(800, 600);
        return camera;
    }

    private static Object3D CreateBoundsObject(Vector3 min, Vector3 max)
    {
        var obj = new Object3D();
        var mockFactory = CreateMockFactory();
        obj.Initialize(mockFactory.Object, CreateBoundsMesh(min, max));
        return obj;
    }

    private static MeshData CreateBoundsMesh(Vector3 min, Vector3 max)
    {
        return new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(min.X, min.Y, min.Z), Vector3.UnitY, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(min.X, min.Y, max.Z), Vector3.UnitY, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(min.X, max.Y, min.Z), Vector3.UnitY, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(min.X, max.Y, max.Z), Vector3.UnitY, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(max.X, min.Y, min.Z), Vector3.UnitY, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(max.X, min.Y, max.Z), Vector3.UnitY, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(max.X, max.Y, min.Z), Vector3.UnitY, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(max.X, max.Y, max.Z), Vector3.UnitY, RgbaFloat.White)
            ],
            Indices = [0u, 1u, 2u],
            Topology = MeshTopology.Triangles
        };
    }

    private static Mock<IResourceFactory> CreateMockFactory()
    {
        var mockFactory = new Mock<IResourceFactory>();
        var mockBuffer = new Mock<IBuffer>();
        mockBuffer.SetupGet(b => b.SizeInBytes).Returns(1024u);

        mockFactory.Setup(f => f.CreateVertexBuffer(It.IsAny<uint>())).Returns(mockBuffer.Object);
        mockFactory.Setup(f => f.CreateIndexBuffer(It.IsAny<uint>())).Returns(mockBuffer.Object);
        mockFactory.Setup(f => f.CreateUniformBuffer(It.IsAny<uint>())).Returns(mockBuffer.Object);

        return mockFactory;
    }
}
