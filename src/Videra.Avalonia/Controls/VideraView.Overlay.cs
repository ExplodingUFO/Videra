using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Videra.Avalonia.Controls;

public partial class VideraView
{
    private void SynchronizeOverlayState() => _runtime.SynchronizeOverlayState();

    private void PushOverlayRenderState() => _runtime.PushOverlayRenderState();

    private void SynchronizeOverlayPresentation() => _runtime.SynchronizeOverlayPresentation();

    private void RenderOverlay(DrawingContext context)
    {
        var overlayState = _runtime.OverlayState;
        if (overlayState.SelectionOutlines.Count == 0 &&
            overlayState.Labels.Count == 0 &&
            overlayState.Measurements.Count == 0)
        {
            return;
        }

        VideraViewOverlayPresenter.RenderOverlay(context, overlayState);
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

        foreach (var measurement in overlayState.Measurements)
        {
            DrawMeasurement(context, measurement);
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

    private static void DrawMeasurement(DrawingContext context, VideraOverlayMeasurement measurement)
    {
        var start = new Point(measurement.StartScreenPosition.X, measurement.StartScreenPosition.Y);
        var end = new Point(measurement.EndScreenPosition.X, measurement.EndScreenPosition.Y);
        var stroke = new Pen(new SolidColorBrush(measurement.Color), 2d);
        context.DrawLine(stroke, start, end);
        context.DrawEllipse(new SolidColorBrush(measurement.Color), null, start, 3d, 3d);
        context.DrawEllipse(new SolidColorBrush(measurement.Color), null, end, 3d, 3d);

        var markerCenter = new Point((start.X + end.X) * 0.5d, (start.Y + end.Y) * 0.5d);
        var label = new VideraOverlayLabel(
            measurement.MeasurementId,
            measurement.Text,
            measurement.Color,
            new System.Numerics.Vector2((float)markerCenter.X, (float)markerCenter.Y),
            default,
            null);
        DrawLabel(context, label);
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
