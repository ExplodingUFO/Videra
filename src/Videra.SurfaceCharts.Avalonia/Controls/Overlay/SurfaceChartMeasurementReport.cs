using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes an immutable host-owned chart measurement report.
/// </summary>
public sealed class SurfaceChartMeasurementReport
{
    private SurfaceChartMeasurementReport(
        SurfaceChartMeasurementKind kind,
        SurfaceChartAnnotationAnchor start,
        SurfaceChartAnnotationAnchor end,
        SurfaceDataWindow? dataWindow)
    {
        Kind = kind;
        Start = start;
        End = end;
        DataWindow = dataWindow;
        SampleDeltaX = end.SampleX - start.SampleX;
        SampleDeltaY = end.SampleY - start.SampleY;
        AxisDeltaX = end.AxisX - start.AxisX;
        AxisDeltaY = end.AxisY - start.AxisY;
        ValueDelta = start.Value is double startValue && end.Value is double endValue
            ? endValue - startValue
            : null;
        SampleDistance = Math.Sqrt((SampleDeltaX * SampleDeltaX) + (SampleDeltaY * SampleDeltaY));
        AxisDistance = Math.Sqrt((AxisDeltaX * AxisDeltaX) + (AxisDeltaY * AxisDeltaY));
    }

    /// <summary>
    /// Gets the measurement kind.
    /// </summary>
    public SurfaceChartMeasurementKind Kind { get; }

    /// <summary>
    /// Gets the measurement start anchor.
    /// </summary>
    public SurfaceChartAnnotationAnchor Start { get; }

    /// <summary>
    /// Gets the measurement end anchor.
    /// </summary>
    public SurfaceChartAnnotationAnchor End { get; }

    /// <summary>
    /// Gets the horizontal sample-space delta.
    /// </summary>
    public double SampleDeltaX { get; }

    /// <summary>
    /// Gets the vertical sample-space delta.
    /// </summary>
    public double SampleDeltaY { get; }

    /// <summary>
    /// Gets the horizontal axis-space delta.
    /// </summary>
    public double AxisDeltaX { get; }

    /// <summary>
    /// Gets the vertical axis-space delta.
    /// </summary>
    public double AxisDeltaY { get; }

    /// <summary>
    /// Gets the value delta when both anchors carry values.
    /// </summary>
    public double? ValueDelta { get; }

    /// <summary>
    /// Gets the Euclidean sample-space distance.
    /// </summary>
    public double SampleDistance { get; }

    /// <summary>
    /// Gets the Euclidean axis-space distance.
    /// </summary>
    public double AxisDistance { get; }

    /// <summary>
    /// Gets the selected data window for rectangle measurements.
    /// </summary>
    public SurfaceDataWindow? DataWindow { get; }

    /// <summary>
    /// Creates a point-to-point measurement between two annotation anchors.
    /// </summary>
    public static SurfaceChartMeasurementReport Between(
        SurfaceChartAnnotationAnchor start,
        SurfaceChartAnnotationAnchor end)
    {
        return new SurfaceChartMeasurementReport(SurfaceChartMeasurementKind.PointToPoint, start, end, dataWindow: null);
    }

    /// <summary>
    /// Creates a rectangle extent measurement from a selection report.
    /// </summary>
    public static SurfaceChartMeasurementReport FromSelection(SurfaceChartSelectionReport selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return new SurfaceChartMeasurementReport(
            SurfaceChartMeasurementKind.RectangleExtent,
            SurfaceChartAnnotationAnchor.FromSelectionStart(selection),
            SurfaceChartAnnotationAnchor.FromSelectionEnd(selection),
            selection.DataWindow);
    }
}

/// <summary>
/// Describes the measurement report shape.
/// </summary>
public enum SurfaceChartMeasurementKind
{
    /// <summary>A direct measurement between two anchors.</summary>
    PointToPoint,

    /// <summary>A rectangle extent measurement derived from a selection report.</summary>
    RectangleExtent,
}
