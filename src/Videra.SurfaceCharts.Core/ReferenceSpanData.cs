namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes a reference region between two axis values with a semi-transparent fill.
/// </summary>
public sealed class ReferenceSpanData
{
    public ReferenceSpanData(
        ReferenceAxis axis,
        double start,
        double end,
        uint color = 0x40FF0000u,
        uint? borderColor = null,
        double borderWidth = 1d,
        string? label = null)
    {
        Axis = axis;
        Start = start;
        End = end;
        Color = color;
        BorderColor = borderColor;
        BorderWidth = borderWidth;
        Label = label;
    }

    /// <summary>
    /// Gets which axis the span is associated with.
    /// </summary>
    public ReferenceAxis Axis { get; }

    /// <summary>
    /// Gets the start axis value.
    /// </summary>
    public double Start { get; }

    /// <summary>
    /// Gets the end axis value.
    /// </summary>
    public double End { get; }

    /// <summary>
    /// Gets the ARGB fill color (typically semi-transparent).
    /// </summary>
    public uint Color { get; }

    /// <summary>
    /// Gets the optional ARGB border color. When null, no border is drawn.
    /// </summary>
    public uint? BorderColor { get; }

    /// <summary>
    /// Gets the border width in device-independent pixels.
    /// </summary>
    public double BorderWidth { get; }

    /// <summary>
    /// Gets the optional label rendered inside the span.
    /// </summary>
    public string? Label { get; }
}
