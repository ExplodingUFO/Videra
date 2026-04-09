using System.Numerics;

namespace Videra.Core.Selection.Annotations;

public enum AnnotationProjectionClipStatus
{
    Visible = 0,
    MissingObject = 1,
    ObjectHasNoWorldBounds = 2,
    BehindCamera = 3,
    OutsideClipDepth = 4,
    OutsideViewport = 5
}

public readonly record struct AnnotationProjectionResult
{
    public AnnotationProjectionResult(
        bool isVisible,
        Vector2 screenPosition,
        AnnotationProjectionClipStatus clipStatus,
        Guid? resolvedObjectId)
    {
        IsVisible = isVisible;
        ScreenPosition = screenPosition;
        ClipStatus = clipStatus;
        ResolvedObjectId = resolvedObjectId;
    }

    public bool IsVisible { get; }

    public Vector2 ScreenPosition { get; }

    public AnnotationProjectionClipStatus ClipStatus { get; }

    public Guid? ResolvedObjectId { get; }

    public static AnnotationProjectionResult Visible(Vector2 screenPosition, Guid? resolvedObjectId = null)
    {
        return new AnnotationProjectionResult(true, screenPosition, AnnotationProjectionClipStatus.Visible, resolvedObjectId);
    }

    public static AnnotationProjectionResult Hidden(
        AnnotationProjectionClipStatus clipStatus,
        Guid? resolvedObjectId = null)
    {
        return new AnnotationProjectionResult(false, default, clipStatus, resolvedObjectId);
    }
}
