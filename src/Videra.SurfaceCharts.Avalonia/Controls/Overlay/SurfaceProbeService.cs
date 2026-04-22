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

            if (TryResolveInterpolatedValue(tile, probeRequest.SampleX, probeRequest.SampleY, out var interpolatedValue))
            {
                return SurfaceProbeInfo.FromResolvedSample(metadata, tile, probeRequest, interpolatedValue);
            }

            if (!TryResolveDiscreteValue(tile, probeRequest.SampleX, probeRequest.SampleY, out var discreteValue))
            {
                return null;
            }

            return SurfaceProbeInfo.FromResolvedSample(metadata, tile, probeRequest, discreteValue);
        }

        return null;
    }

    private static bool TryResolveDiscreteValue(
        SurfaceTile tile,
        double sampleX,
        double sampleY,
        out float value)
    {
        var tileX = MapSampleToTileIndex(sampleX, tile.Bounds.StartX, tile.Bounds.Width, tile.Width);
        var tileY = MapSampleToTileIndex(sampleY, tile.Bounds.StartY, tile.Bounds.Height, tile.Height);
        var valueIndex = (tileY * tile.Width) + tileX;
        if (tile.Mask is not null && !tile.Mask.Values.Span[valueIndex])
        {
            value = default;
            return false;
        }

        value = tile.Values.Span[valueIndex];
        return float.IsFinite(value);
    }

    private static bool TryResolveInterpolatedValue(
        SurfaceTile tile,
        double sampleX,
        double sampleY,
        out float value)
    {
        value = default;

        if (tile.Bounds.Width != tile.Width || tile.Bounds.Height != tile.Height || tile.Width < 2 || tile.Height < 2)
        {
            return false;
        }

        var localX = sampleX - tile.Bounds.StartX;
        var localY = sampleY - tile.Bounds.StartY;
        var leftColumn = (int)Math.Floor(localX);
        var topRow = (int)Math.Floor(localY);
        if (leftColumn < 0 || topRow < 0 || leftColumn >= tile.Width - 1 || topRow >= tile.Height - 1)
        {
            return false;
        }

        var tx = localX - leftColumn;
        var ty = localY - topRow;
        var topLeftIndex = (topRow * tile.Width) + leftColumn;
        var topRightIndex = topLeftIndex + 1;
        var bottomLeftIndex = topLeftIndex + tile.Width;
        var bottomRightIndex = bottomLeftIndex + 1;

        var values = tile.Values.Span;
        var topLeft = values[topLeftIndex];
        var topRight = values[topRightIndex];
        var bottomLeft = values[bottomLeftIndex];
        var bottomRight = values[bottomRightIndex];
        if (!float.IsFinite(topLeft) || !float.IsFinite(topRight) || !float.IsFinite(bottomLeft) || !float.IsFinite(bottomRight))
        {
            return false;
        }

        if (tile.Mask is not null)
        {
            var mask = tile.Mask.Values.Span;
            if (!mask[topLeftIndex] || !mask[topRightIndex] || !mask[bottomLeftIndex] || !mask[bottomRightIndex])
            {
                return false;
            }
        }

        var top = (topLeft * (1f - (float)tx)) + (topRight * (float)tx);
        var bottom = (bottomLeft * (1f - (float)tx)) + (bottomRight * (float)tx);
        value = (top * (1f - (float)ty)) + (bottom * (float)ty);
        return float.IsFinite(value);
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
