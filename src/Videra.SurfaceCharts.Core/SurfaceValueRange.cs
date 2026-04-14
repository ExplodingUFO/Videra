namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents an inclusive value range for surface-chart data.
/// </summary>
public readonly record struct SurfaceValueRange
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceValueRange"/> struct.
    /// </summary>
    /// <param name="minimum">The inclusive range minimum.</param>
    /// <param name="maximum">The inclusive range maximum.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="maximum"/> is less than <paramref name="minimum"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minimum"/> or <paramref name="maximum"/> is not finite.</exception>
    public SurfaceValueRange(double minimum, double maximum)
    {
        if (!double.IsFinite(minimum))
        {
            throw new ArgumentOutOfRangeException(nameof(minimum), "Value range minimum must be finite.");
        }

        if (!double.IsFinite(maximum))
        {
            throw new ArgumentOutOfRangeException(nameof(maximum), "Value range maximum must be finite.");
        }

        if (maximum < minimum)
        {
            throw new ArgumentException("Value range maximum must be greater than or equal to value range minimum.", nameof(maximum));
        }

        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    /// Gets the inclusive minimum value.
    /// </summary>
    public double Minimum { get; }

    /// <summary>
    /// Gets the inclusive maximum value.
    /// </summary>
    public double Maximum { get; }

    /// <summary>
    /// Gets the range span.
    /// </summary>
    public double Span => Maximum - Minimum;
}
