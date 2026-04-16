using System.Numerics;
using Videra.SurfaceCharts.Core.Rendering;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Estimates projected screen footprint and sampling error for camera-aware LOD.
/// </summary>
public static class SurfaceScreenErrorEstimator
{
    /// <summary>
    /// Estimates the projected footprint for the active data window.
    /// </summary>
    /// <param name="metadata">The dataset metadata.</param>
    /// <param name="dataWindow">The active data window.</param>
    /// <param name="cameraFrame">The active camera frame.</param>
    /// <returns>The projected footprint for the requested data window.</returns>
    public static SurfaceTileProjectedFootprint EstimateDataWindowFootprint(
        SurfaceMetadata metadata,
        SurfaceDataWindow dataWindow,
        SurfaceCameraFrame cameraFrame)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var clampedWindow = dataWindow.ClampTo(metadata);
        var plotBounds = SurfaceProjectionMath.CreatePlotBounds(metadata, clampedWindow);
        return EstimateFootprint(plotBounds.GetCorners(), clampedWindow.Width, clampedWindow.Height, cameraFrame);
    }

    /// <summary>
    /// Estimates the projected footprint for one tile bounds region.
    /// </summary>
    /// <param name="metadata">The dataset metadata.</param>
    /// <param name="tileBounds">The tile sample-space bounds.</param>
    /// <param name="cameraFrame">The active camera frame.</param>
    /// <returns>The projected footprint for the tile bounds.</returns>
    public static SurfaceTileProjectedFootprint EstimateTileFootprint(
        SurfaceMetadata metadata,
        SurfaceTileBounds tileBounds,
        SurfaceCameraFrame cameraFrame)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var minimum = CreateWorldPoint(
            metadata,
            tileBounds.StartX,
            tileBounds.StartY,
            cameraFrame.PlotBounds.Minimum.Y);
        var maximum = CreateWorldPoint(
            metadata,
            tileBounds.EndXExclusive - 1d,
            tileBounds.EndYExclusive - 1d,
            cameraFrame.PlotBounds.Maximum.Y);
        var plotBounds = new SurfacePlotBounds(
            new Vector3(
                MathF.Min(minimum.X, maximum.X),
                MathF.Min(minimum.Y, maximum.Y),
                MathF.Min(minimum.Z, maximum.Z)),
            new Vector3(
                MathF.Max(minimum.X, maximum.X),
                MathF.Max(minimum.Y, maximum.Y),
                MathF.Max(minimum.Z, maximum.Z)));

        return EstimateFootprint(plotBounds.GetCorners(), tileBounds.Width, tileBounds.Height, cameraFrame);
    }

    private static SurfaceTileProjectedFootprint EstimateFootprint(
        IReadOnlyList<Vector3> worldCorners,
        double sampleWidth,
        double sampleHeight,
        SurfaceCameraFrame cameraFrame)
    {
        ArgumentNullException.ThrowIfNull(worldCorners);

        var minX = float.PositiveInfinity;
        var minY = float.PositiveInfinity;
        var maxX = float.NegativeInfinity;
        var maxY = float.NegativeInfinity;
        var minDepth = float.PositiveInfinity;
        var maxDepth = float.NegativeInfinity;

        foreach (var worldCorner in worldCorners)
        {
            var screenPoint = SurfaceProjectionMath.ProjectToScreen(worldCorner, cameraFrame);
            minX = MathF.Min(minX, screenPoint.X);
            minY = MathF.Min(minY, screenPoint.Y);
            maxX = MathF.Max(maxX, screenPoint.X);
            maxY = MathF.Max(maxY, screenPoint.Y);
            minDepth = MathF.Min(minDepth, screenPoint.Z);
            maxDepth = MathF.Max(maxDepth, screenPoint.Z);
        }

        var isVisible =
            maxX >= 0f &&
            maxY >= 0f &&
            minX <= cameraFrame.ViewportPixels.X &&
            minY <= cameraFrame.ViewportPixels.Y &&
            maxDepth >= 0f &&
            minDepth <= 1f;

        return new SurfaceTileProjectedFootprint(
            new Vector2(minX, minY),
            new Vector2(maxX, maxY),
            minDepth,
            isVisible,
            sampleWidth,
            sampleHeight);
    }

    private static Vector3 CreateWorldPoint(
        SurfaceMetadata metadata,
        double sampleX,
        double sampleY,
        float value)
    {
        return new Vector3(
            (float)MapAxis(metadata.HorizontalAxis, sampleX, metadata.Width),
            value,
            (float)MapAxis(metadata.VerticalAxis, sampleY, metadata.Height));
    }

    private static double MapAxis(SurfaceAxisDescriptor axis, double sampleIndex, int sampleCount)
    {
        if (sampleCount <= 1 || axis.Maximum <= axis.Minimum)
        {
            return axis.Minimum;
        }

        var normalized = Math.Clamp(sampleIndex / (sampleCount - 1d), 0d, 1d);
        return axis.Minimum + (axis.Span * normalized);
    }
}
