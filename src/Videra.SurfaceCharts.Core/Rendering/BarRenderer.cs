using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Builds render-ready bar geometry from immutable bar chart data.
/// </summary>
public static class BarRenderer
{
    /// <summary>
    /// The fraction of category width occupied by a single bar (0.8 = 80% bar, 20% gap).
    /// </summary>
    private const float BarWidthFraction = 0.8f;

    /// <summary>
    /// The depth of each bar as a fraction of the bar width.
    /// </summary>
    private const float BarDepthFraction = 0.6f;

    /// <summary>
    /// Small epsilon offset to prevent z-fighting between adjacent bars.
    /// </summary>
    private const float ZFightEpsilon = 0.001f;

    /// <summary>
    /// Builds one render-ready bar scene from immutable bar chart data.
    /// </summary>
    /// <param name="data">The source dataset.</param>
    /// <returns>The render-ready bar scene.</returns>
    public static BarRenderScene BuildScene(BarChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return data.Layout switch
        {
            BarChartLayout.Grouped => BuildGroupedScene(data),
            BarChartLayout.Stacked => BuildStackedScene(data),
            _ => throw new ArgumentOutOfRangeException(nameof(data), "Unsupported bar chart layout."),
        };
    }

    private static BarRenderScene BuildGroupedScene(BarChartData data)
    {
        var categoryCount = data.CategoryCount;
        var seriesCount = data.SeriesCount;
        var bars = new List<BarRenderBar>(categoryCount * seriesCount);

        // Category spacing: each category occupies 1.0 unit, bars are centered in category
        var totalBarWidthPerCategory = BarWidthFraction;
        var singleBarWidth = seriesCount > 1
            ? totalBarWidthPerCategory / seriesCount
            : totalBarWidthPerCategory;

        // Calculate max value for scaling (use max across all series)
        var maxValue = CalculateMaxValue(data);

        for (var seriesIndex = 0; seriesIndex < seriesCount; seriesIndex++)
        {
            var series = data.Series[seriesIndex];

            for (var categoryIndex = 0; categoryIndex < categoryCount; categoryIndex++)
            {
                var value = series.Values[categoryIndex];

                // Horizontal position: center of category + offset for grouped layout
                var categoryCenter = categoryIndex;
                var seriesOffset = seriesCount > 1
                    ? (seriesIndex - (seriesCount - 1) / 2.0) * singleBarWidth
                    : 0.0;

                var x = (float)(categoryCenter + seriesOffset);
                var barHeight = maxValue > 0 ? (float)(value / maxValue) : 0f;
                var y = barHeight / 2f; // Center of bar height
                var z = 0f; // Base plane

                var size = new Vector3(
                    singleBarWidth - ZFightEpsilon,
                    barHeight,
                    BarDepthFraction * singleBarWidth);

                bars.Add(new BarRenderBar(
                    new Vector3(x, y, z),
                    size,
                    series.Color));
            }
        }

        return new BarRenderScene(categoryCount, seriesCount, BarChartLayout.Grouped, bars);
    }

    private static BarRenderScene BuildStackedScene(BarChartData data)
    {
        var categoryCount = data.CategoryCount;
        var seriesCount = data.SeriesCount;
        var bars = new List<BarRenderBar>(categoryCount * seriesCount);

        // Calculate max stacked value for scaling
        var maxStackedValue = CalculateMaxStackedValue(data);

        // Bar width is full category width for stacked
        var barWidth = BarWidthFraction;

        // Track cumulative heights per category for stacking
        var cumulativeHeights = new double[categoryCount];

        for (var seriesIndex = 0; seriesIndex < seriesCount; seriesIndex++)
        {
            var series = data.Series[seriesIndex];

            for (var categoryIndex = 0; categoryIndex < categoryCount; categoryIndex++)
            {
                var value = series.Values[categoryIndex];
                var scaledValue = maxStackedValue > 0 ? value / maxStackedValue : 0.0;

                var x = (float)categoryIndex;
                var baseHeight = cumulativeHeights[categoryIndex];
                var barHeight = (float)scaledValue;
                var y = (float)(baseHeight + barHeight / 2.0); // Center of this segment

                var size = new Vector3(
                    barWidth - ZFightEpsilon,
                    barHeight,
                    BarDepthFraction * barWidth);

                bars.Add(new BarRenderBar(
                    new Vector3(x, y, 0f),
                    size,
                    series.Color));

                cumulativeHeights[categoryIndex] = baseHeight + scaledValue;
            }
        }

        return new BarRenderScene(categoryCount, seriesCount, BarChartLayout.Stacked, bars);
    }

    private static double CalculateMaxValue(BarChartData data)
    {
        var max = 0d;
        foreach (var series in data.Series)
        {
            foreach (var value in series.Values)
            {
                if (value > max)
                {
                    max = value;
                }
            }
        }

        return max;
    }

    private static double CalculateMaxStackedValue(BarChartData data)
    {
        var categoryCount = data.CategoryCount;
        var stackedValues = new double[categoryCount];

        foreach (var series in data.Series)
        {
            for (var i = 0; i < categoryCount; i++)
            {
                stackedValues[i] += series.Values[i];
            }
        }

        var max = 0d;
        foreach (var value in stackedValues)
        {
            if (value > max)
            {
                max = value;
            }
        }

        return max;
    }
}
