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
            throw new InvalidOperationException(
                $"Scene material '{material.Name}' requires tangent data for normal texture sampling but the mesh does not provide it.");
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
        var bitangent = Vector3.Normalize(Vector3.Cross(normal, tangent) * transformedTangent.W);

        var worldNormal =
            (tangent * tangentSpaceNormal.X) +
            (bitangent * tangentSpaceNormal.Y) +
            (normal * tangentSpaceNormal.Z);

        return Vector3.Normalize(worldNormal);
    }
}
