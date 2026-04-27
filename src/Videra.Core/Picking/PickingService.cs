using System.Numerics;
using Videra.Core.Cameras;
using Videra.Core.Graphics;
using Videra.Core.Inspection;
using Videra.Core.Scene;
using Videra.Core.Selection;
using Videra.Core.Selection.Annotations;

namespace Videra.Core.Picking;

public sealed class PickingService
{
    private readonly SceneHitTestService _hitTestService = new();
    private readonly VideraMeasurementSnapService _measurementSnapService = new();

    public SceneHitTestResult HitTest(
        OrbitCamera camera,
        Vector2 viewportSize,
        Vector2 screenPoint,
        IReadOnlyList<Object3D> objects,
        IReadOnlyList<InstanceBatchEntry>? instanceBatches = null)
    {
        ArgumentNullException.ThrowIfNull(camera);
        ArgumentNullException.ThrowIfNull(objects);

        return _hitTestService.HitTest(new SceneHitTestRequest(
            camera,
            viewportSize,
            screenPoint,
            objects,
            instanceBatches));
    }

    public bool TryResolveAnnotationAnchor(
        OrbitCamera camera,
        Vector2 viewportSize,
        Vector2 screenPoint,
        IReadOnlyList<Object3D> objects,
        out AnnotationAnchorDescriptor anchor)
    {
        return TryResolveAnnotationAnchor(camera, viewportSize, screenPoint, objects, null, out anchor);
    }

    public bool TryResolveAnnotationAnchor(
        OrbitCamera camera,
        Vector2 viewportSize,
        Vector2 screenPoint,
        IReadOnlyList<Object3D> objects,
        IReadOnlyList<InstanceBatchEntry>? instanceBatches,
        out AnnotationAnchorDescriptor anchor)
    {
        ArgumentNullException.ThrowIfNull(camera);
        ArgumentNullException.ThrowIfNull(objects);

        var hit = HitTest(camera, viewportSize, screenPoint, objects, instanceBatches).PrimaryHit;
        if (hit is not null)
        {
            anchor = AnnotationAnchorDescriptor.ForObject(hit.ObjectId);
            return true;
        }

        if (!camera.TryCreatePickingRay(screenPoint, viewportSize, out var origin, out var direction))
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

    public bool TryResolveMeasurementAnchor(
        OrbitCamera camera,
        Vector2 viewportSize,
        Vector2 screenPoint,
        IReadOnlyList<Object3D> objects,
        VideraMeasurementSnapMode snapMode,
        VideraMeasurementAnchor? pendingAnchor,
        out VideraMeasurementAnchor anchor)
    {
        return TryResolveMeasurementAnchor(camera, viewportSize, screenPoint, objects, null, snapMode, pendingAnchor, out anchor);
    }

    public bool TryResolveMeasurementAnchor(
        OrbitCamera camera,
        Vector2 viewportSize,
        Vector2 screenPoint,
        IReadOnlyList<Object3D> objects,
        IReadOnlyList<InstanceBatchEntry>? instanceBatches,
        VideraMeasurementSnapMode snapMode,
        VideraMeasurementAnchor? pendingAnchor,
        out VideraMeasurementAnchor anchor)
    {
        ArgumentNullException.ThrowIfNull(camera);
        ArgumentNullException.ThrowIfNull(objects);

        if (!camera.TryCreatePickingRay(screenPoint, viewportSize, out var origin, out var direction))
        {
            anchor = default;
            return false;
        }

        var hit = HitTest(camera, viewportSize, screenPoint, objects, instanceBatches).PrimaryHit;
        if (hit is not null)
        {
            anchor = _measurementSnapService.ResolveAnchor(
                hit,
                snapMode,
                pendingAnchor,
                hit.WorldPoint);
            return true;
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

        anchor = _measurementSnapService.ResolveAnchor(
            null,
            snapMode,
            pendingAnchor,
            origin + (direction * distance));
        return true;
    }
}
