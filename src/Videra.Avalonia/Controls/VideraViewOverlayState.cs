using System.Numerics;
using Avalonia.Media;
using Videra.Core.Selection.Annotations;

namespace Videra.Avalonia.Controls;

internal sealed record VideraSelectionOutline(Guid ObjectId, bool IsPrimary);

internal sealed record VideraOverlayLabel(
    Guid AnnotationId,
    string Text,
    Color Color,
    Vector2 ScreenPosition,
    AnnotationAnchorDescriptor Anchor,
    Guid? ResolvedObjectId);

internal sealed class VideraViewOverlayState
{
    public static VideraViewOverlayState Empty { get; } = new(
        Array.Empty<VideraSelectionOutline>(),
        Array.Empty<VideraOverlayLabel>());

    public VideraViewOverlayState(
        IReadOnlyList<VideraSelectionOutline>? selectionOutlines,
        IReadOnlyList<VideraOverlayLabel>? labels)
    {
        SelectionOutlines = selectionOutlines?.ToArray() ?? Array.Empty<VideraSelectionOutline>();
        Labels = labels?.ToArray() ?? Array.Empty<VideraOverlayLabel>();
    }

    public IReadOnlyList<VideraSelectionOutline> SelectionOutlines { get; }

    public IReadOnlyList<VideraOverlayLabel> Labels { get; }
}
