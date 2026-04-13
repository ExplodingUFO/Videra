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
    private readonly int maxTileWidth;
    private readonly int maxTileHeight;

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
        this.maxTileWidth = maxTileWidth;
        this.maxTileHeight = maxTileHeight;
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
        var startX = tileKey.TileX * maxTileWidth;
        var startY = tileKey.TileY * maxTileHeight;
        if (startX >= matrix.Metadata.Width || startY >= matrix.Metadata.Height)
        {
            return ValueTask.FromResult<SurfaceTile?>(null);
        }

        var tileWidth = Math.Min(maxTileWidth, matrix.Metadata.Width - startX);
        var tileHeight = Math.Min(maxTileHeight, matrix.Metadata.Height - startY);
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
}
