using System.Numerics;
using Avalonia;
using Videra.Core.Geometry;
using Videra.Core.Selection.Annotations;
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
                .Concat(_measurements
                    .Where(measurement => measurement.IsVisible)
                    .SelectMany(measurement => new[]
                    {
                        new AnnotationOverlayAnchor(CreateMeasurementAnchorId(measurement.Id, isStart: true), AnnotationAnchorDescriptor.ForWorldPoint(measurement.Start.WorldPoint)),
                        new AnnotationOverlayAnchor(CreateMeasurementAnchorId(measurement.Id, isStart: false), AnnotationAnchorDescriptor.ForWorldPoint(measurement.End.WorldPoint))
                    }))
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
            _measurements,
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
        RuntimeTraceLog.Write("VideraViewRuntime.OnRenderSessionBackendReady");
        OnSceneBackendReady();
        SynchronizeOverlayState();
        RefreshBackendDiagnostics(lastInitializationError: null);
        _owner.RaiseBackendReadyFromRuntime();
    }

    internal void OnRenderSessionFrameRequested()
    {
        SynchronizeOverlayPresentation();
        RefreshBackendDiagnostics(_backendDiagnostics.LastInitializationError);
        _owner.InvalidateVisualsFromRuntime();
    }

    private static Guid CreateMeasurementAnchorId(Guid measurementId, bool isStart)
    {
        var bytes = measurementId.ToByteArray();
        bytes[15] ^= isStart ? (byte)0x41 : (byte)0x82;
        return new Guid(bytes);
    }
}
