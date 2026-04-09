using System.Numerics;
using Avalonia;
using Avalonia.Input;
using Videra.Core.Cameras;
using Videra.Core.Graphics;
using Videra.Core.Selection;

namespace Videra.Avalonia.Controls.Interaction;

internal sealed class VideraSelectionIntentResolver
{
    private readonly SceneHitTestService _hitTestService = new();
    private readonly SceneBoxSelectionService _boxSelectionService = new();

    public VideraSelectionRequest? CreateClickRequest(
        OrbitCamera camera,
        Vector2 viewportSize,
        IReadOnlyList<Object3D> objects,
        Point position,
        RawInputModifiers modifiers,
        VideraInteractionOptions options)
    {
        ArgumentNullException.ThrowIfNull(camera);
        ArgumentNullException.ThrowIfNull(objects);
        ArgumentNullException.ThrowIfNull(options);

        var hit = _hitTestService.HitTest(new SceneHitTestRequest(
            camera,
            viewportSize,
            ToVector2(position),
            objects)).PrimaryHit;

        if (hit is null)
        {
            if (HasAdditiveModifier(modifiers) || options.EmptySpaceSelectionBehavior == VideraEmptySpaceSelectionBehavior.PreserveSelection)
            {
                return null;
            }

            return new VideraSelectionRequest(
                VideraSelectionOperation.Replace,
                Array.Empty<Guid>(),
                primaryObjectId: null,
                options.EmptySpaceSelectionBehavior);
        }

        return new VideraSelectionRequest(
            HasAdditiveModifier(modifiers) ? VideraSelectionOperation.Toggle : VideraSelectionOperation.Replace,
            [hit.ObjectId],
            hit.ObjectId,
            options.EmptySpaceSelectionBehavior);
    }

    public VideraSelectionRequest? CreateBoxRequest(
        OrbitCamera camera,
        Vector2 viewportSize,
        IReadOnlyList<Object3D> objects,
        Point start,
        Point end,
        RawInputModifiers modifiers,
        VideraInteractionOptions options)
    {
        ArgumentNullException.ThrowIfNull(camera);
        ArgumentNullException.ThrowIfNull(objects);
        ArgumentNullException.ThrowIfNull(options);

        var result = _boxSelectionService.Select(new SceneBoxSelectionQuery(
            camera,
            viewportSize,
            ToVector2(start),
            ToVector2(end),
            objects));

        if (result.ObjectIds.Count == 0)
        {
            if (HasAdditiveModifier(modifiers) || options.EmptySpaceSelectionBehavior == VideraEmptySpaceSelectionBehavior.PreserveSelection)
            {
                return null;
            }

            return new VideraSelectionRequest(
                VideraSelectionOperation.Replace,
                Array.Empty<Guid>(),
                primaryObjectId: null,
                options.EmptySpaceSelectionBehavior);
        }

        return new VideraSelectionRequest(
            HasAdditiveModifier(modifiers) ? VideraSelectionOperation.Add : VideraSelectionOperation.Replace,
            result.ObjectIds,
            result.ObjectIds[0],
            options.EmptySpaceSelectionBehavior);
    }

    private static bool HasAdditiveModifier(RawInputModifiers modifiers)
    {
        return (modifiers & RawInputModifiers.Control) != 0 || (modifiers & RawInputModifiers.Shift) != 0;
    }

    private static Vector2 ToVector2(Point point)
    {
        return new Vector2((float)point.X, (float)point.Y);
    }
}
