using System.Numerics;
using System.Runtime.CompilerServices;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;

namespace Videra.Core.Graphics;

internal readonly record struct MeshPayloadSegment(
    uint StartIndex,
    uint IndexCount,
    MaterialAlphaSettings Alpha);

internal sealed class MeshPayload
{
    public MeshPayload(
        VertexPositionNormalColor[] vertices,
        uint[] indices,
        MeshTopology topology,
        Vector4[]? tangents = null,
        MeshPayloadSegment[]? segments = null)
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

        segments ??= Array.Empty<MeshPayloadSegment>();
        ValidateSegments(segments, indices.Length);

        Vertices = vertices;
        Indices = indices;
        Tangents = tangents;
        Topology = topology;
        Segments = segments;
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

    public MeshPayloadSegment[] Segments { get; }

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

    private static void ValidateSegments(MeshPayloadSegment[] segments, int indexCount)
    {
        var totalIndexCount = (uint)indexCount;
        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            if (segment.IndexCount == 0)
            {
                continue;
            }

            if (segment.StartIndex > totalIndexCount ||
                segment.IndexCount > totalIndexCount - segment.StartIndex)
            {
                throw new ArgumentException(
                    $"Segment {i} references indices outside the payload bounds.",
                    nameof(segments));
            }
        }
    }
}
