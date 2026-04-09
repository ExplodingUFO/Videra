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

    [Fact]
    public void Select_WhenObjectIsInsideNearPlane_DoesNotSelectClippedObject()
    {
        var camera = CreateCamera();
        var forward = Vector3.Normalize(camera.Target - camera.Position);
        var center = camera.Position + forward * 0.05f;
        var clipped = CreateBoundsObject(center - new Vector3(0.01f), center + new Vector3(0.01f));
        var query = new SceneBoxSelectionQuery(
            camera,
            new Vector2(800f, 600f),
            new Vector2(350f, 250f),
            new Vector2(450f, 350f),
            [clipped],
            SceneBoxSelectionMode.Touch);

        var result = new SceneBoxSelectionService().Select(query);

        result.ObjectIds.Should().BeEmpty();
    }

    [Fact]
    public void Select_WhenObjectIsBeyondFarPlane_DoesNotSelectClippedObject()
    {
        var camera = CreateCamera();
        var forward = Vector3.Normalize(camera.Target - camera.Position);
        var center = camera.Position + forward * 1005f;
        var clipped = CreateBoundsObject(center - new Vector3(0.5f), center + new Vector3(0.5f));
        var query = new SceneBoxSelectionQuery(
            camera,
            new Vector2(800f, 600f),
            new Vector2(350f, 250f),
            new Vector2(450f, 350f),
            [clipped],
            SceneBoxSelectionMode.Touch);

        var result = new SceneBoxSelectionService().Select(query);

        result.ObjectIds.Should().BeEmpty();
    }

    [Fact]
    public void Select_WhenBoundsIntersectNearPlane_UsesClippedProjectionExtents()
    {
        var camera = CreateCamera();
        var viewportSize = new Vector2(800f, 600f);
        var partiallyClipped = CreateNearPlaneIntersectingObject(camera);
        partiallyClipped.WorldBounds.Should().NotBeNull();

        var projected = SceneBoundsProjector.TryProjectBounds(
            partiallyClipped.WorldBounds!.Value,
            camera,
            viewportSize,
            out var clippedRect);
        var cornerOnlyRect = TryProjectVisibleCornersOnly(partiallyClipped.WorldBounds.Value, camera, viewportSize);

        projected.Should().BeTrue();
        cornerOnlyRect.Should().NotBeNull();
        clippedRect.Width.Should().BeGreaterThan(cornerOnlyRect!.Value.Width + 1f);

        var intersectsOnLeft = clippedRect.MinX < cornerOnlyRect.Value.MinX - 1f;
        var selectionStart = intersectsOnLeft
            ? new Vector2(clippedRect.MinX + 1f, clippedRect.MinY + 1f)
            : new Vector2(cornerOnlyRect.Value.MaxX + 1f, clippedRect.MinY + 1f);
        var selectionEnd = intersectsOnLeft
            ? new Vector2(cornerOnlyRect.Value.MinX - 1f, clippedRect.MaxY - 1f)
            : new Vector2(clippedRect.MaxX - 1f, clippedRect.MaxY - 1f);

        var query = new SceneBoxSelectionQuery(
            camera,
            viewportSize,
            selectionStart,
            selectionEnd,
            [partiallyClipped],
            SceneBoxSelectionMode.Touch);

        var result = new SceneBoxSelectionService().Select(query);

        result.ObjectIds.Should().Contain(partiallyClipped.Id);
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

    private static Object3D CreateNearPlaneIntersectingObject(OrbitCamera camera)
    {
        var forward = Vector3.Normalize(camera.Target - camera.Position);
        var center = camera.Position + forward * 0.16f;
        return CreateBoundsObject(
            center - new Vector3(0.08f, 0.08f, 0.08f),
            center + new Vector3(0.08f, 0.08f, 0.08f));
    }

    private static ProjectedScreenRect? TryProjectVisibleCornersOnly(
        BoundingBox3 bounds,
        OrbitCamera camera,
        Vector2 viewportSize)
    {
        var projectedPoints = CreateCorners(bounds)
            .Where(corner => camera.TryProjectWorldPoint(corner, viewportSize, out _))
            .Select(corner =>
            {
                camera.TryProjectWorldPoint(corner, viewportSize, out var screenPoint);
                return screenPoint;
            })
            .ToArray();

        if (projectedPoints.Length == 0)
        {
            return null;
        }

        return new ProjectedScreenRect(
            projectedPoints.Min(point => point.X),
            projectedPoints.Min(point => point.Y),
            projectedPoints.Max(point => point.X),
            projectedPoints.Max(point => point.Y));
    }

    private static Vector3[] CreateCorners(BoundingBox3 bounds)
    {
        return
        [
            new Vector3(bounds.Min.X, bounds.Min.Y, bounds.Min.Z),
            new Vector3(bounds.Min.X, bounds.Min.Y, bounds.Max.Z),
            new Vector3(bounds.Min.X, bounds.Max.Y, bounds.Min.Z),
            new Vector3(bounds.Min.X, bounds.Max.Y, bounds.Max.Z),
            new Vector3(bounds.Max.X, bounds.Min.Y, bounds.Min.Z),
            new Vector3(bounds.Max.X, bounds.Min.Y, bounds.Max.Z),
            new Vector3(bounds.Max.X, bounds.Max.Y, bounds.Min.Z),
            new Vector3(bounds.Max.X, bounds.Max.Y, bounds.Max.Z)
        ];
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
