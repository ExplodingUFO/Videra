using System.Numerics;
using Videra.Core.Selection.Annotations;

namespace Videra.Avalonia.Controls.Interaction;

public sealed class VideraWorldPointAnnotation : VideraAnnotation
{
    public Vector3 WorldPoint { get; set; }

    public override AnnotationAnchorDescriptor Anchor => AnnotationAnchorDescriptor.ForWorldPoint(WorldPoint);
}
