using System.Numerics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Geometry;

/// <summary>
/// RGBA浮点颜色，与Veldrid的RgbaFloat兼容
/// </summary>
public struct RgbaFloat
{
    public float R;
    public float G;
    public float B;
    public float A;

    public RgbaFloat(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public static readonly RgbaFloat Red = new(1, 0, 0, 1);
    public static readonly RgbaFloat Green = new(0, 1, 0, 1);
    public static readonly RgbaFloat Blue = new(0, 0, 1, 1);
    public static readonly RgbaFloat White = new(1, 1, 1, 1);
    public static readonly RgbaFloat Black = new(0, 0, 0, 1);
    public static readonly RgbaFloat Grey = new(0.5f, 0.5f, 0.5f, 1);
    public static readonly RgbaFloat LightGrey = new(0.75f, 0.75f, 0.75f, 1);
    public static readonly RgbaFloat DarkGrey = new(0.25f, 0.25f, 0.25f, 1);
}

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

public class MeshData
{
    public VertexPositionNormalColor[] Vertices { get; set; } = Array.Empty<VertexPositionNormalColor>();
    public uint[] Indices { get; set; } = Array.Empty<uint>();
    public MeshTopology Topology { get; set; } = MeshTopology.Triangles;
}
