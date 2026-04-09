using Videra.Core.Geometry;
using Videra.Core.Selection.Annotations;

namespace Videra.Core.Selection.Rendering;

public sealed record AnnotationOverlayAnchor(Guid AnnotationId, AnnotationAnchorDescriptor Anchor);

public sealed record AnnotationOverlayProjection(
    Guid AnnotationId,
    AnnotationAnchorDescriptor Anchor,
    AnnotationProjectionResult Projection);

public sealed class AnnotationOverlayRenderState
{
    public static AnnotationOverlayRenderState Empty { get; } = new();

    public AnnotationOverlayRenderState(
        IReadOnlyList<AnnotationOverlayAnchor>? anchors = null,
        RgbaFloat? markerColor = null,
        float markerWorldSize = 0.08f)
    {
        Anchors = anchors?.ToArray() ?? Array.Empty<AnnotationOverlayAnchor>();
        MarkerColor = markerColor ?? RgbaFloat.Red;
        MarkerWorldSize = Math.Max(0f, markerWorldSize);
    }

    public IReadOnlyList<AnnotationOverlayAnchor> Anchors { get; }

    public RgbaFloat MarkerColor { get; }

    public float MarkerWorldSize { get; }

    public bool HasOverlay => Anchors.Count > 0 && MarkerWorldSize > 0f;

    public AnnotationOverlayRenderState Snapshot()
    {
        return new(
            Anchors,
            MarkerColor,
            MarkerWorldSize);
    }
}
