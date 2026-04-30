using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for a box plot plottable.
/// </summary>
public sealed class BoxPlotPlot3DSeries : Plot3DSeries
{
    internal BoxPlotPlot3DSeries(string? name, BoxPlotData data)
        : base(Plot3DSeriesKind.BoxPlot, name,
            surfaceSource: null, scatterData: null, barData: null, contourData: null,
            lineData: null, ribbonData: null,
            vectorFieldData: null, heatmapSliceData: null, boxPlotData: data)
    {
    }

    /// <summary>
    /// Updates the color for a specific category box.
    /// </summary>
    /// <param name="index">The zero-based category index.</param>
    /// <param name="color">The replacement ARGB color.</param>
    public void SetCategoryColor(int index, uint color)
    {
        var data = BoxPlotData ?? throw new InvalidOperationException("Box plot series requires box plot data.");
        if (index < 0 || index >= data.CategoryCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var categories = data.Categories.ToArray();
        var old = categories[index];
        categories[index] = new BoxPlotCategory(old.Label, old.Min, old.Q1, old.Median, old.Q3, old.Max, old.Outliers.ToArray(), color);
        ReplaceBoxPlotData(new BoxPlotData(categories));
    }
}
