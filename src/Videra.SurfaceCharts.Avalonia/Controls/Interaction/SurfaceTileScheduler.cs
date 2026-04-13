using Avalonia;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceTileScheduler
{
    private readonly SurfaceTileCache _tileCache;
    private readonly SurfaceLodPolicy _lodPolicy;
    private readonly Action _tilesChanged;
    private ISurfaceTileSource? _source;

    public SurfaceTileScheduler(
        SurfaceTileCache tileCache,
        Action tilesChanged,
        SurfaceLodPolicy? lodPolicy = null)
    {
        _tileCache = tileCache ?? throw new ArgumentNullException(nameof(tileCache));
        _tilesChanged = tilesChanged ?? throw new ArgumentNullException(nameof(tilesChanged));
        _lodPolicy = lodPolicy ?? SurfaceLodPolicy.Default;
    }

    public void UpdateSource(ISurfaceTileSource? source)
    {
        _source = source;
    }

    public Task RequestOverviewAsync(CancellationToken cancellationToken)
    {
        return RequestTileAsync(new SurfaceTileKey(0, 0, 0, 0), cancellationToken);
    }

    public async Task RequestViewportAsync(SurfaceViewport viewport, Size outputSize, CancellationToken cancellationToken)
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
            await RequestTileAsync(key, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task RequestTileAsync(SurfaceTileKey key, CancellationToken cancellationToken)
    {
        var source = _source;
        if (source is null || !_tileCache.TryMarkRequested(key))
        {
            return;
        }

        var tile = await source.GetTileAsync(key, cancellationToken).ConfigureAwait(false);
        if (tile is null)
        {
            return;
        }

        _tileCache.Store(tile);
        _tilesChanged();
    }
}
