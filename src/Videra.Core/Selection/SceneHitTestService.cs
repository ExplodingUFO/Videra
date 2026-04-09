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

        if (!TryCreateRay(request, out var rayOrigin, out var rayDirection))
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

    private static bool TryCreateRay(
        SceneHitTestRequest request,
        out Vector3 origin,
        out Vector3 direction)
    {
        var viewport = NormalizeViewport(request.ViewportSize);
        var x = (request.ScreenPoint.X / viewport.X) * 2f - 1f;
        var y = 1f - (request.ScreenPoint.Y / viewport.Y) * 2f;
        var viewProjection = request.Camera.ViewMatrix * request.Camera.ProjectionMatrix;

        if (!Matrix4x4.Invert(viewProjection, out var inverseViewProjection))
        {
            origin = default;
            direction = default;
            return false;
        }

        var nearPoint = Unproject(new Vector4(x, y, 0f, 1f), inverseViewProjection);
        var farPoint = Unproject(new Vector4(x, y, 1f, 1f), inverseViewProjection);
        origin = request.Camera.Position;
        direction = Vector3.Normalize(farPoint - nearPoint);
        return direction.LengthSquared() > 0f;
    }

    private static Vector3 Unproject(Vector4 point, Matrix4x4 inverseViewProjection)
    {
        var world = Vector4.Transform(point, inverseViewProjection);
        return new Vector3(world.X, world.Y, world.Z) / world.W;
    }

    private static Vector2 NormalizeViewport(Vector2 viewportSize)
    {
        return new Vector2(
            viewportSize.X > 0f ? viewportSize.X : 1f,
            viewportSize.Y > 0f ? viewportSize.Y : 1f);
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
