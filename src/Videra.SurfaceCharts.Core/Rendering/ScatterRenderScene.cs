using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a render-ready scatter-chart snapshot.
/// </summary>
public sealed class ScatterRenderScene
{
    private readonly ReadOnlyCollection<ScatterRenderSeries> _seriesView;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScatterRenderScene"/> class.
    /// </summary>
    /// <param name="metadata">The scatter metadata for the render snapshot.</param>
    /// <param name="series">The render-ready series collection.</param>
    public ScatterRenderScene(ScatterChartMetadata metadata, IReadOnlyList<ScatterRenderSeries> series)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(series);

        Metadata = metadata;
        _seriesView = Array.AsReadOnly(series.ToArray());
    }

    /// <summary>
    /// Gets the scatter metadata for the render snapshot.
    /// </summary>
    public ScatterChartMetadata Metadata { get; }

    /// <summary>
    /// Gets the immutable render-ready series collection.
    /// </summary>
    public IReadOnlyList<ScatterRenderSeries> Series => _seriesView;
}
