using System.Numerics;

namespace Videra.Core.Geometry;

public readonly record struct BoundingBox3(Vector3 Min, Vector3 Max)
{
    public Vector3 Center => (Min + Max) * 0.5f;

    public Vector3 Size => Max - Min;

    public bool IsValid =>
        Min.X <= Max.X &&
        Min.Y <= Max.Y &&
        Min.Z <= Max.Z;

    public static BoundingBox3 FromVertices(ReadOnlySpan<VertexPositionNormalColor> vertices)
    {
        if (vertices.Length == 0)
        {
            throw new ArgumentException("At least one vertex is required to compute bounds.", nameof(vertices));
        }

        var min = vertices[0].Position;
        var max = vertices[0].Position;

        for (var i = 1; i < vertices.Length; i++)
        {
            min = Vector3.Min(min, vertices[i].Position);
            max = Vector3.Max(max, vertices[i].Position);
        }

        return new BoundingBox3(min, max);
    }

    public BoundingBox3 Encapsulate(BoundingBox3 other)
    {
        return new BoundingBox3(
            Vector3.Min(Min, other.Min),
            Vector3.Max(Max, other.Max));
    }

    public BoundingBox3 Transform(Matrix4x4 matrix)
    {
        var corners = GetCorners();
        var min = Vector3.Transform(corners[0], matrix);
        var max = min;

        for (var i = 1; i < corners.Length; i++)
        {
            var transformed = Vector3.Transform(corners[i], matrix);
            min = Vector3.Min(min, transformed);
            max = Vector3.Max(max, transformed);
        }

        return new BoundingBox3(min, max);
    }

    private Vector3[] GetCorners()
    {
        return
        [
            new Vector3(Min.X, Min.Y, Min.Z),
            new Vector3(Min.X, Min.Y, Max.Z),
            new Vector3(Min.X, Max.Y, Min.Z),
            new Vector3(Min.X, Max.Y, Max.Z),
            new Vector3(Max.X, Min.Y, Min.Z),
            new Vector3(Max.X, Min.Y, Max.Z),
            new Vector3(Max.X, Max.Y, Min.Z),
            new Vector3(Max.X, Max.Y, Max.Z)
        ];
    }
}
