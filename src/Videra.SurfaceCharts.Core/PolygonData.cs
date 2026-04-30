using System.Collections.ObjectModel;
using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable polygon dataset in 3D space.
/// </summary>
public sealed class PolygonData
{
    private readonly ReadOnlyCollection<Vector3> _verticesView;

    /// <summary>
    /// Initializes a new instance of the <see cref="PolygonData"/> class.
    /// </summary>
    /// <param name="vertices">The polygon vertices. Must have at least 3 points.</param>
    /// <param name="fillColor">The ARGB fill color.</param>
    /// <param name="strokeColor">The ARGB stroke color.</param>
    /// <param name="strokeWidth">The stroke width in pixels.</param>
    public PolygonData(
        IReadOnlyList<Vector3> vertices,
        uint fillColor = 0x404DA3FFu,
        uint strokeColor = 0xFF4DA3FFu,
        double strokeWidth = 2d)
    {
        ArgumentNullException.ThrowIfNull(vertices);

        if (vertices.Count < 3)
        {
            throw new ArgumentException("Polygon requires at least 3 vertices.", nameof(vertices));
        }

        _verticesView = Array.AsReadOnly(vertices.ToArray());
        FillColor = fillColor;
        StrokeColor = strokeColor;
        StrokeWidth = strokeWidth;
    }

    /// <summary>
    /// Gets the immutable polygon vertices.
    /// </summary>
    public IReadOnlyList<Vector3> Vertices => _verticesView;

    /// <summary>
    /// Gets the number of vertices.
    /// </summary>
    public int VertexCount => _verticesView.Count;

    /// <summary>
    /// Gets the ARGB fill color.
    /// </summary>
    public uint FillColor { get; }

    /// <summary>
    /// Gets the ARGB stroke color.
    /// </summary>
    public uint StrokeColor { get; }

    /// <summary>
    /// Gets the stroke width in pixels.
    /// </summary>
    public double StrokeWidth { get; }
}
