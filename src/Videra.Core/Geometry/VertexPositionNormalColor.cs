using System.Numerics;
using Veldrid;

namespace Videra.Core.Geometry;

public struct VertexPositionNormalColor
{
    public Vector3 Position;
    public Vector3 Normal;
    public RgbaFloat Color;
    public const uint SizeInBytes = 40;

    public VertexPositionNormalColor(Vector3 pos, Vector3 normal, RgbaFloat col)
    {
        Position = pos;
        Normal = normal;
        Color = col;
    }
}

public enum MeshTopology
{
    Triangles,
    Points
}

public class MeshData
{
    public VertexPositionNormalColor[] Vertices { get; set; } = Array.Empty<VertexPositionNormalColor>();
    public uint[] Indices { get; set; } = Array.Empty<uint>();
    public MeshTopology Topology { get; set; } = MeshTopology.Triangles;
}