namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a resolved surface probe in both sample space and axis space.
/// </summary>
public readonly record struct SurfaceProbeInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceProbeInfo"/> struct.
    /// </summary>
    /// <param name="sampleX">The resolved horizontal sample-space coordinate.</param>
    /// <param name="sampleY">The resolved vertical sample-space coordinate.</param>
    /// <param name="axisX">The resolved horizontal axis-space coordinate.</param>
    /// <param name="axisY">The resolved vertical axis-space coordinate.</param>
    /// <param name="value">The resolved finite surface value.</param>
    /// <param name="isApproximate">Whether the resolved value comes from a coarse tile.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a coordinate or value is not finite.</exception>
    public SurfaceProbeInfo(
        double sampleX,
        double sampleY,
        double axisX,
        double axisY,
        double value,
        bool isApproximate)
    {
        if (!double.IsFinite(sampleX))
        {
            throw new ArgumentOutOfRangeException(nameof(sampleX), "Probe coordinates must be finite.");
        }

        if (!double.IsFinite(sampleY))
        {
            throw new ArgumentOutOfRangeException(nameof(sampleY), "Probe coordinates must be finite.");
        }

        if (!double.IsFinite(axisX))
        {
            throw new ArgumentOutOfRangeException(nameof(axisX), "Probe axis coordinates must be finite.");
        }

        if (!double.IsFinite(axisY))
        {
            throw new ArgumentOutOfRangeException(nameof(axisY), "Probe axis coordinates must be finite.");
        }

        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Probe values must be finite.");
        }

        SampleX = sampleX;
        SampleY = sampleY;
        AxisX = axisX;
        AxisY = axisY;
        Value = value;
        IsApproximate = isApproximate;
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
    /// Gets the resolved horizontal axis-space coordinate.
    /// </summary>
    public double AxisX { get; }

    /// <summary>
    /// Gets the resolved vertical axis-space coordinate.
    /// </summary>
    public double AxisY { get; }

    /// <summary>
    /// Gets the resolved finite surface value.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets a value indicating whether the probe result comes from a coarse tile approximation.
    /// </summary>
    public bool IsApproximate { get; }
}
