using System.Diagnostics.CodeAnalysis;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Provides keyed access to surface-chart tiles.
/// </summary>
public interface ISurfaceTileSource
{
    /// <summary>
    /// Gets the dataset metadata for the source.
    /// </summary>
    SurfaceMetadata Metadata { get; }

    /// <summary>
    /// Tries to retrieve a tile for the specified key.
    /// </summary>
    /// <param name="tileKey">The tile key to read.</param>
    /// <param name="tile">When this method returns <c>true</c>, contains the requested tile.</param>
    /// <returns><c>true</c> when the tile exists; otherwise, <c>false</c>.</returns>
    bool TryGetTile(SurfaceTileKey tileKey, [NotNullWhen(true)] out SurfaceTile? tile);
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
    /// <returns>The requested tile.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <c>null</c>.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the tile is unavailable.</exception>
    public static SurfaceTile GetRequiredTile(this ISurfaceTileSource source, SurfaceTileKey tileKey)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!source.TryGetTile(tileKey, out var tile) || tile is null)
        {
            throw new KeyNotFoundException($"Tile '{tileKey}' was not found.");
        }

        return tile;
    }
}

