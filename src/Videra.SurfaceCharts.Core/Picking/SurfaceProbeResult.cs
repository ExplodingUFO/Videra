namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents the resolved sample coordinate and value for a surface probe.
/// </summary>
public readonly record struct SurfaceProbeResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceProbeResult"/> struct.
    /// </summary>
    /// <param name="sampleX">The resolved horizontal sample-space coordinate.</param>
    /// <param name="sampleY">The resolved vertical sample-space coordinate.</param>
    /// <param name="value">The resolved finite surface value.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a coordinate or value is not finite.</exception>
    public SurfaceProbeResult(double sampleX, double sampleY, double value)
    {
        if (!double.IsFinite(sampleX))
        {
            throw new ArgumentOutOfRangeException(nameof(sampleX), "Probe coordinates must be finite.");
        }

        if (!double.IsFinite(sampleY))
        {
            throw new ArgumentOutOfRangeException(nameof(sampleY), "Probe coordinates must be finite.");
        }

        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Probe values must be finite.");
        }

        SampleX = sampleX;
        SampleY = sampleY;
        Value = value;
    }

    /// <summary>
    /// Gets the resolved horizontal sample-space coordinate.
    /// </summary>
    public double SampleX { get; }

    /// <summary>
    /// Gets the resolved vertical sample-space coordinate.
    /// </summary>
    public double SampleY { get; }

    /// <summary>
    /// Gets the resolved finite surface value.
    /// </summary>
    public double Value { get; }
}
