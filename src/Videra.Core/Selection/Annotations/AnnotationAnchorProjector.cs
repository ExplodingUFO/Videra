using System.Numerics;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics;

namespace Videra.Core.Selection.Annotations;

public sealed class AnnotationAnchorProjector
{
    public AnnotationProjectionResult Project(
        AnnotationAnchorDescriptor anchor,
        OrbitCamera camera,
        Vector2 viewportSize,
        IReadOnlyList<Object3D> objects)
    {
        ArgumentNullException.ThrowIfNull(camera);
        ArgumentNullException.ThrowIfNull(objects);

        return anchor.Kind switch
        {
            AnnotationAnchorKind.Object => ProjectObjectAnchor(anchor, camera, viewportSize, objects),
            AnnotationAnchorKind.WorldPoint => ProjectWorldPointAnchor(anchor, camera, viewportSize),
            _ => AnnotationProjectionResult.Hidden(AnnotationProjectionClipStatus.MissingObject)
        };
    }

    private static AnnotationProjectionResult ProjectObjectAnchor(
        AnnotationAnchorDescriptor anchor,
        OrbitCamera camera,
        Vector2 viewportSize,
        IReadOnlyList<Object3D> objects)
    {
        if (anchor.ObjectId is not Guid objectId)
        {
            return AnnotationProjectionResult.Hidden(AnnotationProjectionClipStatus.MissingObject);
        }

        var sceneObject = objects.FirstOrDefault(obj => obj.Id == objectId);
        if (sceneObject is null)
        {
            return AnnotationProjectionResult.Hidden(AnnotationProjectionClipStatus.MissingObject);
        }

        if (sceneObject.WorldBounds is not BoundingBox3 bounds)
        {
            return AnnotationProjectionResult.Hidden(AnnotationProjectionClipStatus.ObjectHasNoWorldBounds, sceneObject.Id);
        }

        return ProjectWorldPoint(camera, viewportSize, bounds.Center, sceneObject.Id);
    }

    private static AnnotationProjectionResult ProjectWorldPointAnchor(
        AnnotationAnchorDescriptor anchor,
        OrbitCamera camera,
        Vector2 viewportSize)
    {
        if (anchor.WorldPoint is not Vector3 worldPoint)
        {
            return AnnotationProjectionResult.Hidden(AnnotationProjectionClipStatus.OutsideClipDepth);
        }

        return ProjectWorldPoint(camera, viewportSize, worldPoint, null);
    }

    private static AnnotationProjectionResult ProjectWorldPoint(
        OrbitCamera camera,
        Vector2 viewportSize,
        Vector3 worldPoint,
        Guid? resolvedObjectId)
    {
        if (camera.TryProjectWorldPoint(worldPoint, viewportSize, out var screenPoint))
        {
            return AnnotationProjectionResult.Visible(screenPoint, resolvedObjectId);
        }

        return AnnotationProjectionResult.Hidden(
            ClassifyWorldPointVisibility(camera, worldPoint),
            resolvedObjectId);
    }

    private static AnnotationProjectionClipStatus ClassifyWorldPointVisibility(
        OrbitCamera camera,
        Vector3 worldPoint)
    {
        var clip = Vector4.Transform(new Vector4(worldPoint, 1f), camera.ViewMatrix * camera.ProjectionMatrix);

        if (clip.W <= 0f)
        {
            return AnnotationProjectionClipStatus.BehindCamera;
        }

        var ndc = new Vector3(clip.X, clip.Y, clip.Z) / clip.W;

        if (float.IsNaN(ndc.Z) || float.IsInfinity(ndc.Z) || ndc.Z < 0f || ndc.Z > 1f)
        {
            return AnnotationProjectionClipStatus.OutsideClipDepth;
        }

        return AnnotationProjectionClipStatus.OutsideClipDepth;
    }
}
