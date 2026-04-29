using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a single contour line segment in normalized 3D coordinates.
/// </summary>
public readonly struct ContourSegment
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContourSegment"/> struct.
    /// </summary>
    public ContourSegment(Vector3 start, Vector3 end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Gets the segment start point in normalized coordinates.
    /// </summary>
    public Vector3 Start { get; }

    /// <summary>
    /// Gets the segment end point in normalized coordinates.
    /// </summary>
    public Vector3 End { get; }
}
