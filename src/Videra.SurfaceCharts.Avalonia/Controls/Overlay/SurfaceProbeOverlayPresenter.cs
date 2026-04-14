using System.Globalization;
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

    public static SurfaceProbeOverlayState CreateState(
        ISurfaceTileSource? source,
        SurfaceViewport viewport,
        Size viewSize,
        IReadOnlyList<SurfaceTile> loadedTiles,
        Point? probeScreenPosition)
    {
        ArgumentNullException.ThrowIfNull(loadedTiles);

        if (source is null || loadedTiles.Count == 0)
        {
            return new SurfaceProbeOverlayState(
                hasNoData: true,
                noDataText: "No data",
                readoutText: null,
                probeScreenPosition: null,
                probeResult: null);
        }

        if (probeScreenPosition is null || viewSize.Width <= 0d || viewSize.Height <= 0d)
        {
            return SurfaceProbeOverlayState.Empty;
        }

        var probeResult = ResolveProbeResult(source.Metadata, viewport, viewSize, loadedTiles, probeScreenPosition.Value);
        var readoutText = probeResult is null
            ? null
            : string.Create(
                CultureInfo.InvariantCulture,
                $"X {probeResult.Value.SampleX:0.###}, Y {probeResult.Value.SampleY:0.###}, Value {probeResult.Value.Value:0.###}");

        return new SurfaceProbeOverlayState(
            hasNoData: false,
            noDataText: null,
            readoutText: readoutText,
            probeScreenPosition: probeScreenPosition,
            probeResult: probeResult);
    }

    public static void Render(DrawingContext context, SurfaceProbeOverlayState overlayState, Size viewSize)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(overlayState);

        if (overlayState.HasNoData && !string.IsNullOrWhiteSpace(overlayState.NoDataText))
        {
            DrawCenteredText(context, overlayState.NoDataText, viewSize, EmptyForeground);
        }

        if (!string.IsNullOrWhiteSpace(overlayState.ReadoutText) && overlayState.ProbeScreenPosition is Point probeScreenPosition)
        {
            DrawReadout(context, overlayState.ReadoutText, probeScreenPosition);
        }
    }

    private static SurfaceProbeResult? ResolveProbeResult(
        SurfaceMetadata metadata,
        SurfaceViewport viewport,
        Size viewSize,
        IReadOnlyList<SurfaceTile> loadedTiles,
        Point probeScreenPosition)
    {
        var clampedViewport = viewport.ClampTo(metadata);
        var normalizedX = Math.Clamp(probeScreenPosition.X / viewSize.Width, 0d, 1d);
        var normalizedY = Math.Clamp(probeScreenPosition.Y / viewSize.Height, 0d, 1d);
        var sampleX = Math.Clamp(
            clampedViewport.StartX + (normalizedX * clampedViewport.Width),
            0d,
            metadata.Width - 1d);
        var sampleY = Math.Clamp(
            clampedViewport.StartY + (normalizedY * clampedViewport.Height),
            0d,
            metadata.Height - 1d);
        var sampleIndexX = Math.Clamp((int)Math.Floor(sampleX), 0, metadata.Width - 1);
        var sampleIndexY = Math.Clamp((int)Math.Floor(sampleY), 0, metadata.Height - 1);

        foreach (var tile in loadedTiles
                     .OrderByDescending(static tile => tile.Key.LevelX + tile.Key.LevelY)
                     .ThenByDescending(static tile => tile.Key.LevelX)
                     .ThenByDescending(static tile => tile.Key.LevelY))
        {
            if (sampleIndexX < tile.Bounds.StartX || sampleIndexX >= tile.Bounds.EndXExclusive)
            {
                continue;
            }

            if (sampleIndexY < tile.Bounds.StartY || sampleIndexY >= tile.Bounds.EndYExclusive)
            {
                continue;
            }

            // Loaded coarse tiles no longer assume a 1:1 bounds-to-grid shape, so probe lookups
            // must remap source-space coordinates back into the tile's value grid.
            var tileX = MapSampleToTileIndex(sampleX, tile.Bounds.StartX, tile.Bounds.Width, tile.Width);
            var tileY = MapSampleToTileIndex(sampleY, tile.Bounds.StartY, tile.Bounds.Height, tile.Height);
            var valueIndex = (tileY * tile.Width) + tileX;
            var value = tile.Values.Span[valueIndex];
            if (!float.IsFinite(value))
            {
                return null;
            }

            return new SurfaceProbeResult(sampleX, sampleY, value);
        }

        return null;
    }

    private static int MapSampleToTileIndex(double sampleCoordinate, int start, int span, int gridSize)
    {
        if (gridSize <= 1)
        {
            return 0;
        }

        var offset = sampleCoordinate - start;
        var normalized = Math.Clamp(offset / span, 0d, 1d);
        return Math.Min(gridSize - 1, (int)Math.Floor(normalized * gridSize));
    }

    private static void DrawCenteredText(DrawingContext context, string text, Size viewSize, IBrush foreground)
    {
        var formattedText = CreateText(text, foreground);
        var origin = new Point(
            Math.Max(0d, (viewSize.Width - formattedText.Width) / 2d),
            Math.Max(0d, (viewSize.Height - formattedText.Height) / 2d));
        context.DrawText(formattedText, origin);
    }

    private static void DrawReadout(DrawingContext context, string readoutText, Point probeScreenPosition)
    {
        var text = CreateText(readoutText, BubbleForeground);
        var bubbleOrigin = new Point(probeScreenPosition.X + 8d, probeScreenPosition.Y - text.Height - 6d);
        var bubbleRect = new Rect(
            bubbleOrigin.X - 4d,
            bubbleOrigin.Y - 2d,
            text.Width + 8d,
            text.Height + 4d);

        context.DrawLine(new Pen(BubbleForeground, 1d), probeScreenPosition, new Point(bubbleRect.X, bubbleRect.Bottom));
        context.DrawEllipse(BubbleForeground, null, probeScreenPosition, 3d, 3d);
        context.DrawRectangle(BubbleBackground, new Pen(BubbleBorder, 1d), bubbleRect, 4d, 4d);
        context.DrawText(text, bubbleOrigin);
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
