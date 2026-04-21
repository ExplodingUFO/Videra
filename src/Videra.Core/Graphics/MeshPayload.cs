using System.Numerics;
using System.Runtime.CompilerServices;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics;

internal sealed class MeshPayload
{
    public MeshPayload(
        VertexPositionNormalColor[] vertices,
        uint[] indices,
        MeshTopology topology,
        Vector4[]? tangents = null)
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

        tangents ??= Array.Empty<Vector4>();
        if (tangents.Length != 0 && tangents.Length != vertices.Length)
        {
            throw new ArgumentException("Tangent count must match vertex count when tangent data is present.", nameof(tangents));
        }

        Vertices = vertices;
        Indices = indices;
        Tangents = tangents;
        Topology = topology;
        LocalBounds = BoundingBox3.FromVertices(vertices);
        ApproximateGpuBytes =
            (long)vertices.Length * Unsafe.SizeOf<VertexPositionNormalColor>() +
            (long)indices.Length * sizeof(uint) +
            64L;
    }

    public VertexPositionNormalColor[] Vertices { get; }

    public uint[] Indices { get; }

    public Vector4[] Tangents { get; }

    public MeshTopology Topology { get; }

    public BoundingBox3 LocalBounds { get; }

    public long ApproximateGpuBytes { get; }

    public MeshData ToMeshData()
    {
        return new MeshData
        {
            Vertices = Vertices,
            Indices = Indices,
            Tangents = Tangents,
            Topology = Topology
        };
    }

    public static MeshPayload FromMesh(MeshData mesh, bool cloneArrays)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(mesh.Vertices);
        ArgumentNullException.ThrowIfNull(mesh.Indices);
        ArgumentNullException.ThrowIfNull(mesh.Tangents);

        var vertices = cloneArrays
            ? (VertexPositionNormalColor[])mesh.Vertices.Clone()
            : mesh.Vertices;
        var indices = cloneArrays
            ? (uint[])mesh.Indices.Clone()
            : mesh.Indices;
        var tangents = cloneArrays
            ? (Vector4[])mesh.Tangents.Clone()
            : mesh.Tangents;

        return new MeshPayload(vertices, indices, mesh.Topology, tangents);
    }
}
