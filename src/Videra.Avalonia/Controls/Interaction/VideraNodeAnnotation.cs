using Videra.Core.Selection.Annotations;

namespace Videra.Avalonia.Controls.Interaction;

public sealed class VideraNodeAnnotation : VideraAnnotation
{
    public Guid ObjectId { get; set; }

    public override AnnotationAnchorDescriptor Anchor => AnnotationAnchorDescriptor.ForObject(ObjectId);
}
