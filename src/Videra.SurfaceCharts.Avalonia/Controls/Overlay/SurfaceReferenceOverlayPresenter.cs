using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal static class SurfaceReferenceOverlayPresenter
{
    private static readonly Typeface DefaultTypeface = new("Consolas");

    public static SurfaceReferenceOverlayState CreateState(
        IReadOnlyList<ReferenceLineData>? referenceLines,
        IReadOnlyList<ReferenceSpanData>? referenceSpans,
        IReadOnlyList<ShapeAnnotationData>? shapeAnnotations,
        SurfaceChartProjection? projection,
        SurfaceMetadata? metadata,
        SurfaceValueRange? valueRange)
    {
        if (projection is null || metadata is null || valueRange is null)
        {
            return SurfaceReferenceOverlayState.Empty;
        }

        var hasLines = referenceLines is { Count: > 0 };
        var hasSpans = referenceSpans is { Count: > 0 };
        var hasShapes = shapeAnnotations is { Count: > 0 };

        if (!hasLines && !hasSpans && !hasShapes)
        {
            return SurfaceReferenceOverlayState.Empty;
        }

        var vr = valueRange.Value;
        var xMin = (float)metadata.HorizontalAxis.Minimum;
        var xMax = (float)metadata.HorizontalAxis.Maximum;
        var yMin = (float)vr.Minimum;
        var yMax = (float)vr.Maximum;
        var zMin = (float)metadata.VerticalAxis.Minimum;
        var zMax = (float)metadata.VerticalAxis.Maximum;

        List<SurfaceReferenceLineState> lineStates = [];
        if (hasLines)
        {
            foreach (var line in referenceLines!)
            {
                var (start, end) = GetLineEndpoints(line.Axis, line.Value, xMin, xMax, yMin, yMax, zMin, zMax);
                var startScreen = projection.Project(start);
                var endScreen = projection.Project(end);

                Point? labelPos = null;
                if (!string.IsNullOrWhiteSpace(line.Label))
                {
                    labelPos = new Point(
                        (startScreen.X + endScreen.X) * 0.5d + 4,
                        (startScreen.Y + endScreen.Y) * 0.5d - 14);
                }

                lineStates.Add(new SurfaceReferenceLineState(
                    startScreen, endScreen, line.Color, line.LineWidth, line.Label, labelPos));
            }
        }

        List<SurfaceReferenceSpanState> spanStates = [];
        if (hasSpans)
        {
            foreach (var span in referenceSpans!)
            {
                var corners = GetSpanCorners(span.Axis, span.Start, span.End, xMin, xMax, yMin, yMax, zMin, zMax);
                var tl = projection.Project(corners.topLeft);
                var tr = projection.Project(corners.topRight);
                var br = projection.Project(corners.bottomRight);
                var bl = projection.Project(corners.bottomLeft);

                Point? labelPos = null;
                if (!string.IsNullOrWhiteSpace(span.Label))
                {
                    labelPos = new Point(
                        (tl.X + tr.X + br.X + bl.X) * 0.25d,
                        (tl.Y + tr.Y + br.Y + bl.Y) * 0.25d - 6);
                }

                spanStates.Add(new SurfaceReferenceSpanState(
                    tl, tr, br, bl, span.Color, span.BorderColor, span.BorderWidth, span.Label, labelPos));
            }
        }

        List<SurfaceShapeAnnotationState> shapeStates = [];
        if (hasShapes)
        {
            foreach (var shape in shapeAnnotations!)
            {
                var centerScreen = projection.Project(shape.Center);

                // Compute screen dimensions by projecting offset points
                var halfW = (float)(shape.Width * 0.5d);
                var halfH = (float)(shape.Height * 0.5d);
                var rightScreen = projection.Project(shape.Center + new Vector3(halfW, 0, 0));
                var topScreen = projection.Project(shape.Center + new Vector3(0, halfH, 0));
                var screenWidth = Math.Abs(rightScreen.X - centerScreen.X) * 2d;
                var screenHeight = Math.Abs(topScreen.Y - centerScreen.Y) * 2d;

                shapeStates.Add(new SurfaceShapeAnnotationState(
                    shape.Kind,
                    centerScreen,
                    screenWidth,
                    screenHeight,
                    shape.RotationDegrees,
                    shape.FillColor,
                    shape.BorderColor,
                    shape.BorderWidth,
                    shape.Label));
            }
        }

        return new SurfaceReferenceOverlayState(lineStates, spanStates, shapeStates);
    }

    public static void Render(DrawingContext context, SurfaceReferenceOverlayState state)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(state);

        // Render spans first (background layer)
        foreach (var span in state.Spans)
        {
            RenderSpan(context, span);
        }

        // Then lines
        foreach (var line in state.Lines)
        {
            RenderLine(context, line);
        }

        // Then shapes on top
        foreach (var shape in state.Shapes)
        {
            RenderShape(context, shape);
        }
    }

    private static void RenderLine(DrawingContext context, SurfaceReferenceLineState line)
    {
        var color = Color.FromUInt32(line.Color);
        var pen = new Pen(new SolidColorBrush(color), line.LineWidth);
        context.DrawLine(pen, line.StartScreen, line.EndScreen);

        if (!string.IsNullOrWhiteSpace(line.Label) && line.LabelPosition.HasValue)
        {
            var formattedText = CreateText(line.Label, 11d, null, color);
            context.DrawText(formattedText, line.LabelPosition.Value);
        }
    }

    private static void RenderSpan(DrawingContext context, SurfaceReferenceSpanState span)
    {
        var fillColor = Color.FromUInt32(span.Color);
        var brush = new SolidColorBrush(fillColor);

        var geometry = new StreamGeometry();
        using (var sgCtx = geometry.Open())
        {
            sgCtx.BeginFigure(span.TopLeft, true);
            sgCtx.LineTo(span.TopRight);
            sgCtx.LineTo(span.BottomRight);
            sgCtx.LineTo(span.BottomLeft);
            sgCtx.EndFigure(true);
        }

        Pen? pen = null;
        if (span.BorderColor.HasValue)
        {
            var borderColor = Color.FromUInt32(span.BorderColor.Value);
            pen = new Pen(new SolidColorBrush(borderColor), span.BorderWidth);
        }

        context.DrawGeometry(brush, pen, geometry);

        if (!string.IsNullOrWhiteSpace(span.Label) && span.LabelPosition.HasValue)
        {
            var labelColor = Color.FromUInt32(span.BorderColor ?? 0xFFFFFFFFu);
            var formattedText = CreateText(span.Label, 11d, null, labelColor);
            context.DrawText(formattedText, span.LabelPosition.Value);
        }
    }

    private static void RenderShape(DrawingContext context, SurfaceShapeAnnotationState shape)
    {
        var fillColor = Color.FromUInt32(shape.FillColor);
        var brush = new SolidColorBrush(fillColor);

        Pen? pen = null;
        if (shape.BorderColor.HasValue)
        {
            var borderColor = Color.FromUInt32(shape.BorderColor.Value);
            pen = new Pen(new SolidColorBrush(borderColor), shape.BorderWidth);
        }

        var halfW = shape.ScreenWidth * 0.5d;
        var halfH = shape.ScreenHeight * 0.5d;

        var rect = new Rect(
            shape.CenterScreen.X - halfW,
            shape.CenterScreen.Y - halfH,
            shape.ScreenWidth,
            shape.ScreenHeight);

        if (shape.Kind == ShapeKind.Ellipse)
        {
            context.DrawGeometry(brush, pen, new EllipseGeometry(rect));
        }
        else if (Math.Abs(shape.RotationDegrees) > double.Epsilon)
        {
            var rad = shape.RotationDegrees * Math.PI / 180d;
            var cos = Math.Cos(rad);
            var sin = Math.Sin(rad);
            var cx = shape.CenterScreen.X;
            var cy = shape.CenterScreen.Y;

            var corners = new[]
            {
                RotatePoint(-halfW, -halfH, cos, sin, cx, cy),
                RotatePoint(halfW, -halfH, cos, sin, cx, cy),
                RotatePoint(halfW, halfH, cos, sin, cx, cy),
                RotatePoint(-halfW, halfH, cos, sin, cx, cy),
            };

            var sg = new StreamGeometry();
            using (var sgCtx = sg.Open())
            {
                sgCtx.BeginFigure(corners[0], true);
                sgCtx.LineTo(corners[1]);
                sgCtx.LineTo(corners[2]);
                sgCtx.LineTo(corners[3]);
                sgCtx.EndFigure(true);
            }

            context.DrawGeometry(brush, pen, sg);
        }
        else
        {
            context.DrawRectangle(brush, pen, rect);
        }

        if (!string.IsNullOrWhiteSpace(shape.Label))
        {
            var labelColor = Color.FromUInt32(shape.BorderColor ?? 0xFFFFFFFFu);
            var formattedText = CreateText(shape.Label, 11d, null, labelColor);
            var textWidth = formattedText.Width;
            var textHeight = formattedText.Height;
            context.DrawText(formattedText, new Point(
                shape.CenterScreen.X - textWidth * 0.5d,
                shape.CenterScreen.Y - textHeight * 0.5d));
        }
    }

    private static (Vector3 start, Vector3 end) GetLineEndpoints(
        ReferenceAxis axis, double value,
        float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        return axis switch
        {
            ReferenceAxis.X => (new Vector3((float)value, yMin, zMin), new Vector3((float)value, yMax, zMax)),
            ReferenceAxis.Y => (new Vector3(xMin, (float)value, zMin), new Vector3(xMax, (float)value, zMax)),
            ReferenceAxis.Z => (new Vector3(xMin, yMin, (float)value), new Vector3(xMax, yMax, (float)value)),
            _ => (Vector3.Zero, Vector3.Zero),
        };
    }

    private static (Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft) GetSpanCorners(
        ReferenceAxis axis, double start, double end,
        float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        var s = (float)start;
        var e = (float)end;

        return axis switch
        {
            ReferenceAxis.X => (
                new Vector3(s, yMax, zMin),
                new Vector3(e, yMax, zMin),
                new Vector3(e, yMin, zMax),
                new Vector3(s, yMin, zMax)),
            ReferenceAxis.Y => (
                new Vector3(xMin, e, zMin),
                new Vector3(xMax, e, zMin),
                new Vector3(xMax, s, zMax),
                new Vector3(xMin, s, zMax)),
            ReferenceAxis.Z => (
                new Vector3(xMin, yMax, s),
                new Vector3(xMax, yMax, e),
                new Vector3(xMax, yMin, e),
                new Vector3(xMin, yMin, s)),
            _ => (Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero),
        };
    }

    private static Point RotatePoint(double x, double y, double cos, double sin, double cx, double cy)
    {
        return new Point(cx + x * cos - y * sin, cy + x * sin + y * cos);
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
