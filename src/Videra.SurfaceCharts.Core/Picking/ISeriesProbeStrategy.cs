namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Resolves probe information for a specific series kind at a given chart-space position.
/// </summary>
public interface ISeriesProbeStrategy
{
    /// <summary>
    /// Attempts to resolve a probe at the specified chart-space coordinates.
    /// </summary>
    /// <param name="chartX">The horizontal chart-space coordinate.</param>
    /// <param name="chartZ">The depth chart-space coordinate.</param>
    /// <param name="metadata">The surface metadata for axis mapping.</param>
    /// <returns>A <see cref="SurfaceProbeInfo"/> if a probe was resolved; otherwise, <c>null</c>.</returns>
    SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata);
}
