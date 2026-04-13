using System.Threading;
using System.Threading.Tasks;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Provides asynchronous keyed access to surface-chart tiles.
/// </summary>
public interface ISurfaceTileSource
{
    /// <summary>
    /// Gets the dataset metadata for the source.
    /// </summary>
    SurfaceMetadata Metadata { get; }

    /// <summary>
    /// Gets the tile for the specified key, or <c>null</c> when the tile is missing.
    /// </summary>
    /// <param name="tileKey">The tile key to read.</param>
    /// <param name="cancellationToken">The cancellation token used to cancel an outstanding tile read.</param>
    /// <returns>The requested tile, or <c>null</c> when the tile is unavailable.</returns>
    ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default);
}

/// <summary>
/// Convenience helpers for <see cref="ISurfaceTileSource"/>.
/// </summary>
public static class SurfaceTileSourceExtensions
{
    /// <summary>
    /// Gets the metadata from a non-null tile source.
    /// </summary>
    /// <param name="source">The tile source.</param>
    /// <returns>The source metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <c>null</c>.</exception>
    public static SurfaceMetadata GetRequiredMetadata(this ISurfaceTileSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Metadata;
    }

    /// <summary>
    /// Gets a tile from a non-null tile source, or throws when the tile is missing.
    /// </summary>
    /// <param name="source">The tile source.</param>
    /// <param name="tileKey">The tile key to read.</param>
    /// <param name="cancellationToken">The cancellation token used to cancel an outstanding tile read.</param>
    /// <returns>The requested tile.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <c>null</c>.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the tile is unavailable.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the source returns a tile that does not match the requested key.</exception>
    public static async ValueTask<SurfaceTile> GetRequiredTileAsync(
        this ISurfaceTileSource source,
        SurfaceTileKey tileKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var tile = await source.GetTileAsync(tileKey, cancellationToken).ConfigureAwait(false);
        if (tile is null)
        {
            throw new KeyNotFoundException($"Tile '{tileKey}' was not found.");
        }

        if (tile.Key != tileKey)
        {
            throw new InvalidOperationException(
                $"Tile source returned tile '{tile.Key}' for requested tile '{tileKey}'.");
        }

        return tile;
    }
}
