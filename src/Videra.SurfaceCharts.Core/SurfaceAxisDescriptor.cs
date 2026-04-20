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
        : this(label, unit, minimum, maximum, SurfaceAxisScaleKind.Linear)
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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minimum"/> or <paramref name="maximum"/> is not finite.</exception>
    public SurfaceAxisDescriptor(string label, string? unit, double minimum, double maximum, SurfaceAxisScaleKind scaleKind)
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
            throw new ArgumentException(
                "Logarithmic axis scaling is reserved until raw axis values and display-space coordinates are separated.",
                nameof(scaleKind));
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
    /// Gets the span of the axis.
    /// </summary>
    public double Span => Maximum - Minimum;
}
