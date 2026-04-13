namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes the selected LOD levels and tile request range for a surface viewport.
/// </summary>
public readonly record struct SurfaceLodSelection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceLodSelection"/> struct.
    /// </summary>
    /// <param name="request">The viewport request that produced the selection.</param>
    /// <param name="levelX">The selected horizontal pyramid level.</param>
    /// <param name="levelY">The selected vertical pyramid level.</param>
    /// <param name="tileXStart">The first tile column to request.</param>
    /// <param name="tileXEnd">The last tile column to request.</param>
    /// <param name="tileYStart">The first tile row to request.</param>
    /// <param name="tileYEnd">The last tile row to request.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the level or tile range is invalid.</exception>
    public SurfaceLodSelection(
        SurfaceViewportRequest request,
        int levelX,
        int levelY,
        int tileXStart,
        int tileXEnd,
        int tileYStart,
        int tileYEnd)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(levelX);
        ArgumentOutOfRangeException.ThrowIfNegative(levelY);
        ArgumentOutOfRangeException.ThrowIfNegative(tileXStart);
        ArgumentOutOfRangeException.ThrowIfNegative(tileYStart);

        if (tileXEnd < tileXStart)
        {
            throw new ArgumentOutOfRangeException(nameof(tileXEnd));
        }

        if (tileYEnd < tileYStart)
        {
            throw new ArgumentOutOfRangeException(nameof(tileYEnd));
        }

        Request = request;
        LevelX = levelX;
        LevelY = levelY;
        TileXStart = tileXStart;
        TileXEnd = tileXEnd;
        TileYStart = tileYStart;
        TileYEnd = tileYEnd;
    }

    /// <summary>
    /// Gets the viewport request that produced the selection.
    /// </summary>
    public SurfaceViewportRequest Request { get; }

    /// <summary>
    /// Gets the selected horizontal pyramid level.
    /// </summary>
    public int LevelX { get; }

    /// <summary>
    /// Gets the selected vertical pyramid level.
    /// </summary>
    public int LevelY { get; }

    /// <summary>
    /// Gets the first tile column to request.
    /// </summary>
    public int TileXStart { get; }

    /// <summary>
    /// Gets the last tile column to request.
    /// </summary>
    public int TileXEnd { get; }

    /// <summary>
    /// Gets the first tile row to request.
    /// </summary>
    public int TileYStart { get; }

    /// <summary>
    /// Gets the last tile row to request.
    /// </summary>
    public int TileYEnd { get; }

    /// <summary>
    /// Enumerates the tile keys for the selection in row-major order.
    /// </summary>
    /// <returns>The selected tile keys.</returns>
    public IEnumerable<SurfaceTileKey> EnumerateTileKeys()
    {
        for (var tileY = TileYStart; tileY <= TileYEnd; tileY++)
        {
            for (var tileX = TileXStart; tileX <= TileXEnd; tileX++)
            {
                yield return new SurfaceTileKey(LevelX, LevelY, tileX, tileY);
            }
        }
    }
}
