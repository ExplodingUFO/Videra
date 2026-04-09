using System.Numerics;
using System.Runtime.CompilerServices;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.RenderPipeline.Extensibility;
using Videra.Core.Selection.Annotations;

namespace Videra.Core.Selection.Rendering;

public sealed class AnnotationAnchorOverlayContributor : IRenderPassContributor, IDisposable
{
    private bool _disposed;

    public void Contribute(RenderPassContributionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (_disposed || !context.AnnotationOverlay.HasOverlay)
        {
            return;
        }

        var markerVertices = BuildMarkerVertices(context);
        if (markerVertices.Length == 0)
        {
            return;
        }

        var vertexBufferSize = checked((uint)(markerVertices.Length * Unsafe.SizeOf<VertexPositionNormalColor>()));
        var markerIndices = BuildIndices(markerVertices.Length);
        using var vertexBuffer = context.ResourceFactory.CreateVertexBuffer(vertexBufferSize);
        using var indexBuffer = context.ResourceFactory.CreateIndexBuffer(checked((uint)(markerIndices.Length * sizeof(uint))));
        using var worldBuffer = context.ResourceFactory.CreateUniformBuffer(64);

        vertexBuffer.SetData(markerVertices, 0);
        indexBuffer.SetData(markerIndices, 0);
        worldBuffer.SetData(Matrix4x4.Identity, 0);

        context.CommandExecutor.SetPipeline(context.MeshPipeline);
        context.CommandExecutor.SetDepthState(testEnabled: false, writeEnabled: false);
        context.CommandExecutor.SetVertexBuffer(vertexBuffer, RenderBindingSlots.Vertex);
        context.CommandExecutor.SetVertexBuffer(worldBuffer, RenderBindingSlots.World);
        context.CommandExecutor.SetIndexBuffer(indexBuffer);
        context.CommandExecutor.DrawIndexed(PrimitiveCommandKind.LineList, (uint)markerIndices.Length, 1, 0, 0, 0);
        context.CommandExecutor.ResetDepthState();
    }

    public void Dispose()
    {
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private static VertexPositionNormalColor[] BuildMarkerVertices(RenderPassContributionContext context)
    {
        var vertices = new List<VertexPositionNormalColor>(context.AnnotationOverlay.Anchors.Count * 6);
        var halfMarkerSize = context.AnnotationOverlay.MarkerWorldSize * 0.5f;

        foreach (var overlayAnchor in context.AnnotationOverlay.Anchors)
        {
            if (!TryResolveAnchorWorldPoint(overlayAnchor.Anchor, context.SceneObjects, out var worldPoint))
            {
                continue;
            }

            AppendMarker(vertices, worldPoint, halfMarkerSize, context.AnnotationOverlay.MarkerColor);
        }

        return vertices.ToArray();
    }

    private static void AppendMarker(
        List<VertexPositionNormalColor> vertices,
        Vector3 worldPoint,
        float halfMarkerSize,
        RgbaFloat markerColor)
    {
        vertices.Add(new VertexPositionNormalColor(worldPoint + new Vector3(-halfMarkerSize, 0f, 0f), Vector3.UnitZ, markerColor));
        vertices.Add(new VertexPositionNormalColor(worldPoint + new Vector3(halfMarkerSize, 0f, 0f), Vector3.UnitZ, markerColor));
        vertices.Add(new VertexPositionNormalColor(worldPoint + new Vector3(0f, -halfMarkerSize, 0f), Vector3.UnitZ, markerColor));
        vertices.Add(new VertexPositionNormalColor(worldPoint + new Vector3(0f, halfMarkerSize, 0f), Vector3.UnitZ, markerColor));
        vertices.Add(new VertexPositionNormalColor(worldPoint + new Vector3(0f, 0f, -halfMarkerSize), Vector3.UnitZ, markerColor));
        vertices.Add(new VertexPositionNormalColor(worldPoint + new Vector3(0f, 0f, halfMarkerSize), Vector3.UnitZ, markerColor));
    }

    private static uint[] BuildIndices(int vertexCount)
    {
        var indices = new uint[vertexCount];
        for (var i = 0; i < indices.Length; i++)
        {
            indices[i] = (uint)i;
        }

        return indices;
    }

    private static bool TryResolveAnchorWorldPoint(
        AnnotationAnchorDescriptor anchor,
        IReadOnlyList<Object3D> sceneObjects,
        out Vector3 worldPoint)
    {
        switch (anchor.Kind)
        {
            case AnnotationAnchorKind.WorldPoint when anchor.WorldPoint is Vector3 point:
                worldPoint = point;
                return true;
            case AnnotationAnchorKind.Object when anchor.ObjectId is Guid objectId:
            {
                var sceneObject = sceneObjects.FirstOrDefault(obj => obj.Id == objectId);
                if (sceneObject?.WorldBounds is Geometry.BoundingBox3 bounds)
                {
                    worldPoint = bounds.Center;
                    return true;
                }

                break;
            }
        }

        worldPoint = default;
        return false;
    }
}
