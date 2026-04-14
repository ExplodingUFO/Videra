using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal static class SurfaceProbeOverlayPresenter
{
    private static readonly Typeface OverlayTypeface = new("Consolas");
    private static readonly IBrush BubbleBackground = new SolidColorBrush(Color.FromArgb(235, 24, 24, 24));
    private static readonly IBrush BubbleBorder = Brushes.White;
    private static readonly IBrush BubbleForeground = Brushes.White;
    private static readonly IBrush EmptyForeground = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255));
    private static readonly IBrush PinnedMarkerFill = new SolidColorBrush(Color.FromArgb(255, 255, 208, 64));
    private const double BubbleMargin = 8d;

    public static SurfaceProbeOverlayState CreateState(
        ISurfaceTileSource? source,
        SurfaceViewport viewport,
        Size viewSize,
        IReadOnlyList<SurfaceTile> loadedTiles,
        Point? probeScreenPosition,
        IReadOnlyList<SurfaceProbeRequest>? pinnedProbeRequests = null,
        SurfaceProbeService? probeService = null)
    {
        ArgumentNullException.ThrowIfNull(loadedTiles);

        if (source is null || loadedTiles.Count == 0)
        {
            return new SurfaceProbeOverlayState(
                hasNoData: true,
                noDataText: "No data",
                hoveredProbeScreenPosition: null,
                hoveredProbe: null,
                pinnedProbes: []);
        }

        var resolvedProbeService = probeService ?? new SurfaceProbeService();
        var hoveredProbe = probeScreenPosition is Point hoveredProbeScreenPosition
            ? resolvedProbeService.ResolveFromScreenPosition(source.Metadata, viewport, viewSize, loadedTiles, hoveredProbeScreenPosition)
            : null;
        var pinnedProbes = ResolvePinnedProbes(source.Metadata, loadedTiles, pinnedProbeRequests);

        return new SurfaceProbeOverlayState(
            hasNoData: false,
            noDataText: null,
            hoveredProbeScreenPosition: hoveredProbe is null ? null : probeScreenPosition,
            hoveredProbe: hoveredProbe,
            pinnedProbes: pinnedProbes);
    }

    public static void Render(
        DrawingContext context,
        SurfaceProbeOverlayState overlayState,
        Size viewSize,
        SurfaceChartProjection? projection)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(overlayState);

        if (overlayState.HasNoData && !string.IsNullOrWhiteSpace(overlayState.NoDataText))
        {
            DrawCenteredText(context, overlayState.NoDataText, viewSize, EmptyForeground);
        }

        if (projection is not null && overlayState.PinnedProbes.Count > 0)
        {
            DrawPinnedReadouts(context, overlayState.PinnedProbes, projection, viewSize);
        }

        if (overlayState.HoveredProbe is SurfaceProbeInfo hoveredProbe &&
            overlayState.HoveredProbeScreenPosition is Point hoveredProbeScreenPosition)
        {
            DrawReadoutBubble(
                context,
                CreateHoveredReadoutText(hoveredProbe),
                hoveredProbeScreenPosition,
                viewSize,
                bubbleIndex: 0,
                markerPosition: null);
        }
    }

    private static IReadOnlyList<SurfaceProbeInfo> ResolvePinnedProbes(
        SurfaceMetadata metadata,
        IReadOnlyList<SurfaceTile> loadedTiles,
        IReadOnlyList<SurfaceProbeRequest>? pinnedProbeRequests)
    {
        if (pinnedProbeRequests is null || pinnedProbeRequests.Count == 0)
        {
            return [];
        }

        List<SurfaceProbeInfo> pinnedProbes = new(pinnedProbeRequests.Count);
        foreach (var pinnedProbeRequest in pinnedProbeRequests)
        {
            var resolvedProbe = SurfaceProbeService.Resolve(metadata, loadedTiles, pinnedProbeRequest);
            if (resolvedProbe is SurfaceProbeInfo probe)
            {
                pinnedProbes.Add(probe);
            }
        }

        return pinnedProbes;
    }

    private static void DrawCenteredText(DrawingContext context, string text, Size viewSize, IBrush foreground)
    {
        var formattedText = CreateText(text, foreground);
        var origin = new Point(
            Math.Max(0d, (viewSize.Width - formattedText.Width) / 2d),
            Math.Max(0d, (viewSize.Height - formattedText.Height) / 2d));
        context.DrawText(formattedText, origin);
    }

    private static void DrawPinnedReadouts(
        DrawingContext context,
        IReadOnlyList<SurfaceProbeInfo> pinnedProbes,
        SurfaceChartProjection projection,
        Size viewSize)
    {
        for (var index = 0; index < pinnedProbes.Count; index++)
        {
            var probe = pinnedProbes[index];
            var markerPosition = projection.Project(CreateMarkerVector(probe));
            context.DrawEllipse(PinnedMarkerFill, new Pen(BubbleBorder, 1d), markerPosition, 4d, 4d);

            DrawReadoutBubble(
                context,
                CreatePinnedReadoutText(index + 1, probe),
                markerPosition,
                viewSize,
                bubbleIndex: index,
                markerPosition: markerPosition);
        }
    }

    private static void DrawReadoutBubble(
        DrawingContext context,
        string readoutText,
        Point anchorPosition,
        Size viewSize,
        int bubbleIndex,
        Point? markerPosition)
    {
        var text = CreateText(readoutText, BubbleForeground);
        var bubbleOrigin = markerPosition is not null
            ? CreatePinnedBubbleOrigin(text, viewSize, bubbleIndex)
            : CreateHoveredBubbleOrigin(text, anchorPosition, viewSize);
        var bubbleRect = new Rect(
            bubbleOrigin.X - 4d,
            bubbleOrigin.Y - 2d,
            text.Width + 8d,
            text.Height + 4d);

        var lineStart = markerPosition ?? anchorPosition;
        var lineEnd = markerPosition is null
            ? new Point(bubbleRect.X, bubbleRect.Bottom)
            : new Point(bubbleRect.X, bubbleRect.Center.Y);

        context.DrawLine(new Pen(BubbleForeground, 1d), lineStart, lineEnd);
        context.DrawEllipse(markerPosition is null ? BubbleForeground : PinnedMarkerFill, new Pen(BubbleBorder, 1d), lineStart, 3d, 3d);
        context.DrawRectangle(BubbleBackground, new Pen(BubbleBorder, 1d), bubbleRect, 4d, 4d);
        context.DrawText(text, bubbleOrigin);
    }

    private static Point CreateHoveredBubbleOrigin(FormattedText text, Point anchorPosition, Size viewSize)
    {
        var candidate = new Point(anchorPosition.X + BubbleMargin, anchorPosition.Y - text.Height - 6d);
        return ClampBubbleOrigin(candidate, text, viewSize);
    }

    private static Point CreatePinnedBubbleOrigin(FormattedText text, Size viewSize, int bubbleIndex)
    {
        var candidate = new Point(
            Math.Max(BubbleMargin, viewSize.Width - text.Width - 20d),
            BubbleMargin + (bubbleIndex * (text.Height + BubbleMargin)));
        return ClampBubbleOrigin(candidate, text, viewSize);
    }

    private static Point ClampBubbleOrigin(Point candidate, FormattedText text, Size viewSize)
    {
        var maxX = Math.Max(BubbleMargin, viewSize.Width - text.Width - BubbleMargin);
        var maxY = Math.Max(BubbleMargin, viewSize.Height - text.Height - BubbleMargin);
        return new Point(
            Math.Clamp(candidate.X, BubbleMargin, maxX),
            Math.Clamp(candidate.Y, BubbleMargin, maxY));
    }

    private static string CreateHoveredReadoutText(SurfaceProbeInfo probe)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"X {probe.AxisX:0.###} (sample {probe.SampleX:0.###})\nY {probe.AxisY:0.###} (sample {probe.SampleY:0.###})\nValue {probe.Value:0.###} {(probe.IsApproximate ? "Approx" : "Exact")}");
    }

    private static string CreatePinnedReadoutText(int pinnedIndex, SurfaceProbeInfo probe)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"Pin {pinnedIndex} {(probe.IsApproximate ? "Approx" : "Exact")}\nX {probe.AxisX:0.###} (sample {probe.SampleX:0.###})\nY {probe.AxisY:0.###} (sample {probe.SampleY:0.###})\nValue {probe.Value:0.###}");
    }

    private static Vector3 CreateMarkerVector(SurfaceProbeInfo probe)
    {
        return new Vector3((float)probe.AxisX, (float)probe.Value, (float)probe.AxisY);
    }

    private static FormattedText CreateText(string text, IBrush foreground)
    {
        return new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            OverlayTypeface,
            12d,
            foreground);
    }
}
