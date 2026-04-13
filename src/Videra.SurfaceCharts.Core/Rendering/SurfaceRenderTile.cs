using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a single surface-render vertex with world position and mapped color.
/// </summary>
/// <param name="Position">The world position of the vertex.</param>
/// <param name="Color">The mapped ARGB color.</param>
public readonly record struct SurfaceRenderVertex(Vector3 Position, uint Color);

/// <summary>
/// Represents the render-ready input for one surface tile.
/// </summary>
public sealed class SurfaceRenderTile
{
    private readonly SurfaceRenderVertex[] vertices;

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceRenderTile"/> class.
    /// </summary>
    /// <param name="key">The source tile key.</param>
    /// <param name="bounds">The source tile bounds.</param>
    /// <param name="geometry">The shared patch geometry.</param>
    /// <param name="vertices">The render vertices for the tile.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="geometry"/> or <paramref name="vertices"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="vertices"/> does not match the geometry vertex count.</exception>
    public SurfaceRenderTile(
        SurfaceTileKey key,
        SurfaceTileBounds bounds,
        SurfacePatchGeometry geometry,
        IReadOnlyList<SurfaceRenderVertex> vertices)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(vertices);

        if (vertices.Count != geometry.VertexCount)
        {
            throw new ArgumentException("Render vertices must match the geometry vertex count.", nameof(vertices));
        }

        Key = key;
        Bounds = bounds;
        Geometry = geometry;
        this.vertices = vertices as SurfaceRenderVertex[] ?? vertices.ToArray();
    }

    /// <summary>
    /// Gets the source tile key.
    /// </summary>
    public SurfaceTileKey Key { get; }

    /// <summary>
    /// Gets the source tile bounds.
    /// </summary>
    public SurfaceTileBounds Bounds { get; }

    /// <summary>
    /// Gets the shared patch geometry.
    /// </summary>
    public SurfacePatchGeometry Geometry { get; }

    /// <summary>
    /// Gets the render vertices for the tile.
    /// </summary>
    public IReadOnlyList<SurfaceRenderVertex> Vertices => vertices;
}
