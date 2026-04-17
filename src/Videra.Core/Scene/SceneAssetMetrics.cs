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

        var approximateGpuBytes =
            (long)mesh.Vertices.Length * Unsafe.SizeOf<VertexPositionNormalColor>() +
            (long)mesh.Indices.Length * sizeof(uint) +
            64L;

        return new SceneAssetMetrics(
            mesh.Vertices.Length,
            mesh.Indices.Length,
            approximateGpuBytes,
            BoundingBox3.FromVertices(mesh.Vertices));
    }
}
