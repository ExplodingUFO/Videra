using System.Runtime.CompilerServices;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics;

internal sealed class MeshPayload
{
    public MeshPayload(
        VertexPositionNormalColor[] vertices,
        uint[] indices,
        MeshTopology topology)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(indices);

        if (vertices.Length == 0)
        {
            throw new ArgumentException("Invalid mesh data", nameof(vertices));
        }

        if (indices.Length == 0)
        {
            throw new ArgumentException("Invalid index data", nameof(indices));
        }

        Vertices = vertices;
        Indices = indices;
        Topology = topology;
        LocalBounds = BoundingBox3.FromVertices(vertices);
        ApproximateGpuBytes =
            (long)vertices.Length * Unsafe.SizeOf<VertexPositionNormalColor>() +
            (long)indices.Length * sizeof(uint) +
            64L;
    }

    public VertexPositionNormalColor[] Vertices { get; }

    public uint[] Indices { get; }

    public MeshTopology Topology { get; }

    public BoundingBox3 LocalBounds { get; }

    public long ApproximateGpuBytes { get; }

    public MeshData ToMeshData()
    {
        return new MeshData
        {
            Vertices = Vertices,
            Indices = Indices,
            Topology = Topology
        };
    }

    public static MeshPayload FromMesh(MeshData mesh, bool cloneArrays)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(mesh.Vertices);
        ArgumentNullException.ThrowIfNull(mesh.Indices);

        var vertices = cloneArrays
            ? (VertexPositionNormalColor[])mesh.Vertices.Clone()
            : mesh.Vertices;
        var indices = cloneArrays
            ? (uint[])mesh.Indices.Clone()
            : mesh.Indices;

        return new MeshPayload(vertices, indices, mesh.Topology);
    }
}
