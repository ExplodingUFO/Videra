using System.Numerics;
using Videra.Core.Geometry;

namespace Videra.Core.Scene;

public static class SceneGeometry
{
    public static MeshData Buffer(
        ReadOnlySpan<Vector3> positions,
        ReadOnlySpan<uint> indices,
        ReadOnlySpan<Vector3> normals = default,
        ReadOnlySpan<RgbaFloat> colors = default)
    {
        var vertices = new VertexPositionNormalColor[positions.Length];
        for (var i = 0; i < positions.Length; i++)
        {
            vertices[i] = new VertexPositionNormalColor(
                positions[i],
                normals.Length == positions.Length ? normals[i] : Vector3.UnitZ,
                colors.Length == positions.Length ? colors[i] : RgbaFloat.White);
        }

        return new MeshData
        {
            Vertices = vertices,
            Indices = indices.ToArray()
        };
    }

    public static MeshData Triangle(float size = 1f, RgbaFloat? color = null)
    {
        var half = size * 0.5f;
        var positions = new[]
        {
            new Vector3(-half, -half, 0f),
            new Vector3(half, -half, 0f),
            new Vector3(0f, half, 0f)
        };
        var colors = FillColors(positions.Length, color ?? RgbaFloat.White);
        return Buffer(positions, [0, 1, 2], colors: colors);
    }

    public static MeshData Quad(float width = 1f, float height = 1f, RgbaFloat? color = null)
    {
        var halfWidth = width * 0.5f;
        var halfHeight = height * 0.5f;
        var positions = new[]
        {
            new Vector3(-halfWidth, -halfHeight, 0f),
            new Vector3(halfWidth, -halfHeight, 0f),
            new Vector3(halfWidth, halfHeight, 0f),
            new Vector3(-halfWidth, halfHeight, 0f)
        };
        var colors = FillColors(positions.Length, color ?? RgbaFloat.White);
        return Buffer(positions, [0, 1, 2, 0, 2, 3], colors: colors);
    }

    public static MeshData Cube(float size = 1f, RgbaFloat? color = null)
    {
        var half = size * 0.5f;
        var c = color ?? RgbaFloat.White;
        var vertices = new List<VertexPositionNormalColor>(24);
        var indices = new List<uint>(36);

        AddFace(vertices, indices, Vector3.UnitZ, c,
            new Vector3(-half, -half, half),
            new Vector3(half, -half, half),
            new Vector3(half, half, half),
            new Vector3(-half, half, half));
        AddFace(vertices, indices, -Vector3.UnitZ, c,
            new Vector3(half, -half, -half),
            new Vector3(-half, -half, -half),
            new Vector3(-half, half, -half),
            new Vector3(half, half, -half));
        AddFace(vertices, indices, Vector3.UnitX, c,
            new Vector3(half, -half, half),
            new Vector3(half, -half, -half),
            new Vector3(half, half, -half),
            new Vector3(half, half, half));
        AddFace(vertices, indices, -Vector3.UnitX, c,
            new Vector3(-half, -half, -half),
            new Vector3(-half, -half, half),
            new Vector3(-half, half, half),
            new Vector3(-half, half, -half));
        AddFace(vertices, indices, Vector3.UnitY, c,
            new Vector3(-half, half, half),
            new Vector3(half, half, half),
            new Vector3(half, half, -half),
            new Vector3(-half, half, -half));
        AddFace(vertices, indices, -Vector3.UnitY, c,
            new Vector3(-half, -half, -half),
            new Vector3(half, -half, -half),
            new Vector3(half, -half, half),
            new Vector3(-half, -half, half));

        return new MeshData
        {
            Vertices = vertices.ToArray(),
            Indices = indices.ToArray()
        };
    }

    private static RgbaFloat[] FillColors(int count, RgbaFloat color)
    {
        var colors = new RgbaFloat[count];
        Array.Fill(colors, color);
        return colors;
    }

    private static void AddFace(
        ICollection<VertexPositionNormalColor> vertices,
        ICollection<uint> indices,
        Vector3 normal,
        RgbaFloat color,
        Vector3 a,
        Vector3 b,
        Vector3 c,
        Vector3 d)
    {
        var start = (uint)vertices.Count;
        vertices.Add(new VertexPositionNormalColor(a, normal, color));
        vertices.Add(new VertexPositionNormalColor(b, normal, color));
        vertices.Add(new VertexPositionNormalColor(c, normal, color));
        vertices.Add(new VertexPositionNormalColor(d, normal, color));
        indices.Add(start);
        indices.Add(start + 1);
        indices.Add(start + 2);
        indices.Add(start);
        indices.Add(start + 2);
        indices.Add(start + 3);
    }
}
