using Avalonia;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes an immutable host-owned chart annotation anchor.
/// </summary>
public readonly record struct SurfaceChartAnnotationAnchor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceChartAnnotationAnchor"/> struct.
    /// </summary>
    public SurfaceChartAnnotationAnchor(
        SurfaceChartAnnotationAnchorKind kind,
        double sampleX,
        double sampleY,
        double axisX,
        double axisY,
        double? value = null,
        Point? screenPosition = null,
        string? label = null)
    {
        ValidateFinite(sampleX, nameof(sampleX));
        ValidateFinite(sampleY, nameof(sampleY));
        ValidateFinite(axisX, nameof(axisX));
        ValidateFinite(axisY, nameof(axisY));
        if (value is double anchorValue)
        {
            ValidateFinite(anchorValue, nameof(value));
        }

        Kind = kind;
        SampleX = sampleX;
        SampleY = sampleY;
        AxisX = axisX;
        AxisY = axisY;
        Value = value;
        ScreenPosition = screenPosition;
        Label = string.IsNullOrWhiteSpace(label) ? null : label;
    }

    /// <summary>
    /// Gets the anchor source kind.
    /// </summary>
    public SurfaceChartAnnotationAnchorKind Kind { get; }

    /// <summary>
    /// Gets the horizontal sample-space coordinate.
    /// </summary>
    public double SampleX { get; }

    /// <summary>
    /// Gets the vertical sample-space coordinate.
    /// </summary>
    public double SampleY { get; }

    /// <summary>
    /// Gets the horizontal axis-space coordinate.
    /// </summary>
    public double AxisX { get; }

    /// <summary>
    /// Gets the vertical axis-space coordinate.
    /// </summary>
    public double AxisY { get; }

    /// <summary>
    /// Gets the resolved value for probe anchors.
    /// </summary>
    public double? Value { get; }

    /// <summary>
    /// Gets the originating screen position when available.
    /// </summary>
    public Point? ScreenPosition { get; }

    /// <summary>
    /// Gets the optional host-authored label.
    /// </summary>
    public string? Label { get; }

    /// <summary>
    /// Creates an annotation anchor from a resolved probe.
    /// </summary>
    public static SurfaceChartAnnotationAnchor FromProbe(SurfaceProbeInfo probe, string? label = null)
    {
        return new SurfaceChartAnnotationAnchor(
            SurfaceChartAnnotationAnchorKind.Probe,
            probe.SampleX,
            probe.SampleY,
            probe.AxisX,
            probe.AxisY,
            probe.Value,
            screenPosition: null,
            label);
    }

    /// <summary>
    /// Creates an annotation anchor from the start of a selection report.
    /// </summary>
    public static SurfaceChartAnnotationAnchor FromSelectionStart(SurfaceChartSelectionReport selection, string? label = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return new SurfaceChartAnnotationAnchor(
            SurfaceChartAnnotationAnchorKind.SelectionStart,
            selection.SampleStartX,
            selection.SampleStartY,
            selection.AxisStartX,
            selection.AxisStartY,
            value: null,
            selection.ScreenStart,
            label);
    }

    /// <summary>
    /// Creates an annotation anchor from the end of a selection report.
    /// </summary>
    public static SurfaceChartAnnotationAnchor FromSelectionEnd(SurfaceChartSelectionReport selection, string? label = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return new SurfaceChartAnnotationAnchor(
            SurfaceChartAnnotationAnchorKind.SelectionEnd,
            selection.SampleEndX,
            selection.SampleEndY,
            selection.AxisEndX,
            selection.AxisEndY,
            value: null,
            selection.ScreenEnd,
            label);
    }

    private static void ValidateFinite(double value, string paramName)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(paramName, "Annotation anchor coordinates must be finite.");
        }
    }
}

/// <summary>
/// Describes where an annotation anchor was derived from.
/// </summary>
public enum SurfaceChartAnnotationAnchorKind
{
    /// <summary>The anchor was derived from a resolved probe.</summary>
    Probe,

    /// <summary>The anchor was derived from the selection start.</summary>
    SelectionStart,

    /// <summary>The anchor was derived from the selection end.</summary>
    SelectionEnd,
}
