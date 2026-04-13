namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes a single axis in surface-chart space.
/// </summary>
public sealed class SurfaceAxisDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceAxisDescriptor"/> class.
    /// </summary>
    /// <param name="label">The axis label.</param>
    /// <param name="unit">The optional unit for the axis.</param>
    /// <param name="minimum">The inclusive axis minimum.</param>
    /// <param name="maximum">The inclusive axis maximum.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="label"/> is blank or <paramref name="maximum"/> is less than <paramref name="minimum"/>.</exception>
    public SurfaceAxisDescriptor(string label, string? unit, double minimum, double maximum)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        if (maximum < minimum)
        {
            throw new ArgumentException("Axis maximum must be greater than or equal to axis minimum.", nameof(maximum));
        }

        Label = label;
        Unit = unit;
        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    /// Gets the axis label.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the optional axis unit.
    /// </summary>
    public string? Unit { get; }

    /// <summary>
    /// Gets the inclusive minimum axis value.
    /// </summary>
    public double Minimum { get; }

    /// <summary>
    /// Gets the inclusive maximum axis value.
    /// </summary>
    public double Maximum { get; }

    /// <summary>
    /// Gets the span of the axis.
    /// </summary>
    public double Span => Maximum - Minimum;
}

