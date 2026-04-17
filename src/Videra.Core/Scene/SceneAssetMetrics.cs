using System.Runtime.CompilerServices;
using Videra.Core.Geometry;
using Videra.Core.Graphics;

namespace Videra.Core.Scene;

public sealed record SceneAssetMetrics(
    int VertexCount,
    int IndexCount,
    long ApproximateGpuBytes,
    BoundingBox3 Bounds)
{
    public static SceneAssetMetrics FromMesh(MeshData mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(mesh.Vertices);
        ArgumentNullException.ThrowIfNull(mesh.Indices);
        return FromPayload(MeshPayload.FromMesh(mesh, cloneArrays: false));
    }

    internal static SceneAssetMetrics FromPayload(MeshPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return new SceneAssetMetrics(
            payload.Vertices.Length,
            payload.Indices.Length,
            payload.ApproximateGpuBytes,
            payload.LocalBounds);
    }
}
