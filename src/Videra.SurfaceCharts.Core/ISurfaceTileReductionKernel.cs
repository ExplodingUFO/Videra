namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Summarizes rectangular source regions into aggregate tile statistics.
/// </summary>
public interface ISurfaceTileReductionKernel
{
    /// <summary>
    /// Reduces a rectangular region from a dense source matrix.
    /// </summary>
    /// <param name="sourceValues">The source matrix values in row-major order.</param>
    /// <param name="sourceWidth">The source matrix width.</param>
    /// <param name="startX">The inclusive starting X coordinate in source space.</param>
    /// <param name="startY">The inclusive starting Y coordinate in source space.</param>
    /// <param name="width">The region width in samples.</param>
    /// <param name="height">The region height in samples.</param>
    /// <returns>The aggregate statistics for the selected region.</returns>
    SurfaceTileStatistics ReduceRegion(
        ReadOnlySpan<float> sourceValues,
        int sourceWidth,
        int startX,
        int startY,
        int width,
        int height);
}
