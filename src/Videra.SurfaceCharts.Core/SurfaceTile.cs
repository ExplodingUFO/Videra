namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a rectangular tile of surface data.
/// </summary>
public sealed class SurfaceTile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceTile"/> class.
    /// </summary>
    /// <param name="key">The tile key.</param>
    /// <param name="width">The tile width in samples.</param>
    /// <param name="height">The tile height in samples.</param>
    /// <param name="bounds">The inclusive-exclusive sample bounds covered by the tile.</param>
    /// <param name="values">The tile values laid out in row-major order.</param>
    /// <param name="valueRange">The inclusive value range for the tile.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="bounds"/> or <paramref name="values"/> does not match the declared tile shape.</exception>
    public SurfaceTile(
        SurfaceTileKey key,
        int width,
        int height,
        SurfaceTileBounds bounds,
        ReadOnlyMemory<float> values,
        SurfaceValueRange valueRange)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        if (bounds.Width != width || bounds.Height != height)
        {
            throw new ArgumentException("Tile bounds must match the declared tile shape.", nameof(bounds));
        }

        var expectedValueCount = (long)width * height;
        if (expectedValueCount > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Tile shape is too large.");
        }

        if (values.Length != expectedValueCount)
        {
            throw new ArgumentException("Tile values must match the declared tile shape.", nameof(values));
        }

        Key = key;
        Width = width;
        Height = height;
        Bounds = bounds;
        Values = values;
        ValueRange = valueRange;
    }

    /// <summary>
    /// Gets the tile key.
    /// </summary>
    public SurfaceTileKey Key { get; }

    /// <summary>
    /// Gets the tile width in samples.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the tile height in samples.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the sample bounds covered by the tile.
    /// </summary>
    public SurfaceTileBounds Bounds { get; }

    /// <summary>
    /// Gets the tile values in row-major order.
    /// </summary>
    public ReadOnlyMemory<float> Values { get; }

    /// <summary>
    /// Gets the inclusive value range covered by the tile.
    /// </summary>
    public SurfaceValueRange ValueRange { get; }
}
