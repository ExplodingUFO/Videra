using System.Numerics;
using System.Reflection;
using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Inspection;
using Xunit;

namespace Videra.Core.Tests.Inspection;

public sealed class VideraClipPlaneTests
{
    [Fact]
    public void ApplyClippingPlanes_ShouldPreserveSourcePayloadAndShrinkActiveBounds()
    {
        var mesh = new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(0f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(1f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0f, 1f, 0f), Vector3.UnitZ, RgbaFloat.White)
            ],
            Indices = [0u, 1u, 2u],
            Topology = MeshTopology.Triangles
        };

        var sceneObject = new Object3D { Name = "Clipped" };
        sceneObject.PrepareDeferredMesh(mesh);

        var originalBounds = sceneObject.LocalBounds;
        var applyMethod = typeof(Object3D).GetMethod("ApplyClippingPlanes", BindingFlags.Instance | BindingFlags.NonPublic);
        applyMethod.Should().NotBeNull("Object3D should expose an internal clipping entry point");

        var sourcePayloadProperty = typeof(Object3D).GetProperty("SourceMeshPayload", BindingFlags.Instance | BindingFlags.NonPublic);
        var activePayloadProperty = typeof(Object3D).GetProperty("MeshPayload", BindingFlags.Instance | BindingFlags.NonPublic);
        sourcePayloadProperty.Should().NotBeNull();
        activePayloadProperty.Should().NotBeNull();

        var sourcePayload = sourcePayloadProperty!.GetValue(sceneObject);

        applyMethod!.Invoke(
            sceneObject,
            [new[] { VideraClipPlane.FromPointNormal(new Vector3(0.5f, 0f, 0f), Vector3.UnitX) }]);

        sceneObject.LocalBounds.Should().NotBeNull();
        sceneObject.LocalBounds!.Value.Min.X.Should().BeGreaterThan(0.49f);
        sourcePayloadProperty.GetValue(sceneObject).Should().BeSameAs(sourcePayload);
        activePayloadProperty!.GetValue(sceneObject).Should().NotBeSameAs(sourcePayload);
        originalBounds.Should().NotBeNull();
        originalBounds!.Value.Min.X.Should().Be(0f);
    }

    [Fact]
    public void ApplyClippingPlanes_ShouldReuseCachedPayload_ForIdenticalPlaneSets()
    {
        var mesh = new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(0f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(1f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0f, 1f, 0f), Vector3.UnitZ, RgbaFloat.White)
            ],
            Indices = [0u, 1u, 2u],
            Topology = MeshTopology.Triangles
        };

        var sceneObject = new Object3D { Name = "CachedClip" };
        sceneObject.PrepareDeferredMesh(mesh);

        var applyMethod = typeof(Object3D).GetMethod("ApplyClippingPlanes", BindingFlags.Instance | BindingFlags.NonPublic);
        var activePayloadProperty = typeof(Object3D).GetProperty("MeshPayload", BindingFlags.Instance | BindingFlags.NonPublic);
        applyMethod.Should().NotBeNull();
        activePayloadProperty.Should().NotBeNull();

        var clippingPlanes = new[] { VideraClipPlane.FromPointNormal(new Vector3(0.5f, 0f, 0f), Vector3.UnitX) };

        applyMethod!.Invoke(sceneObject, [clippingPlanes]);
        var firstPayload = activePayloadProperty!.GetValue(sceneObject);

        applyMethod.Invoke(sceneObject, [clippingPlanes]);
        var secondPayload = activePayloadProperty.GetValue(sceneObject);

        secondPayload.Should().BeSameAs(firstPayload);
    }

    [Fact]
    public void ApplyClippingPlanes_ShouldPreserveTangents_WhenPresent()
    {
        var mesh = new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(0f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(1f, 0f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0f, 1f, 0f), Vector3.UnitZ, RgbaFloat.White)
            ],
            Indices = [0u, 1u, 2u],
            Tangents =
            [
                new Vector4(1f, 0f, 0f, 1f),
                new Vector4(1f, 0f, 0f, 1f),
                new Vector4(1f, 0f, 0f, 1f)
            ],
            Topology = MeshTopology.Triangles
        };

        var sceneObject = new Object3D { Name = "TangentClip" };
        sceneObject.PrepareDeferredMesh(mesh);

        var applyMethod = typeof(Object3D).GetMethod("ApplyClippingPlanes", BindingFlags.Instance | BindingFlags.NonPublic);
        var activePayloadProperty = typeof(Object3D).GetProperty("MeshPayload", BindingFlags.Instance | BindingFlags.NonPublic);
        applyMethod.Should().NotBeNull();
        activePayloadProperty.Should().NotBeNull();

        applyMethod!.Invoke(
            sceneObject,
            [new[] { VideraClipPlane.FromPointNormal(new Vector3(0.5f, 0f, 0f), Vector3.UnitX) }]);

        var payload = activePayloadProperty!.GetValue(sceneObject);
        payload.Should().NotBeNull();

        var verticesProperty = payload!.GetType().GetProperty("Vertices", BindingFlags.Instance | BindingFlags.Public);
        var tangentsProperty = payload.GetType().GetProperty("Tangents", BindingFlags.Instance | BindingFlags.Public);
        verticesProperty.Should().NotBeNull();
        tangentsProperty.Should().NotBeNull();

        var vertices = (VertexPositionNormalColor[])verticesProperty!.GetValue(payload)!;
        var tangents = (Vector4[])tangentsProperty!.GetValue(payload)!;

        tangents.Should().HaveCount(vertices.Length);
        tangents.Should().OnlyContain(static tangent => tangent == new Vector4(1f, 0f, 0f, 1f));
    }
}
