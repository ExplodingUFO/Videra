using System.Threading;
using System.Threading.Tasks;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Processing;

/// <summary>
/// Adapts a loaded surface cache to the core tile-source contract with lazy on-demand loading.
/// </summary>
public sealed class SurfaceCacheTileSource : ISurfaceTileSource, ISurfaceTileBatchSource, IAsyncDisposable
{
    private readonly SurfaceCacheReader cacheReader;
    private readonly SemaphoreSlim sessionSync = new(1, 1);
    private SurfaceCachePayloadSession? payloadSession;

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

        return GetTileAsyncCore(tileKey, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<SurfaceTile?>> GetTilesAsync(
        IReadOnlyList<SurfaceTileKey> tileKeys,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tileKeys);
        if (tileKeys.Count == 0)
        {
            return Array.Empty<SurfaceTile?>();
        }

        cancellationToken.ThrowIfCancellationRequested();

        var hasCachedTile = false;
        foreach (var tileKey in tileKeys)
        {
            if (cacheReader.ContainsTile(tileKey))
            {
                hasCachedTile = true;
                break;
            }
        }

        if (!hasCachedTile)
        {
            return new SurfaceTile?[tileKeys.Count];
        }

        var session = await GetOrCreatePayloadSessionAsync(cancellationToken).ConfigureAwait(false);
        return await session.LoadTilesAsync(tileKeys, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        SurfaceCachePayloadSession? sessionToDispose = null;

        await sessionSync.WaitAsync().ConfigureAwait(false);
        try
        {
            sessionToDispose = payloadSession;
            payloadSession = null;
        }
        finally
        {
            sessionSync.Release();
            sessionSync.Dispose();
        }

        if (sessionToDispose is not null)
        {
            await sessionToDispose.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async ValueTask<SurfaceTile?> GetTileAsyncCore(
        SurfaceTileKey tileKey,
        CancellationToken cancellationToken)
    {
        if (!cacheReader.ContainsTile(tileKey))
        {
            return null;
        }

        var session = await GetOrCreatePayloadSessionAsync(cancellationToken).ConfigureAwait(false);
        return await session.LoadTileAsync(tileKey, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<SurfaceCachePayloadSession> GetOrCreatePayloadSessionAsync(CancellationToken cancellationToken)
    {
        if (payloadSession is not null)
        {
            return payloadSession;
        }

        await sessionSync.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            payloadSession ??= cacheReader.CreatePayloadSession();
            return payloadSession;
        }
        finally
        {
            sessionSync.Release();
        }
    }
}
