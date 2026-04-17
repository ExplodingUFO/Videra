using System.Numerics;
using Videra.Core.Cameras;
using Videra.Core.Graphics;
using Videra.Core.Selection.Rendering;

namespace Videra.Core.Overlay;

public sealed class OverlayProjectionService
{
    private readonly SelectionOutlineProjector _selectionOutlineProjector = new();
    private readonly AnnotationLabelLayoutService _annotationLabelLayoutService = new();

    public IReadOnlyList<SelectionOutlineProjection> ProjectSelectionOutlines(
        IReadOnlyList<Guid> selectionObjectIds,
        Guid? primaryObjectId,
        IReadOnlyList<Object3D> sceneObjects,
        OrbitCamera camera,
        Vector2 viewportSize)
    {
        return _selectionOutlineProjector.Project(selectionObjectIds, primaryObjectId, sceneObjects, camera, viewportSize);
    }

    public IReadOnlyList<AnnotationLabelProjection> ProjectAnnotationLabels(
        VideraEngine engine,
        AnnotationOverlayRenderState overlayState,
        Vector2 viewportSize)
    {
        return _annotationLabelLayoutService.ProjectVisibleLabels(engine, overlayState, viewportSize);
    }
}
