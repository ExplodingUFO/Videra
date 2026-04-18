using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Videra.Core.Selection.Annotations;

namespace Videra.Avalonia.Controls;

internal sealed record VideraSelectionOutline(Guid ObjectId, Rect ScreenBounds, Color Color, bool IsPrimary);

internal sealed record VideraOverlayLabel(
    Guid AnnotationId,
    string Text,
    Color Color,
    Vector2 ScreenPosition,
    AnnotationAnchorDescriptor Anchor,
    Guid? ResolvedObjectId);

internal sealed record VideraOverlayMeasurement(
    Guid MeasurementId,
    Vector2 StartScreenPosition,
    Vector2 EndScreenPosition,
    string Text,
    Color Color);

internal sealed class VideraViewOverlayState
{
    public static VideraViewOverlayState Empty { get; } = new(
        Array.Empty<VideraSelectionOutline>(),
        Array.Empty<VideraOverlayLabel>(),
        Array.Empty<VideraOverlayMeasurement>());

    public VideraViewOverlayState(
        IReadOnlyList<VideraSelectionOutline>? selectionOutlines,
        IReadOnlyList<VideraOverlayLabel>? labels,
        IReadOnlyList<VideraOverlayMeasurement>? measurements)
    {
        SelectionOutlines = selectionOutlines?.ToArray() ?? Array.Empty<VideraSelectionOutline>();
        Labels = labels?.ToArray() ?? Array.Empty<VideraOverlayLabel>();
        Measurements = measurements?.ToArray() ?? Array.Empty<VideraOverlayMeasurement>();
    }

    public IReadOnlyList<VideraSelectionOutline> SelectionOutlines { get; }

    public IReadOnlyList<VideraOverlayLabel> Labels { get; }

    public IReadOnlyList<VideraOverlayMeasurement> Measurements { get; }
}
