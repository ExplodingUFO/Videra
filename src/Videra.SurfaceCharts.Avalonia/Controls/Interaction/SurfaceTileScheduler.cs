using System.Numerics;
using Avalonia;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceTileScheduler
{
    private static readonly SurfaceTileKey OverviewKey = new(0, 0, 0, 0);
    private const float RefineCenterDistanceBucketPixels = 16f;
    private const float InteractiveCenterDistanceBucketPixels = 32f;
    private const float RefineScreenAreaBucketPixels = 512f;
    private const float InteractiveScreenAreaBucketPixels = 2048f;
    private const float DepthBucketStep = 0.05f;

    private readonly SurfaceTileCache _tileCache;
    private readonly SurfaceLodPolicy _lodPolicy;
    private readonly Action _tilesChanged;
    private readonly Action<SurfaceTileKey, Exception>? _tileFailed;
    private readonly int _maxConcurrentRequests;
    private ISurfaceTileSource? _source;
    private long _activeGeneration;

    public SurfaceTileScheduler(
        SurfaceTileCache tileCache,
        Action tilesChanged,
        Action<SurfaceTileKey, Exception>? tileFailed = null,
        SurfaceLodPolicy? lodPolicy = null,
        int maxConcurrentRequests = 4)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxConcurrentRequests);

        _tileCache = tileCache ?? throw new ArgumentNullException(nameof(tileCache));
        _tilesChanged = tilesChanged ?? throw new ArgumentNullException(nameof(tilesChanged));
        _tileFailed = tileFailed;
        _lodPolicy = lodPolicy ?? SurfaceLodPolicy.Default;
        _maxConcurrentRequests = maxConcurrentRequests;
    }

    public void UpdateSource(ISurfaceTileSource? source)
    {
        _source = source;
    }

    public SurfaceMetadata? Metadata => _source?.Metadata;

    public void SetActiveGeneration(long requestGeneration)
    {
        Interlocked.Exchange(ref _activeGeneration, requestGeneration);
    }

    public SurfaceTileRequestPlan CreateRequestPlan(SurfaceViewport viewport, Size outputSize)
    {
        var retainedKeys = new HashSet<SurfaceTileKey> { OverviewKey };
        var orderedKeys = new List<SurfaceTileKey>();
        var source = _source;
        var includeOverview = source is not null && !_tileCache.Contains(OverviewKey);

        if (includeOverview)
        {
            orderedKeys.Add(OverviewKey);
        }

        var outputWidth = (int)Math.Ceiling(outputSize.Width);
        var outputHeight = (int)Math.Ceiling(outputSize.Height);
        if (source is null || outputWidth <= 0 || outputHeight <= 0)
        {
            return new SurfaceTileRequestPlan(orderedKeys.ToArray(), retainedKeys, includeOverview);
        }

        var request = new SurfaceViewportRequest(source.Metadata, viewport, outputWidth, outputHeight);
        var selection = _lodPolicy.Select(request);
        var visibleTileXStart = GetTileIndexStart(request.ClampedViewport.StartX, source.Metadata.Width, 1 << selection.LevelX);
        var visibleTileXEnd = GetTileIndexEnd(request.ClampedViewport.EndXExclusive, source.Metadata.Width, 1 << selection.LevelX);
        var visibleTileYStart = GetTileIndexStart(request.ClampedViewport.StartY, source.Metadata.Height, 1 << selection.LevelY);
        var visibleTileYEnd = GetTileIndexEnd(request.ClampedViewport.EndYExclusive, source.Metadata.Height, 1 << selection.LevelY);
        var centerTileX = GetTileIndexStart(request.ClampedViewport.StartX + (request.ClampedViewport.Width / 2d), source.Metadata.Width, 1 << selection.LevelX);
        var centerTileY = GetTileIndexStart(request.ClampedViewport.StartY + (request.ClampedViewport.Height / 2d), source.Metadata.Height, 1 << selection.LevelY);

        var prioritizedKeys = selection.EnumerateTileKeys()
            .Where(key => key != OverviewKey)
            .Select(static (key, sequence) => new { Key = key, Sequence = sequence })
            .Select(entry => new
            {
                entry.Key,
                Priority = CreatePriority(
                    entry.Key,
                    visibleTileXStart,
                    visibleTileXEnd,
                    visibleTileYStart,
                    visibleTileYEnd,
                    centerTileX,
                    centerTileY,
                    entry.Sequence)
            })
            .OrderBy(entry => entry.Priority.Bucket)
            .ThenBy(entry => entry.Priority.CenterDistanceBucket)
            .ThenBy(entry => entry.Priority.DepthBucket)
            .ThenBy(entry => entry.Priority.ScreenAreaPenalty)
            .ThenBy(entry => entry.Priority.LevelPenalty)
            .ThenBy(entry => entry.Priority.Sequence)
            .Select(entry => entry.Key)
            .ToArray();

        foreach (var key in prioritizedKeys)
        {
            retainedKeys.Add(key);
        }

        orderedKeys.AddRange(prioritizedKeys);
        return new SurfaceTileRequestPlan(orderedKeys.ToArray(), retainedKeys, includeOverview);
    }

    public SurfaceTileRequestPlan CreateRequestPlan(
        SurfaceViewState viewState,
        SurfaceCameraFrame cameraFrame,
        Size outputSize,
        SurfaceChartInteractionQuality interactionQuality)
    {
        var retainedKeys = new HashSet<SurfaceTileKey> { OverviewKey };
        var orderedKeys = new List<SurfaceTileKey>();
        var source = _source;
        var includeOverview = source is not null && !_tileCache.Contains(OverviewKey);

        if (includeOverview)
        {
            orderedKeys.Add(OverviewKey);
        }

        var outputWidth = (int)Math.Ceiling(outputSize.Width);
        var outputHeight = (int)Math.Ceiling(outputSize.Height);
        if (source is null || outputWidth <= 0 || outputHeight <= 0)
        {
            return new SurfaceTileRequestPlan(orderedKeys.ToArray(), retainedKeys, includeOverview);
        }

        var request = new SurfaceViewportRequest(source.Metadata, viewState.DataWindow, outputWidth, outputHeight);
        var selection = _lodPolicy.Select(request, cameraFrame);

        var prioritizedKeys = selection.EnumerateTileKeys()
            .Where(key => key != OverviewKey)
            .Select(static (key, sequence) => new { Key = key, Sequence = sequence })
            .Select(entry => new
            {
                entry.Key,
                Priority = CreateCameraAwarePriority(
                    source.Metadata,
                    cameraFrame,
                    interactionQuality,
                    entry.Key,
                    entry.Sequence)
            })
            .OrderBy(entry => entry.Priority.Bucket)
            .ThenBy(entry => entry.Priority.CenterDistanceBucket)
            .ThenBy(entry => entry.Priority.DepthBucket)
            .ThenBy(entry => entry.Priority.ScreenAreaPenalty)
            .ThenBy(entry => entry.Priority.LevelPenalty)
            .ThenBy(entry => entry.Priority.Sequence)
            .Select(entry => entry.Key)
            .ToArray();

        foreach (var key in prioritizedKeys)
        {
            retainedKeys.Add(key);
        }

        orderedKeys.AddRange(prioritizedKeys);
        return new SurfaceTileRequestPlan(orderedKeys.ToArray(), retainedKeys, includeOverview);
    }

    public async Task RequestPlanAsync(
        SurfaceTileRequestPlan plan,
        long requestGeneration,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var source = _source;
        if (source is null || plan.OrderedKeys.Count == 0)
        {
            return;
        }

        var startIndex = 0;
        if (plan.IncludesOverview && plan.OrderedKeys[0] == OverviewKey)
        {
            await RequestTileAsync(OverviewKey, source, requestGeneration, cancellationToken).ConfigureAwait(false);
            startIndex = 1;
        }

        if (startIndex >= plan.OrderedKeys.Count)
        {
            return;
        }

        if (source is ISurfaceTileBatchSource batchSource)
        {
            await RequestBatchesAsync(
                plan.OrderedKeys.Skip(startIndex).ToArray(),
                batchSource,
                requestGeneration,
                cancellationToken).ConfigureAwait(false);
            return;
        }

        var nextIndex = startIndex - 1;
        var workerCount = Math.Min(_maxConcurrentRequests, plan.OrderedKeys.Count - startIndex);
        var workers = Enumerable.Range(0, workerCount)
            .Select(_ => RunWorkerAsync())
            .ToArray();

        await Task.WhenAll(workers).ConfigureAwait(false);

        async Task RunWorkerAsync()
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var keyIndex = Interlocked.Increment(ref nextIndex);
                if (keyIndex >= plan.OrderedKeys.Count)
                {
                    return;
                }

                await RequestTileAsync(plan.OrderedKeys[keyIndex], source, requestGeneration, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static SurfaceTileRequestPriority CreatePriority(
        SurfaceTileKey key,
        int visibleTileXStart,
        int visibleTileXEnd,
        int visibleTileYStart,
        int visibleTileYEnd,
        int centerTileX,
        int centerTileY,
        int sequence)
    {
        var isVisible = key.TileX >= visibleTileXStart &&
                        key.TileX <= visibleTileXEnd &&
                        key.TileY >= visibleTileYStart &&
                        key.TileY <= visibleTileYEnd;
        var distance = Math.Abs(key.TileX - centerTileX) + Math.Abs(key.TileY - centerTileY);
        var levelPenalty = key.LevelX + key.LevelY;

        return new SurfaceTileRequestPriority(
            isVisible ? 0 : 1,
            distance,
            0,
            0,
            levelPenalty,
            sequence);
    }

    private static SurfaceTileRequestPriority CreateCameraAwarePriority(
        SurfaceMetadata metadata,
        SurfaceCameraFrame cameraFrame,
        SurfaceChartInteractionQuality interactionQuality,
        SurfaceTileKey key,
        int sequence)
    {
        if (!TryGetTileBounds(metadata, key, out var tileBounds))
        {
            return new SurfaceTileRequestPriority(
                Bucket: 2,
                CenterDistanceBucket: int.MaxValue,
                DepthBucket: int.MaxValue,
                ScreenAreaPenalty: int.MaxValue,
                LevelPenalty: key.LevelX + key.LevelY,
                Sequence: sequence);
        }

        var footprint = SurfaceScreenErrorEstimator.EstimateTileFootprint(metadata, tileBounds, cameraFrame);
        var viewportCenter = cameraFrame.ViewportPixels * 0.5f;
        var centerDistance = Vector2.Distance(footprint.ScreenCenter, viewportCenter);
        var centerBucketSize = interactionQuality == SurfaceChartInteractionQuality.Interactive
            ? InteractiveCenterDistanceBucketPixels
            : RefineCenterDistanceBucketPixels;
        var screenAreaBucketSize = interactionQuality == SurfaceChartInteractionQuality.Interactive
            ? InteractiveScreenAreaBucketPixels
            : RefineScreenAreaBucketPixels;

        return new SurfaceTileRequestPriority(
            footprint.IsVisible ? 0 : 1,
            QuantizeNonNegative(centerDistance, centerBucketSize),
            QuantizeNonNegative(footprint.ViewDepth, DepthBucketStep),
            -QuantizeNonNegative(footprint.ScreenAreaPixels, screenAreaBucketSize),
            key.LevelX + key.LevelY,
            sequence);
    }

    private static int GetTileIndexStart(double viewportStart, int datasetSpan, int tileCount)
    {
        var normalizedStart = viewportStart / datasetSpan;
        var tileIndex = (int)Math.Floor(normalizedStart * tileCount);
        return Math.Clamp(tileIndex, 0, tileCount - 1);
    }

    private static int GetTileIndexEnd(double viewportEndExclusive, int datasetSpan, int tileCount)
    {
        var normalizedEndExclusive = viewportEndExclusive / datasetSpan;
        var tileIndex = (int)Math.Ceiling(normalizedEndExclusive * tileCount) - 1;
        return Math.Clamp(tileIndex, 0, tileCount - 1);
    }

    private static bool TryGetTileBounds(SurfaceMetadata metadata, SurfaceTileKey key, out SurfaceTileBounds tileBounds)
    {
        tileBounds = default;

        if (!TryGetTilePartition(metadata.Width, key.LevelX, key.TileX, out var startX, out var width) ||
            !TryGetTilePartition(metadata.Height, key.LevelY, key.TileY, out var startY, out var height))
        {
            return false;
        }

        tileBounds = new SurfaceTileBounds(startX, startY, width, height);
        return true;
    }

    private static bool TryGetTilePartition(int dimension, int level, int tileIndex, out int start, out int size)
    {
        start = 0;
        size = 0;

        if (level >= 63)
        {
            return false;
        }

        var tileCount = 1L << level;
        if (tileIndex >= tileCount)
        {
            return false;
        }

        var startLong = ((long)dimension * tileIndex) / tileCount;
        var endLong = ((long)dimension * (tileIndex + 1L)) / tileCount;
        if (endLong <= startLong)
        {
            return false;
        }

        start = (int)startLong;
        size = (int)(endLong - startLong);
        return true;
    }

    private static int QuantizeNonNegative(float value, float step)
    {
        if (!float.IsFinite(value))
        {
            return int.MaxValue;
        }

        return (int)MathF.Floor(MathF.Max(value, 0f) / step);
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

    private async Task RequestBatchesAsync(
        IReadOnlyList<SurfaceTileKey> orderedKeys,
        ISurfaceTileBatchSource batchSource,
        long requestGeneration,
        CancellationToken cancellationToken)
    {
        for (var index = 0; index < orderedKeys.Count; index += _maxConcurrentRequests)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var batchKeys = orderedKeys
                .Skip(index)
                .Take(_maxConcurrentRequests)
                .ToArray();

            await RequestTileBatchAsync(batchKeys, batchSource, requestGeneration, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task RequestTileBatchAsync(
        IReadOnlyList<SurfaceTileKey> keys,
        ISurfaceTileBatchSource batchSource,
        long requestGeneration,
        CancellationToken cancellationToken)
    {
        if (keys.Count == 0)
        {
            return;
        }

        var requestedKeys = keys
            .Where(key => _tileCache.TryMarkRequested(key, requestGeneration))
            .ToList();

        if (requestedKeys.Count == 0)
        {
            return;
        }

        try
        {
            var tiles = await batchSource.GetTilesAsync(requestedKeys, cancellationToken).ConfigureAwait(false);
            if (tiles.Count != requestedKeys.Count)
            {
                throw new InvalidOperationException(
                    $"Batch tile source returned {tiles.Count} tiles for {requestedKeys.Count} requested keys.");
            }

            var storedAny = false;
            for (var index = 0; index < requestedKeys.Count; index++)
            {
                var tile = tiles[index];
                if (tile is null)
                {
                    _tileCache.ReleaseRequested(requestedKeys[index], requestGeneration);
                    continue;
                }

                if (_tileCache.TryStore(tile, requestGeneration))
                {
                    storedAny = true;
                }
            }

            if (storedAny)
            {
                _tilesChanged();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            ReleaseRequestedKeys(requestedKeys, requestGeneration);
        }
        catch (Exception ex)
        {
            ReleaseRequestedKeys(requestedKeys, requestGeneration);

            if (requestGeneration == Interlocked.Read(ref _activeGeneration))
            {
                _tileFailed?.Invoke(requestedKeys[0], ex);
            }
        }
    }

    private void ReleaseRequestedKeys(
        IReadOnlyList<SurfaceTileKey> keys,
        long requestGeneration)
    {
        foreach (var key in keys)
        {
            _tileCache.ReleaseRequested(key, requestGeneration);
        }
    }
}
