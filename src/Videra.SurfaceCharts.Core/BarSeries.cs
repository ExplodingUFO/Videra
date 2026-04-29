using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable bar chart series.
/// </summary>
public sealed class BarSeries
{
    private readonly ReadOnlyCollection<double> _valuesView;

    /// <summary>
    /// Initializes a new instance of the <see cref="BarSeries"/> class.
    /// </summary>
    /// <param name="values">The bar values (one per category). Must not be empty and must not contain NaN.</param>
    /// <param name="color">The ARGB series color.</param>
    /// <param name="label">The optional series label.</param>
    public BarSeries(IReadOnlyList<double> values, uint color, string? label = null)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Count == 0)
        {
            throw new ArgumentException("Bar series values must not be empty.", nameof(values));
        }

        for (var i = 0; i < values.Count; i++)
        {
            if (double.IsNaN(values[i]))
            {
                throw new ArgumentException($"Bar series values must not contain NaN (found at index {i}).", nameof(values));
            }
        }

        Color = color;
        Label = label;
        _valuesView = Array.AsReadOnly(values.ToArray());
    }

    /// <summary>
    /// Gets the optional series label.
    /// </summary>
    public string? Label { get; }

    /// <summary>
    /// Gets the ARGB series color.
    /// </summary>
    public uint Color { get; }

    /// <summary>
    /// Gets the immutable bar values (one per category).
    /// </summary>
    public IReadOnlyList<double> Values => _valuesView;

    /// <summary>
    /// Gets the number of categories in this series.
    /// </summary>
    public int CategoryCount => _valuesView.Count;
}
