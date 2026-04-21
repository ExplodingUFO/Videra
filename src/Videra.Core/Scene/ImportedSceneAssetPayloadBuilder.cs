using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Scene;

internal static class ImportedSceneAssetPayloadBuilder
{
    public static MeshPayload Build(IReadOnlyList<SceneNode> nodes, IReadOnlyList<MeshPrimitive> primitives)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(primitives);

        var primitiveById = primitives.ToDictionary(static primitive => primitive.Id);
        var nodeById = nodes.ToDictionary(static node => node.Id);
        var resolvedTransforms = new Dictionary<SceneNodeId, Matrix4x4>();
        var resolvingNodes = new HashSet<SceneNodeId>();

        var vertices = new List<VertexPositionNormalColor>();
        var indices = new List<uint>();
        uint indexOffset = 0;

        foreach (var node in nodes)
        {
            var worldTransform = ResolveWorldTransform(node, nodeById, resolvedTransforms, resolvingNodes);
            foreach (var primitiveId in node.PrimitiveIds)
            {
                if (!primitiveById.TryGetValue(primitiveId, out var primitive))
                {
                    throw new InvalidOperationException($"Scene node '{node.Name}' references unknown primitive '{primitiveId.Value}'.");
                }

                AppendPrimitive(primitive.Payload, worldTransform, vertices, indices, ref indexOffset);
            }
        }

        return new MeshPayload(vertices.ToArray(), indices.ToArray(), MeshTopology.Triangles);
    }

    private static Matrix4x4 ResolveWorldTransform(
        SceneNode node,
        IReadOnlyDictionary<SceneNodeId, SceneNode> nodeById,
        IDictionary<SceneNodeId, Matrix4x4> resolvedTransforms,
        ISet<SceneNodeId> resolvingNodes)
    {
        if (resolvedTransforms.TryGetValue(node.Id, out var cached))
        {
            return cached;
        }

        if (!resolvingNodes.Add(node.Id))
        {
            throw new InvalidOperationException($"Scene graph contains a cycle at node '{node.Name}'.");
        }

        try
        {
            var worldTransform = node.ParentId is { } parentId
                ? node.LocalTransform * ResolveParentWorldTransform(parentId, nodeById, resolvedTransforms, resolvingNodes)
                : node.LocalTransform;

            resolvedTransforms[node.Id] = worldTransform;
            return worldTransform;
        }
        finally
        {
            resolvingNodes.Remove(node.Id);
        }
    }

    private static Matrix4x4 ResolveParentWorldTransform(
        SceneNodeId parentId,
        IReadOnlyDictionary<SceneNodeId, SceneNode> nodeById,
        IDictionary<SceneNodeId, Matrix4x4> resolvedTransforms,
        ISet<SceneNodeId> resolvingNodes)
    {
        if (!nodeById.TryGetValue(parentId, out var parentNode))
        {
            throw new InvalidOperationException($"Scene graph references unknown parent node '{parentId.Value}'.");
        }

        return ResolveWorldTransform(parentNode, nodeById, resolvedTransforms, resolvingNodes);
    }

    private static void AppendPrimitive(
        MeshPayload payload,
        Matrix4x4 transform,
        ICollection<VertexPositionNormalColor> vertices,
        ICollection<uint> indices,
        ref uint indexOffset)
    {
        Matrix4x4.Invert(transform, out var inverseTransform);
        var normalTransform = Matrix4x4.Transpose(inverseTransform);

        foreach (var vertex in payload.Vertices)
        {
            var position = Vector3.Transform(vertex.Position, transform);
            var normal = Vector3.Normalize(Vector3.TransformNormal(vertex.Normal, normalTransform));
            vertices.Add(new VertexPositionNormalColor(position, normal, vertex.Color));
        }

        foreach (var index in payload.Indices)
        {
            indices.Add(indexOffset + index);
        }

        indexOffset += (uint)payload.Vertices.Length;
    }
}
