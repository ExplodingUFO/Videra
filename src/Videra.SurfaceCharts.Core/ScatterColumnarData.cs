namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one batch of scatter points stored as contiguous coordinate columns.
/// </summary>
public readonly record struct ScatterColumnarData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScatterColumnarData"/> struct.
    /// </summary>
    /// <param name="x">The horizontal-axis coordinates.</param>
    /// <param name="y">The value-axis coordinates.</param>
    /// <param name="z">The depth-axis coordinates.</param>
    /// <param name="size">The optional per-point marker sizes.</param>
    /// <param name="color">The optional per-point ARGB colors.</param>
    public ScatterColumnarData(
        ReadOnlyMemory<float> x,
        ReadOnlyMemory<float> y,
        ReadOnlyMemory<float> z,
        ReadOnlyMemory<float> size = default,
        ReadOnlyMemory<uint> color = default)
    {
        if (x.Length == 0)
        {
            throw new ArgumentException("Columnar scatter data must include at least one point.", nameof(x));
        }

        if (y.Length != x.Length)
        {
            throw new ArgumentException("Y column length must match X column length.", nameof(y));
        }

        if (z.Length != x.Length)
        {
            throw new ArgumentException("Z column length must match X column length.", nameof(z));
        }

        if (!size.IsEmpty && size.Length != x.Length)
        {
            throw new ArgumentException("Size column length must match X column length.", nameof(size));
        }

        if (!color.IsEmpty && color.Length != x.Length)
        {
            throw new ArgumentException("Color column length must match X column length.", nameof(color));
        }

        X = x;
        Y = y;
        Z = z;
        Size = size;
        Color = color;
        Count = x.Length;
    }

    /// <summary>
    /// Gets the horizontal-axis coordinates.
    /// </summary>
    public ReadOnlyMemory<float> X { get; }

    /// <summary>
    /// Gets the value-axis coordinates.
    /// </summary>
    public ReadOnlyMemory<float> Y { get; }

    /// <summary>
    /// Gets the depth-axis coordinates.
    /// </summary>
    public ReadOnlyMemory<float> Z { get; }

    /// <summary>
    /// Gets the optional per-point marker sizes.
    /// </summary>
    public ReadOnlyMemory<float> Size { get; }

    /// <summary>
    /// Gets the optional per-point ARGB colors.
    /// </summary>
    public ReadOnlyMemory<uint> Color { get; }

    /// <summary>
    /// Gets the point count represented by the columns.
    /// </summary>
    public int Count { get; }
}
