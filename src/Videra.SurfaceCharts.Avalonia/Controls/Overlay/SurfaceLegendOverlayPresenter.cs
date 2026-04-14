using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal static class SurfaceLegendOverlayPresenter
{
    private static readonly Typeface OverlayTypeface = new("Consolas");
    private static readonly IBrush LegendTextBrush = new SolidColorBrush(Color.FromArgb(220, 245, 245, 245));
    private static readonly Pen LegendBorderPen = new(LegendTextBrush, 1d);
    private const int SwatchSteps = 24;
    private const double SwatchWidth = 18d;

    public static SurfaceLegendOverlayState CreateState(
        SurfaceMetadata? metadata,
        SurfaceColorMap? colorMap,
        bool useColorMapRange,
        SurfaceChartProjection? projection)
    {
        if (metadata is null || colorMap is null || projection is null)
        {
            return SurfaceLegendOverlayState.Empty;
        }

        var legendRange = useColorMapRange ? colorMap.Range : metadata.ValueRange;

        var maximumSwatchHeight = Math.Max(72d, projection.ViewSize.Height - 40d);
        var swatchHeight = Math.Clamp(projection.ScreenBounds.Height * 0.6d, 72d, maximumSwatchHeight);
        var swatchX = Math.Clamp(
            projection.ScreenBounds.Right + 16d,
            16d,
            Math.Max(16d, projection.ViewSize.Width - SwatchWidth - 48d));
        var swatchY = Math.Clamp(
            (projection.ViewSize.Height - swatchHeight) / 2d,
            16d,
            Math.Max(16d, projection.ViewSize.Height - swatchHeight - 16d));
        var swatchBounds = new Rect(swatchX, swatchY, SwatchWidth, swatchHeight);
        var labelX = swatchBounds.Right + 8d;

        return new SurfaceLegendOverlayState(
            titleText: "Value",
            titlePosition: new Point(swatchBounds.X - 2d, swatchBounds.Y - 18d),
            swatchBounds,
            CreateSwatches(colorMap, legendRange, swatchBounds),
            minimumText: FormatNumber(legendRange.Minimum),
            minimumTextPosition: new Point(labelX, swatchBounds.Bottom - 8d),
            maximumText: FormatNumber(legendRange.Maximum),
            maximumTextPosition: new Point(labelX, swatchBounds.Y - 8d));
    }

    public static void Render(DrawingContext context, SurfaceLegendOverlayState legendOverlayState)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(legendOverlayState);

        foreach (var swatch in legendOverlayState.Swatches)
        {
            context.DrawRectangle(new SolidColorBrush(ToColor(swatch.Color)), null, swatch.Bounds);
        }

        if (legendOverlayState.SwatchBounds.Width > 0d && legendOverlayState.SwatchBounds.Height > 0d)
        {
            context.DrawRectangle(null, LegendBorderPen, legendOverlayState.SwatchBounds);
        }

        if (!string.IsNullOrWhiteSpace(legendOverlayState.TitleText))
        {
            context.DrawText(CreateText(legendOverlayState.TitleText), legendOverlayState.TitlePosition);
        }

        if (!string.IsNullOrWhiteSpace(legendOverlayState.MaximumText))
        {
            context.DrawText(CreateText(legendOverlayState.MaximumText), legendOverlayState.MaximumTextPosition);
        }

        if (!string.IsNullOrWhiteSpace(legendOverlayState.MinimumText))
        {
            context.DrawText(CreateText(legendOverlayState.MinimumText), legendOverlayState.MinimumTextPosition);
        }
    }

    private static IReadOnlyList<SurfaceLegendSwatchState> CreateSwatches(
        SurfaceColorMap colorMap,
        SurfaceValueRange legendRange,
        Rect swatchBounds)
    {
        List<SurfaceLegendSwatchState> swatches = new(SwatchSteps);
        var segmentHeight = swatchBounds.Height / SwatchSteps;
        var rangeSpan = legendRange.Maximum - legendRange.Minimum;

        for (var step = 0; step < SwatchSteps; step++)
        {
            var top = swatchBounds.Y + (segmentHeight * step);
            var height = step == SwatchSteps - 1
                ? swatchBounds.Bottom - top
                : segmentHeight;
            var sample = legendRange.Maximum - (((step + 0.5d) / SwatchSteps) * rangeSpan);

            swatches.Add(
                new SurfaceLegendSwatchState(
                    new Rect(swatchBounds.X, top, swatchBounds.Width, height),
                    colorMap.Map(sample)));
        }

        return swatches;
    }

    private static string FormatNumber(double value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
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
