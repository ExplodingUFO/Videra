using System.Numerics;

namespace Videra.Core.Selection.Annotations;

public readonly record struct AnnotationAnchorDescriptor
{
    public AnnotationAnchorDescriptor(
        AnnotationAnchorKind kind,
        Guid? objectId,
        Vector3? worldPoint)
    {
        Kind = kind;
        ObjectId = objectId;
        WorldPoint = worldPoint;
    }

    public AnnotationAnchorKind Kind { get; }

    public Guid? ObjectId { get; }

    public Vector3? WorldPoint { get; }

    public static AnnotationAnchorDescriptor ForObject(Guid objectId)
    {
        return new AnnotationAnchorDescriptor(AnnotationAnchorKind.Object, objectId, null);
    }

    public static AnnotationAnchorDescriptor ForWorldPoint(Vector3 worldPoint)
    {
        return new AnnotationAnchorDescriptor(AnnotationAnchorKind.WorldPoint, null, worldPoint);
    }
}
