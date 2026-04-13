namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a chart-specific surface render snapshot composed of render tiles.
/// </summary>
public sealed class SurfaceRenderScene
{
    private readonly SurfaceRenderTile[] tiles;

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceRenderScene"/> class.
    /// </summary>
    /// <param name="metadata">The dataset metadata for the render snapshot.</param>
    /// <param name="tiles">The render tiles that make up the snapshot.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> or <paramref name="tiles"/> is <c>null</c>.</exception>
    public SurfaceRenderScene(SurfaceMetadata metadata, IReadOnlyList<SurfaceRenderTile> tiles)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(tiles);

        Metadata = metadata;
        this.tiles = tiles as SurfaceRenderTile[] ?? tiles.ToArray();
    }

    /// <summary>
    /// Gets the dataset metadata for the render snapshot.
    /// </summary>
    public SurfaceMetadata Metadata { get; }

    /// <summary>
    /// Gets the render tiles in the snapshot.
    /// </summary>
    public IReadOnlyList<SurfaceRenderTile> Tiles => tiles;
}
