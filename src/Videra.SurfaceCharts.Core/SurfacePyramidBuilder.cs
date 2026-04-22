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
        var hasSourceMask = source.Mask is not null;
        var sourceMaskValues = hasSourceMask ? source.Mask!.Values.Span : default(ReadOnlySpan<bool>);
        var heightField = CreateReducedField(
            source.Values.Span,
            source.Metadata.Width,
            source.Metadata.Height,
            outputWidth,
            outputHeight,
            blockWidth,
            blockHeight,
            sourceMaskValues,
            reductionKernel);

        SurfaceScalarField? colorField = null;
        if (source.ColorField is not null)
        {
            var reducedColorField = CreateReducedField(
                source.ColorField.Values.Span,
                source.Metadata.Width,
                source.Metadata.Height,
                outputWidth,
                outputHeight,
                blockWidth,
                blockHeight,
                sourceMaskValues,
                reductionKernel);

            colorField = new SurfaceScalarField(
                outputWidth,
                outputHeight,
                reducedColorField.Values,
                SurfaceTileStatistics.FromValues(
                    reducedColorField.Values,
                    isExact: false,
                    heightField.Mask).Range);
        }

        var metadata = source.Metadata.Geometry is SurfaceExplicitGrid explicitGrid
            ? CreateReducedExplicitMetadata(source.Metadata, explicitGrid, blockWidth, blockHeight)
            : new SurfaceMetadata(
                outputWidth,
                outputHeight,
                source.Metadata.HorizontalAxis,
                source.Metadata.VerticalAxis,
                source.Metadata.ValueRange);

        return new SurfaceMatrix(
            metadata,
            new SurfaceScalarField(outputWidth, outputHeight, heightField.Values, metadata.ValueRange),
            colorField,
            heightField.Mask);
    }

    private static (float[] Values, SurfaceMask? Mask) CreateReducedField(
        ReadOnlySpan<float> sourceValues,
        int sourceWidth,
        int sourceHeight,
        int outputWidth,
        int outputHeight,
        long blockWidth,
        long blockHeight,
        ReadOnlySpan<bool> sourceMaskValues,
        ISurfaceTileReductionKernel reductionKernel)
    {
        var outputValues = new float[checked(outputWidth * outputHeight)];
        bool[]? outputMaskValues = sourceMaskValues.Length != 0 ? new bool[outputValues.Length] : null;
        var hasMaskedOutputSamples = false;
        var destinationIndex = 0;

        for (var outputY = 0; outputY < outputHeight; outputY++)
        {
            var startY = (int)(outputY * blockHeight);
            var endY = (int)Math.Min(sourceHeight, startY + blockHeight);

            for (var outputX = 0; outputX < outputWidth; outputX++)
            {
                var startX = (int)(outputX * blockWidth);
                var endX = (int)Math.Min(sourceWidth, startX + blockWidth);
                var statistics = sourceMaskValues.Length != 0
                    ? SurfaceMaskedReduction.ReduceRegion(
                        sourceValues,
                        sourceMaskValues,
                        sourceWidth,
                        startX,
                        startY,
                        endX - startX,
                        endY - startY,
                        isExact: false)
                    : reductionKernel.ReduceRegion(
                        sourceValues,
                        sourceWidth,
                        startX,
                        startY,
                        endX - startX,
                        endY - startY);

                if (outputMaskValues is not null)
                {
                    var hasVisibleSamples = RegionHasVisibleSamples(
                        sourceMaskValues,
                        sourceWidth,
                        startX,
                        startY,
                        endX - startX,
                        endY - startY);
                    outputMaskValues[destinationIndex] = hasVisibleSamples;
                    hasMaskedOutputSamples |= !hasVisibleSamples;
                }

                outputValues[destinationIndex++] = (float)statistics.Average;
            }
        }

        return (outputValues, hasMaskedOutputSamples ? new SurfaceMask(outputWidth, outputHeight, outputMaskValues!) : null);
    }

    private static SurfaceMetadata CreateReducedExplicitMetadata(
        SurfaceMetadata sourceMetadata,
        SurfaceExplicitGrid sourceGrid,
        long blockWidth,
        long blockHeight)
    {
        var horizontalCoordinates = ReduceExplicitCoordinates(sourceGrid.HorizontalCoordinates.Span, blockWidth);
        var verticalCoordinates = ReduceExplicitCoordinates(sourceGrid.VerticalCoordinates.Span, blockHeight);
        var geometry = new SurfaceExplicitGrid(horizontalCoordinates, verticalCoordinates);

        return new SurfaceMetadata(
            geometry,
            new SurfaceAxisDescriptor(
                sourceMetadata.HorizontalAxis.Label,
                sourceMetadata.HorizontalAxis.Unit,
                geometry.GetHorizontalMinimum(sourceMetadata.HorizontalAxis),
                geometry.GetHorizontalMaximum(sourceMetadata.HorizontalAxis),
                SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceAxisDescriptor(
                sourceMetadata.VerticalAxis.Label,
                sourceMetadata.VerticalAxis.Unit,
                geometry.GetVerticalMinimum(sourceMetadata.VerticalAxis),
                geometry.GetVerticalMaximum(sourceMetadata.VerticalAxis),
                SurfaceAxisScaleKind.ExplicitCoordinates),
            sourceMetadata.ValueRange);
    }

    private static double[] ReduceExplicitCoordinates(ReadOnlySpan<double> coordinates, long blockSize)
    {
        var outputLength = DivideRoundUp(coordinates.Length, blockSize);
        var reducedCoordinates = new double[outputLength];

        for (var outputIndex = 0; outputIndex < outputLength; outputIndex++)
        {
            var startIndex = (int)(outputIndex * blockSize);
            var endIndex = Math.Min(coordinates.Length - 1, (int)((outputIndex + 1L) * blockSize) - 1);
            reducedCoordinates[outputIndex] = (coordinates[startIndex] + coordinates[endIndex]) * 0.5d;
        }

        return reducedCoordinates;
    }

    private static int DivideRoundUp(int value, long divisor)
    {
        return checked((int)((value + divisor - 1) / divisor));
    }

    private static bool RegionHasVisibleSamples(
        ReadOnlySpan<bool> maskValues,
        int sourceWidth,
        int startX,
        int startY,
        int width,
        int height)
    {
        for (var offsetY = 0; offsetY < height; offsetY++)
        {
            var sourceIndex = ((startY + offsetY) * sourceWidth) + startX;
            for (var offsetX = 0; offsetX < width; offsetX++)
            {
                if (maskValues[sourceIndex + offsetX])
                {
                    return true;
                }
            }
        }

        return false;
    }
}
