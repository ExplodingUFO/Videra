using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes how multiple bar series are positioned relative to each other.
/// </summary>
public enum BarChartLayout
{
    /// <summary>
    /// Bar series are positioned side by side within each category.
    /// </summary>
    Grouped,

    /// <summary>
    /// Bar series are stacked vertically within each category.
    /// </summary>
    Stacked,
}

/// <summary>
/// Represents one immutable bar-chart dataset.
/// </summary>
public sealed class BarChartData
{
    private readonly ReadOnlyCollection<BarSeries> _seriesView;

    /// <summary>
    /// Initializes a new instance of the <see cref="BarChartData"/> class.
    /// </summary>
    /// <param name="series">The bar series collection. Must not be empty.</param>
    /// <param name="layout">The bar layout mode.</param>
    public BarChartData(IReadOnlyList<BarSeries> series, BarChartLayout layout = BarChartLayout.Grouped)
    {
        ArgumentNullException.ThrowIfNull(series);

        if (series.Count == 0)
        {
            throw new ArgumentException("Bar chart data must contain at least one series.", nameof(series));
        }

        var categoryCount = series[0].CategoryCount;
        for (var i = 1; i < series.Count; i++)
        {
            if (series[i].CategoryCount != categoryCount)
            {
                throw new ArgumentException(
                    $"All bar series must have the same number of categories. Series 0 has {categoryCount} but series {i} has {series[i].CategoryCount}.",
                    nameof(series));
            }
        }

        Layout = layout;
        _seriesView = Array.AsReadOnly(series.ToArray());
    }

    /// <summary>
    /// Gets the bar layout mode.
    /// </summary>
    public BarChartLayout Layout { get; }

    /// <summary>
    /// Gets the immutable bar series collection.
    /// </summary>
    public IReadOnlyList<BarSeries> Series => _seriesView;

    /// <summary>
    /// Gets the number of series in the dataset.
    /// </summary>
    public int SeriesCount => _seriesView.Count;

    /// <summary>
    /// Gets the number of categories (bars per series).
    /// </summary>
    public int CategoryCount => _seriesView.Count > 0 ? _seriesView[0].CategoryCount : 0;
}
