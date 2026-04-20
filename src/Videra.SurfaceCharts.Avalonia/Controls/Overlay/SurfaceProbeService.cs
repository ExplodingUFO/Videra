using Avalonia;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal static class SurfaceProbeService
{
    public static SurfaceProbeInfo? ResolveFromScreenPosition(
        SurfaceMetadata metadata,
        SurfaceCameraFrame cameraFrame,
        IReadOnlyList<SurfaceTile> loadedTiles,
        Point probeScreenPosition)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(loadedTiles);

        if (probeScreenPosition.X < 0d ||
            probeScreenPosition.Y < 0d ||
            probeScreenPosition.X > cameraFrame.ViewportPixels.X ||
            probeScreenPosition.Y > cameraFrame.ViewportPixels.Y)
        {
            return null;
        }

        var pickRay = SurfaceHeightfieldPicker.CreatePickRay(
            new System.Numerics.Vector2((float)probeScreenPosition.X, (float)probeScreenPosition.Y),
            cameraFrame);
        var pickHit = SurfaceHeightfieldPicker.Pick(metadata, loadedTiles, pickRay);
        return pickHit is SurfacePickHit hit
            ? SurfaceProbeInfo.FromPickHit(metadata, hit)
            : null;
    }

    public static SurfaceProbeInfo? ResolveFromScreenPosition(
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
            if (tile.Mask is not null && !tile.Mask.Values.Span[valueIndex])
            {
                return null;
            }

            var value = tile.Values.Span[valueIndex];
            if (!float.IsFinite(value))
            {
                return null;
            }

            return SurfaceProbeInfo.FromResolvedSample(metadata, tile, probeRequest, value);
        }

        return null;
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
