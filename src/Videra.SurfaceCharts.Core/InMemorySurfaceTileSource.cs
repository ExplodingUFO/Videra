using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Provides tile reads from an in-memory surface pyramid.
/// </summary>
public sealed class InMemorySurfaceTileSource : ISurfaceTileSource
{
    private readonly Dictionary<(int LevelX, int LevelY), SurfacePyramidLevel> levelsByIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemorySurfaceTileSource"/> class.
    /// </summary>
    /// <param name="metadata">The source metadata.</param>
    /// <param name="levels">The pyramid levels.</param>
    /// <param name="maxTileWidth">The maximum tile width in samples.</param>
    /// <param name="maxTileHeight">The maximum tile height in samples.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> or <paramref name="levels"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a tile size is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when the level set is empty or contains duplicates.</exception>
    public InMemorySurfaceTileSource(
        SurfaceMetadata metadata,
        IEnumerable<SurfacePyramidLevel> levels,
        int maxTileWidth,
        int maxTileHeight)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(levels);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTileWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTileHeight);

        var levelList = new List<SurfacePyramidLevel>();
        levelsByIndex = new Dictionary<(int LevelX, int LevelY), SurfacePyramidLevel>();

        foreach (var level in levels)
        {
            ArgumentNullException.ThrowIfNull(level);

            if (!levelsByIndex.TryAdd((level.LevelX, level.LevelY), level))
            {
                throw new ArgumentException(
                    $"Duplicate surface pyramid level '{level.LevelX},{level.LevelY}' was provided.",
                    nameof(levels));
            }

            levelList.Add(level);
        }

        if (levelList.Count == 0)
        {
            throw new ArgumentException("At least one surface pyramid level must be provided.", nameof(levels));
        }

        Metadata = metadata;
        Levels = new ReadOnlyCollection<SurfacePyramidLevel>(levelList);
    }

    /// <inheritdoc />
    public SurfaceMetadata Metadata { get; }

    /// <summary>
    /// Gets the pyramid levels available from the source.
    /// </summary>
    public IReadOnlyList<SurfacePyramidLevel> Levels { get; }

    /// <inheritdoc />
    public ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!levelsByIndex.TryGetValue((tileKey.LevelX, tileKey.LevelY), out var level))
        {
            return ValueTask.FromResult<SurfaceTile?>(null);
        }

        var matrix = level.Matrix;
        if (!TryGetTilePartition(matrix.Metadata.Width, level.LevelX, tileKey.TileX, out var startX, out var tileWidth) ||
            !TryGetTilePartition(matrix.Metadata.Height, level.LevelY, tileKey.TileY, out var startY, out var tileHeight))
        {
            return ValueTask.FromResult<SurfaceTile?>(null);
        }

        var tileValues = new float[tileWidth * tileHeight];
        var sourceValues = matrix.Values.Span;
        var matrixWidth = matrix.Metadata.Width;

        var minimum = double.PositiveInfinity;
        var maximum = double.NegativeInfinity;
        var destinationIndex = 0;

        for (var offsetY = 0; offsetY < tileHeight; offsetY++)
        {
            var sourceIndex = ((startY + offsetY) * matrixWidth) + startX;
            for (var offsetX = 0; offsetX < tileWidth; offsetX++)
            {
                var value = sourceValues[sourceIndex + offsetX];
                tileValues[destinationIndex++] = value;

                minimum = Math.Min(minimum, value);
                maximum = Math.Max(maximum, value);
            }
        }

        var tile = new SurfaceTile(
            tileKey,
            tileWidth,
            tileHeight,
            new SurfaceTileBounds(startX, startY, tileWidth, tileHeight),
            tileValues,
            new SurfaceValueRange(minimum, maximum));

        return ValueTask.FromResult<SurfaceTile?>(tile);
    }

    private static bool TryGetTilePartition(int dimension, int level, int tileIndex, out int start, out int size)
    {
        start = 0;
        size = 0;

        if (level >= 63)
        {
            return false;
        }

        var tileCount = 1L << level;
        if (tileIndex >= tileCount)
        {
            return false;
        }

        var startLong = ((long)dimension * tileIndex) / tileCount;
        var endLong = ((long)dimension * (tileIndex + 1L)) / tileCount;
        if (endLong <= startLong)
        {
            return false;
        }

        start = (int)startLong;
        size = (int)(endLong - startLong);
        return true;
    }
}
