namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Identifies a tile inside a surface pyramid.
/// </summary>
public readonly record struct SurfaceTileKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceTileKey"/> struct.
    /// </summary>
    /// <param name="levelX">The horizontal pyramid level, where 0 is the overview level.</param>
    /// <param name="levelY">The vertical pyramid level, where 0 is the overview level.</param>
    /// <param name="tileX">The tile column at the given level.</param>
    /// <param name="tileY">The tile row at the given level.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any coordinate is negative.</exception>
    public SurfaceTileKey(int levelX, int levelY, int tileX, int tileY)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(levelX);
        ArgumentOutOfRangeException.ThrowIfNegative(levelY);
        ArgumentOutOfRangeException.ThrowIfNegative(tileX);
        ArgumentOutOfRangeException.ThrowIfNegative(tileY);

        LevelX = levelX;
        LevelY = levelY;
        TileX = tileX;
        TileY = tileY;
    }

    /// <summary>
    /// Gets the horizontal pyramid level.
    /// </summary>
    public int LevelX { get; }

    /// <summary>
    /// Gets the vertical pyramid level.
    /// </summary>
    public int LevelY { get; }

    /// <summary>
    /// Gets the tile column.
    /// </summary>
    public int TileX { get; }

    /// <summary>
    /// Gets the tile row.
    /// </summary>
    public int TileY { get; }
}
