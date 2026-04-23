using System.Numerics;
using Videra.Core.Geometry;

namespace Videra.Core.Scene;

internal static class MaterialTextureNormalBaker
{
    public static Vector3 ResolveVertexNormal(
        Vector3 transformedNormal,
        Vector4 transformedTangent,
        bool hasTangents,
        int vertexIndex,
        MeshData meshData,
        MaterialInstance? material,
        IReadOnlyDictionary<Texture2DId, Texture2D> textureById,
        IReadOnlyDictionary<SamplerId, Sampler> samplerById)
    {
        ArgumentNullException.ThrowIfNull(meshData);
        ArgumentNullException.ThrowIfNull(textureById);
        ArgumentNullException.ThrowIfNull(samplerById);

        if (material is null || material.NormalTexture is not { } normalBinding)
        {
            return Vector3.Normalize(transformedNormal);
        }

        if (!hasTangents)
        {
            return Vector3.Normalize(transformedNormal);
        }

        var normalSample = MaterialTextureColorBaker.ResolveTextureSample(
            normalBinding.Texture,
            material.Name,
            vertexIndex,
            meshData,
            textureById,
            samplerById);

        var tangentSpaceNormal = new Vector3(
            (normalSample.R * 2f - 1f) * normalBinding.Scale,
            (normalSample.G * 2f - 1f) * normalBinding.Scale,
            normalSample.B * 2f - 1f);

        var normal = Vector3.Normalize(transformedNormal);
        var tangent = Vector3.Normalize(new Vector3(transformedTangent.X, transformedTangent.Y, transformedTangent.Z));
        if (float.IsNaN(tangent.X) || float.IsNaN(tangent.Y) || float.IsNaN(tangent.Z))
        {
            return normal;
        }
        var bitangent = Vector3.Normalize(Vector3.Cross(normal, tangent) * transformedTangent.W);
        if (float.IsNaN(bitangent.X) || float.IsNaN(bitangent.Y) || float.IsNaN(bitangent.Z))
        {
            return normal;
        }

        var worldNormal =
            (tangent * tangentSpaceNormal.X) +
            (bitangent * tangentSpaceNormal.Y) +
            (normal * tangentSpaceNormal.Z);

        return Vector3.Normalize(worldNormal);
    }
}
