namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents an ordered set of ARGB colors used by a surface color map.
/// </summary>
public sealed class SurfaceColorMapPalette
{
    private readonly uint[] colors;

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceColorMapPalette"/> class.
    /// </summary>
    /// <param name="colors">The ordered ARGB palette colors.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="colors"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when fewer than two colors are supplied.</exception>
    public SurfaceColorMapPalette(params uint[] colors)
    {
        ArgumentNullException.ThrowIfNull(colors);

        if (colors.Length < 2)
        {
            throw new ArgumentException("A surface color map palette must contain at least two colors.", nameof(colors));
        }

        this.colors = (uint[])colors.Clone();
    }

    /// <summary>
    /// Gets the number of colors in the palette.
    /// </summary>
    public int Count => colors.Length;

    /// <summary>
    /// Gets a palette color by zero-based index.
    /// </summary>
    /// <param name="index">The zero-based palette index.</param>
    /// <returns>The ARGB color at the requested index.</returns>
    public uint this[int index] => colors[index];
}
