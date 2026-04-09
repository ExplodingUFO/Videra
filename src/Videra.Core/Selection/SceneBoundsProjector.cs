using System.Numerics;
using Videra.Core.Cameras;
using Videra.Core.Geometry;

namespace Videra.Core.Selection;

internal static class SceneBoundsProjector
{
    private const float ClipEpsilon = 1e-5f;

    private static readonly (int A, int B, int C, int D)[] BoxFaces =
    [
        (0, 1, 3, 2),
        (4, 6, 7, 5),
        (0, 4, 5, 1),
        (2, 3, 7, 6),
        (0, 2, 6, 4),
        (1, 5, 7, 3)
    ];

    public static bool TryProjectBounds(
        BoundingBox3 bounds,
        OrbitCamera camera,
        Vector2 viewportSize,
        out ProjectedScreenRect screenRect)
    {
        ArgumentNullException.ThrowIfNull(camera);

        if (!bounds.IsValid || viewportSize.X <= 0f || viewportSize.Y <= 0f)
        {
            screenRect = default;
            return false;
        }

        var viewProjection = camera.ViewMatrix * camera.ProjectionMatrix;
        var clipCorners = CreateClipCorners(bounds, viewProjection);
        var projectedPoints = new List<Vector2>(24);

        foreach (var face in BoxFaces)
        {
            var polygon = new List<Vector4>(4)
            {
                clipCorners[face.A],
                clipCorners[face.B],
                clipCorners[face.C],
                clipCorners[face.D]
            };

            ClipPolygonAgainstFrustum(polygon);
            if (polygon.Count == 0)
            {
                continue;
            }

            foreach (var clipPoint in polygon)
            {
                if (TryProjectClipPoint(clipPoint, viewportSize, out var screenPoint))
                {
                    projectedPoints.Add(screenPoint);
                }
            }
        }

        if (projectedPoints.Count == 0)
        {
            screenRect = default;
            return false;
        }

        var minX = projectedPoints.Min(point => point.X);
        var minY = projectedPoints.Min(point => point.Y);
        var maxX = projectedPoints.Max(point => point.X);
        var maxY = projectedPoints.Max(point => point.Y);
        screenRect = new ProjectedScreenRect(minX, minY, maxX, maxY);
        return true;
    }

    private static Vector4[] CreateClipCorners(BoundingBox3 bounds, Matrix4x4 viewProjection)
    {
        var corners = CreateCorners(bounds);
        var clipCorners = new Vector4[corners.Length];
        for (var i = 0; i < corners.Length; i++)
        {
            clipCorners[i] = Vector4.Transform(new Vector4(corners[i], 1f), viewProjection);
        }

        return clipCorners;
    }

    private static void ClipPolygonAgainstFrustum(List<Vector4> polygon)
    {
        ClipPolygonAgainstPlane(polygon, static point => point.X + point.W);
        ClipPolygonAgainstPlane(polygon, static point => point.W - point.X);
        ClipPolygonAgainstPlane(polygon, static point => point.Y + point.W);
        ClipPolygonAgainstPlane(polygon, static point => point.W - point.Y);
        ClipPolygonAgainstPlane(polygon, static point => point.Z);
        ClipPolygonAgainstPlane(polygon, static point => point.W - point.Z);
    }

    private static void ClipPolygonAgainstPlane(List<Vector4> polygon, Func<Vector4, float> planeDistance)
    {
        if (polygon.Count == 0)
        {
            return;
        }

        var input = polygon.ToArray();
        polygon.Clear();

        var previous = input[^1];
        var previousDistance = planeDistance(previous);
        var previousInside = previousDistance >= -ClipEpsilon;

        foreach (var current in input)
        {
            var currentDistance = planeDistance(current);
            var currentInside = currentDistance >= -ClipEpsilon;

            if (currentInside != previousInside)
            {
                var interpolation = previousDistance / (previousDistance - currentDistance);
                polygon.Add(Vector4.Lerp(previous, current, interpolation));
            }

            if (currentInside)
            {
                polygon.Add(current);
            }

            previous = current;
            previousDistance = currentDistance;
            previousInside = currentInside;
        }
    }

    private static bool TryProjectClipPoint(Vector4 clipPoint, Vector2 viewportSize, out Vector2 screenPoint)
    {
        if (clipPoint.W <= ClipEpsilon)
        {
            screenPoint = default;
            return false;
        }

        var ndc = new Vector3(clipPoint.X, clipPoint.Y, clipPoint.Z) / clipPoint.W;
        if (float.IsNaN(ndc.X) || float.IsInfinity(ndc.X) ||
            float.IsNaN(ndc.Y) || float.IsInfinity(ndc.Y) ||
            float.IsNaN(ndc.Z) || float.IsInfinity(ndc.Z))
        {
            screenPoint = default;
            return false;
        }

        var x = Math.Clamp(ndc.X, -1f, 1f);
        var y = Math.Clamp(ndc.Y, -1f, 1f);
        screenPoint = new Vector2(
            ((x + 1f) * 0.5f) * viewportSize.X,
            ((1f - y) * 0.5f) * viewportSize.Y);
        return true;
    }

    private static Vector3[] CreateCorners(BoundingBox3 bounds)
    {
        return
        [
            new Vector3(bounds.Min.X, bounds.Min.Y, bounds.Min.Z),
            new Vector3(bounds.Min.X, bounds.Min.Y, bounds.Max.Z),
            new Vector3(bounds.Min.X, bounds.Max.Y, bounds.Min.Z),
            new Vector3(bounds.Min.X, bounds.Max.Y, bounds.Max.Z),
            new Vector3(bounds.Max.X, bounds.Min.Y, bounds.Min.Z),
            new Vector3(bounds.Max.X, bounds.Min.Y, bounds.Max.Z),
            new Vector3(bounds.Max.X, bounds.Max.Y, bounds.Min.Z),
            new Vector3(bounds.Max.X, bounds.Max.Y, bounds.Max.Z)
        ];
    }
}

internal readonly record struct ProjectedScreenRect(float MinX, float MinY, float MaxX, float MaxY)
{
    public float Width => MaxX - MinX;

    public float Height => MaxY - MinY;
}
