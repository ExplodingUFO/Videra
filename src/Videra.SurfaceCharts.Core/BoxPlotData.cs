using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable box-plot dataset.
/// </summary>
public sealed class BoxPlotData
{
    private readonly ReadOnlyCollection<BoxPlotCategory> _categoriesView;

    public BoxPlotData(IReadOnlyList<BoxPlotCategory> categories)
    {
        ArgumentNullException.ThrowIfNull(categories);
        if (categories.Count == 0)
        {
            throw new ArgumentException("Box plot data must contain at least one category.", nameof(categories));
        }
        _categoriesView = Array.AsReadOnly(categories.ToArray());
    }

    public IReadOnlyList<BoxPlotCategory> Categories => _categoriesView;
    public int CategoryCount => _categoriesView.Count;
}

/// <summary>
/// Describes one box-plot category with statistical summary.
/// </summary>
public sealed class BoxPlotCategory
{
    private readonly ReadOnlyCollection<double> _outliersView;

    public BoxPlotCategory(
        string label,
        double min,
        double q1,
        double median,
        double q3,
        double max,
        IReadOnlyList<double>? outliers = null,
        uint color = 0xFF4488CCu)
    {
        if (q1 > q3) throw new ArgumentException("Q1 must not exceed Q3.", nameof(q1));
        if (min > q1) throw new ArgumentException("Min must not exceed Q1.", nameof(min));
        if (max < q3) throw new ArgumentException("Max must not be less than Q3.", nameof(max));
        if (median < q1 || median > q3) throw new ArgumentException("Median must be between Q1 and Q3.", nameof(median));

        Label = label;
        Min = min;
        Q1 = q1;
        Median = median;
        Q3 = q3;
        Max = max;
        _outliersView = Array.AsReadOnly((outliers ?? Array.Empty<double>()).ToArray());
        Color = color;
    }

    public string Label { get; }
    public double Min { get; }
    public double Q1 { get; }
    public double Median { get; }
    public double Q3 { get; }
    public double Max { get; }
    public IReadOnlyList<double> Outliers => _outliersView;
    public uint Color { get; }
}
