using System.Numerics;
using Videra.Core.Cameras;
using Videra.Core.Graphics;
using Videra.Core.Selection;

namespace Videra.Core.Overlay;

public sealed class SelectionOutlineProjector
{
    public IReadOnlyList<SelectionOutlineProjection> Project(
        IReadOnlyList<Guid> selectionObjectIds,
        Guid? primaryObjectId,
        IReadOnlyList<Object3D> sceneObjects,
        OrbitCamera camera,
        Vector2 viewportSize)
    {
        ArgumentNullException.ThrowIfNull(selectionObjectIds);
        ArgumentNullException.ThrowIfNull(sceneObjects);
        ArgumentNullException.ThrowIfNull(camera);

        if (viewportSize.X <= 0f || viewportSize.Y <= 0f)
        {
            return Array.Empty<SelectionOutlineProjection>();
        }

        var projections = new List<SelectionOutlineProjection>(selectionObjectIds.Count);
        foreach (var objectId in selectionObjectIds.Distinct())
        {
            var sceneObject = sceneObjects.FirstOrDefault(obj => obj.Id == objectId);
            if (sceneObject?.WorldBounds is not Geometry.BoundingBox3 bounds)
            {
                continue;
            }

            if (!SceneBoundsProjector.TryProjectBounds(bounds, camera, viewportSize, out var screenBounds))
            {
                continue;
            }

            projections.Add(new SelectionOutlineProjection(
                objectId,
                screenBounds,
                objectId == primaryObjectId));
        }

        return projections;
    }
}
