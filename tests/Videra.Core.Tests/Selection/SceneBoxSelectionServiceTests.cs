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

public class SceneBoxSelectionServiceTests
{
    [Fact]
    public void Select_WhenModeIsTouchOrFullyInside_AppliesExpectedSelectionPolicy()
    {
        var inside = CreateBoundsObject(new Vector3(-0.4f), new Vector3(0.4f));
        var touching = CreateBoundsObject(new Vector3(-2f, -2f, -0.5f), new Vector3(2f, 2f, 0.5f));
        var camera = CreateCamera();
        var query = new SceneBoxSelectionQuery(
            camera,
            new Vector2(800f, 600f),
            new Vector2(360f, 260f),
            new Vector2(440f, 340f),
            [inside, touching],
            SceneBoxSelectionMode.Touch);

        var service = new SceneBoxSelectionService();
        var touch = service.Select(query);
        var fullyInside = service.Select(query with { Mode = SceneBoxSelectionMode.FullyInside });

        touch.ObjectIds.Should().Contain(inside.Id);
        touch.ObjectIds.Should().Contain(touching.Id);
        fullyInside.ObjectIds.Should().Contain(inside.Id);
        fullyInside.ObjectIds.Should().NotContain(touching.Id);
    }

    [Fact]
    public void Select_WhenSceneIsEmpty_ReturnsEmptyStructuredResult()
    {
        var camera = CreateCamera();
        var query = new SceneBoxSelectionQuery(
            camera,
            new Vector2(800f, 600f),
            new Vector2(100f, 100f),
            new Vector2(200f, 200f),
            [],
            SceneBoxSelectionMode.Touch);

        var result = new SceneBoxSelectionService().Select(query);

        result.Should().NotBeNull();
        result.ObjectIds.Should().NotBeNull().And.BeEmpty();
        result.Objects.Should().NotBeNull().And.BeEmpty();
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
