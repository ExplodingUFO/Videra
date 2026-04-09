using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Videra.Core.Geometry;
using Videra.Core.Selection.Rendering;

namespace Videra.Avalonia.Controls;

public partial class VideraView
{
    private static readonly RgbaFloat DefaultSelectedLineColor = RgbaFloat.Black;
    private static readonly RgbaFloat DefaultHoverLineColor = RgbaFloat.Green;
    private static readonly RgbaFloat DefaultMarkerColor = RgbaFloat.Red;

    private void SynchronizeOverlayState()
    {
        PushOverlayRenderState();
        SynchronizeOverlayPresentation();
    }

    private void PushOverlayRenderState()
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

        Engine.SetSelectionOverlayState(selectionOverlay);
        Engine.SetAnnotationOverlayState(annotationOverlay);
    }

    private void SynchronizeOverlayPresentation()
    {
        _overlayState = _sessionBridge.CreateOverlayState(
            _selectionState,
            _annotations,
            CreateOverlayViewportSize());
        _overlayPresenter?.UpdateOverlayState(_overlayState);
    }

    private Vector2 CreateOverlayViewportSize()
    {
        var width = (float)Math.Max(0d, Bounds.Width);
        var height = (float)Math.Max(0d, Bounds.Height);
        if (width > 0f && height > 0f)
        {
            return new Vector2(width, height);
        }

        var snapshot = _renderSession.OrchestrationSnapshot;
        return snapshot.Inputs.Width > 0 && snapshot.Inputs.Height > 0
            ? new Vector2(snapshot.Inputs.Width, snapshot.Inputs.Height)
            : Vector2.Zero;
    }

    private void RenderOverlay(DrawingContext context)
    {
        if (_overlayState.SelectionOutlines.Count == 0 && _overlayState.Labels.Count == 0)
        {
            return;
        }

        VideraViewOverlayPresenter.RenderOverlay(context, _overlayState);
    }
}

internal sealed class VideraViewOverlayPresenter : Control
{
    private static readonly Typeface OverlayTypeface = new("Consolas");
    private static readonly IBrush BubbleBackground = new SolidColorBrush(Color.FromArgb(235, 24, 24, 24));
    private static readonly IBrush BubbleBorder = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

    private VideraViewOverlayState _overlayState = VideraViewOverlayState.Empty;

    public void UpdateOverlayState(VideraViewOverlayState overlayState)
    {
        _overlayState = overlayState ?? VideraViewOverlayState.Empty;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        RenderOverlay(context, _overlayState);
    }

    internal static void RenderOverlay(DrawingContext context, VideraViewOverlayState overlayState)
    {
        foreach (var outline in overlayState.SelectionOutlines)
        {
            DrawSelectionOutline(context, outline);
        }

        foreach (var label in overlayState.Labels)
        {
            DrawLabel(context, label);
        }
    }

    private static void DrawSelectionOutline(DrawingContext context, VideraSelectionOutline outline)
    {
        var strokeThickness = outline.IsPrimary ? 2d : 1d;
        context.DrawRectangle(
            brush: null,
            pen: new Pen(new SolidColorBrush(outline.Color), strokeThickness),
            rect: outline.ScreenBounds);
    }

    private static void DrawLabel(DrawingContext context, VideraOverlayLabel label)
    {
        var markerCenter = new Point(label.ScreenPosition.X, label.ScreenPosition.Y);
        var text = CreateText(label);
        var bubbleOrigin = new Point(markerCenter.X + 8d, markerCenter.Y - text.Height - 6d);
        var bubbleRect = new Rect(
            bubbleOrigin.X - 4d,
            bubbleOrigin.Y - 2d,
            text.Width + 8d,
            text.Height + 4d);

        context.DrawLine(new Pen(new SolidColorBrush(label.Color), 1d), markerCenter, new Point(bubbleRect.X, bubbleRect.Bottom));
        context.DrawEllipse(new SolidColorBrush(label.Color), null, markerCenter, 3d, 3d);
        context.DrawRectangle(BubbleBackground, new Pen(BubbleBorder, 1d), bubbleRect, 4d, 4d);
        context.DrawText(text, bubbleOrigin);
    }

    private static FormattedText CreateText(VideraOverlayLabel label)
    {
        return new FormattedText(
            label.Text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            OverlayTypeface,
            12d,
            new SolidColorBrush(label.Color));
    }
}
