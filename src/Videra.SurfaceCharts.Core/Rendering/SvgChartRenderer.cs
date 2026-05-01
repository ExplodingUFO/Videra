using System.Globalization;
using System.Numerics;
using System.Text;

namespace Videra.SurfaceCharts.Core.Rendering;

/// <summary>
/// Renders 2D chart render scenes to SVG (Scalable Vector Graphics) format.
/// Uses orthographic projection: horizontal axis maps to X, value axis maps to Y.
/// </summary>
public static class SvgChartRenderer
{
    private const float DefaultMarginLeft = 60f;
    private const float DefaultMarginRight = 30f;
    private const float DefaultMarginTop = 30f;
    private const float DefaultMarginBottom = 50f;

    /// <summary>
    /// Renders a scatter render scene to SVG.
    /// </summary>
    public static string RenderScatter(ScatterRenderScene scene, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var (xMin, xMax, zMin, zMax) = GetScatterBounds(scene);
        var sb = new StringBuilder();
        AppendSvgHeader(sb, width, height);

        var plotWidth = width - DefaultMarginLeft - DefaultMarginRight;
        var plotHeight = height - DefaultMarginTop - DefaultMarginBottom;

        foreach (var series in scene.Series)
        {
            sb.AppendLine($"  <g class=\"series\" data-label=\"{Escape(series.Label ?? "series")}\">");
            foreach (var point in series.Points)
            {
                var cx = DefaultMarginLeft + (point.Position.X - xMin) / (xMax - xMin) * plotWidth;
                var cy = DefaultMarginTop + plotHeight - (point.Position.Z - zMin) / (zMax - zMin) * plotHeight;
                var r = Math.Max(2f, point.Size);
                sb.AppendLine(FormattableString.Invariant(
                    $"    <circle cx=\"{cx:F2}\" cy=\"{cy:F2}\" r=\"{r:F1}\" fill=\"{ColorToHex(point.Color)}\" />"));
            }

            if (series.ConnectPoints && series.Points.Count > 1)
            {
                sb.Append("    <polyline points=\"");
                for (var i = 0; i < series.Points.Count; i++)
                {
                    var point = series.Points[i];
                    var px = DefaultMarginLeft + (point.Position.X - xMin) / (xMax - xMin) * plotWidth;
                    var py = DefaultMarginTop + plotHeight - (point.Position.Z - zMin) / (zMax - zMin) * plotHeight;
                    if (i > 0) sb.Append(' ');
                    sb.Append(FormattableString.Invariant($"{px:F2},{py:F2}"));
                }

                sb.AppendLine(FormattableString.Invariant(
                    $"\" fill=\"none\" stroke=\"{ColorToHex(series.Color)}\" stroke-width=\"1.5\" />"));
            }

            sb.AppendLine("  </g>");
        }

        AppendSvgFooter(sb);
        return sb.ToString();
    }

    /// <summary>
    /// Renders a line render scene to SVG.
    /// </summary>
    public static string RenderLine(LineRenderScene scene, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var (xMin, xMax, yMin, yMax) = GetLineBounds(scene);
        var sb = new StringBuilder();
        AppendSvgHeader(sb, width, height);

        var plotWidth = width - DefaultMarginLeft - DefaultMarginRight;
        var plotHeight = height - DefaultMarginTop - DefaultMarginBottom;

        // Group segments by color for efficient rendering
        var groups = scene.Segments.GroupBy(s => s.Color);
        foreach (var group in groups)
        {
            sb.AppendLine(FormattableString.Invariant($"  <g stroke=\"{ColorToHex(group.Key)}\" stroke-width=\"{group.First().Width:F1}\" fill=\"none\">"));
            foreach (var seg in group)
            {
                var x1 = DefaultMarginLeft + (seg.Start.X - xMin) / (xMax - xMin) * plotWidth;
                var y1 = DefaultMarginTop + plotHeight - (seg.Start.Y - yMin) / (yMax - yMin) * plotHeight;
                var x2 = DefaultMarginLeft + (seg.End.X - xMin) / (xMax - xMin) * plotWidth;
                var y2 = DefaultMarginTop + plotHeight - (seg.End.Y - yMin) / (yMax - yMin) * plotHeight;
                sb.AppendLine(FormattableString.Invariant(
                    $"    <line x1=\"{x1:F2}\" y1=\"{y1:F2}\" x2=\"{x2:F2}\" y2=\"{y2:F2}\" />"));
            }

            sb.AppendLine("  </g>");
        }

        // Render markers
        foreach (var marker in scene.Markers)
        {
            var cx = DefaultMarginLeft + (marker.Position.X - xMin) / (xMax - xMin) * plotWidth;
            var cy = DefaultMarginTop + plotHeight - (marker.Position.Y - yMin) / (yMax - yMin) * plotHeight;
            var r = Math.Max(2f, marker.Size / 2f);
            sb.AppendLine(FormattableString.Invariant(
                $"  <circle cx=\"{cx:F2}\" cy=\"{cy:F2}\" r=\"{r:F1}\" fill=\"{ColorToHex(marker.Color)}\" />"));
        }

        AppendSvgFooter(sb);
        return sb.ToString();
    }

    /// <summary>
    /// Renders a bar render scene to SVG.
    /// </summary>
    public static string RenderBar(BarRenderScene scene, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var sb = new StringBuilder();
        AppendSvgHeader(sb, width, height);

        var plotWidth = width - DefaultMarginLeft - DefaultMarginRight;
        var plotHeight = height - DefaultMarginTop - DefaultMarginBottom;

        // Find value range
        var maxValue = 0f;
        foreach (var bar in scene.Bars)
        {
            var top = bar.Position.Y + bar.Size.Y / 2f;
            if (top > maxValue) maxValue = top;
        }

        if (maxValue <= 0f) maxValue = 1f;

        // Find horizontal range
        var xMin = float.MaxValue;
        var xMax = float.MinValue;
        foreach (var bar in scene.Bars)
        {
            var left = bar.Position.X - bar.Size.X / 2f;
            var right = bar.Position.X + bar.Size.X / 2f;
            if (left < xMin) xMin = left;
            if (right > xMax) xMax = right;
        }

        var xRange = xMax - xMin;
        if (xRange <= 0f) xRange = 1f;

        foreach (var bar in scene.Bars)
        {
            var barLeft = bar.Position.X - bar.Size.X / 2f;
            var barTop = bar.Position.Y + bar.Size.Y / 2f;

            var svgX = DefaultMarginLeft + (barLeft - xMin) / xRange * plotWidth;
            var svgY = DefaultMarginTop + plotHeight - barTop / maxValue * plotHeight;
            var svgW = bar.Size.X / xRange * plotWidth;
            var svgH = bar.Size.Y / maxValue * plotHeight;

            sb.AppendLine(FormattableString.Invariant(
                $"  <rect x=\"{svgX:F2}\" y=\"{svgY:F2}\" width=\"{svgW:F2}\" height=\"{svgH:F2}\" fill=\"{ColorToHex(bar.Color)}\" />"));
        }

        AppendSvgFooter(sb);
        return sb.ToString();
    }

    /// <summary>
    /// Renders a pie render scene to SVG.
    /// </summary>
    public static string RenderPie(PieRenderScene scene, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var sb = new StringBuilder();
        AppendSvgHeader(sb, width, height);

        var cx = width / 2f;
        var cy = height / 2f;
        var outerRadius = Math.Min(width, height) / 2f - 20f;
        var innerRadius = (float)scene.HoleRatio * outerRadius;

        foreach (var slice in scene.Slices)
        {
            var startRad = slice.StartAngle * MathF.PI / 180f;
            var endRad = (slice.StartAngle + slice.SweepAngle) * MathF.PI / 180f;
            var largeArc = slice.SweepAngle > 180f ? 1 : 0;

            var explodeX = cx + MathF.Cos(startRad + (endRad - startRad) / 2f) * slice.ExplodeDistance * outerRadius;
            var explodeY = cy + MathF.Sin(startRad + (endRad - startRad) / 2f) * slice.ExplodeDistance * outerRadius;

            // Outer arc
            var x1 = explodeX + outerRadius * MathF.Cos(startRad);
            var y1 = explodeY + outerRadius * MathF.Sin(startRad);
            var x2 = explodeX + outerRadius * MathF.Cos(endRad);
            var y2 = explodeY + outerRadius * MathF.Sin(endRad);

            if (innerRadius > 0f)
            {
                // Donut: outer arc + inner arc
                var ix1 = explodeX + innerRadius * MathF.Cos(endRad);
                var iy1 = explodeY + innerRadius * MathF.Sin(endRad);
                var ix2 = explodeX + innerRadius * MathF.Cos(startRad);
                var iy2 = explodeY + innerRadius * MathF.Sin(startRad);

                sb.Append("  <path d=\"M ");
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2}", x1);
                sb.Append(' ');
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2}", y1);
                sb.Append(' ');
                sb.AppendFormat(CultureInfo.InvariantCulture, "A {0:F2} {0:F2} 0 {1} 1 ", outerRadius, largeArc);
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2} {1:F2}", x2, y2);
                sb.Append(' ');
                sb.AppendFormat(CultureInfo.InvariantCulture, "L {0:F2} {1:F2} ", ix1, iy1);
                sb.AppendFormat(CultureInfo.InvariantCulture, "A {0:F2} {0:F2} 0 {1} 0 ", innerRadius, largeArc);
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2} {1:F2}", ix2, iy2);
                sb.Append(" Z\" fill=\"");
                sb.Append(ColorToHex(slice.Color));
                sb.AppendLine("\" />");
            }
            else
            {
                // Full pie
                sb.Append("  <path d=\"M ");
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2} {1:F2}", explodeX, explodeY);
                sb.Append(' ');
                sb.AppendFormat(CultureInfo.InvariantCulture, "L {0:F2} {1:F2}", x1, y1);
                sb.Append(' ');
                sb.AppendFormat(CultureInfo.InvariantCulture, "A {0:F2} {0:F2} 0 {1} 1 ", outerRadius, largeArc);
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2} {1:F2}", x2, y2);
                sb.Append(" Z\" fill=\"");
                sb.Append(ColorToHex(slice.Color));
                sb.AppendLine("\" />");
            }
        }

        AppendSvgFooter(sb);
        return sb.ToString();
    }

    /// <summary>
    /// Renders a histogram render scene to SVG.
    /// </summary>
    public static string RenderHistogram(HistogramRenderScene scene, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var sb = new StringBuilder();
        AppendSvgHeader(sb, width, height);

        var plotWidth = width - DefaultMarginLeft - DefaultMarginRight;
        var plotHeight = height - DefaultMarginTop - DefaultMarginBottom;

        var xMin = float.MaxValue;
        var xMax = float.MinValue;
        var yMax = 0f;
        foreach (var bin in scene.Bins)
        {
            var left = bin.Position.X - bin.Size.X / 2f;
            var right = bin.Position.X + bin.Size.X / 2f;
            if (left < xMin) xMin = left;
            if (right > xMax) xMax = right;
            var top = bin.Position.Y + bin.Size.Y / 2f;
            if (top > yMax) yMax = top;
        }

        var xRange = xMax - xMin;
        if (xRange <= 0f) xRange = 1f;
        if (yMax <= 0f) yMax = 1f;

        foreach (var bin in scene.Bins)
        {
            var barLeft = bin.Position.X - bin.Size.X / 2f;
            var barTop = bin.Position.Y + bin.Size.Y / 2f;

            var svgX = DefaultMarginLeft + (barLeft - xMin) / xRange * plotWidth;
            var svgY = DefaultMarginTop + plotHeight - barTop / yMax * plotHeight;
            var svgW = bin.Size.X / xRange * plotWidth;
            var svgH = bin.Size.Y / yMax * plotHeight;

            sb.AppendLine(FormattableString.Invariant(
                $"  <rect x=\"{svgX:F2}\" y=\"{svgY:F2}\" width=\"{svgW:F2}\" height=\"{svgH:F2}\" fill=\"{ColorToHex(bin.Color)}\" />"));
        }

        AppendSvgFooter(sb);
        return sb.ToString();
    }

    /// <summary>
    /// Renders a box plot render scene to SVG.
    /// </summary>
    public static string RenderBoxPlot(BoxPlotRenderScene scene, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var sb = new StringBuilder();
        AppendSvgHeader(sb, width, height);

        var plotWidth = width - DefaultMarginLeft - DefaultMarginRight;
        var plotHeight = height - DefaultMarginTop - DefaultMarginBottom;

        var categoryCount = scene.CategoryCount;
        if (categoryCount == 0 || scene.Boxes.Count == 0)
        {
            AppendSvgFooter(sb);
            return sb.ToString();
        }

        var categoryWidth = plotWidth / categoryCount;
        var boxWidth = categoryWidth * 0.6f;

        // Find value range from whiskers (min/max) and outliers
        var yMin = float.MaxValue;
        var yMax = float.MinValue;
        foreach (var whisker in scene.Whiskers)
        {
            if (whisker.Start.Y < yMin) yMin = whisker.Start.Y;
            if (whisker.End.Y < yMin) yMin = whisker.End.Y;
            if (whisker.Start.Y > yMax) yMax = whisker.Start.Y;
            if (whisker.End.Y > yMax) yMax = whisker.End.Y;
        }

        foreach (var outlier in scene.Outliers)
        {
            if (outlier.Position.Y < yMin) yMin = outlier.Position.Y;
            if (outlier.Position.Y > yMax) yMax = outlier.Position.Y;
        }

        var yRange = yMax - yMin;
        if (yRange <= 0f) yRange = 1f;
        var yPad = yRange * 0.05f;

        Func<float, float> mapY = val => DefaultMarginTop + plotHeight - (val - yMin + yPad) / (yRange + 2 * yPad) * plotHeight;

        // Draw IQR boxes
        foreach (var box in scene.Boxes)
        {
            var categoryIndex = (int)Math.Round(box.Position.X);
            var centerX = DefaultMarginLeft + (categoryIndex + 0.5f) * categoryWidth;
            var halfBox = boxWidth / 2f;
            var boxTop = mapY(box.Position.Y + box.Size.Y);
            var boxBottom = mapY(box.Position.Y);
            var boxHeight = boxBottom - boxTop;

            sb.AppendLine(FormattableString.Invariant(
                $"  <rect x=\"{centerX - halfBox:F2}\" y=\"{boxTop:F2}\" width=\"{boxWidth:F2}\" height=\"{boxHeight:F2}\" fill=\"{ColorToHex(box.Color)}\" stroke=\"#333\" stroke-width=\"1\" />"));
        }

        // Draw whiskers (min-to-Q1, Q3-to-max, and median lines)
        foreach (var whisker in scene.Whiskers)
        {
            var categoryIndex = (int)Math.Round(whisker.Start.X);
            var centerX = DefaultMarginLeft + (categoryIndex + 0.5f) * categoryWidth;

            // Median lines span horizontally; whisker lines are vertical
            var isMedian = Math.Abs(whisker.Start.X - whisker.End.X) > 0.01f;
            if (isMedian)
            {
                var halfBox = boxWidth / 2f;
                sb.AppendLine(FormattableString.Invariant(
                    $"  <line x1=\"{centerX - halfBox:F2}\" y1=\"{mapY(whisker.Start.Y):F2}\" x2=\"{centerX + halfBox:F2}\" y2=\"{mapY(whisker.End.Y):F2}\" stroke=\"#000\" stroke-width=\"2\" />"));
            }
            else
            {
                sb.AppendLine(FormattableString.Invariant(
                    $"  <line x1=\"{centerX:F2}\" y1=\"{mapY(whisker.Start.Y):F2}\" x2=\"{centerX:F2}\" y2=\"{mapY(whisker.End.Y):F2}\" stroke=\"#666\" stroke-width=\"1\" />"));
            }
        }

        // Draw outliers
        foreach (var outlier in scene.Outliers)
        {
            var categoryIndex = (int)Math.Round(outlier.Position.X);
            var centerX = DefaultMarginLeft + (categoryIndex + 0.5f) * categoryWidth;
            sb.AppendLine(FormattableString.Invariant(
                $"  <circle cx=\"{centerX:F2}\" cy=\"{mapY(outlier.Position.Y):F2}\" r=\"3\" fill=\"none\" stroke=\"#c00\" stroke-width=\"1\" />"));
        }

        AppendSvgFooter(sb);
        return sb.ToString();
    }

    /// <summary>
    /// Renders an OHLC render scene to SVG.
    /// </summary>
    public static string RenderOHLC(OHLCRenderScene scene, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var sb = new StringBuilder();
        AppendSvgHeader(sb, width, height);

        var plotWidth = width - DefaultMarginLeft - DefaultMarginRight;
        var plotHeight = height - DefaultMarginTop - DefaultMarginBottom;

        if (scene.Bars.Count == 0)
        {
            AppendSvgFooter(sb);
            return sb.ToString();
        }

        // Find ranges
        var xMin = float.MaxValue;
        var xMax = float.MinValue;
        var yMin = float.MaxValue;
        var yMax = float.MinValue;
        foreach (var bar in scene.Bars)
        {
            if (bar.BodyMin.X < xMin) xMin = bar.BodyMin.X;
            if (bar.BodyMax.X > xMax) xMax = bar.BodyMax.X;
            if (bar.WickBottom.Y < yMin) yMin = bar.WickBottom.Y;
            if (bar.WickTop.Y > yMax) yMax = bar.WickTop.Y;
        }

        var xRange = xMax - xMin;
        if (xRange <= 0f) xRange = 1f;
        var yRange = yMax - yMin;
        if (yRange <= 0f) yRange = 1f;
        var yPad = yRange * 0.05f;
        var barWidth = plotWidth / scene.Bars.Count * 0.7f;

        var mapX = (float val) => DefaultMarginLeft + (val - xMin) / xRange * plotWidth;
        var mapY = (float val) => DefaultMarginTop + plotHeight - (val - yMin + yPad) / (yRange + 2 * yPad) * plotHeight;

        foreach (var bar in scene.Bars)
        {
            var centerX = mapX((bar.BodyMin.X + bar.BodyMax.X) / 2f);
            var halfBar = barWidth / 2f;
            var color = ColorToHex(bar.Color);

            // Wick (high-low line)
            sb.AppendLine(FormattableString.Invariant(
                $"  <line x1=\"{centerX:F2}\" y1=\"{mapY(bar.WickTop.Y):F2}\" x2=\"{centerX:F2}\" y2=\"{mapY(bar.WickBottom.Y):F2}\" stroke=\"{color}\" stroke-width=\"1\" />"));

            // Body (open-close rectangle)
            var bodyTop = mapY(Math.Max(bar.BodyMin.Y, bar.BodyMax.Y));
            var bodyBottom = mapY(Math.Min(bar.BodyMin.Y, bar.BodyMax.Y));
            var bodyHeight = Math.Max(1f, bodyBottom - bodyTop);

            if (scene.Style == OHLCStyle.Candlestick)
            {
                var fillColor = bar.IsBullish ? color : "#fff";
                sb.Append("  <rect x=\"");
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2}", centerX - halfBar);
                sb.Append("\" y=\"");
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2}", bodyTop);
                sb.Append("\" width=\"");
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2}", barWidth);
                sb.Append("\" height=\"");
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2}", bodyHeight);
                sb.Append("\" fill=\"");
                sb.Append(fillColor);
                sb.Append("\" stroke=\"");
                sb.Append(color);
                sb.AppendLine("\" stroke-width=\"1\" />");
            }
            else
            {
                // OHLC style: just tick marks
                sb.AppendLine(FormattableString.Invariant(
                    $"  <line x1=\"{centerX - halfBar:F2}\" y1=\"{mapY(bar.BodyMin.Y):F2}\" x2=\"{centerX:F2}\" y2=\"{mapY(bar.BodyMin.Y):F2}\" stroke=\"{color}\" stroke-width=\"1.5\" />"));
                sb.AppendLine(FormattableString.Invariant(
                    $"  <line x1=\"{centerX:F2}\" y1=\"{mapY(bar.BodyMax.Y):F2}\" x2=\"{centerX + halfBar:F2}\" y2=\"{mapY(bar.BodyMax.Y):F2}\" stroke=\"{color}\" stroke-width=\"1.5\" />"));
            }
        }

        AppendSvgFooter(sb);
        return sb.ToString();
    }

    private static (float XMin, float XMax, float YMin, float YMax) GetScatterBounds(ScatterRenderScene scene)
    {
        var xMin = float.MaxValue;
        var xMax = float.MinValue;
        var yMin = float.MaxValue;
        var yMax = float.MinValue;

        foreach (var series in scene.Series)
        {
            foreach (var point in series.Points)
            {
                if (point.Position.X < xMin) xMin = point.Position.X;
                if (point.Position.X > xMax) xMax = point.Position.X;
                if (point.Position.Z < yMin) yMin = point.Position.Z;
                if (point.Position.Z > yMax) yMax = point.Position.Z;
            }
        }

        if (xMax <= xMin) xMax = xMin + 1f;
        if (yMax <= yMin) yMax = yMin + 1f;
        return (xMin, xMax, yMin, yMax);
    }

    private static (float XMin, float XMax, float YMin, float YMax) GetLineBounds(LineRenderScene scene)
    {
        var xMin = float.MaxValue;
        var xMax = float.MinValue;
        var yMin = float.MaxValue;
        var yMax = float.MinValue;

        foreach (var seg in scene.Segments)
        {
            if (seg.Start.X < xMin) xMin = seg.Start.X;
            if (seg.End.X < xMin) xMin = seg.End.X;
            if (seg.Start.X > xMax) xMax = seg.Start.X;
            if (seg.End.X > xMax) xMax = seg.End.X;
            if (seg.Start.Y < yMin) yMin = seg.Start.Y;
            if (seg.End.Y < yMin) yMin = seg.End.Y;
            if (seg.Start.Y > yMax) yMax = seg.Start.Y;
            if (seg.End.Y > yMax) yMax = seg.End.Y;
        }

        foreach (var marker in scene.Markers)
        {
            if (marker.Position.X < xMin) xMin = marker.Position.X;
            if (marker.Position.X > xMax) xMax = marker.Position.X;
            if (marker.Position.Y < yMin) yMin = marker.Position.Y;
            if (marker.Position.Y > yMax) yMax = marker.Position.Y;
        }

        if (xMax <= xMin) xMax = xMin + 1f;
        if (yMax <= yMin) yMax = yMin + 1f;
        return (xMin, xMax, yMin, yMax);
    }

    private static void AppendSvgHeader(StringBuilder sb, int width, int height)
    {
        sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" viewBox=\"0 0 {width} {height}\">");
        sb.AppendLine("  <style>");
        sb.AppendLine("    text { font-family: 'Segoe UI', Arial, sans-serif; font-size: 11px; }");
        sb.AppendLine("  </style>");
    }

    private static void AppendSvgFooter(StringBuilder sb)
    {
        sb.AppendLine("</svg>");
    }

    private static string ColorToHex(uint argb)
    {
        var r = (argb >> 16) & 0xFF;
        var g = (argb >> 8) & 0xFF;
        var b = argb & 0xFF;
        var a = (argb >> 24) & 0xFF;

        if (a == 255)
        {
            return FormattableString.Invariant($"#{r:X2}{g:X2}{b:X2}");
        }

        var opacity = a / 255.0;
        return FormattableString.Invariant($"#{r:X2}{g:X2}{b:X2}\" opacity=\"{opacity:F2}");
    }

    private static string Escape(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }
}
