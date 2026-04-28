using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Scene;

public static class SceneGeometry
{
    public static MeshData Buffer(
        ReadOnlySpan<Vector3> positions,
        ReadOnlySpan<uint> indices,
        ReadOnlySpan<Vector3> normals = default,
        ReadOnlySpan<RgbaFloat> colors = default,
        ReadOnlySpan<Vector2> textureCoordinates = default,
        MeshTopology topology = MeshTopology.Triangles)
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
            Indices = indices.ToArray(),
            TextureCoordinateSets = textureCoordinates.Length == positions.Length
                ? [new MeshTextureCoordinateSet(0, textureCoordinates.ToArray())]
                : [],
            Topology = topology
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

    public static MeshData Plane(float width = 1f, float depth = 1f, RgbaFloat? color = null)
    {
        var halfWidth = width * 0.5f;
        var halfDepth = depth * 0.5f;
        var positions = new[]
        {
            new Vector3(-halfWidth, 0f, -halfDepth),
            new Vector3(halfWidth, 0f, -halfDepth),
            new Vector3(halfWidth, 0f, halfDepth),
            new Vector3(-halfWidth, 0f, halfDepth)
        };
        var normals = Enumerable.Repeat(Vector3.UnitY, positions.Length).ToArray();
        var colors = FillColors(positions.Length, color ?? RgbaFloat.White);
        var uvs = new[]
        {
            Vector2.Zero,
            Vector2.UnitX,
            Vector2.One,
            Vector2.UnitY
        };

        return Buffer(positions, [0, 1, 2, 0, 2, 3], normals, colors, uvs);
    }

    public static MeshData Grid(float width = 1f, float depth = 1f, int divisions = 10, RgbaFloat? color = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(divisions);

        var lineCount = divisions + 1;
        var vertices = new VertexPositionNormalColor[lineCount * 4];
        var indices = new uint[vertices.Length];
        var halfWidth = width * 0.5f;
        var halfDepth = depth * 0.5f;
        var c = color ?? RgbaFloat.LightGrey;
        var cursor = 0;

        for (var i = 0; i <= divisions; i++)
        {
            var x = -halfWidth + width * i / divisions;
            vertices[cursor] = new VertexPositionNormalColor(new Vector3(x, 0f, -halfDepth), Vector3.UnitY, c);
            indices[cursor] = (uint)cursor;
            cursor++;
            vertices[cursor] = new VertexPositionNormalColor(new Vector3(x, 0f, halfDepth), Vector3.UnitY, c);
            indices[cursor] = (uint)cursor;
            cursor++;

            var z = -halfDepth + depth * i / divisions;
            vertices[cursor] = new VertexPositionNormalColor(new Vector3(-halfWidth, 0f, z), Vector3.UnitY, c);
            indices[cursor] = (uint)cursor;
            cursor++;
            vertices[cursor] = new VertexPositionNormalColor(new Vector3(halfWidth, 0f, z), Vector3.UnitY, c);
            indices[cursor] = (uint)cursor;
            cursor++;
        }

        return new MeshData
        {
            Vertices = vertices,
            Indices = indices,
            Topology = MeshTopology.Lines
        };
    }

    public static MeshData Polyline(ReadOnlySpan<Vector3> points, RgbaFloat? color = null)
    {
        if (points.Length < 2)
        {
            throw new ArgumentException("Polyline requires at least two points.", nameof(points));
        }

        var indices = new uint[(points.Length - 1) * 2];
        var cursor = 0;
        for (var i = 0; i < points.Length - 1; i++)
        {
            indices[cursor++] = (uint)i;
            indices[cursor++] = (uint)(i + 1);
        }

        var colors = FillColors(points.Length, color ?? RgbaFloat.White);
        return Buffer(points, indices, colors: colors, topology: MeshTopology.Lines);
    }

    public static MeshData PointCloud(ReadOnlySpan<Vector3> points, ReadOnlySpan<RgbaFloat> colors = default)
    {
        if (points.IsEmpty)
        {
            throw new ArgumentException("Point cloud requires at least one point.", nameof(points));
        }

        var indices = new uint[points.Length];
        for (var i = 0; i < points.Length; i++)
        {
            indices[i] = (uint)i;
        }

        return Buffer(points, indices, colors: colors, topology: MeshTopology.Points);
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

    public static MeshData Sphere(float radius = 0.5f, int segments = 16, int rings = 8, RgbaFloat? color = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius);
        ArgumentOutOfRangeException.ThrowIfLessThan(segments, 3);
        ArgumentOutOfRangeException.ThrowIfLessThan(rings, 2);

        var c = color ?? RgbaFloat.White;
        var vertices = new List<VertexPositionNormalColor>(2 + (segments * (rings - 1)));
        var indices = new List<uint>(segments * (6 * rings - 6));

        vertices.Add(new VertexPositionNormalColor(new Vector3(0f, radius, 0f), Vector3.UnitY, c));

        for (var ring = 1; ring < rings; ring++)
        {
            var v = MathF.PI * ring / rings;
            var y = MathF.Cos(v);
            var horizontal = MathF.Sin(v);

            for (var segment = 0; segment < segments; segment++)
            {
                var u = 2f * MathF.PI * segment / segments;
                var normal = new Vector3(
                    horizontal * MathF.Cos(u),
                    y,
                    horizontal * MathF.Sin(u));
                vertices.Add(new VertexPositionNormalColor(normal * radius, normal, c));
            }
        }

        var bottomIndex = (uint)vertices.Count;
        vertices.Add(new VertexPositionNormalColor(new Vector3(0f, -radius, 0f), -Vector3.UnitY, c));

        for (var segment = 0; segment < segments; segment++)
        {
            var current = (uint)(1 + segment);
            var next = (uint)(1 + ((segment + 1) % segments));
            indices.Add(0);
            indices.Add(next);
            indices.Add(current);
        }

        for (var ring = 0; ring < rings - 2; ring++)
        {
            var currentRing = (uint)(1 + (ring * segments));
            var nextRing = (uint)(currentRing + segments);

            for (var segment = 0; segment < segments; segment++)
            {
                var current = currentRing + (uint)segment;
                var next = currentRing + (uint)((segment + 1) % segments);
                var below = nextRing + (uint)segment;
                var belowNext = nextRing + (uint)((segment + 1) % segments);
                indices.Add(current);
                indices.Add(next);
                indices.Add(belowNext);
                indices.Add(current);
                indices.Add(belowNext);
                indices.Add(below);
            }
        }

        var bottomRing = (uint)(1 + ((rings - 2) * segments));
        for (var segment = 0; segment < segments; segment++)
        {
            var current = bottomRing + (uint)segment;
            var next = bottomRing + (uint)((segment + 1) % segments);
            indices.Add(current);
            indices.Add(next);
            indices.Add(bottomIndex);
        }

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
