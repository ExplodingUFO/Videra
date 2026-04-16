namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Builds an in-memory surface pyramid from a dense source matrix.
/// </summary>
public sealed class SurfacePyramidBuilder
{
    private readonly ISurfaceTileReductionKernel reductionKernel;

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfacePyramidBuilder"/> class.
    /// </summary>
    /// <param name="maxTileWidth">The target maximum tile width in samples.</param>
    /// <param name="maxTileHeight">The target maximum tile height in samples.</param>
    /// <param name="reductionKernel">The reduction kernel used for pyramid generation and statistics.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a tile size is not positive.</exception>
    public SurfacePyramidBuilder(
        int maxTileWidth,
        int maxTileHeight,
        ISurfaceTileReductionKernel? reductionKernel = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTileWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTileHeight);

        MaxTileWidth = maxTileWidth;
        MaxTileHeight = maxTileHeight;
        this.reductionKernel = reductionKernel ?? new ManagedSurfaceTileReductionKernel();
    }

    /// <summary>
    /// Gets the maximum tile width in samples.
    /// </summary>
    public int MaxTileWidth { get; }

    /// <summary>
    /// Gets the maximum tile height in samples.
    /// </summary>
    public int MaxTileHeight { get; }

    /// <summary>
    /// Gets the peak number of temporary samples allocated while generating the last pyramid.
    /// </summary>
    public int LastBuildPeakScratchSampleCount { get; private set; }

    /// <summary>
    /// Builds an in-memory pyramid that keeps explicit horizontal and vertical LOD levels.
    /// </summary>
    /// <param name="source">The source matrix.</param>
    /// <returns>The built surface tile source.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <c>null</c>.</exception>
    public ISurfaceTileSource Build(SurfaceMatrix source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var sourceLevelX = CalculateDetailLevel(source.Metadata.Width, MaxTileWidth);
        var sourceLevelY = CalculateDetailLevel(source.Metadata.Height, MaxTileHeight);
        var levels = new List<SurfacePyramidLevel>((sourceLevelX + 1) * (sourceLevelY + 1));

        LastBuildPeakScratchSampleCount = 0;

        for (var levelY = 0; levelY <= sourceLevelY; levelY++)
        {
            for (var levelX = 0; levelX <= sourceLevelX; levelX++)
            {
                SurfaceMatrix matrix;
                if (levelX == sourceLevelX && levelY == sourceLevelY)
                {
                    matrix = source;
                }
                else
                {
                    matrix = CreateLevelMatrix(source, sourceLevelX, sourceLevelY, levelX, levelY, reductionKernel);
                    LastBuildPeakScratchSampleCount = Math.Max(LastBuildPeakScratchSampleCount, matrix.Values.Length);
                }

                levels.Add(new SurfacePyramidLevel(levelX, levelY, matrix));
            }
        }

        return new InMemorySurfaceTileSource(
            source,
            source.Metadata,
            levels,
            MaxTileWidth,
            MaxTileHeight,
            sourceLevelX,
            sourceLevelY,
            reductionKernel);
    }

    private static int CalculateDetailLevel(int dimension, int maxTileDimension)
    {
        var level = 0;
        while (dimension > maxTileDimension)
        {
            dimension = DivideRoundUp(dimension, 2L);
            level++;
        }

        return level;
    }

    private static SurfaceMatrix CreateLevelMatrix(
        SurfaceMatrix source,
        int sourceLevelX,
        int sourceLevelY,
        int levelX,
        int levelY,
        ISurfaceTileReductionKernel reductionKernel)
    {
        var blockWidth = 1L << (sourceLevelX - levelX);
        var blockHeight = 1L << (sourceLevelY - levelY);
        var outputWidth = DivideRoundUp(source.Metadata.Width, blockWidth);
        var outputHeight = DivideRoundUp(source.Metadata.Height, blockHeight);
        var outputValues = new float[checked(outputWidth * outputHeight)];
        var sourceValues = source.Values.Span;
        var sourceWidth = source.Metadata.Width;
        var destinationIndex = 0;

        for (var outputY = 0; outputY < outputHeight; outputY++)
        {
            var startY = (int)(outputY * blockHeight);
            var endY = (int)Math.Min(source.Metadata.Height, startY + blockHeight);

            for (var outputX = 0; outputX < outputWidth; outputX++)
            {
                var startX = (int)(outputX * blockWidth);
                var endX = (int)Math.Min(source.Metadata.Width, startX + blockWidth);
                var statistics = reductionKernel.ReduceRegion(
                    sourceValues,
                    sourceWidth,
                    startX,
                    startY,
                    endX - startX,
                    endY - startY);

                outputValues[destinationIndex++] = (float)statistics.Average;
            }
        }

        var metadata = new SurfaceMetadata(
            outputWidth,
            outputHeight,
            source.Metadata.HorizontalAxis,
            source.Metadata.VerticalAxis,
            source.Metadata.ValueRange);

        return new SurfaceMatrix(metadata, outputValues);
    }

    private static int DivideRoundUp(int value, long divisor)
    {
        return checked((int)((value + divisor - 1) / divisor));
    }
}
