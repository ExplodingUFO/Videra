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

            if (!SceneBoundsProjector.TryProjectBounds(bounds, query.Camera, query.ViewportSize, out var objectRect))
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

    private static ProjectedScreenRect CreateSelectionRect(Vector2 startPoint, Vector2 endPoint)
    {
        return new ProjectedScreenRect(
            MathF.Min(startPoint.X, endPoint.X),
            MathF.Min(startPoint.Y, endPoint.Y),
            MathF.Max(startPoint.X, endPoint.X),
            MathF.Max(startPoint.Y, endPoint.Y));
    }

    private static bool Contains(ProjectedScreenRect outer, ProjectedScreenRect inner)
    {
        return inner.MinX >= outer.MinX &&
               inner.MaxX <= outer.MaxX &&
               inner.MinY >= outer.MinY &&
               inner.MaxY <= outer.MaxY;
    }

    private static bool Intersects(ProjectedScreenRect left, ProjectedScreenRect right)
    {
        return left.MinX <= right.MaxX &&
               left.MaxX >= right.MinX &&
               left.MinY <= right.MaxY &&
               left.MaxY >= right.MinY;
    }
}
