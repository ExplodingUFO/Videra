using Avalonia.Media;
using Videra.Core.Selection.Annotations;

namespace Videra.Avalonia.Controls.Interaction;

public abstract class VideraAnnotation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Text { get; set; } = string.Empty;

    public Color Color { get; set; } = Colors.White;

    public bool IsVisible { get; set; } = true;

    public abstract AnnotationAnchorDescriptor Anchor { get; }
}
