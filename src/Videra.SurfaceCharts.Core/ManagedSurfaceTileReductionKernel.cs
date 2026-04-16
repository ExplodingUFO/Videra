namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Provides the default managed implementation for region reduction.
/// </summary>
public sealed class ManagedSurfaceTileReductionKernel : ISurfaceTileReductionKernel
{
    /// <inheritdoc />
    public SurfaceTileStatistics ReduceRegion(
        ReadOnlySpan<float> sourceValues,
        int sourceWidth,
        int startX,
        int startY,
        int width,
        int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sourceWidth);
        ArgumentOutOfRangeException.ThrowIfNegative(startX);
        ArgumentOutOfRangeException.ThrowIfNegative(startY);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        var sampleCount = checked(width * height);
        var minimum = double.PositiveInfinity;
        var maximum = double.NegativeInfinity;
        double sum = 0d;

        for (var offsetY = 0; offsetY < height; offsetY++)
        {
            var sourceIndex = ((startY + offsetY) * sourceWidth) + startX;
            for (var offsetX = 0; offsetX < width; offsetX++)
            {
                var value = sourceValues[sourceIndex + offsetX];
                minimum = Math.Min(minimum, value);
                maximum = Math.Max(maximum, value);
                sum += value;
            }
        }

        return new SurfaceTileStatistics(
            new SurfaceValueRange(minimum, maximum),
            sum / sampleCount,
            sampleCount,
            isExact: false);
    }
}
