using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

/// <summary>
/// Presents the crosshair overlay — two projected ground-plane guidelines (X + Z) through the probe point
/// with axis-value pills at the guideline endpoints.
/// </summary>
internal static class SurfaceCrosshairOverlayPresenter
{
    private static readonly Typeface OverlayTypeface = new("Consolas");
    private static readonly IBrush GuidelineBrush = new SolidColorBrush(Color.FromArgb(160, 255, 255, 255));
    private static readonly Pen GuidelinePen = new(GuidelineBrush, 1d) { DashStyle = DashStyle.Dash };
    private static readonly IBrush PillBackground = new SolidColorBrush(Color.FromArgb(200, 30, 30, 30));
    private static readonly IBrush PillBorder = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255));
    private static readonly IBrush PillForeground = new SolidColorBrush(Color.FromArgb(240, 255, 255, 255));
    private static readonly Pen PillPen = new(PillBorder, 1d);

    private const double PillPaddingX = 6d;
    private const double PillPaddingY = 3d;
    private const double PillCornerRadius = 3d;
    private const double TextFontSize = 11d;

    /// <summary>
    /// Creates the crosshair overlay state from the current probe screen position.
    /// </summary>
    public static SurfaceCrosshairOverlayState CreateState(
        Point? probeScreenPosition,
        SurfaceChartProjection? projection,
        SurfaceChartOverlayOptions? overlayOptions,
        SurfaceMetadata? metadata)
    {
        overlayOptions ??= SurfaceChartOverlayOptions.Default;

        if (!overlayOptions.ShowCrosshair
            || projection is null
            || metadata is null
            || probeScreenPosition is not Point screenPos)
        {
            return SurfaceCrosshairOverlayState.Empty;
        }

        // Inverse-project the screen position to get the world-space X/Z on the ground plane.
        // The crosshair lines run along the ground plane (Y = value-range minimum).
        var valueRange = metadata.ValueRange;
        var yMin = (float)valueRange.Minimum;
        var xMin = (float)metadata.HorizontalAxis.Minimum;
        var xMax = (float)metadata.HorizontalAxis.Maximum;
        var zMin = (float)metadata.VerticalAxis.Minimum;
        var zMax = (float)metadata.VerticalAxis.Maximum;

        // Resolve probe world X/Z from the hovered probe info or estimate from screen position.
        // For crosshair, we use the probe's actual world coordinates if available,
        // otherwise we approximate from the screen position by projecting onto the ground plane.
        var (probeX, probeZ) = ResolveProbeWorldCoordinates(
            screenPos, projection, metadata, yMin, xMin, xMax, zMin, zMax);

        // Project X guideline: horizontal line at probe Z, spanning X range
        var xLineStart = projection.Project(new Vector3(xMin, yMin, probeZ));
        var xLineEnd = projection.Project(new Vector3(xMax, yMin, probeZ));

        // Project Z guideline: vertical line at probe X, spanning Z range
        var zLineStart = projection.Project(new Vector3(probeX, yMin, zMin));
        var zLineEnd = projection.Project(new Vector3(probeX, yMin, zMax));

        // Determine outer endpoints for pill positioning (farther from projected center)
        var projectedCenter = projection.ProjectCenter(metadata, valueRange);
        var xPillPosition = GetOuterEndpoint(xLineStart, xLineEnd, projectedCenter);
        var zPillPosition = GetOuterEndpoint(zLineStart, zLineEnd, projectedCenter);

        // Format axis values
        var xValueText = overlayOptions.FormatProbeAxisX(probeX);
        var zValueText = overlayOptions.FormatProbeAxisY(probeZ);

        return new SurfaceCrosshairOverlayState(
            isVisible: true,
            xGuidelineStart: xLineStart,
            xGuidelineEnd: xLineEnd,
            zGuidelineStart: zLineStart,
            zGuidelineEnd: zLineEnd,
            xValueText: xValueText,
            zValueText: zValueText,
            xPillPosition: xPillPosition,
            zPillPosition: zPillPosition);
    }

    /// <summary>
    /// Renders the crosshair overlay — dashed guidelines and axis-value pills.
    /// </summary>
    public static void Render(DrawingContext context, SurfaceCrosshairOverlayState crosshairState)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(crosshairState);

        if (!crosshairState.IsVisible)
        {
            return;
        }

        // Draw X guideline
        context.DrawLine(GuidelinePen, crosshairState.XGuidelineStart, crosshairState.XGuidelineEnd);

        // Draw Z guideline
        context.DrawLine(GuidelinePen, crosshairState.ZGuidelineStart, crosshairState.ZGuidelineEnd);

        // Draw X value pill
        if (!string.IsNullOrEmpty(crosshairState.XValueText))
        {
            DrawValuePill(context, crosshairState.XValueText, crosshairState.XPillPosition);
        }

        // Draw Z value pill
        if (!string.IsNullOrEmpty(crosshairState.ZValueText))
        {
            DrawValuePill(context, crosshairState.ZValueText, crosshairState.ZPillPosition);
        }
    }

    private static (float X, float Z) ResolveProbeWorldCoordinates(
        Point screenPos,
        SurfaceChartProjection projection,
        SurfaceMetadata metadata,
        float yMin,
        float xMin,
        float xMax,
        float zMin,
        float zMax)
    {
        // Estimate world X/Z from screen position by projecting the four ground-plane corners
        // and finding the nearest match. This is an approximation — for exact values, the
        // hovered probe info should be used (available in the coordinator's probe state).
        var centerWorld = new Vector3(
            (xMin + xMax) * 0.5f,
            yMin,
            (zMin + zMax) * 0.5f);
        var centerScreen = projection.Project(centerWorld);

        // Use the screen offset from center to estimate world position
        var screenBounds = projection.ScreenBounds;
        var normalizedX = screenBounds.Width > 0
            ? (screenPos.X - screenBounds.X) / screenBounds.Width
            : 0.5;
        var normalizedY = screenBounds.Height > 0
            ? (screenPos.Y - screenBounds.Y) / screenBounds.Height
            : 0.5;

        var estimatedX = xMin + (float)(normalizedX * (xMax - xMin));
        var estimatedZ = zMin + (float)(normalizedY * (zMax - zMin));

        // Clamp to axis bounds
        estimatedX = Math.Clamp(estimatedX, xMin, xMax);
        estimatedZ = Math.Clamp(estimatedZ, zMin, zMax);

        return (estimatedX, estimatedZ);
    }

    private static void DrawValuePill(DrawingContext context, string text, Point position)
    {
        var formattedText = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            OverlayTypeface,
            TextFontSize,
            PillForeground);

        var pillWidth = formattedText.Width + (PillPaddingX * 2);
        var pillHeight = formattedText.Height + (PillPaddingY * 2);

        // Center the pill on the position
        var pillRect = new Rect(
            position.X - (pillWidth / 2d),
            position.Y - (pillHeight / 2d),
            pillWidth,
            pillHeight);

        context.DrawRectangle(PillBackground, PillPen, pillRect, PillCornerRadius, PillCornerRadius);

        var textOrigin = new Point(
            pillRect.X + PillPaddingX,
            pillRect.Y + PillPaddingY);

        context.DrawText(formattedText, textOrigin);
    }

    private static Point GetOuterEndpoint(Point start, Point end, Point projectedCenter)
    {
        return GetDistance(start, projectedCenter) >= GetDistance(end, projectedCenter) ? start : end;
    }

    private static double GetDistance(Point first, Point second)
    {
        var dx = second.X - first.X;
        var dy = second.Y - first.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }
}
