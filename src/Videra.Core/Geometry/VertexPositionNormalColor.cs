using System.Numerics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Geometry;

/// <summary>
/// RGBA浮点颜色，与Veldrid的RgbaFloat兼容
/// </summary>
public struct RgbaFloat : IEquatable<RgbaFloat>
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

    public readonly Vector4 ToVector4() => new(R, G, B, A);

    public readonly bool Equals(RgbaFloat other) =>
        R == other.R && G == other.G && B == other.B && A == other.A;

    public override readonly bool Equals(object? obj) => obj is RgbaFloat other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(R, G, B, A);

    public static bool operator ==(RgbaFloat left, RgbaFloat right) => left.Equals(right);
    public static bool operator !=(RgbaFloat left, RgbaFloat right) => !left.Equals(right);
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
    public MeshTextureCoordinateSet[] TextureCoordinateSets { get; set; } = Array.Empty<MeshTextureCoordinateSet>();
    public MeshTopology Topology { get; set; } = MeshTopology.Triangles;
}
