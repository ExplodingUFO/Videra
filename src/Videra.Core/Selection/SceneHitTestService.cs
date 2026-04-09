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
        if (obj.WorldBounds is not BoundingBox3 bounds)
        {
            return null;
        }

        return TryIntersectRay(bounds, rayOrigin, rayDirection, out var distance)
            ? new SceneHitTestResult.SceneHit(obj.Id, obj, distance)
            : null;
    }
    private static bool TryIntersectRay(BoundingBox3 bounds, Vector3 origin, Vector3 direction, out float distance)
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
}
