using System.Numerics;
using Avalonia;
using Videra.Core.Cameras;
using Videra.Core.Graphics;
using Videra.Core.Selection;
using Videra.Core.Selection.Annotations;

namespace Videra.Avalonia.Controls.Interaction;

internal sealed class VideraAnnotationIntentResolver
{
    private readonly SceneHitTestService _hitTestService = new();

    public bool TryResolveAnchor(
        OrbitCamera camera,
        Vector2 viewportSize,
        IReadOnlyList<Object3D> objects,
        Point position,
        out AnnotationAnchorDescriptor anchor)
    {
        ArgumentNullException.ThrowIfNull(camera);
        ArgumentNullException.ThrowIfNull(objects);

        var hit = _hitTestService.HitTest(new SceneHitTestRequest(
            camera,
            viewportSize,
            ToVector2(position),
            objects)).PrimaryHit;
        if (hit is not null)
        {
            anchor = AnnotationAnchorDescriptor.ForObject(hit.ObjectId);
            return true;
        }

        if (!camera.TryCreatePickingRay(ToVector2(position), viewportSize, out var origin, out var direction))
        {
            anchor = default;
            return false;
        }

        var planeNormal = Vector3.Normalize(camera.Target - camera.Position);
        var denominator = Vector3.Dot(direction, planeNormal);
        if (Math.Abs(denominator) <= 1e-5f)
        {
            anchor = default;
            return false;
        }

        var distance = Vector3.Dot(camera.Target - origin, planeNormal) / denominator;
        if (distance <= 0f)
        {
            anchor = default;
            return false;
        }

        anchor = AnnotationAnchorDescriptor.ForWorldPoint(origin + direction * distance);
        return true;
    }

    private static Vector2 ToVector2(Point point)
    {
        return new Vector2((float)point.X, (float)point.Y);
    }
}
