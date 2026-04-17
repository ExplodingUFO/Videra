using System.Numerics;
using Videra.Core.Selection.Annotations;

namespace Videra.Core.Overlay;

public sealed record AnnotationLabelProjection(
    Guid AnnotationId,
    Vector2 ScreenPosition,
    AnnotationAnchorDescriptor Anchor,
    Guid? ResolvedObjectId);
