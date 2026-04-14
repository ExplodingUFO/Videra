namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes the inclusive-exclusive sample bounds covered by a surface tile.
/// </summary>
public readonly record struct SurfaceTileBounds
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceTileBounds"/> struct.
    /// </summary>
    /// <param name="startX">The starting horizontal sample index.</param>
    /// <param name="startY">The starting vertical sample index.</param>
    /// <param name="width">The tile width in samples.</param>
    /// <param name="height">The tile height in samples.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startX"/> or <paramref name="startY"/> is negative, when <paramref name="width"/> or <paramref name="height"/> is not positive, or when the exclusive end coordinate would exceed the <see cref="int"/> range.</exception>
    public SurfaceTileBounds(int startX, int startY, int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startX);
        ArgumentOutOfRangeException.ThrowIfNegative(startY);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        if ((long)startX + width > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(startX), "Tile bounds exceed the supported sample range.");
        }

        if ((long)startY + height > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(startY), "Tile bounds exceed the supported sample range.");
        }

        StartX = startX;
        StartY = startY;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets the starting horizontal sample index.
    /// </summary>
    public int StartX { get; }

    /// <summary>
    /// Gets the starting vertical sample index.
    /// </summary>
    public int StartY { get; }

    /// <summary>
    /// Gets the tile width in samples.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the tile height in samples.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the exclusive horizontal end sample index.
    /// </summary>
    public int EndXExclusive => checked(StartX + Width);

    /// <summary>
    /// Gets the exclusive vertical end sample index.
    /// </summary>
    public int EndYExclusive => checked(StartY + Height);
}
