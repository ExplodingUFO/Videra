using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics;

namespace Videra.Core.Selection;

public sealed class SceneBoxSelectionService
{
    public SceneBoxSelectionResult Select(SceneBoxSelectionQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Objects.Count == 0)
        {
            return SceneBoxSelectionResult.Empty;
        }

        var selectionRect = CreateSelectionRect(query.StartPoint, query.EndPoint);
        var selectedObjects = new List<Object3D>(query.Objects.Count);
        var selectedIds = new List<Guid>(query.Objects.Count);

        foreach (var obj in query.Objects)
        {
            if (obj.WorldBounds is not BoundingBox3 bounds)
            {
                continue;
            }

            if (!TryProjectBounds(bounds, query.Camera, query.ViewportSize, out var objectRect))
            {
                continue;
            }

            var isMatch = query.Mode == SceneBoxSelectionMode.FullyInside
                ? Contains(selectionRect, objectRect)
                : Intersects(selectionRect, objectRect);

            if (!isMatch)
            {
                continue;
            }

            selectedObjects.Add(obj);
            selectedIds.Add(obj.Id);
        }

        return selectedIds.Count == 0
            ? SceneBoxSelectionResult.Empty
            : new SceneBoxSelectionResult(selectedIds.ToArray(), selectedObjects.ToArray());
    }

    private static bool TryProjectBounds(
        BoundingBox3 bounds,
        Cameras.OrbitCamera camera,
        Vector2 viewportSize,
        out ScreenRect screenRect)
    {
        var corners = GetCorners(bounds);
        var projectedPoints = new List<Vector2>(corners.Length);

        foreach (var corner in corners)
        {
            if (TryProjectPoint(corner, camera, viewportSize, out var screenPoint))
            {
                projectedPoints.Add(screenPoint);
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
        screenRect = new ScreenRect(minX, minY, maxX, maxY);
        return true;
    }

    private static bool TryProjectPoint(
        Vector3 point,
        Cameras.OrbitCamera camera,
        Vector2 viewportSize,
        out Vector2 screenPoint)
    {
        var viewport = NormalizeViewport(viewportSize);
        var clip = Vector4.Transform(new Vector4(point, 1f), camera.ViewMatrix * camera.ProjectionMatrix);

        if (clip.W <= 0f)
        {
            screenPoint = default;
            return false;
        }

        var ndc = new Vector3(clip.X, clip.Y, clip.Z) / clip.W;
        screenPoint = new Vector2(
            ((ndc.X + 1f) * 0.5f) * viewport.X,
            ((1f - ndc.Y) * 0.5f) * viewport.Y);
        return true;
    }

    private static Vector2 NormalizeViewport(Vector2 viewportSize)
    {
        return new Vector2(
            viewportSize.X > 0f ? viewportSize.X : 1f,
            viewportSize.Y > 0f ? viewportSize.Y : 1f);
    }

    private static ScreenRect CreateSelectionRect(Vector2 startPoint, Vector2 endPoint)
    {
        return new ScreenRect(
            MathF.Min(startPoint.X, endPoint.X),
            MathF.Min(startPoint.Y, endPoint.Y),
            MathF.Max(startPoint.X, endPoint.X),
            MathF.Max(startPoint.Y, endPoint.Y));
    }

    private static bool Contains(ScreenRect outer, ScreenRect inner)
    {
        return inner.MinX >= outer.MinX &&
               inner.MaxX <= outer.MaxX &&
               inner.MinY >= outer.MinY &&
               inner.MaxY <= outer.MaxY;
    }

    private static bool Intersects(ScreenRect left, ScreenRect right)
    {
        return left.MinX <= right.MaxX &&
               left.MaxX >= right.MinX &&
               left.MinY <= right.MaxY &&
               left.MaxY >= right.MinY;
    }

    private static Vector3[] GetCorners(BoundingBox3 bounds)
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

    private readonly record struct ScreenRect(float MinX, float MinY, float MaxX, float MaxY);
}
