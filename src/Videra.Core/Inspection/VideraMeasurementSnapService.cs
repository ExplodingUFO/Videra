using System.Numerics;
using Videra.Core.Graphics;
using Videra.Core.Selection;

namespace Videra.Core.Inspection;

internal sealed class VideraMeasurementSnapService
{
    public VideraMeasurementAnchor ResolveAnchor(
        SceneHitTestResult.SceneHit? hit,
        VideraMeasurementSnapMode snapMode,
        VideraMeasurementAnchor? pendingAnchor,
        Vector3 fallbackWorldPoint)
    {
        var candidate = hit is not null
            ? ResolveHitAnchor(hit, snapMode)
            : VideraMeasurementAnchor.ForWorldPoint(fallbackWorldPoint);

        if (snapMode != VideraMeasurementSnapMode.AxisLocked || pendingAnchor is not VideraMeasurementAnchor startAnchor)
        {
            return candidate;
        }

        return ApplyAxisLock(startAnchor, candidate);
    }

    private static VideraMeasurementAnchor ResolveHitAnchor(
        SceneHitTestResult.SceneHit hit,
        VideraMeasurementSnapMode snapMode)
    {
        return snapMode switch
        {
            VideraMeasurementSnapMode.Vertex when TryResolveNearestVertex(hit, out var vertex) =>
                VideraMeasurementAnchor.ForObjectPoint(hit.ObjectId, vertex),
            VideraMeasurementSnapMode.EdgeMidpoint when TryResolveNearestEdgeMidpoint(hit, out var midpoint) =>
                VideraMeasurementAnchor.ForObjectPoint(hit.ObjectId, midpoint),
            _ => VideraMeasurementAnchor.ForObjectPoint(hit.ObjectId, hit.WorldPoint)
        };
    }

    private static bool TryResolveNearestVertex(SceneHitTestResult.SceneHit hit, out Vector3 worldPoint)
    {
        worldPoint = default;
        if (!TryGetWorldTriangle(hit, out var a, out var b, out var c))
        {
            return false;
        }

        worldPoint = a;
        var closestDistance = Vector3.DistanceSquared(hit.WorldPoint, a);

        var bDistance = Vector3.DistanceSquared(hit.WorldPoint, b);
        if (bDistance < closestDistance)
        {
            closestDistance = bDistance;
            worldPoint = b;
        }

        var cDistance = Vector3.DistanceSquared(hit.WorldPoint, c);
        if (cDistance < closestDistance)
        {
            worldPoint = c;
        }

        return true;
    }

    private static bool TryResolveNearestEdgeMidpoint(SceneHitTestResult.SceneHit hit, out Vector3 worldPoint)
    {
        worldPoint = default;
        if (!TryGetWorldTriangle(hit, out var a, out var b, out var c))
        {
            return false;
        }

        var ab = (a + b) * 0.5f;
        var bc = (b + c) * 0.5f;
        var ca = (c + a) * 0.5f;

        worldPoint = ab;
        var closestDistance = Vector3.DistanceSquared(hit.WorldPoint, ab);

        var bcDistance = Vector3.DistanceSquared(hit.WorldPoint, bc);
        if (bcDistance < closestDistance)
        {
            closestDistance = bcDistance;
            worldPoint = bc;
        }

        var caDistance = Vector3.DistanceSquared(hit.WorldPoint, ca);
        if (caDistance < closestDistance)
        {
            worldPoint = ca;
        }

        return true;
    }

    private static bool TryGetWorldTriangle(
        SceneHitTestResult.SceneHit hit,
        out Vector3 a,
        out Vector3 b,
        out Vector3 c)
    {
        a = default;
        b = default;
        c = default;

        if (hit.PrimitiveIndex is not int primitiveIndex ||
            hit.Object?.MeshPayload is not MeshPayload payload ||
            payload.Indices.Length < ((primitiveIndex + 1) * 3))
        {
            return false;
        }

        var triangleStart = primitiveIndex * 3;
        var world = hit.Object.WorldMatrix;
        a = Vector3.Transform(payload.Vertices[payload.Indices[triangleStart]].Position, world);
        b = Vector3.Transform(payload.Vertices[payload.Indices[triangleStart + 1]].Position, world);
        c = Vector3.Transform(payload.Vertices[payload.Indices[triangleStart + 2]].Position, world);
        return true;
    }

    private static VideraMeasurementAnchor ApplyAxisLock(
        VideraMeasurementAnchor startAnchor,
        VideraMeasurementAnchor candidateAnchor)
    {
        var start = startAnchor.WorldPoint;
        var candidate = candidateAnchor.WorldPoint;
        var delta = candidate - start;
        var absX = MathF.Abs(delta.X);
        var absY = MathF.Abs(delta.Y);
        var absZ = MathF.Abs(delta.Z);

        Vector3 lockedPoint;
        if (absX >= absY && absX >= absZ)
        {
            lockedPoint = new Vector3(candidate.X, start.Y, start.Z);
        }
        else if (absY >= absZ)
        {
            lockedPoint = new Vector3(start.X, candidate.Y, start.Z);
        }
        else
        {
            lockedPoint = new Vector3(start.X, start.Y, candidate.Z);
        }

        return VideraMeasurementAnchor.ForWorldPoint(lockedPoint);
    }
}
