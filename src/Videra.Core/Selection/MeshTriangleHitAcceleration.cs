using System.Numerics;
using System.Runtime.CompilerServices;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Selection;

internal static class MeshTriangleHitAccelerationCache
{
    private static readonly ConditionalWeakTable<MeshPayload, MeshTriangleHitAcceleration> Cache = new();

    public static MeshTriangleHitAcceleration GetOrCreate(MeshPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return Cache.GetValue(payload, static key => new MeshTriangleHitAcceleration(key));
    }
}

internal sealed class MeshTriangleHitAcceleration
{
    private const int LeafTriangleThreshold = 4;
    private readonly MeshPayload _payload;
    private readonly int[] _triangleOrder;
    private readonly Node[] _nodes;

    public MeshTriangleHitAcceleration(MeshPayload payload)
    {
        _payload = payload ?? throw new ArgumentNullException(nameof(payload));

        var triangleCount = HasValidTriangleIndices(payload)
            ? payload.Indices.Length / 3
            : 0;
        _triangleOrder = Enumerable.Range(0, triangleCount).ToArray();
        if (triangleCount == 0)
        {
            _nodes = Array.Empty<Node>();
            return;
        }

        var nodes = new List<Node>(triangleCount * 2);
        BuildNode(nodes, 0, triangleCount);
        _nodes = nodes.ToArray();
    }

    private static bool HasValidTriangleIndices(MeshPayload payload)
    {
        if (payload.Topology != MeshTopology.Triangles || payload.Indices.Length < 3 || payload.Indices.Length % 3 != 0)
        {
            return false;
        }

        var vertexCount = payload.Vertices.Length;
        return payload.Indices.All(index => index < vertexCount);
    }

    public bool TryIntersect(Vector3 localOrigin, Vector3 localDirection, out MeshTriangleHit hit)
    {
        hit = default;
        if (_nodes.Length == 0)
        {
            return false;
        }

        var stack = new Stack<int>();
        stack.Push(0);

        var found = false;
        var closestDistance = float.PositiveInfinity;
        var closestPrimitive = -1;
        var closestPoint = Vector3.Zero;
        var closestNormal = Vector3.Zero;

        while (stack.Count > 0)
        {
            var nodeIndex = stack.Pop();
            var node = _nodes[nodeIndex];
            if (!TryIntersectBounds(node.Bounds, localOrigin, localDirection, out var nodeDistance) ||
                nodeDistance > closestDistance)
            {
                continue;
            }

            if (node.IsLeaf)
            {
                for (var i = node.Start; i < node.Start + node.Count; i++)
                {
                    var primitiveIndex = _triangleOrder[i];
                    if (!TryIntersectTriangle(localOrigin, localDirection, primitiveIndex, out var primitiveDistance, out var localNormal))
                    {
                        continue;
                    }

                    if (primitiveDistance >= closestDistance)
                    {
                        continue;
                    }

                    closestDistance = primitiveDistance;
                    closestPrimitive = primitiveIndex;
                    closestPoint = localOrigin + (localDirection * primitiveDistance);
                    closestNormal = localNormal;
                    found = true;
                }

                continue;
            }

            var hitLeft = TryIntersectBounds(_nodes[node.Left].Bounds, localOrigin, localDirection, out var leftDistance);
            var hitRight = TryIntersectBounds(_nodes[node.Right].Bounds, localOrigin, localDirection, out var rightDistance);

            if (hitLeft && hitRight)
            {
                if (leftDistance <= rightDistance)
                {
                    stack.Push(node.Right);
                    stack.Push(node.Left);
                }
                else
                {
                    stack.Push(node.Left);
                    stack.Push(node.Right);
                }
            }
            else if (hitLeft)
            {
                stack.Push(node.Left);
            }
            else if (hitRight)
            {
                stack.Push(node.Right);
            }
        }

        if (!found)
        {
            return false;
        }

        hit = new MeshTriangleHit(closestPrimitive, closestDistance, closestPoint, closestNormal);
        return true;
    }

    private int BuildNode(List<Node> nodes, int start, int count)
    {
        var bounds = ComputeBounds(start, count);
        var nodeIndex = nodes.Count;
        nodes.Add(default);

        if (count <= LeafTriangleThreshold)
        {
            nodes[nodeIndex] = Node.Leaf(bounds, start, count);
            return nodeIndex;
        }

        var axis = SelectSplitAxis(start, count);
        Array.Sort(_triangleOrder, start, count, Comparer<int>.Create((left, right) =>
        {
            var leftCentroid = GetTriangleBounds(left).Center;
            var rightCentroid = GetTriangleBounds(right).Center;
            return axis switch
            {
                0 => leftCentroid.X.CompareTo(rightCentroid.X),
                1 => leftCentroid.Y.CompareTo(rightCentroid.Y),
                _ => leftCentroid.Z.CompareTo(rightCentroid.Z)
            };
        }));

        var midpoint = start + (count / 2);
        var left = BuildNode(nodes, start, midpoint - start);
        var right = BuildNode(nodes, midpoint, (start + count) - midpoint);
        nodes[nodeIndex] = Node.Branch(bounds, left, right);
        return nodeIndex;
    }

    private BoundingBox3 ComputeBounds(int start, int count)
    {
        var bounds = GetTriangleBounds(_triangleOrder[start]);
        for (var i = start + 1; i < start + count; i++)
        {
            bounds = bounds.Encapsulate(GetTriangleBounds(_triangleOrder[i]));
        }

        return bounds;
    }

    private int SelectSplitAxis(int start, int count)
    {
        var centroidBounds = GetTriangleBounds(_triangleOrder[start]);
        centroidBounds = new BoundingBox3(centroidBounds.Center, centroidBounds.Center);
        for (var i = start + 1; i < start + count; i++)
        {
            var triangleBounds = GetTriangleBounds(_triangleOrder[i]);
            centroidBounds = centroidBounds.Encapsulate(new BoundingBox3(triangleBounds.Center, triangleBounds.Center));
        }

        var size = centroidBounds.Size;
        if (size.X >= size.Y && size.X >= size.Z)
        {
            return 0;
        }

        return size.Y >= size.Z ? 1 : 2;
    }

    private BoundingBox3 GetTriangleBounds(int primitiveIndex)
    {
        GetTriangleVertices(primitiveIndex, out var a, out var b, out var c);
        var min = Vector3.Min(a, Vector3.Min(b, c));
        var max = Vector3.Max(a, Vector3.Max(b, c));
        return new BoundingBox3(min, max);
    }

    private void GetTriangleVertices(int primitiveIndex, out Vector3 a, out Vector3 b, out Vector3 c)
    {
        var triangleStart = primitiveIndex * 3;
        var indices = _payload.Indices;
        var vertices = _payload.Vertices;
        a = vertices[indices[triangleStart]].Position;
        b = vertices[indices[triangleStart + 1]].Position;
        c = vertices[indices[triangleStart + 2]].Position;
    }

    private bool TryIntersectTriangle(
        Vector3 localOrigin,
        Vector3 localDirection,
        int primitiveIndex,
        out float distance,
        out Vector3 localNormal)
    {
        const float epsilon = 1e-6f;
        GetTriangleVertices(primitiveIndex, out var a, out var b, out var c);

        var edge1 = b - a;
        var edge2 = c - a;
        var p = Vector3.Cross(localDirection, edge2);
        var determinant = Vector3.Dot(edge1, p);
        if (MathF.Abs(determinant) <= epsilon)
        {
            distance = 0f;
            localNormal = Vector3.Zero;
            return false;
        }

        var inverseDeterminant = 1f / determinant;
        var t = localOrigin - a;
        var u = Vector3.Dot(t, p) * inverseDeterminant;
        if (u < 0f || u > 1f)
        {
            distance = 0f;
            localNormal = Vector3.Zero;
            return false;
        }

        var q = Vector3.Cross(t, edge1);
        var v = Vector3.Dot(localDirection, q) * inverseDeterminant;
        if (v < 0f || u + v > 1f)
        {
            distance = 0f;
            localNormal = Vector3.Zero;
            return false;
        }

        distance = Vector3.Dot(edge2, q) * inverseDeterminant;
        if (distance < 0f)
        {
            localNormal = Vector3.Zero;
            return false;
        }

        var normal = Vector3.Cross(edge1, edge2);
        if (normal.LengthSquared() <= epsilon)
        {
            localNormal = Vector3.Zero;
            return false;
        }

        localNormal = Vector3.Normalize(normal);
        return true;
    }

    private static bool TryIntersectBounds(BoundingBox3 bounds, Vector3 origin, Vector3 direction, out float distance)
    {
        const float epsilon = 1e-6f;
        var tMin = 0f;
        var tMax = float.PositiveInfinity;

        if (!TryIntersectAxis(origin.X, direction.X, bounds.Min.X, bounds.Max.X, ref tMin, ref tMax, epsilon) ||
            !TryIntersectAxis(origin.Y, direction.Y, bounds.Min.Y, bounds.Max.Y, ref tMin, ref tMax, epsilon) ||
            !TryIntersectAxis(origin.Z, direction.Z, bounds.Min.Z, bounds.Max.Z, ref tMin, ref tMax, epsilon))
        {
            distance = 0f;
            return false;
        }

        distance = tMin >= 0f ? tMin : tMax;
        return distance >= 0f;
    }

    private static bool TryIntersectAxis(
        float origin,
        float direction,
        float min,
        float max,
        ref float tMin,
        ref float tMax,
        float epsilon)
    {
        if (MathF.Abs(direction) < epsilon)
        {
            return origin >= min && origin <= max;
        }

        var inverseDirection = 1f / direction;
        var t1 = (min - origin) * inverseDirection;
        var t2 = (max - origin) * inverseDirection;

        if (t1 > t2)
        {
            (t1, t2) = (t2, t1);
        }

        tMin = MathF.Max(tMin, t1);
        tMax = MathF.Min(tMax, t2);
        return tMin <= tMax;
    }

    private readonly record struct Node(
        BoundingBox3 Bounds,
        int Start,
        int Count,
        int Left,
        int Right)
    {
        public bool IsLeaf => Count > 0;

        public static Node Leaf(BoundingBox3 bounds, int start, int count)
        {
            return new Node(bounds, start, count, -1, -1);
        }

        public static Node Branch(BoundingBox3 bounds, int left, int right)
        {
            return new Node(bounds, 0, 0, left, right);
        }
    }
}

internal readonly record struct MeshTriangleHit(
    int PrimitiveIndex,
    float Distance,
    Vector3 LocalPoint,
    Vector3 LocalNormal);
