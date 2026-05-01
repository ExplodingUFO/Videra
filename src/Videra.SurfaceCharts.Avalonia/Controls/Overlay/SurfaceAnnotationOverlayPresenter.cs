using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal static class SurfaceAnnotationOverlayPresenter
{
    private static readonly Typeface DefaultTypeface = new("Consolas");

    public static SurfaceAnnotationOverlayState CreateState(
        IReadOnlyList<TextAnnotationData>? textAnnotations,
        IReadOnlyList<ArrowAnnotationData>? arrowAnnotations,
        SurfaceChartProjection? projection)
    {
        if (projection is null)
        {
            return SurfaceAnnotationOverlayState.Empty;
        }

        var hasText = textAnnotations is { Count: > 0 };
        var hasArrows = arrowAnnotations is { Count: > 0 };

        if (!hasText && !hasArrows)
        {
            return SurfaceAnnotationOverlayState.Empty;
        }

        List<SurfaceTextAnnotationState> textStates = [];
        if (hasText)
        {
            foreach (var annotation in textAnnotations!)
            {
                var screenPos = projection.Project(annotation.Position);
                textStates.Add(new SurfaceTextAnnotationState(
                    annotation.Text,
                    screenPos,
                    annotation.Color,
                    annotation.FontSize,
                    annotation.FontFamily));
            }
        }

        List<SurfaceArrowAnnotationState> arrowStates = [];
        if (hasArrows)
        {
            foreach (var annotation in arrowAnnotations!)
            {
                var startScreen = projection.Project(annotation.Start);
                var endScreen = projection.Project(annotation.End);
                arrowStates.Add(new SurfaceArrowAnnotationState(
                    startScreen,
                    endScreen,
                    annotation.Color,
                    annotation.LineWidth,
                    annotation.HeadLength,
                    annotation.HeadWidth,
                    annotation.Label));
            }
        }

        return new SurfaceAnnotationOverlayState(textStates, arrowStates);
    }

    public static void Render(DrawingContext context, SurfaceAnnotationOverlayState state)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(state);

        foreach (var arrow in state.ArrowAnnotations)
        {
            RenderArrow(context, arrow);
        }

        foreach (var text in state.TextAnnotations)
        {
            RenderText(context, text);
        }
    }

    private static void RenderArrow(DrawingContext context, SurfaceArrowAnnotationState arrow)
    {
        var color = Color.FromUInt32(arrow.Color);
        var brush = new SolidColorBrush(color);
        var pen = new Pen(brush, arrow.LineWidth);

        context.DrawLine(pen, arrow.StartScreen, arrow.EndScreen);

        // Draw arrowhead
        var dx = arrow.EndScreen.X - arrow.StartScreen.X;
        var dy = arrow.EndScreen.Y - arrow.StartScreen.Y;
        var length = Math.Sqrt((dx * dx) + (dy * dy));

        if (length > double.Epsilon)
        {
            var ux = dx / length;
            var uy = dy / length;
            var px = -uy; // perpendicular
            var py = ux;

            var headLen = arrow.HeadLength;
            var headWid = arrow.HeadWidth * 0.5d;

            var tipX = arrow.EndScreen.X;
            var tipY = arrow.EndScreen.Y;
            var baseX = tipX - (ux * headLen);
            var baseY = tipY - (uy * headLen);
            var leftX = baseX + (px * headWid);
            var leftY = baseY + (py * headWid);
            var rightX = baseX - (px * headWid);
            var rightY = baseY - (py * headWid);

            var geometry = new StreamGeometry();
            using (var sgCtx = geometry.Open())
            {
                sgCtx.BeginFigure(new Point(tipX, tipY), true);
                sgCtx.LineTo(new Point(leftX, leftY));
                sgCtx.LineTo(new Point(rightX, rightY));
                sgCtx.EndFigure(true);
            }

            context.DrawGeometry(brush, null, geometry);
        }

        if (!string.IsNullOrWhiteSpace(arrow.Label))
        {
            var midX = (arrow.StartScreen.X + arrow.EndScreen.X) * 0.5d;
            var midY = (arrow.StartScreen.Y + arrow.EndScreen.Y) * 0.5d;
            var textPos = new Point(midX + 4, midY - 16);
            context.DrawText(CreateText(arrow.Label, 11d, null, color), textPos);
        }
    }

    private static void RenderText(DrawingContext context, SurfaceTextAnnotationState text)
    {
        var color = Color.FromUInt32(text.Color);
        var formattedText = CreateText(text.Text, text.FontSize, text.FontFamily, color);
        context.DrawText(formattedText, text.ScreenPosition);
    }

    private static FormattedText CreateText(string text, double fontSize, string? fontFamily, Color color)
    {
        var typeface = string.IsNullOrWhiteSpace(fontFamily)
            ? DefaultTypeface
            : new Typeface(fontFamily);

        return new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            new SolidColorBrush(color));
    }
}
