using System.Numerics;
using FluentAssertions;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Inspection;
using Videra.Core.Picking;
using Xunit;

namespace Videra.Core.Tests.Picking;

public sealed class PickingServiceTests
{
    [Fact]
    public void TryResolveMeasurementAnchor_WhenTriangleMeshIsHit_UsesMeshAccurateWorldPoint()
    {
        var service = new PickingService();
        var sceneObject = new Object3D();
        sceneObject.PrepareDeferredMesh(CreateSlantedTriangleMesh());
        var camera = CreateCamera();

        var resolved = service.TryResolveMeasurementAnchor(
            camera,
            new Vector2(800f, 600f),
            new Vector2(400f, 300f),
            [sceneObject],
            VideraMeasurementSnapMode.Free,
            pendingAnchor: null,
            out var anchor);

        resolved.Should().BeTrue();
        anchor.ObjectId.Should().Be(sceneObject.Id);
        anchor.WorldPoint.X.Should().BeApproximately(0f, 0.01f);
        anchor.WorldPoint.Y.Should().BeApproximately(0f, 0.01f);
        anchor.WorldPoint.Z.Should().BeApproximately(1.5f, 0.01f);
    }

    [Fact]
    public void TryResolveMeasurementAnchor_WhenVertexSnapModeIsRequested_UsesNearestTriangleVertex()
    {
        var service = new PickingService();
        var sceneObject = new Object3D();
        sceneObject.PrepareDeferredMesh(CreateSlantedTriangleMesh());
        var camera = CreateCamera();

        var resolved = service.TryResolveMeasurementAnchor(
            camera,
            new Vector2(800f, 600f),
            new Vector2(400f, 300f),
            [sceneObject],
            VideraMeasurementSnapMode.Vertex,
            pendingAnchor: null,
            out var anchor);

        resolved.Should().BeTrue();
        anchor.ObjectId.Should().Be(sceneObject.Id);
        anchor.WorldPoint.Should().Be(new Vector3(0f, 1f, 2f));
    }

    [Fact]
    public void TryResolveMeasurementAnchor_WhenEdgeMidpointSnapModeIsRequested_UsesNearestTriangleEdgeMidpoint()
    {
        var service = new PickingService();
        var sceneObject = new Object3D();
        sceneObject.PrepareDeferredMesh(CreateSlantedTriangleMesh());
        var camera = CreateCamera();

        var resolved = service.TryResolveMeasurementAnchor(
            camera,
            new Vector2(800f, 600f),
            new Vector2(400f, 300f),
            [sceneObject],
            VideraMeasurementSnapMode.EdgeMidpoint,
            pendingAnchor: null,
            out var anchor);

        resolved.Should().BeTrue();
        anchor.ObjectId.Should().Be(sceneObject.Id);
        anchor.WorldPoint.Should().Be(new Vector3(0.5f, 0f, 2f));
    }

    [Fact]
    public void TryResolveMeasurementAnchor_WhenAxisLocked_ConstrainsSecondAnchorToDominantAxis()
    {
        var service = new PickingService();
        var sceneObject = new Object3D();
        sceneObject.PrepareDeferredMesh(CreateSlantedTriangleMesh());
        var camera = CreateCamera();
        camera.TryProjectWorldPoint(new Vector3(2f, 1f, 0f), new Vector2(800f, 600f), out var screenPoint).Should().BeTrue();

        var resolved = service.TryResolveMeasurementAnchor(
            camera,
            new Vector2(800f, 600f),
            screenPoint,
            [sceneObject],
            VideraMeasurementSnapMode.AxisLocked,
            VideraMeasurementAnchor.ForWorldPoint(new Vector3(0f, 0f, 1.5f)),
            out var anchor);

        resolved.Should().BeTrue();
        anchor.ObjectId.Should().BeNull();
        anchor.WorldPoint.X.Should().BeApproximately(2f, 0.001f);
        anchor.WorldPoint.Y.Should().BeApproximately(0f, 0.001f);
        anchor.WorldPoint.Z.Should().BeApproximately(1.5f, 0.001f);
    }

    private static OrbitCamera CreateCamera()
    {
        var camera = new OrbitCamera();
        camera.SetOrbit(Vector3.Zero, 10f, 0f, 0f);
        camera.UpdateProjection(800, 600);
        return camera;
    }

    private static MeshData CreateSlantedTriangleMesh()
    {
        return new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(-1f, -1f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(1f, -1f, 2f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0f, 1f, 2f), Vector3.UnitZ, RgbaFloat.White)
            ],
            Indices = [0u, 1u, 2u],
            Topology = MeshTopology.Triangles
        };
    }
}
