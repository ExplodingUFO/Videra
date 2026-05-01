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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minimum"/> or <paramref name="maximum"/> is not finite.</exception>
    public SurfaceAxisDescriptor(string label, string? unit, double minimum, double maximum)
        : this(label, unit, minimum, maximum, SurfaceAxisScaleKind.Linear, isInverted: false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceAxisDescriptor"/> class.
    /// </summary>
    /// <param name="label">The axis label.</param>
    /// <param name="unit">The optional unit for the axis.</param>
    /// <param name="minimum">The inclusive axis minimum.</param>
    /// <param name="maximum">The inclusive axis maximum.</param>
    /// <param name="scaleKind">The axis scale semantics.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="label"/> is blank or <paramref name="maximum"/> is less than <paramref name="minimum"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minimum"/> or <paramref name="maximum"/> is not finite, or when <paramref name="scaleKind"/> is Log and <paramref name="minimum"/> or <paramref name="maximum"/> is not positive.</exception>
    public SurfaceAxisDescriptor(string label, string? unit, double minimum, double maximum, SurfaceAxisScaleKind scaleKind)
        : this(label, unit, minimum, maximum, scaleKind, isInverted: false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceAxisDescriptor"/> class.
    /// </summary>
    /// <param name="label">The axis label.</param>
    /// <param name="unit">The optional unit for the axis.</param>
    /// <param name="minimum">The inclusive axis minimum.</param>
    /// <param name="maximum">The inclusive axis maximum.</param>
    /// <param name="scaleKind">The axis scale semantics.</param>
    /// <param name="isInverted">Whether the axis direction is inverted (maximum at origin, minimum at far end).</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="label"/> is blank or <paramref name="maximum"/> is less than <paramref name="minimum"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minimum"/> or <paramref name="maximum"/> is not finite, or when <paramref name="scaleKind"/> is Log and <paramref name="minimum"/> or <paramref name="maximum"/> is not positive.</exception>
    public SurfaceAxisDescriptor(string label, string? unit, double minimum, double maximum, SurfaceAxisScaleKind scaleKind, bool isInverted)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        if (!double.IsFinite(minimum))
        {
            throw new ArgumentOutOfRangeException(nameof(minimum), "Axis minimum must be finite.");
        }

        if (!double.IsFinite(maximum))
        {
            throw new ArgumentOutOfRangeException(nameof(maximum), "Axis maximum must be finite.");
        }

        if (scaleKind == SurfaceAxisScaleKind.Log)
        {
            if (minimum <= 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimum),
                    "Logarithmic axis minimum must be positive (greater than zero).");
            }

            if (maximum <= 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximum),
                    "Logarithmic axis maximum must be positive (greater than zero).");
            }
        }

        if (maximum < minimum)
        {
            throw new ArgumentException("Axis maximum must be greater than or equal to axis minimum.", nameof(maximum));
        }

        Label = label;
        Unit = unit;
        Minimum = minimum;
        Maximum = maximum;
        ScaleKind = scaleKind;
        IsInverted = isInverted;
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
    /// Gets the axis scale semantics.
    /// </summary>
    public SurfaceAxisScaleKind ScaleKind { get; }

    /// <summary>
    /// Gets the inclusive minimum axis value.
    /// </summary>
    public double Minimum { get; }

    /// <summary>
    /// Gets the inclusive maximum axis value.
    /// </summary>
    public double Maximum { get; }

    /// <summary>
    /// Gets a value indicating whether the axis direction is inverted.
    /// </summary>
    public bool IsInverted { get; }

    /// <summary>
    /// Gets the span of the axis.
    /// </summary>
    public double Span => Maximum - Minimum;
}
