using Avalonia;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes a host-owned chart selection report in screen, sample, and axis space.
/// </summary>
public sealed class SurfaceChartSelectionReport
{
    internal SurfaceChartSelectionReport(
        SurfaceChartSelectionKind kind,
        Point screenStart,
        Point screenEnd,
        double sampleStartX,
        double sampleStartY,
        double sampleEndX,
        double sampleEndY,
        double axisStartX,
        double axisStartY,
        double axisEndX,
        double axisEndY,
        SurfaceDataWindow? dataWindow)
    {
        Kind = kind;
        ScreenStart = screenStart;
        ScreenEnd = screenEnd;
        ScreenRect = CreateRect(screenStart, screenEnd);
        SampleStartX = sampleStartX;
        SampleStartY = sampleStartY;
        SampleEndX = sampleEndX;
        SampleEndY = sampleEndY;
        AxisStartX = axisStartX;
        AxisStartY = axisStartY;
        AxisEndX = axisEndX;
        AxisEndY = axisEndY;
        DataWindow = dataWindow;
    }

    /// <summary>
    /// Gets whether the report represents a click or rectangle selection.
    /// </summary>
    public SurfaceChartSelectionKind Kind { get; }

    /// <summary>
    /// Gets the selection start in screen coordinates.
    /// </summary>
    public Point ScreenStart { get; }

    /// <summary>
    /// Gets the selection end in screen coordinates.
    /// </summary>
    public Point ScreenEnd { get; }

    /// <summary>
    /// Gets the normalized screen rectangle between <see cref="ScreenStart"/> and <see cref="ScreenEnd"/>.
    /// </summary>
    public Rect ScreenRect { get; }

    /// <summary>
    /// Gets the selection start horizontal sample coordinate.
    /// </summary>
    public double SampleStartX { get; }

    /// <summary>
    /// Gets the selection start vertical sample coordinate.
    /// </summary>
    public double SampleStartY { get; }

    /// <summary>
    /// Gets the selection end horizontal sample coordinate.
    /// </summary>
    public double SampleEndX { get; }

    /// <summary>
    /// Gets the selection end vertical sample coordinate.
    /// </summary>
    public double SampleEndY { get; }

    /// <summary>
    /// Gets the selection start horizontal axis coordinate.
    /// </summary>
    public double AxisStartX { get; }

    /// <summary>
    /// Gets the selection start vertical axis coordinate.
    /// </summary>
    public double AxisStartY { get; }

    /// <summary>
    /// Gets the selection end horizontal axis coordinate.
    /// </summary>
    public double AxisEndX { get; }

    /// <summary>
    /// Gets the selection end vertical axis coordinate.
    /// </summary>
    public double AxisEndY { get; }

    /// <summary>
    /// Gets the selected sample-space data window for rectangle selections.
    /// </summary>
    public SurfaceDataWindow? DataWindow { get; }

    private static Rect CreateRect(Point start, Point end)
    {
        return new Rect(
            Math.Min(start.X, end.X),
            Math.Min(start.Y, end.Y),
            Math.Abs(end.X - start.X),
            Math.Abs(end.Y - start.Y));
    }
}

/// <summary>
/// Provides event data for a host-owned chart selection report.
/// </summary>
public sealed class SurfaceChartSelectionReportedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceChartSelectionReportedEventArgs"/> class.
    /// </summary>
    public SurfaceChartSelectionReportedEventArgs(SurfaceChartSelectionReport report)
    {
        Report = report ?? throw new ArgumentNullException(nameof(report));
    }

    /// <summary>
    /// Gets the immutable selection report.
    /// </summary>
    public SurfaceChartSelectionReport Report { get; }
}

/// <summary>
/// Describes the shape of a host-owned chart selection report.
/// </summary>
public enum SurfaceChartSelectionKind
{
    /// <summary>A single pointer location was reported.</summary>
    Click,

    /// <summary>A rectangular sample-space region was reported.</summary>
    Rectangle,
}
