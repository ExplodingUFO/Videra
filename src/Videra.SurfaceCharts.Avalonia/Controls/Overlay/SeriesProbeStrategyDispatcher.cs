using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

/// <summary>
/// Dispatches probe resolution to the appropriate strategy based on series kind.
/// </summary>
internal sealed class SeriesProbeStrategyDispatcher
{
    private readonly Dictionary<Plot3DSeriesKind, ISeriesProbeStrategy> _strategies;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeriesProbeStrategyDispatcher"/> class.
    /// </summary>
    /// <param name="strategies">The strategy mapping from series kind to probe strategy.</param>
    public SeriesProbeStrategyDispatcher(IReadOnlyDictionary<Plot3DSeriesKind, ISeriesProbeStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);
        _strategies = new Dictionary<Plot3DSeriesKind, ISeriesProbeStrategy>(strategies);
    }

    /// <summary>
    /// Attempts to resolve a probe for the specified series kind.
    /// </summary>
    /// <param name="kind">The series kind to resolve for.</param>
    /// <param name="chartX">The horizontal chart-space coordinate.</param>
    /// <param name="chartZ">The depth chart-space coordinate.</param>
    /// <param name="metadata">The surface metadata for axis mapping.</param>
    /// <returns>A <see cref="SurfaceProbeInfo"/> if resolved; otherwise, <c>null</c>.</returns>
    public SurfaceProbeInfo? TryResolve(Plot3DSeriesKind kind, double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        if (_strategies.TryGetValue(kind, out var strategy))
        {
            return strategy.TryResolve(chartX, chartZ, metadata);
        }

        return null;
    }

    /// <summary>
    /// Gets whether a strategy is registered for the specified series kind.
    /// </summary>
    /// <param name="kind">The series kind to check.</param>
    /// <returns><c>true</c> when a strategy is registered; otherwise, <c>false</c>.</returns>
    public bool HasStrategy(Plot3DSeriesKind kind)
    {
        return _strategies.ContainsKey(kind);
    }
}
