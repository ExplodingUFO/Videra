using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Builds chart-specific render input from surface tiles.
/// </summary>
public class SurfaceRenderer
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
    public virtual SurfaceRenderTile BuildTile(SurfaceMetadata metadata, SurfaceTile tile, SurfaceColorMap colorMap)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(tile);
        ArgumentNullException.ThrowIfNull(colorMap);

        ValidateTileBounds(metadata, tile);

        var geometry = geometryBuilder.Build(tile.Width, tile.Height);
        var sourceValues = tile.Values.Span;
        var hasColorField = tile.ColorField is not null;
        var colorValues = hasColorField ? tile.ColorField!.Values.Span : default;
        var vertices = new SurfaceRenderVertex[sourceValues.Length];

        for (var row = 0; row < tile.Height; row++)
        {
            for (var column = 0; column < tile.Width; column++)
            {
                var vertexIndex = checked((row * tile.Width) + column);
                var heightValue = sourceValues[vertexIndex];
                var colorValue = hasColorField ? colorValues[vertexIndex] : heightValue;
                // Coarse LOD tiles can cover a wider source-space span than their value grid,
                // so vertex placement must be distributed across the covered bounds.
                var sampleX = MapTileSampleCoordinate(tile.Bounds.StartX, tile.Bounds.Width, tile.Width, column);
                var sampleY = MapTileSampleCoordinate(tile.Bounds.StartY, tile.Bounds.Height, tile.Height, row);

                vertices[vertexIndex] = new SurfaceRenderVertex(
                    new Vector3(
                        (float)metadata.MapHorizontalCoordinate(sampleX),
                        heightValue,
                        (float)metadata.MapVerticalCoordinate(sampleY)),
                    colorMap.Map(colorValue));
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

    private static double MapTileSampleCoordinate(int start, int span, int sampleCount, int sampleIndex)
    {
        if (sampleCount <= 1)
        {
            return start + ((span - 1d) / 2d);
        }

        return start + (sampleIndex * ((span - 1d) / (sampleCount - 1d)));
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
