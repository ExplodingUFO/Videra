using Avalonia;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal sealed class SurfaceProbeService
{
    public SurfaceProbeInfo? ResolveFromScreenPosition(
        SurfaceMetadata metadata,
        SurfaceViewport viewport,
        Size viewSize,
        IReadOnlyList<SurfaceTile> loadedTiles,
        Point probeScreenPosition)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(loadedTiles);

        if (viewSize.Width <= 0d || viewSize.Height <= 0d)
        {
            return null;
        }

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

        return Resolve(metadata, loadedTiles, new SurfaceProbeRequest(sampleX, sampleY));
    }

    public static SurfaceProbeInfo? Resolve(
        SurfaceMetadata metadata,
        IReadOnlyList<SurfaceTile> loadedTiles,
        SurfaceProbeRequest probeRequest)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(loadedTiles);

        var sampleIndexX = Math.Clamp((int)Math.Floor(probeRequest.SampleX), 0, metadata.Width - 1);
        var sampleIndexY = Math.Clamp((int)Math.Floor(probeRequest.SampleY), 0, metadata.Height - 1);

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

            var tileX = MapSampleToTileIndex(probeRequest.SampleX, tile.Bounds.StartX, tile.Bounds.Width, tile.Width);
            var tileY = MapSampleToTileIndex(probeRequest.SampleY, tile.Bounds.StartY, tile.Bounds.Height, tile.Height);
            var valueIndex = (tileY * tile.Width) + tileX;
            var value = tile.Values.Span[valueIndex];
            if (!float.IsFinite(value))
            {
                return null;
            }

            return new SurfaceProbeInfo(
                probeRequest.SampleX,
                probeRequest.SampleY,
                MapAxis(metadata.HorizontalAxis, probeRequest.SampleX, metadata.Width),
                MapAxis(metadata.VerticalAxis, probeRequest.SampleY, metadata.Height),
                value,
                isApproximate: tile.Bounds.Width != tile.Width || tile.Bounds.Height != tile.Height);
        }

        return null;
    }

    private static double MapAxis(SurfaceAxisDescriptor axis, double sampleIndex, int sampleCount)
    {
        if (sampleCount <= 1 || axis.Maximum <= axis.Minimum)
        {
            return axis.Minimum;
        }

        var normalized = sampleIndex / (sampleCount - 1d);
        return axis.Minimum + (axis.Span * normalized);
    }

    private static int MapSampleToTileIndex(double sampleCoordinate, int start, int span, int gridSize)
    {
        if (gridSize <= 1)
        {
            return 0;
        }

        var normalized = Math.Clamp((sampleCoordinate - start) / span, 0d, 1d);
        return Math.Min(gridSize - 1, (int)Math.Floor(normalized * gridSize));
    }
}
