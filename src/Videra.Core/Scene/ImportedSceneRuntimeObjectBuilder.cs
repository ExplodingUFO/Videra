using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics;

namespace Videra.Core.Scene;

internal static class ImportedSceneRuntimeObjectBuilder
{
    public static IReadOnlyList<Object3D> CreateDeferredObjects(ImportedSceneAsset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);

        var primitiveById = asset.Primitives.ToDictionary(static primitive => primitive.Id);
        var materialById = asset.Materials.ToDictionary(static material => material.Id);
        var nodeById = asset.Nodes.ToDictionary(static node => node.Id);
        var resolvedTransforms = new Dictionary<SceneNodeId, Matrix4x4>();
        var resolvingNodes = new HashSet<SceneNodeId>();
        var runtimeObjects = new List<Object3D>();

        foreach (var node in asset.Nodes)
        {
            var worldTransform = ResolveWorldTransform(node, nodeById, resolvedTransforms, resolvingNodes);
            foreach (var primitiveId in node.PrimitiveIds)
            {
                if (!primitiveById.TryGetValue(primitiveId, out var primitive))
                {
                    throw new InvalidOperationException(
                        $"Scene node '{node.Name}' references unknown primitive '{primitiveId.Value}'.");
                }

                var runtimeObject = new Object3D
                {
                    Name = $"{asset.Name}::{node.Name}::{primitive.Name}"
                };
                runtimeObject.PrepareDeferredMesh(TransformPayload(primitive.Payload, worldTransform));
                runtimeObject.ApplyMaterialAlpha(ResolveMaterialAlpha(primitive.MaterialId, materialById));
                runtimeObjects.Add(runtimeObject);
            }
        }

        if (runtimeObjects.Count == 0)
        {
            throw new InvalidOperationException("Imported scene asset did not produce any runtime objects.");
        }

        return runtimeObjects;
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

    private static MeshPayload TransformPayload(MeshPayload payload, Matrix4x4 transform)
    {
        Matrix4x4.Invert(transform, out var inverseTransform);
        var normalTransform = Matrix4x4.Transpose(inverseTransform);
        var vertices = new VertexPositionNormalColor[payload.Vertices.Length];
        var tangents = payload.Tangents.Length == 0
            ? Array.Empty<Vector4>()
            : new Vector4[payload.Tangents.Length];

        for (var i = 0; i < payload.Vertices.Length; i++)
        {
            var vertex = payload.Vertices[i];
            vertices[i] = new VertexPositionNormalColor(
                Vector3.Transform(vertex.Position, transform),
                Vector3.Normalize(Vector3.TransformNormal(vertex.Normal, normalTransform)),
                vertex.Color);

            if (tangents.Length != 0)
            {
                tangents[i] = TransformTangent(payload.Tangents[i], transform);
            }
        }

        var indices = (uint[])payload.Indices.Clone();
        var segments = payload.Segments.Length == 0
            ? Array.Empty<MeshPayloadSegment>()
            : (MeshPayloadSegment[])payload.Segments.Clone();

        return new MeshPayload(vertices, indices, payload.Topology, tangents, segments);
    }

    private static MaterialAlphaSettings ResolveMaterialAlpha(
        MaterialInstanceId? materialId,
        IReadOnlyDictionary<MaterialInstanceId, MaterialInstance> materialById)
    {
        if (materialId is null || !materialById.TryGetValue(materialId.Value, out var material))
        {
            return MaterialAlphaSettings.Opaque;
        }

        return material.Alpha;
    }

    private static Vector4 TransformTangent(Vector4 tangent, Matrix4x4 transform)
    {
        var direction = Vector3.TransformNormal(new Vector3(tangent.X, tangent.Y, tangent.Z), transform);
        if (direction.LengthSquared() > float.Epsilon)
        {
            direction = Vector3.Normalize(direction);
        }

        return new Vector4(direction, tangent.W);
    }
}
