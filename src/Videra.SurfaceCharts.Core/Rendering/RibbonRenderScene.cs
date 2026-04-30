using System.Collections.ObjectModel;
using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// A render-ready ribbon segment connecting two 3D points with a tube radius, cross-section sides, and color.
/// </summary>
/// <param name="Start">The segment start position in chart-space coordinates.</param>
/// <param name="End">The segment end position in chart-space coordinates.</param>
/// <param name="Radius">The tube radius for the ribbon cross-section.</param>
/// <param name="Sides">The number of sides in the tube cross-section polygon. Defaults to 8.</param>
/// <param name="Color">The ARGB segment color.</param>
public readonly record struct RibbonRenderSegment(Vector3 Start, Vector3 End, float Radius, int Sides, uint Color);

/// <summary>
/// Represents a render-ready ribbon-chart snapshot containing ribbon segments for all series.
/// </summary>
public sealed class RibbonRenderScene
{
    private readonly ReadOnlyCollection<RibbonRenderSegment> _segmentsView;

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonRenderScene"/> class.
    /// </summary>
    /// <param name="seriesCount">The number of series. Must be positive.</param>
    /// <param name="segments">The render-ready ribbon segments.</param>
    public RibbonRenderScene(int seriesCount, IReadOnlyList<RibbonRenderSegment> segments)
    {
        ArgumentNullException.ThrowIfNull(segments);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seriesCount);

        SeriesCount = seriesCount;
        _segmentsView = Array.AsReadOnly(segments.ToArray());
    }

    /// <summary>
    /// Gets the number of series in the scene.
    /// </summary>
    public int SeriesCount { get; }

    /// <summary>
    /// Gets the immutable render-ready ribbon segments.
    /// </summary>
    public IReadOnlyList<RibbonRenderSegment> Segments => _segmentsView;
}
