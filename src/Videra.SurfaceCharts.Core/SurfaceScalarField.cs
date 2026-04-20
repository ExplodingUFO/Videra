namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one scalar field sampled across a surface grid or tile.
/// </summary>
public sealed class SurfaceScalarField
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceScalarField"/> class.
    /// </summary>
    /// <param name="width">The field width in samples.</param>
    /// <param name="height">The field height in samples.</param>
    /// <param name="values">The field values in row-major order.</param>
    /// <param name="range">The inclusive field value range.</param>
    public SurfaceScalarField(
        int width,
        int height,
        ReadOnlyMemory<float> values,
        SurfaceValueRange range)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        var expectedValueCount = (long)width * height;
        if (expectedValueCount > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Scalar-field shape is too large.");
        }

        if (values.Length != expectedValueCount)
        {
            throw new ArgumentException("Scalar-field values must match the declared shape.", nameof(values));
        }

        Width = width;
        Height = height;
        Values = values;
        Range = range;
    }

    /// <summary>
    /// Gets the field width in samples.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the field height in samples.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the field values in row-major order.
    /// </summary>
    public ReadOnlyMemory<float> Values { get; }

    /// <summary>
    /// Gets the inclusive field value range.
    /// </summary>
    public SurfaceValueRange Range { get; }
}
