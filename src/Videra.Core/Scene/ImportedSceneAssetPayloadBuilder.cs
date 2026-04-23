using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Scene;

internal static class ImportedSceneAssetPayloadBuilder
{
    public static MeshPayload Build(
        IReadOnlyList<SceneNode> nodes,
        IReadOnlyList<MeshPrimitive> primitives,
        IReadOnlyList<MaterialInstance> materials,
        IReadOnlyList<Texture2D> textures,
        IReadOnlyList<Sampler> samplers)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(primitives);
        ArgumentNullException.ThrowIfNull(materials);
        ArgumentNullException.ThrowIfNull(textures);
        ArgumentNullException.ThrowIfNull(samplers);

        var primitiveById = primitives.ToDictionary(static primitive => primitive.Id);
        var materialById = materials.ToDictionary(static material => material.Id);
        var textureById = textures.ToDictionary(static texture => texture.Id);
        var samplerById = samplers.ToDictionary(static sampler => sampler.Id);
        var nodeById = nodes.ToDictionary(static node => node.Id);
        var resolvedTransforms = new Dictionary<SceneNodeId, Matrix4x4>();
        var resolvingNodes = new HashSet<SceneNodeId>();
        var includeTangents = primitives.Any(static primitive => primitive.MeshData.Tangents.Length > 0);

        var vertices = new List<VertexPositionNormalColor>();
        var tangents = includeTangents ? new List<Vector4>() : null;
        var indices = new List<uint>();
        var segments = new List<MeshPayloadSegment>();
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

                var startIndex = (uint)indices.Count;
                AppendPrimitive(
                    primitive.MeshData,
                    worldTransform,
                    primitive.MaterialId is { } materialId && materialById.TryGetValue(materialId, out var material) ? material : null,
                    textureById,
                    samplerById,
                    vertices,
                    tangents,
                    indices,
                    ref indexOffset);
                segments.Add(new MeshPayloadSegment(
                    startIndex,
                    (uint)primitive.MeshData.Indices.Length,
                    ResolveMaterialAlpha(primitive.MaterialId, materialById)));
            }
        }

        return new MeshPayload(
            vertices.ToArray(),
            indices.ToArray(),
            MeshTopology.Triangles,
            tangents?.ToArray(),
            segments.ToArray());
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
        MeshData meshData,
        Matrix4x4 transform,
        MaterialInstance? material,
        IReadOnlyDictionary<Texture2DId, Texture2D> textureById,
        IReadOnlyDictionary<SamplerId, Sampler> samplerById,
        ICollection<VertexPositionNormalColor> vertices,
        ICollection<Vector4>? tangents,
        ICollection<uint> indices,
        ref uint indexOffset)
    {
        Matrix4x4.Invert(transform, out var inverseTransform);
        var normalTransform = Matrix4x4.Transpose(inverseTransform);
        if (meshData.Tangents.Length != 0 && meshData.Tangents.Length != meshData.Vertices.Length)
        {
            throw new InvalidOperationException("Mesh payload tangent count must match vertex count when tangent data is present.");
        }

        for (var i = 0; i < meshData.Vertices.Length; i++)
        {
            var vertex = meshData.Vertices[i];
            var position = Vector3.Transform(vertex.Position, transform);
            var transformedNormal = Vector3.TransformNormal(vertex.Normal, normalTransform);
            var transformedTangent = meshData.Tangents.Length == 0
                ? default
                : TransformTangent(meshData.Tangents[i], transform);
            var normal = MaterialTextureNormalBaker.ResolveVertexNormal(
                transformedNormal,
                transformedTangent,
                meshData.Tangents.Length != 0,
                i,
                meshData,
                material,
                textureById,
                samplerById);
            var color = MaterialTextureColorBaker.ResolveVertexColor(
                vertex,
                i,
                meshData,
                material,
                textureById,
                samplerById);
            vertices.Add(new VertexPositionNormalColor(position, normal, color));

            if (tangents is not null)
            {
                tangents.Add(transformedTangent);
            }
        }

        foreach (var index in meshData.Indices)
        {
            indices.Add(indexOffset + index);
        }

        indexOffset += (uint)meshData.Vertices.Length;
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
