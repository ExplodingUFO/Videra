using System.Threading;
using System.Threading.Tasks;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Provides additive batch access to surface tiles without replacing <see cref="ISurfaceTileSource"/>.
/// </summary>
public interface ISurfaceTileBatchSource
{
    /// <summary>
    /// Gets the tiles for the specified keys, preserving the input order.
    /// </summary>
    /// <param name="tileKeys">The tile keys to read.</param>
    /// <param name="cancellationToken">The cancellation token used to cancel the batch read.</param>
    /// <returns>The requested tiles, preserving the input order.</returns>
    ValueTask<IReadOnlyList<SurfaceTile?>> GetTilesAsync(
        IReadOnlyList<SurfaceTileKey> tileKeys,
        CancellationToken cancellationToken = default);
}
