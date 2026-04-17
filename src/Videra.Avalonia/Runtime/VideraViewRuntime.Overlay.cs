using System.Numerics;
using Avalonia;
using Videra.Core.Geometry;
using Videra.Core.Selection.Rendering;

namespace Videra.Avalonia.Runtime;

internal sealed partial class VideraViewRuntime
{
    private static readonly RgbaFloat DefaultSelectedLineColor = RgbaFloat.Black;
    private static readonly RgbaFloat DefaultHoverLineColor = RgbaFloat.Green;
    private static readonly RgbaFloat DefaultMarkerColor = RgbaFloat.Red;

    internal void SynchronizeOverlayState()
    {
        PushOverlayRenderState();
        SynchronizeOverlayPresentation();
    }

    internal void PushOverlayRenderState()
    {
        var selectionOverlay = new SelectionOverlayRenderState(
            selectedObjectIds: _selectionState.ObjectIds,
            hoverObjectId: null,
            selectedLineColor: DefaultSelectedLineColor,
            hoverLineColor: DefaultHoverLineColor);
        var annotationOverlay = new AnnotationOverlayRenderState(
            anchors: _annotations
                .Where(annotation => annotation.IsVisible)
                .Select(annotation => new AnnotationOverlayAnchor(annotation.Id, annotation.Anchor))
                .ToArray(),
            markerColor: DefaultMarkerColor,
            markerWorldSize: 0.08f);

        _engine.SetSelectionOverlayState(selectionOverlay);
        _engine.SetAnnotationOverlayState(annotationOverlay);
    }

    internal void SynchronizeOverlayPresentation()
    {
        _overlayState = _sessionBridge.CreateOverlayState(
            _selectionState,
            _annotations,
            CreateOverlayViewportSize());
        _owner.UpdateOverlayPresentationFromRuntime(_overlayState);
    }

    internal Vector2 GetInteractionViewportSize() => CreateOverlayViewportSize();

    internal Vector2 CreateOverlayViewportSize()
    {
        var width = (float)Math.Max(0d, _owner.Bounds.Width);
        var height = (float)Math.Max(0d, _owner.Bounds.Height);
        if (width > 0f && height > 0f)
        {
            return new Vector2(width, height);
        }

        var snapshot = _renderSession.OrchestrationSnapshot;
        return snapshot.Inputs.Width > 0 && snapshot.Inputs.Height > 0
            ? new Vector2(snapshot.Inputs.Width, snapshot.Inputs.Height)
            : Vector2.Zero;
    }

    internal void OnRenderSessionBackendReady(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        SynchronizeOverlayState();
        RefreshBackendDiagnostics(lastInitializationError: null);
        _owner.RaiseBackendReadyFromRuntime();
    }

    internal void OnRenderSessionFrameRequested()
    {
        SynchronizeOverlayPresentation();
        _owner.InvalidateVisualsFromRuntime();
    }
}
