using System.Runtime.InteropServices;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Processing;

internal sealed class SurfaceCachePayloadSession : IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly IReadOnlyDictionary<SurfaceTileKey, SurfaceCacheReader.SurfaceCacheEntry> _entriesByKey;
    private readonly SemaphoreSlim _sync = new(1, 1);

    public SurfaceCachePayloadSession(
        string payloadPath,
        IReadOnlyDictionary<SurfaceTileKey, SurfaceCacheReader.SurfaceCacheEntry> entriesByKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadPath);
        ArgumentNullException.ThrowIfNull(entriesByKey);

        if (!SurfaceCacheFileSystem.Current.FileExists(payloadPath))
        {
            throw new IOException($"Surface cache payload file '{payloadPath}' is missing.");
        }

        _stream = SurfaceCacheFileSystem.Current.OpenRead(payloadPath);
        _entriesByKey = entriesByKey;
    }

    public async ValueTask<SurfaceTile?> LoadTileAsync(
        SurfaceTileKey tileKey,
        CancellationToken cancellationToken = default)
    {
        if (!_entriesByKey.TryGetValue(tileKey, out var entry))
        {
            return null;
        }

        await _sync.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await ReadTileAsync(entry, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _sync.Release();
        }
    }

    public async ValueTask<IReadOnlyList<SurfaceTile?>> LoadTilesAsync(
        IReadOnlyList<SurfaceTileKey> tileKeys,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tileKeys);

        if (tileKeys.Count == 0)
        {
            return Array.Empty<SurfaceTile?>();
        }

        var requests = new List<(int Index, SurfaceCacheReader.SurfaceCacheEntry Entry)>(tileKeys.Count);
        var results = new SurfaceTile?[tileKeys.Count];

        for (var index = 0; index < tileKeys.Count; index++)
        {
            if (!_entriesByKey.TryGetValue(tileKeys[index], out var entry))
            {
                continue;
            }

            requests.Add((index, entry));
        }

        await LoadTilesAsyncCore(requests, results, cancellationToken).ConfigureAwait(false);

        return results;
    }

    public async ValueTask DisposeAsync()
    {
        await _stream.DisposeAsync().ConfigureAwait(false);
        _sync.Dispose();
    }

    private async ValueTask LoadTilesAsyncCore(
        IReadOnlyList<(int Index, SurfaceCacheReader.SurfaceCacheEntry Entry)> requests,
        SurfaceTile?[] results,
        CancellationToken cancellationToken)
    {
        if (requests.Count == 0)
        {
            return;
        }

        var orderedRequests = requests
            .OrderBy(static request => request.Entry.PayloadOffset)
            .ToArray();

        await _sync.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            foreach (var request in orderedRequests)
            {
                results[request.Index] = await ReadTileAsync(request.Entry, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            _sync.Release();
        }
    }

    private async ValueTask<SurfaceTile> ReadTileAsync(
        SurfaceCacheReader.SurfaceCacheEntry entry,
        CancellationToken cancellationToken)
    {
        SurfaceCacheReader.EnsurePayloadRangeIsReadable(_stream, entry);
        _stream.Seek(entry.PayloadOffset, SeekOrigin.Begin);

        var buffer = new byte[entry.PayloadLength];
        var totalRead = 0;
        while (totalRead < entry.PayloadLength)
        {
            var read = await _stream.ReadAsync(buffer.AsMemory(totalRead), cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                throw SurfaceCacheReader.CreatePayloadRangeException(entry, _stream.Length);
            }

            totalRead += read;
        }

        var values = new float[entry.Width * entry.Height];
        MemoryMarshal.Cast<byte, float>(buffer.AsSpan()).CopyTo(values);
        var statistics = entry.Statistics ?? SurfaceTileStatistics.FromValues(values, isExact: false);

        return new SurfaceTile(
            entry.Key,
            entry.Width,
            entry.Height,
            entry.Bounds,
            values,
            entry.ValueRange,
            statistics);
    }
}
