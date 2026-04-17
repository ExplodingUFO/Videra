using System.Numerics;
using Microsoft.Extensions.Logging;
using Videra.Core.Cameras;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Selection.Annotations;
using Videra.Core.Selection.Rendering;

namespace Videra.Core.Graphics;

internal sealed class RenderWorld
{
    private readonly List<Object3D> _sceneObjects = [];
    private readonly AnnotationAnchorProjector _annotationAnchorProjector = new();

    public IReadOnlyList<Object3D> SceneObjects => _sceneObjects;

    public SelectionOverlayRenderState SelectionOverlayState { get; private set; } = SelectionOverlayRenderState.Empty;

    public AnnotationOverlayRenderState AnnotationOverlayState { get; private set; } = AnnotationOverlayRenderState.Empty;

    public bool HasOverlayWireframe => SelectionOverlayState.HasOverlay || AnnotationOverlayState.HasOverlay;

    public void SetSelectionOverlayState(SelectionOverlayRenderState? state)
    {
        SelectionOverlayState = (state ?? SelectionOverlayRenderState.Empty).Snapshot();
    }

    public void SetAnnotationOverlayState(AnnotationOverlayRenderState? state)
    {
        AnnotationOverlayState = (state ?? AnnotationOverlayRenderState.Empty).Snapshot();
    }

    public IReadOnlyList<AnnotationOverlayProjection> ProjectAnnotationAnchors(
        AnnotationOverlayRenderState? state,
        OrbitCamera camera,
        Vector2 viewportSize)
    {
        var overlayState = (state ?? AnnotationOverlayState).Snapshot();
        if (overlayState.Anchors.Count == 0)
        {
            return Array.Empty<AnnotationOverlayProjection>();
        }

        var projections = new AnnotationOverlayProjection[overlayState.Anchors.Count];
        for (var i = 0; i < overlayState.Anchors.Count; i++)
        {
            var anchor = overlayState.Anchors[i];
            projections[i] = new AnnotationOverlayProjection(
                anchor.AnnotationId,
                anchor.Anchor,
                _annotationAnchorProjector.Project(anchor.Anchor, camera, viewportSize, _sceneObjects));
        }

        return projections;
    }

    public void AddObject(Object3D obj, IResourceFactory? resourceFactory, ILogger? logger = null)
    {
        _sceneObjects.Add(obj);

        if (resourceFactory != null)
        {
            obj.RecreateGraphicsResources(resourceFactory, logger);
            obj.InitializeWireframe(resourceFactory, logger);
        }
    }

    public void RemoveObject(Object3D obj)
    {
        _sceneObjects.Remove(obj);
        obj.Dispose();
    }

    public void ClearObjects()
    {
        foreach (var obj in _sceneObjects)
        {
            obj.Dispose();
        }

        _sceneObjects.Clear();
    }

    public void RestoreGraphicsResources(IResourceFactory resourceFactory, ILogger logger)
    {
        foreach (var obj in _sceneObjects)
        {
            obj.RecreateGraphicsResources(resourceFactory, logger);
        }
    }

    public void ReleaseGraphicsResources(bool preserveSceneObjects)
    {
        if (preserveSceneObjects)
        {
            foreach (var obj in _sceneObjects)
            {
                obj.ReleaseGpuResources();
            }

            return;
        }

        ClearObjects();
    }

    public void EnsureWireframeResources(IResourceFactory resourceFactory, ILogger logger)
    {
        foreach (var obj in _sceneObjects)
        {
            if (obj.LineIndexBuffer != null && obj.LineVertexBuffer != null && obj.LineIndexCount > 0)
            {
                continue;
            }

            obj.InitializeWireframe(resourceFactory, logger);
        }
    }
}
