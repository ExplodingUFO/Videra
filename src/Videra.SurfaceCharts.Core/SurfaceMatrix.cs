namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a dense matrix of surface samples in row-major order.
/// </summary>
public sealed class SurfaceMatrix
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceMatrix"/> class.
    /// </summary>
    /// <param name="metadata">The matrix metadata.</param>
    /// <param name="values">The matrix values in row-major order.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> does not match the metadata dimensions.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the matrix shape exceeds the supported addressable range.</exception>
    public SurfaceMatrix(SurfaceMetadata metadata, ReadOnlyMemory<float> values)
        : this(metadata, CreateHeightField(metadata, values), colorField: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceMatrix"/> class.
    /// </summary>
    /// <param name="metadata">The matrix metadata.</param>
    /// <param name="heightField">The primary height field.</param>
    /// <param name="colorField">The optional independent color field.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> or <paramref name="heightField"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when a field shape does not match the metadata dimensions or when the height-field range differs from the metadata value range.</exception>
    public SurfaceMatrix(
        SurfaceMetadata metadata,
        SurfaceScalarField heightField,
        SurfaceScalarField? colorField)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(heightField);

        if (metadata.SampleCount > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(metadata), "Matrix dimensions exceed the supported addressable range.");
        }

        ValidateFieldShape(heightField, metadata.Width, metadata.Height, nameof(heightField));
        if (heightField.Range != metadata.ValueRange)
        {
            throw new ArgumentException("Height-field range must match the metadata value range.", nameof(heightField));
        }

        if (colorField is not null)
        {
            ValidateFieldShape(colorField, metadata.Width, metadata.Height, nameof(colorField));
        }

        Metadata = metadata;
        HeightField = heightField;
        ColorField = colorField;
    }

    /// <summary>
    /// Gets the matrix metadata.
    /// </summary>
    public SurfaceMetadata Metadata { get; }

    /// <summary>
    /// Gets the primary height field.
    /// </summary>
    public SurfaceScalarField HeightField { get; }

    /// <summary>
    /// Gets the optional independent color field.
    /// </summary>
    public SurfaceScalarField? ColorField { get; }

    /// <summary>
    /// Gets the matrix values in row-major order.
    /// </summary>
    public ReadOnlyMemory<float> Values => HeightField.Values;

    private static SurfaceScalarField CreateHeightField(SurfaceMetadata metadata, ReadOnlyMemory<float> values)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        return new SurfaceScalarField(metadata.Width, metadata.Height, values, metadata.ValueRange);
    }

    private static void ValidateFieldShape(SurfaceScalarField field, int expectedWidth, int expectedHeight, string paramName)
    {
        if (field.Width != expectedWidth || field.Height != expectedHeight)
        {
            throw new ArgumentException("Scalar-field shape must match the matrix metadata dimensions.", paramName);
        }
    }
}
