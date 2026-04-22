using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable scatter-chart dataset.
/// </summary>
public sealed class ScatterChartData
{
    private readonly ReadOnlyCollection<ScatterSeries> _seriesView;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScatterChartData"/> class.
    /// </summary>
    /// <param name="metadata">The dataset metadata.</param>
    /// <param name="series">The immutable series collection.</param>
    public ScatterChartData(ScatterChartMetadata metadata, IReadOnlyList<ScatterSeries> series)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(series);

        Metadata = metadata;
        _seriesView = Array.AsReadOnly(series.ToArray());
        PointCount = ValidateAndCountPoints(metadata, _seriesView);
    }

    /// <summary>
    /// Gets the dataset metadata.
    /// </summary>
    public ScatterChartMetadata Metadata { get; }

    /// <summary>
    /// Gets the immutable scatter series collection.
    /// </summary>
    public IReadOnlyList<ScatterSeries> Series => _seriesView;

    /// <summary>
    /// Gets the number of series in the dataset.
    /// </summary>
    public int SeriesCount => _seriesView.Count;

    /// <summary>
    /// Gets the total point count across every series.
    /// </summary>
    public int PointCount { get; }

    private static int ValidateAndCountPoints(ScatterChartMetadata metadata, IReadOnlyList<ScatterSeries> series)
    {
        var totalCount = 0;

        for (var seriesIndex = 0; seriesIndex < series.Count; seriesIndex++)
        {
            var scatterSeries = series[seriesIndex];
            if (scatterSeries is null)
            {
                throw new ArgumentException("Scatter series entries must not be null.", nameof(series));
            }

            totalCount += scatterSeries.Points.Count;
            foreach (var point in scatterSeries.Points)
            {
                if (!metadata.Contains(point))
                {
                    throw new ArgumentException("Scatter points must remain within the declared metadata bounds.", nameof(series));
                }
            }
        }

        return totalCount;
    }
}
