using Avalonia;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceTileScheduler
{
    private readonly SurfaceTileCache _tileCache;
    private readonly SurfaceLodPolicy _lodPolicy;
    private readonly Action _tilesChanged;
    private readonly Action<SurfaceTileKey, Exception>? _tileFailed;
    private ISurfaceTileSource? _source;
    private long _activeGeneration;

    public SurfaceTileScheduler(
        SurfaceTileCache tileCache,
        Action tilesChanged,
        Action<SurfaceTileKey, Exception>? tileFailed = null,
        SurfaceLodPolicy? lodPolicy = null)
    {
        _tileCache = tileCache ?? throw new ArgumentNullException(nameof(tileCache));
        _tilesChanged = tilesChanged ?? throw new ArgumentNullException(nameof(tilesChanged));
        _tileFailed = tileFailed;
        _lodPolicy = lodPolicy ?? SurfaceLodPolicy.Default;
    }

    public void UpdateSource(ISurfaceTileSource? source)
    {
        _source = source;
    }

    public void SetActiveGeneration(long requestGeneration)
    {
        Interlocked.Exchange(ref _activeGeneration, requestGeneration);
    }

    public Task RequestOverviewAsync(long requestGeneration, CancellationToken cancellationToken)
    {
        var source = _source;
        if (source is null)
        {
            return Task.CompletedTask;
        }

        return RequestTileAsync(new SurfaceTileKey(0, 0, 0, 0), source, requestGeneration, cancellationToken);
    }

    public async Task RequestViewportAsync(
        SurfaceViewport viewport,
        Size outputSize,
        long requestGeneration,
        CancellationToken cancellationToken)
    {
        var source = _source;
        if (source is null)
        {
            return;
        }

        var outputWidth = (int)Math.Ceiling(outputSize.Width);
        var outputHeight = (int)Math.Ceiling(outputSize.Height);
        if (outputWidth <= 0 || outputHeight <= 0)
        {
            return;
        }

        var selection = _lodPolicy.Select(new SurfaceViewportRequest(source.Metadata, viewport, outputWidth, outputHeight));
        foreach (var key in selection.EnumerateTileKeys())
        {
            cancellationToken.ThrowIfCancellationRequested();
            await RequestTileAsync(key, source, requestGeneration, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task RequestTileAsync(
        SurfaceTileKey key,
        ISurfaceTileSource source,
        long requestGeneration,
        CancellationToken cancellationToken)
    {
        if (!_tileCache.TryMarkRequested(key, requestGeneration))
        {
            return;
        }

        try
        {
            var tile = await source.GetTileAsync(key, cancellationToken).ConfigureAwait(false);
            if (tile is null)
            {
                _tileCache.ReleaseRequested(key, requestGeneration);
                return;
            }

            if (_tileCache.TryStore(tile, requestGeneration))
            {
                _tilesChanged();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _tileCache.ReleaseRequested(key, requestGeneration);
        }
        catch (Exception ex)
        {
            _tileCache.ReleaseRequested(key, requestGeneration);

            if (requestGeneration == Interlocked.Read(ref _activeGeneration))
            {
                _tileFailed?.Invoke(key, ex);
            }
        }
    }
}
