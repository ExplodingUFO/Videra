using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a render-ready bar-chart snapshot.
/// </summary>
public sealed class BarRenderScene
{
    private readonly ReadOnlyCollection<BarRenderBar> _barsView;

    /// <summary>
    /// Initializes a new instance of the <see cref="BarRenderScene"/> class.
    /// </summary>
    /// <param name="categoryCount">The number of categories.</param>
    /// <param name="seriesCount">The number of series.</param>
    /// <param name="layout">The bar layout mode.</param>
    /// <param name="bars">The render-ready bars.</param>
    public BarRenderScene(
        int categoryCount,
        int seriesCount,
        BarChartLayout layout,
        IReadOnlyList<BarRenderBar> bars)
    {
        ArgumentNullException.ThrowIfNull(bars);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(categoryCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seriesCount);

        CategoryCount = categoryCount;
        SeriesCount = seriesCount;
        Layout = layout;
        _barsView = Array.AsReadOnly(bars.ToArray());
    }

    /// <summary>
    /// Gets the number of categories.
    /// </summary>
    public int CategoryCount { get; }

    /// <summary>
    /// Gets the number of series.
    /// </summary>
    public int SeriesCount { get; }

    /// <summary>
    /// Gets the bar layout mode.
    /// </summary>
    public BarChartLayout Layout { get; }

    /// <summary>
    /// Gets the immutable render-ready bars.
    /// </summary>
    public IReadOnlyList<BarRenderBar> Bars => _barsView;
}
