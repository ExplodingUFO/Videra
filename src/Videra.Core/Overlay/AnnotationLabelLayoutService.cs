using System.Numerics;
using Videra.Core.Graphics;
using Videra.Core.Selection.Annotations;
using Videra.Core.Selection.Rendering;

namespace Videra.Core.Overlay;

public sealed class AnnotationLabelLayoutService
{
    public IReadOnlyList<AnnotationLabelProjection> ProjectVisibleLabels(
        VideraEngine engine,
        AnnotationOverlayRenderState overlayState,
        Vector2 viewportSize)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(overlayState);

        var projectedAnchors = engine.ProjectAnnotationAnchors(overlayState, viewportSize);
        return projectedAnchors
            .Where(projection => projection.Projection.IsVisible)
            .Select(projection => new AnnotationLabelProjection(
                projection.AnnotationId,
                projection.Projection.ScreenPosition,
                projection.Anchor,
                projection.Projection.ResolvedObjectId))
            .ToArray();
    }
}
