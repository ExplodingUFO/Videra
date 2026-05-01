namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes a reference line at a fixed axis value.
/// </summary>
public sealed class ReferenceLineData
{
    public ReferenceLineData(
        ReferenceAxis axis,
        double value,
        uint color = 0xFFFF0000u,
        double lineWidth = 1.5d,
        string? label = null)
    {
        Axis = axis;
        Value = value;
        Color = color;
        LineWidth = lineWidth;
        Label = label;
    }

    /// <summary>
    /// Gets which axis the reference line is parallel to.
    /// </summary>
    public ReferenceAxis Axis { get; }

    /// <summary>
    /// Gets the fixed axis value where the line is drawn.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets the ARGB color of the line.
    /// </summary>
    public uint Color { get; }

    /// <summary>
    /// Gets the line width in device-independent pixels.
    /// </summary>
    public double LineWidth { get; }

    /// <summary>
    /// Gets the optional label rendered near the line.
    /// </summary>
    public string? Label { get; }
}

/// <summary>
/// Identifies which axis a reference line or span is associated with.
/// </summary>
public enum ReferenceAxis
{
    /// <summary>Horizontal axis (X). The line is vertical.</summary>
    X,

    /// <summary>Value axis (Y). The line is horizontal.</summary>
    Y,

    /// <summary>Depth axis (Z). The line spans the Z range.</summary>
    Z,
}
