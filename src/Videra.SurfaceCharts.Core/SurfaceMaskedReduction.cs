namespace Videra.SurfaceCharts.Core;

internal static class SurfaceMaskedReduction
{
    public static SurfaceTileStatistics ReduceRegion(
        ReadOnlySpan<float> sourceValues,
        ReadOnlySpan<bool> sourceMaskValues,
        int sourceWidth,
        int startX,
        int startY,
        int width,
        int height,
        bool isExact)
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
        var validSampleCount = 0;
        var hasMask = sourceMaskValues.Length != 0;

        for (var offsetY = 0; offsetY < height; offsetY++)
        {
            var sourceIndex = ((startY + offsetY) * sourceWidth) + startX;
            for (var offsetX = 0; offsetX < width; offsetX++)
            {
                if (hasMask && !sourceMaskValues[sourceIndex + offsetX])
                {
                    continue;
                }

                var value = sourceValues[sourceIndex + offsetX];
                if (!float.IsFinite(value))
                {
                    continue;
                }

                minimum = Math.Min(minimum, value);
                maximum = Math.Max(maximum, value);
                sum += value;
                validSampleCount++;
            }
        }

        if (validSampleCount == 0)
        {
            return new SurfaceTileStatistics(
                new SurfaceValueRange(0d, 0d),
                average: 0d,
                sampleCount,
                isExact);
        }

        return new SurfaceTileStatistics(
            new SurfaceValueRange(minimum, maximum),
            sum / validSampleCount,
            sampleCount,
            isExact);
    }
}
