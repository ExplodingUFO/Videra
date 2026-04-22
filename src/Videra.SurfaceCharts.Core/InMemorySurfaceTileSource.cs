using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Provides tile reads from an in-memory surface pyramid.
/// </summary>
public sealed class InMemorySurfaceTileSource : ISurfaceTileSource
{
    private readonly SurfaceMatrix sourceMatrix;
    private readonly Dictionary<(int LevelX, int LevelY), SurfacePyramidLevel> levelsByIndex;
    private readonly int detailLevelX;
    private readonly int detailLevelY;
    private readonly ISurfaceTileReductionKernel reductionKernel;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemorySurfaceTileSource"/> class.
    /// </summary>
    /// <param name="sourceMatrix">The source matrix used to compute source-region statistics.</param>
    /// <param name="metadata">The source metadata.</param>
    /// <param name="levels">The pyramid levels.</param>
    /// <param name="maxTileWidth">The maximum tile width in samples.</param>
    /// <param name="maxTileHeight">The maximum tile height in samples.</param>
    /// <param name="detailLevelX">The horizontal detail level that maps to exact source samples.</param>
    /// <param name="detailLevelY">The vertical detail level that maps to exact source samples.</param>
    /// <param name="reductionKernel">The reduction kernel used to summarize source regions.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> or <paramref name="levels"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a tile size is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when the level set is empty or contains duplicates.</exception>
    public InMemorySurfaceTileSource(
        SurfaceMatrix sourceMatrix,
        SurfaceMetadata metadata,
        IEnumerable<SurfacePyramidLevel> levels,
        int maxTileWidth,
        int maxTileHeight,
        int detailLevelX,
        int detailLevelY,
        ISurfaceTileReductionKernel reductionKernel)
    {
        ArgumentNullException.ThrowIfNull(sourceMatrix);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(levels);
        ArgumentNullException.ThrowIfNull(reductionKernel);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTileWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTileHeight);
        ArgumentOutOfRangeException.ThrowIfNegative(detailLevelX);
        ArgumentOutOfRangeException.ThrowIfNegative(detailLevelY);

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

        this.sourceMatrix = sourceMatrix;
        Metadata = metadata;
        Levels = new ReadOnlyCollection<SurfacePyramidLevel>(levelList);
        this.detailLevelX = detailLevelX;
        this.detailLevelY = detailLevelY;
        this.reductionKernel = reductionKernel;
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

        if (!TryGetTilePartition(Metadata.Width, level.LevelX, tileKey.TileX, out var sourceStartX, out var sourceWidth) ||
            !TryGetTilePartition(Metadata.Height, level.LevelY, tileKey.TileY, out var sourceStartY, out var sourceHeight))
        {
            return ValueTask.FromResult<SurfaceTile?>(null);
        }

        var tileValues = new float[tileWidth * tileHeight];
        float[]? tileColorValues = matrix.ColorField is not null ? new float[tileWidth * tileHeight] : null;
        bool[]? tileMaskValues = matrix.Mask is not null ? new bool[tileWidth * tileHeight] : null;
        var sourceValues = matrix.Values.Span;
        var sourceColorValues = matrix.ColorField is not null ? matrix.ColorField.Values.Span : default(ReadOnlySpan<float>);
        var sourceMaskValues = matrix.Mask is not null ? matrix.Mask.Values.Span : default(ReadOnlySpan<bool>);
        var matrixWidth = matrix.Metadata.Width;
        var destinationIndex = 0;
        var tileHasMaskedSamples = false;

        for (var offsetY = 0; offsetY < tileHeight; offsetY++)
        {
            var sourceIndex = ((startY + offsetY) * matrixWidth) + startX;
            for (var offsetX = 0; offsetX < tileWidth; offsetX++)
            {
                if (tileMaskValues is not null)
                {
                    var isVisible = sourceMaskValues[sourceIndex + offsetX];
                    tileMaskValues[destinationIndex] = isVisible;
                    tileHasMaskedSamples |= !isVisible;
                }

                tileValues[destinationIndex++] = sourceValues[sourceIndex + offsetX];
                if (tileColorValues is not null)
                {
                    tileColorValues[destinationIndex - 1] = sourceColorValues[sourceIndex + offsetX];
                }
            }
        }

        var tileMask = tileHasMaskedSamples
            ? new SurfaceMask(tileWidth, tileHeight, tileMaskValues!)
            : null;
        var valueRange = SurfaceTileStatistics.FromValues(tileValues, isExact: true, tileMask).Range;
        SurfaceScalarField? colorField = null;
        if (tileColorValues is not null)
        {
            colorField = new SurfaceScalarField(
                tileWidth,
                tileHeight,
                tileColorValues,
                SurfaceTileStatistics.FromValues(tileColorValues, isExact: true, tileMask).Range);
        }

        var statistics = BuildStatistics(
            sourceStartX,
            sourceStartY,
            sourceWidth,
            sourceHeight,
            tileKey.LevelX == detailLevelX && tileKey.LevelY == detailLevelY);

        var tile = new SurfaceTile(
            tileKey,
            new SurfaceTileBounds(sourceStartX, sourceStartY, sourceWidth, sourceHeight),
            new SurfaceScalarField(tileWidth, tileHeight, tileValues, valueRange),
            colorField,
            mask: tileMask,
            statistics);

        return ValueTask.FromResult<SurfaceTile?>(tile);
    }

    private SurfaceTileStatistics BuildStatistics(int startX, int startY, int width, int height, bool isExact)
    {
        var reducedStatistics = sourceMatrix.Mask is not null
            ? SurfaceMaskedReduction.ReduceRegion(
                sourceMatrix.Values.Span,
                sourceMatrix.Mask.Values.Span,
                sourceMatrix.Metadata.Width,
                startX,
                startY,
                width,
                height,
                isExact)
            : reductionKernel.ReduceRegion(
                sourceMatrix.Values.Span,
                sourceMatrix.Metadata.Width,
                startX,
                startY,
                width,
                height);

        return new SurfaceTileStatistics(
            reducedStatistics.Range,
            reducedStatistics.Average,
            reducedStatistics.SampleCount,
            isExact);
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
