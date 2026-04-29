using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-owned marker overlay state resolved in bounded sample and axis space.
/// </summary>
public readonly record struct SurfaceChartDraggableMarkerOverlay(
    double SampleX,
    double SampleY,
    double AxisX,
    double AxisY);

/// <summary>
/// Host-owned range overlay state resolved in bounded sample and axis space.
/// </summary>
public readonly record struct SurfaceChartDraggableRangeOverlay(
    SurfaceDataWindow DataWindow,
    double AxisStartX,
    double AxisStartY,
    double AxisEndX,
    double AxisEndY);

/// <summary>
/// Creates immutable chart-local draggable overlay states without mutating source data.
/// </summary>
public static class SurfaceChartDraggableOverlayRecipes
{
    /// <summary>
    /// Creates a marker overlay clamped to the supplied metadata bounds.
    /// </summary>
    public static SurfaceChartDraggableMarkerOverlay CreateMarker(
        SurfaceMetadata metadata,
        double sampleX,
        double sampleY)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var clampedX = ClampSampleX(metadata, sampleX);
        var clampedY = ClampSampleY(metadata, sampleY);
        return new SurfaceChartDraggableMarkerOverlay(
            clampedX,
            clampedY,
            metadata.MapHorizontalCoordinate(clampedX),
            metadata.MapVerticalCoordinate(clampedY));
    }

    /// <summary>
    /// Moves a marker overlay to a new sample coordinate, clamped to metadata bounds.
    /// </summary>
    public static SurfaceChartDraggableMarkerOverlay DragMarkerTo(
        SurfaceMetadata metadata,
        SurfaceChartDraggableMarkerOverlay marker,
        double sampleX,
        double sampleY)
    {
        return CreateMarker(metadata, sampleX, sampleY);
    }

    /// <summary>
    /// Creates a range overlay clamped to the supplied metadata bounds.
    /// </summary>
    public static SurfaceChartDraggableRangeOverlay CreateRange(
        SurfaceMetadata metadata,
        SurfaceDataWindow dataWindow)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var clampedWindow = dataWindow.ClampTo(metadata);
        return new SurfaceChartDraggableRangeOverlay(
            clampedWindow,
            metadata.MapHorizontalCoordinate(clampedWindow.StartX),
            metadata.MapVerticalCoordinate(clampedWindow.StartY),
            metadata.MapHorizontalCoordinate(clampedWindow.EndXExclusive),
            metadata.MapVerticalCoordinate(clampedWindow.EndYExclusive));
    }

    /// <summary>
    /// Moves a range overlay by a sample-space delta while preserving its size and clamping to metadata bounds.
    /// </summary>
    public static SurfaceChartDraggableRangeOverlay DragRangeBy(
        SurfaceMetadata metadata,
        SurfaceChartDraggableRangeOverlay range,
        double sampleDeltaX,
        double sampleDeltaY)
    {
        var nextWindow = new SurfaceDataWindow(
            range.DataWindow.StartX + sampleDeltaX,
            range.DataWindow.StartY + sampleDeltaY,
            range.DataWindow.Width,
            range.DataWindow.Height);

        return CreateRange(metadata, nextWindow);
    }

    private static double ClampSampleX(SurfaceMetadata metadata, double sampleX)
    {
        if (!double.IsFinite(sampleX))
        {
            throw new ArgumentOutOfRangeException(nameof(sampleX), "Marker sample coordinates must be finite.");
        }

        return Math.Clamp(sampleX, 0d, metadata.Width - 1d);
    }

    private static double ClampSampleY(SurfaceMetadata metadata, double sampleY)
    {
        if (!double.IsFinite(sampleY))
        {
            throw new ArgumentOutOfRangeException(nameof(sampleY), "Marker sample coordinates must be finite.");
        }

        return Math.Clamp(sampleY, 0d, metadata.Height - 1d);
    }
}
