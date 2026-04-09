using Videra.Core.Selection.Annotations;

namespace Videra.Avalonia.Controls.Interaction;

public sealed class AnnotationRequestedEventArgs : EventArgs
{
    public AnnotationRequestedEventArgs(AnnotationAnchorDescriptor anchor)
    {
        Anchor = anchor;
    }

    public AnnotationAnchorDescriptor Anchor { get; }
}
