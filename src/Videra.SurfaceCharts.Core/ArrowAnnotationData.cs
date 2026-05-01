using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes an arrow annotation with a start and end point in 3D data space.
/// </summary>
public sealed class ArrowAnnotationData
{
    public ArrowAnnotationData(
        Vector3 start,
        Vector3 end,
        uint color = 0xFFFFFFFFu,
        double lineWidth = 1.5d,
        double headLength = 10d,
        double headWidth = 6d,
        string? label = null)
    {
        Start = start;
        End = end;
        Color = color;
        LineWidth = lineWidth;
        HeadLength = headLength;
        HeadWidth = headWidth;
        Label = label;
    }

    /// <summary>
    /// Gets the arrow start position in data coordinates (X, Y, Z).
    /// </summary>
    public Vector3 Start { get; }

    /// <summary>
    /// Gets the arrow end position (tip) in data coordinates (X, Y, Z).
    /// </summary>
    public Vector3 End { get; }

    /// <summary>
    /// Gets the ARGB color for the arrow.
    /// </summary>
    public uint Color { get; }

    /// <summary>
    /// Gets the line width in device-independent pixels.
    /// </summary>
    public double LineWidth { get; }

    /// <summary>
    /// Gets the arrowhead length in device-independent pixels.
    /// </summary>
    public double HeadLength { get; }

    /// <summary>
    /// Gets the arrowhead width in device-independent pixels.
    /// </summary>
    public double HeadWidth { get; }

    /// <summary>
    /// Gets the optional label drawn near the arrow midpoint.
    /// </summary>
    public string? Label { get; }
}
