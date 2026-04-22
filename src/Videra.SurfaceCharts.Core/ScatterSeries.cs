using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable scatter series.
/// </summary>
public sealed class ScatterSeries
{
    private readonly ReadOnlyCollection<ScatterPoint> _pointsView;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScatterSeries"/> class.
    /// </summary>
    /// <param name="points">The series points.</param>
    /// <param name="color">The default series color.</param>
    /// <param name="label">The optional series label.</param>
    public ScatterSeries(IReadOnlyList<ScatterPoint> points, uint color, string? label = null)
    {
        ArgumentNullException.ThrowIfNull(points);

        Label = label;
        Color = color;
        _pointsView = Array.AsReadOnly(points.ToArray());
    }

    /// <summary>
    /// Gets the optional series label.
    /// </summary>
    public string? Label { get; }

    /// <summary>
    /// Gets the default ARGB series color.
    /// </summary>
    public uint Color { get; }

    /// <summary>
    /// Gets the immutable point set.
    /// </summary>
    public IReadOnlyList<ScatterPoint> Points => _pointsView;
}
