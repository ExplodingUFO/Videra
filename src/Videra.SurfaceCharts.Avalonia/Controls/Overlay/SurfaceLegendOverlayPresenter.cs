using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

/// <summary>
/// Presents the multi-series legend overlay with kind-specific visual indicators.
/// </summary>
internal static class SurfaceLegendOverlayPresenter
{
    private static readonly Typeface OverlayTypeface = new("Consolas");
    private static readonly IBrush LegendTextBrush = new SolidColorBrush(Color.FromArgb(220, 245, 245, 245));
    private static readonly IBrush LegendBackgroundBrush = new SolidColorBrush(Color.FromArgb(180, 30, 30, 30));
    private static readonly Pen LegendBorderPen = new(LegendTextBrush, 1d);

    private const double EntryHeight = 20d;
    private const double IndicatorSize = 12d;
    private const double IndicatorSpacing = 8d;
    private const double Padding = 12d;
    private const double MaxLegendWidth = 200d;
    private const double MaxLegendHeight = 300d;

    /// <summary>
    /// Creates the legend state from the current plot series.
    /// </summary>
    public static SurfaceLegendOverlayState CreateState(
        IReadOnlyList<Plot3DSeries> series,
        SurfaceChartProjection? projection,
        SurfaceChartOverlayOptions? overlayOptions)
    {
        ArgumentNullException.ThrowIfNull(series);

        if (series.Count == 0 || projection is null)
        {
            return SurfaceLegendOverlayState.Empty;
        }

        overlayOptions ??= SurfaceChartOverlayOptions.Default;

        var entries = new List<SurfaceLegendEntry>();
        foreach (var s in series)
        {
            if (!s.IsVisible)
            {
                continue;
            }

            var entry = CreateLegendEntry(s);
            entries.Add(entry);
        }

        if (entries.Count == 0)
        {
            return SurfaceLegendOverlayState.Empty;
        }

        // Calculate layout
        var position = overlayOptions.LegendPosition;
        var bounds = CalculateLegendBounds(entries.Count, position, projection.ViewSize);

        // Check for truncation
        var maxEntries = (int)Math.Floor((bounds.Height - Padding * 2) / EntryHeight);
        var isTruncated = entries.Count > maxEntries;
        if (isTruncated)
        {
            entries = entries.Take(maxEntries).ToList();
        }

        return new SurfaceLegendOverlayState(
            entries: entries,
            position: position,
            bounds: bounds,
            isTruncated: isTruncated);
    }

    /// <summary>
    /// Renders the legend overlay.
    /// </summary>
    public static void Render(DrawingContext context, SurfaceLegendOverlayState legendOverlayState)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(legendOverlayState);

        if (legendOverlayState.Entries.Count == 0)
        {
            return;
        }

        var bounds = legendOverlayState.Bounds;

        // Draw background
        context.DrawRectangle(LegendBackgroundBrush, LegendBorderPen, bounds);

        // Draw entries
        var y = bounds.Y + Padding;
        var indicatorX = bounds.X + Padding;
        var textX = indicatorX + IndicatorSize + IndicatorSpacing;

        foreach (var entry in legendOverlayState.Entries)
        {
            var indicatorRect = new Rect(indicatorX, y + (EntryHeight - IndicatorSize) / 2, IndicatorSize, IndicatorSize);
            DrawIndicator(context, entry, indicatorRect);

            var textPoint = new Point(textX, y + 2d);
            context.DrawText(CreateText(entry.SeriesName), textPoint);

            y += EntryHeight;
        }

        // Draw truncation indicator
        if (legendOverlayState.IsTruncated)
        {
            var truncationText = "+ more...";
            var truncationPoint = new Point(indicatorX, y + 2d);
            context.DrawText(CreateText(truncationText), truncationPoint);
        }
    }

    private static SurfaceLegendEntry CreateLegendEntry(Plot3DSeries series)
    {
        var indicatorKind = series.Kind switch
        {
            Plot3DSeriesKind.Surface => LegendIndicatorKind.Swatch,
            Plot3DSeriesKind.Waterfall => LegendIndicatorKind.Swatch,
            Plot3DSeriesKind.Scatter => LegendIndicatorKind.Dot,
            Plot3DSeriesKind.Bar => LegendIndicatorKind.Swatch,
            Plot3DSeriesKind.Contour => LegendIndicatorKind.Swatch,
            Plot3DSeriesKind.Line => LegendIndicatorKind.Line,
            Plot3DSeriesKind.Ribbon => LegendIndicatorKind.Swatch,
            Plot3DSeriesKind.VectorField => LegendIndicatorKind.Swatch,
            Plot3DSeriesKind.HeatmapSlice => LegendIndicatorKind.Swatch,
            Plot3DSeriesKind.BoxPlot => LegendIndicatorKind.Swatch,
            _ => LegendIndicatorKind.Swatch,
        };

        // Default color based on series kind
        var color = series.Kind switch
        {
            Plot3DSeriesKind.Surface => 0xFF4DA3FF, // Blue
            Plot3DSeriesKind.Waterfall => 0xFF4DA3FF, // Blue
            Plot3DSeriesKind.Scatter => 0xFFFF6B6B, // Red
            Plot3DSeriesKind.Bar => 0xFF4488CC, // Steel blue
            Plot3DSeriesKind.Contour => 0xFF4DA3FF, // Blue
            Plot3DSeriesKind.Line => 0xFF4DA3FF, // Blue
            Plot3DSeriesKind.Ribbon => 0xFF9B59B6, // Purple
            Plot3DSeriesKind.VectorField => 0xFF2ECC71, // Green
            Plot3DSeriesKind.HeatmapSlice => 0xFFF39C12, // Orange
            Plot3DSeriesKind.BoxPlot => 0xFF4488CC, // Steel blue
            _ => 0xFFCCCCCC, // Gray
        };

        var name = series.Name ?? $"Series {series.Kind}";

        return new SurfaceLegendEntry(
            seriesName: name,
            seriesKind: series.Kind,
            isVisible: series.IsVisible,
            color: color,
            indicatorKind: indicatorKind);
    }

    private static Rect CalculateLegendBounds(
        int entryCount,
        SurfaceChartLegendPosition position,
        Size viewSize)
    {
        var width = Math.Min(MaxLegendWidth, viewSize.Width * 0.3);
        var height = Math.Min(
            MaxLegendHeight,
            Padding * 2 + entryCount * EntryHeight);

        var x = position switch
        {
            SurfaceChartLegendPosition.TopLeft => Padding,
            SurfaceChartLegendPosition.TopRight => viewSize.Width - width - Padding,
            SurfaceChartLegendPosition.BottomLeft => Padding,
            SurfaceChartLegendPosition.BottomRight => viewSize.Width - width - Padding,
            _ => viewSize.Width - width - Padding,
        };

        var y = position switch
        {
            SurfaceChartLegendPosition.TopLeft => Padding,
            SurfaceChartLegendPosition.TopRight => Padding,
            SurfaceChartLegendPosition.BottomLeft => viewSize.Height - height - Padding,
            SurfaceChartLegendPosition.BottomRight => viewSize.Height - height - Padding,
            _ => Padding,
        };

        return new Rect(x, y, width, height);
    }

    private static void DrawIndicator(DrawingContext context, SurfaceLegendEntry entry, Rect rect)
    {
        var color = ToColor(entry.Color);
        var brush = new SolidColorBrush(color);

        switch (entry.IndicatorKind)
        {
            case LegendIndicatorKind.Swatch:
                context.DrawRectangle(brush, null, rect);
                break;

            case LegendIndicatorKind.Dot:
                var centerX = rect.X + rect.Width / 2;
                var centerY = rect.Y + rect.Height / 2;
                var radius = rect.Width / 2;
                context.DrawEllipse(brush, null, new Point(centerX, centerY), radius, radius);
                break;

            case LegendIndicatorKind.Line:
                var pen = new Pen(brush, 2d);
                var y = rect.Y + rect.Height / 2;
                context.DrawLine(pen, new Point(rect.X, y), new Point(rect.Right, y));
                break;
        }
    }

    private static FormattedText CreateText(string text)
    {
        return new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            OverlayTypeface,
            12d,
            LegendTextBrush);
    }

    private static Color ToColor(uint argb)
    {
        return Color.FromArgb(
            (byte)((argb >> 24) & 0xFFu),
            (byte)((argb >> 16) & 0xFFu),
            (byte)((argb >> 8) & 0xFFu),
            (byte)(argb & 0xFFu));
    }
}
