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
    {
        ArgumentNullException.ThrowIfNull(metadata);

        if (metadata.SampleCount > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(metadata), "Matrix dimensions exceed the supported addressable range.");
        }

        if (values.Length != metadata.SampleCount)
        {
            throw new ArgumentException("Matrix values must match the metadata dimensions.", nameof(values));
        }

        Metadata = metadata;
        Values = values;
    }

    /// <summary>
    /// Gets the matrix metadata.
    /// </summary>
    public SurfaceMetadata Metadata { get; }

    /// <summary>
    /// Gets the matrix values in row-major order.
    /// </summary>
    public ReadOnlyMemory<float> Values { get; }
}
