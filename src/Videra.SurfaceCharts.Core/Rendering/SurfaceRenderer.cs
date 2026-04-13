using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Builds chart-specific render input from surface tiles.
/// </summary>
public sealed class SurfaceRenderer
{
    private readonly SurfacePatchGeometryBuilder geometryBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceRenderer"/> class.
    /// </summary>
    /// <param name="geometryBuilder">The shared patch geometry builder.</param>
    public SurfaceRenderer(SurfacePatchGeometryBuilder? geometryBuilder = null)
    {
        this.geometryBuilder = geometryBuilder ?? new SurfacePatchGeometryBuilder();
    }

    /// <summary>
    /// Builds a render-ready tile from one source tile.
    /// </summary>
    /// <param name="metadata">The dataset metadata.</param>
    /// <param name="tile">The source tile.</param>
    /// <param name="colorMap">The value color map.</param>
    /// <returns>The dedicated render input for the tile.</returns>
    public SurfaceRenderTile BuildTile(SurfaceMetadata metadata, SurfaceTile tile, SurfaceColorMap colorMap)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(tile);
        ArgumentNullException.ThrowIfNull(colorMap);

        ValidateTileBounds(metadata, tile);

        var geometry = geometryBuilder.Build(tile.Width, tile.Height);
        var sourceValues = tile.Values.Span;
        var vertices = new SurfaceRenderVertex[sourceValues.Length];

        for (var row = 0; row < tile.Height; row++)
        {
            for (var column = 0; column < tile.Width; column++)
            {
                var vertexIndex = checked((row * tile.Width) + column);
                var value = sourceValues[vertexIndex];
                var sampleX = checked(tile.Bounds.StartX + column);
                var sampleY = checked(tile.Bounds.StartY + row);

                vertices[vertexIndex] = new SurfaceRenderVertex(
                    new Vector3(
                        (float)MapAxis(metadata.HorizontalAxis, sampleX, metadata.Width),
                        value,
                        (float)MapAxis(metadata.VerticalAxis, sampleY, metadata.Height)),
                    colorMap.Map(value));
            }
        }

        return new SurfaceRenderTile(tile.Key, tile.Bounds, geometry, vertices);
    }

    /// <summary>
    /// Builds a surface render scene from a set of source tiles.
    /// </summary>
    /// <param name="metadata">The dataset metadata.</param>
    /// <param name="tiles">The source tiles to render.</param>
    /// <param name="colorMap">The value color map.</param>
    /// <returns>A dedicated surface render scene.</returns>
    public SurfaceRenderScene BuildScene(SurfaceMetadata metadata, IEnumerable<SurfaceTile> tiles, SurfaceColorMap colorMap)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(tiles);
        ArgumentNullException.ThrowIfNull(colorMap);

        var renderTiles = new List<SurfaceRenderTile>();
        foreach (var tile in tiles)
        {
            renderTiles.Add(BuildTile(metadata, tile, colorMap));
        }

        return new SurfaceRenderScene(metadata, renderTiles);
    }

    private static double MapAxis(SurfaceAxisDescriptor axis, int sampleIndex, int sampleCount)
    {
        if (sampleCount <= 1 || axis.Maximum <= axis.Minimum)
        {
            return axis.Minimum;
        }

        var normalized = (double)sampleIndex / (sampleCount - 1);
        return axis.Minimum + (axis.Span * normalized);
    }

    private static void ValidateTileBounds(SurfaceMetadata metadata, SurfaceTile tile)
    {
        if (tile.Bounds.EndXExclusive > metadata.Width)
        {
            throw new ArgumentOutOfRangeException(nameof(tile), "Tile horizontal bounds exceed the dataset width.");
        }

        if (tile.Bounds.EndYExclusive > metadata.Height)
        {
            throw new ArgumentOutOfRangeException(nameof(tile), "Tile vertical bounds exceed the dataset height.");
        }
    }
}
