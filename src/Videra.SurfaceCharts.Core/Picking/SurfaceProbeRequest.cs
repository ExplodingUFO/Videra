namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a probe request in surface sample space.
/// </summary>
public readonly record struct SurfaceProbeRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceProbeRequest"/> struct.
    /// </summary>
    /// <param name="sampleX">The horizontal sample-space coordinate to probe.</param>
    /// <param name="sampleY">The vertical sample-space coordinate to probe.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a coordinate is not finite.</exception>
    public SurfaceProbeRequest(double sampleX, double sampleY)
    {
        if (!double.IsFinite(sampleX))
        {
            throw new ArgumentOutOfRangeException(nameof(sampleX), "Probe coordinates must be finite.");
        }

        if (!double.IsFinite(sampleY))
        {
            throw new ArgumentOutOfRangeException(nameof(sampleY), "Probe coordinates must be finite.");
        }

        SampleX = sampleX;
        SampleY = sampleY;
    }

    /// <summary>
    /// Gets the horizontal sample-space coordinate to probe.
    /// </summary>
    public double SampleX { get; }

    /// <summary>
    /// Gets the vertical sample-space coordinate to probe.
    /// </summary>
    public double SampleY { get; }
}
