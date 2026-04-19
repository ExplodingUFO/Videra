using System.Numerics;
using Videra.Core.Geometry;

namespace Videra.Core.Selection;

public sealed class SceneHitTestService
{
    public SceneHitTestResult HitTest(SceneHitTestRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Objects.Count == 0)
        {
            return SceneHitTestResult.Empty;
        }

        if (!request.Camera.TryCreatePickingRay(request.ScreenPoint, request.ViewportSize, out var rayOrigin, out var rayDirection))
        {
            return SceneHitTestResult.Empty;
        }

        var hits = request.Objects
            .Select(obj => TryCreateHit(obj, rayOrigin, rayDirection))
            .Where(hit => hit is not null)
            .Select(hit => hit!)
            .OrderBy(hit => hit.Distance)
            .ToArray();

        return hits.Length == 0 ? SceneHitTestResult.Empty : new SceneHitTestResult(hits);
    }

    private static SceneHitTestResult.SceneHit? TryCreateHit(
        Graphics.Object3D obj,
        Vector3 rayOrigin,
        Vector3 rayDirection)
    {
        if (TryCreateMeshHit(obj, rayOrigin, rayDirection, out var meshHit))
        {
            return meshHit;
        }

        if (obj.WorldBounds is not BoundingBox3 bounds)
        {
            return null;
        }

        return TryIntersectRay(bounds, rayOrigin, rayDirection, out var distance, out var normal)
            ? new SceneHitTestResult.SceneHit(
                obj.Id,
                obj,
                distance,
                rayOrigin + (rayDirection * distance),
                normal,
                PrimitiveIndex: null)
            : null;
    }

    private static bool TryCreateMeshHit(
        Graphics.Object3D obj,
        Vector3 rayOrigin,
        Vector3 rayDirection,
        out SceneHitTestResult.SceneHit? hit)
    {
        hit = null;

        var payload = obj.MeshPayload;
        if (payload is null || payload.Topology != Graphics.Abstractions.MeshTopology.Triangles || payload.Indices.Length < 3)
        {
            return false;
        }

        if (!Matrix4x4.Invert(obj.WorldMatrix, out var inverseWorld))
        {
            return false;
        }

        var localOrigin = Vector3.Transform(rayOrigin, inverseWorld);
        var localDirection = Vector3.TransformNormal(rayDirection, inverseWorld);
        if (localDirection.LengthSquared() <= 1e-8f)
        {
            return false;
        }

        localDirection = Vector3.Normalize(localDirection);

        var acceleration = MeshTriangleHitAccelerationCache.GetOrCreate(payload);
        if (!acceleration.TryIntersect(localOrigin, localDirection, out var meshHit))
        {
            return false;
        }

        var worldPoint = Vector3.Transform(meshHit.LocalPoint, obj.WorldMatrix);
        var worldNormal = TransformNormal(meshHit.LocalNormal, obj.WorldMatrix, inverseWorld);
        hit = new SceneHitTestResult.SceneHit(
            obj.Id,
            obj,
            Vector3.Distance(rayOrigin, worldPoint),
            worldPoint,
            worldNormal,
            meshHit.PrimitiveIndex);
        return true;
    }

    private static bool TryIntersectRay(BoundingBox3 bounds, Vector3 origin, Vector3 direction, out float distance, out Vector3 normal)
    {
        const float epsilon = 1e-6f;
        var tMin = 0f;
        var tMax = float.PositiveInfinity;
        var entryNormal = Vector3.Zero;
        var exitNormal = Vector3.Zero;

        if (!TryIntersectAxis(origin.X, direction.X, bounds.Min.X, bounds.Max.X, Vector3.UnitX, ref tMin, ref tMax, ref entryNormal, ref exitNormal, epsilon) ||
            !TryIntersectAxis(origin.Y, direction.Y, bounds.Min.Y, bounds.Max.Y, Vector3.UnitY, ref tMin, ref tMax, ref entryNormal, ref exitNormal, epsilon) ||
            !TryIntersectAxis(origin.Z, direction.Z, bounds.Min.Z, bounds.Max.Z, Vector3.UnitZ, ref tMin, ref tMax, ref entryNormal, ref exitNormal, epsilon))
        {
            distance = 0f;
            normal = Vector3.Zero;
            return false;
        }

        distance = tMin >= 0f ? tMin : tMax;
        normal = tMin >= 0f ? entryNormal : exitNormal;
        return distance >= 0f;
    }

    private static bool TryIntersectAxis(
        float origin,
        float direction,
        float min,
        float max,
        Vector3 axis,
        ref float tMin,
        ref float tMax,
        ref Vector3 entryNormal,
        ref Vector3 exitNormal,
        float epsilon)
    {
        if (MathF.Abs(direction) < epsilon)
        {
            return origin >= min && origin <= max;
        }

        var inverseDirection = 1f / direction;
        var t1 = (min - origin) * inverseDirection;
        var t2 = (max - origin) * inverseDirection;
        var n1 = -axis;
        var n2 = axis;

        if (t1 > t2)
        {
            (t1, t2) = (t2, t1);
            (n1, n2) = (n2, n1);
        }

        if (t1 > tMin)
        {
            tMin = t1;
            entryNormal = n1;
        }

        if (t2 < tMax)
        {
            tMax = t2;
            exitNormal = n2;
        }

        return tMin <= tMax;
    }

    private static Vector3 TransformNormal(Vector3 localNormal, Matrix4x4 worldMatrix, Matrix4x4 inverseWorld)
    {
        var normalMatrix = Matrix4x4.Transpose(inverseWorld);
        var transformed = Vector3.TransformNormal(localNormal, normalMatrix);
        if (transformed.LengthSquared() > 1e-8f)
        {
            return Vector3.Normalize(transformed);
        }

        transformed = Vector3.TransformNormal(localNormal, worldMatrix);
        return transformed.LengthSquared() > 1e-8f ? Vector3.Normalize(transformed) : Vector3.UnitZ;
    }
}
