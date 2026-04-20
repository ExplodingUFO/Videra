namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a rectangular tile of surface data.
/// Instances are treated as immutable snapshots once published to rendering or cache consumers.
/// </summary>
public sealed class SurfaceTile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceTile"/> class.
    /// </summary>
    /// <param name="key">The tile key.</param>
    /// <param name="width">The tile value-grid width in samples.</param>
    /// <param name="height">The tile value-grid height in samples.</param>
    /// <param name="bounds">The inclusive-exclusive source-space sample bounds covered by the tile.</param>
    /// <param name="values">The tile values laid out in row-major order.</param>
    /// <param name="valueRange">The inclusive value range for the tile.</param>
    /// <param name="statistics">Optional summary statistics for the covered source region.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="bounds"/> cannot cover the declared tile grid or when <paramref name="values"/> does not match the declared tile shape.</exception>
    public SurfaceTile(
        SurfaceTileKey key,
        int width,
        int height,
        SurfaceTileBounds bounds,
        ReadOnlyMemory<float> values,
        SurfaceValueRange valueRange,
        SurfaceTileStatistics? statistics = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        if (bounds.Width < width || bounds.Height < height)
        {
            throw new ArgumentException("Tile bounds must cover the declared tile shape.", nameof(bounds));
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
        HeightField = new SurfaceScalarField(width, height, values, valueRange);
        ColorField = null;
        Mask = SurfaceMask.Normalize(
            explicitMask: null,
            width,
            height,
            HeightField.Values.Span,
            default,
            hasColorField: false);
        Statistics = statistics ?? SurfaceTileStatistics.FromValues(HeightField.Values.Span, isExact: true, Mask);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceTile"/> class.
    /// </summary>
    /// <param name="key">The tile key.</param>
    /// <param name="bounds">The inclusive-exclusive source-space sample bounds covered by the tile.</param>
    /// <param name="heightField">The primary height field.</param>
    /// <param name="colorField">The optional independent color field.</param>
    /// <param name="statistics">Optional summary statistics for the covered source region.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="bounds"/> cannot cover the declared tile grid or when <paramref name="colorField"/> does not match the height-field shape.</exception>
    public SurfaceTile(
        SurfaceTileKey key,
        SurfaceTileBounds bounds,
        SurfaceScalarField heightField,
        SurfaceScalarField? colorField = null,
        SurfaceTileStatistics? statistics = null)
        : this(key, bounds, heightField, colorField, mask: null, statistics)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceTile"/> class.
    /// </summary>
    /// <param name="key">The tile key.</param>
    /// <param name="bounds">The inclusive-exclusive source-space sample bounds covered by the tile.</param>
    /// <param name="heightField">The primary height field.</param>
    /// <param name="colorField">The optional independent color field.</param>
    /// <param name="mask">The optional availability mask.</param>
    /// <param name="statistics">Optional summary statistics for the covered source region.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="bounds"/> cannot cover the declared tile grid or when <paramref name="colorField"/> does not match the height-field shape.</exception>
    public SurfaceTile(
        SurfaceTileKey key,
        SurfaceTileBounds bounds,
        SurfaceScalarField heightField,
        SurfaceScalarField? colorField,
        SurfaceMask? mask,
        SurfaceTileStatistics? statistics = null)
    {
        ArgumentNullException.ThrowIfNull(heightField);

        if (bounds.Width < heightField.Width || bounds.Height < heightField.Height)
        {
            throw new ArgumentException("Tile bounds must cover the declared tile shape.", nameof(bounds));
        }

        if (colorField is not null &&
            (colorField.Width != heightField.Width || colorField.Height != heightField.Height))
        {
            throw new ArgumentException("Color-field shape must match the height-field shape.", nameof(colorField));
        }

        if (mask is not null &&
            (mask.Width != heightField.Width || mask.Height != heightField.Height))
        {
            throw new ArgumentException("Mask shape must match the height-field shape.", nameof(mask));
        }

        Key = key;
        Width = heightField.Width;
        Height = heightField.Height;
        Bounds = bounds;
        HeightField = heightField;
        ColorField = colorField;
        Mask = SurfaceMask.Normalize(
            mask,
            heightField.Width,
            heightField.Height,
            heightField.Values.Span,
            colorField is null ? default : colorField.Values.Span,
            colorField is not null);
        Statistics = statistics ?? SurfaceTileStatistics.FromValues(heightField.Values.Span, isExact: true, Mask);
    }

    /// <summary>
    /// Gets the tile key.
    /// </summary>
    public SurfaceTileKey Key { get; }

    /// <summary>
    /// Gets the tile value-grid width in samples.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the tile value-grid height in samples.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the source-space sample bounds covered by the tile.
    /// </summary>
    public SurfaceTileBounds Bounds { get; }

    /// <summary>
    /// Gets the tile values in row-major order.
    /// Callers should replace the tile instance rather than mutating the underlying memory in place.
    /// </summary>
    public ReadOnlyMemory<float> Values => HeightField.Values;

    /// <summary>
    /// Gets the inclusive value range covered by the tile.
    /// </summary>
    public SurfaceValueRange ValueRange => HeightField.Range;

    /// <summary>
    /// Gets the primary height field.
    /// Callers should replace the tile instance rather than mutating the underlying memory in place.
    /// </summary>
    public SurfaceScalarField HeightField { get; }

    /// <summary>
    /// Gets the optional independent color field.
    /// Callers should replace the tile instance rather than mutating the underlying memory in place.
    /// </summary>
    public SurfaceScalarField? ColorField { get; }

    /// <summary>
    /// Gets the optional availability mask where <see langword="true"/> means the sample is present.
    /// </summary>
    public SurfaceMask? Mask { get; }

    /// <summary>
    /// Gets summary statistics for the covered source region.
    /// </summary>
    public SurfaceTileStatistics Statistics { get; }
}
