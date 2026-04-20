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
        return SurfaceMaskedReduction.ReduceRegion(
            sourceValues,
            sourceMaskValues: default,
            sourceWidth,
            startX,
            startY,
            width,
            height,
            isExact: false);
    }
}
