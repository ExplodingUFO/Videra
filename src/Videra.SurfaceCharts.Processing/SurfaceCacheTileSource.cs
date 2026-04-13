using System.Threading;
using System.Threading.Tasks;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Processing;

/// <summary>
/// Adapts a loaded surface cache to the core tile-source contract with lazy on-demand loading.
/// </summary>
public sealed class SurfaceCacheTileSource : ISurfaceTileSource
{
    private readonly SurfaceCacheReader cacheReader;

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceCacheTileSource"/> class.
    /// </summary>
    /// <param name="cacheReader">The loaded cache reader.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheReader"/> is <c>null</c>.</exception>
    public SurfaceCacheTileSource(SurfaceCacheReader cacheReader)
    {
        ArgumentNullException.ThrowIfNull(cacheReader);

        this.cacheReader = cacheReader;
    }

    /// <inheritdoc />
    public SurfaceMetadata Metadata => cacheReader.Metadata;

    /// <inheritdoc />
    public ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return cacheReader.LoadTileAsync(tileKey, cancellationToken);
    }
}
