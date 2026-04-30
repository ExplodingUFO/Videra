using System.Collections.ObjectModel;
using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// A render-ready box (IQR prism) in a box plot scene.
/// </summary>
public readonly record struct BoxPlotRenderBox(Vector3 Position, Vector3 Size, uint Color);

/// <summary>
/// A render-ready whisker line in a box plot scene.
/// </summary>
public readonly record struct BoxPlotRenderWhisker(Vector3 Start, Vector3 End, uint Color);

/// <summary>
/// A render-ready outlier point in a box plot scene.
/// </summary>
public readonly record struct BoxPlotRenderOutlier(Vector3 Position, uint Color);

/// <summary>
/// Represents a render-ready box-plot snapshot containing boxes, whiskers, and outliers.
/// </summary>
public sealed class BoxPlotRenderScene
{
    private readonly ReadOnlyCollection<BoxPlotRenderBox> _boxesView;
    private readonly ReadOnlyCollection<BoxPlotRenderWhisker> _whiskersView;
    private readonly ReadOnlyCollection<BoxPlotRenderOutlier> _outliersView;

    public BoxPlotRenderScene(
        int categoryCount,
        IReadOnlyList<BoxPlotRenderBox> boxes,
        IReadOnlyList<BoxPlotRenderWhisker> whiskers,
        IReadOnlyList<BoxPlotRenderOutlier> outliers)
    {
        ArgumentNullException.ThrowIfNull(boxes);
        ArgumentNullException.ThrowIfNull(whiskers);
        ArgumentNullException.ThrowIfNull(outliers);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(categoryCount);
        CategoryCount = categoryCount;
        _boxesView = Array.AsReadOnly(boxes.ToArray());
        _whiskersView = Array.AsReadOnly(whiskers.ToArray());
        _outliersView = Array.AsReadOnly(outliers.ToArray());
    }

    public int CategoryCount { get; }
    public IReadOnlyList<BoxPlotRenderBox> Boxes => _boxesView;
    public IReadOnlyList<BoxPlotRenderWhisker> Whiskers => _whiskersView;
    public IReadOnlyList<BoxPlotRenderOutlier> Outliers => _outliersView;
}
