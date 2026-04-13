using System.Diagnostics;
using System.Reflection;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartViewLifecycleTests
{
    [Fact]
    public void ControlInitialization_WithoutSource_DoesNotThrow()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var act = () =>
            {
                var view = new SurfaceChartView();
                view.Measure(new Size(320, 180));
                view.Arrange(new Rect(0, 0, 320, 180));
                view.Viewport = new SurfaceViewport(0, 0, 128, 128);
            };

            act.Should().NotThrow();
        });
    }

    [Fact]
    public Task SourceAssignment_IssuesOverviewFirstRequest()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var view = new SurfaceChartView();
            var source = new RecordingSurfaceTileSource(CreateMetadata());

            view.Source = source;

            await source.WaitForRequestCountAsync(1);
            await source.AssertRequestCountSettlesAtAsync(1);

            source.RequestLog.Should().Equal(new SurfaceTileKey(0, 0, 0, 0));
        });
    }

    [Fact]
    public Task ViewportChangesBeforeSourceAssignment_DoNotRequestTiles_AndSourceStillStartsWithOverview()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var view = new SurfaceChartView();
            var source = new RecordingSurfaceTileSource(CreateMetadata());

            view.Measure(new Size(256, 256));
            view.Arrange(new Rect(0, 0, 256, 256));
            view.Viewport = new SurfaceViewport(128, 128, 256, 256);

            source.RequestLog.Should().BeEmpty();

            view.Source = source;

            await source.WaitForRequestCountAsync(1);

            source.RequestLog[0].Should().Be(new SurfaceTileKey(0, 0, 0, 0));
        });
    }

    [Fact]
    public Task RapidSourceReplacement_DoesNotCommitLateTilesFromSupersededSource()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = CreateMetadata();
            var staleSource = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 11);
            var activeSource = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 42);
            var staleRequestStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var staleCompletion = new TaskCompletionSource<SurfaceTile?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var overviewKey = new SurfaceTileKey(0, 0, 0, 0);
            var view = new SurfaceChartView();

            staleSource.EnqueuePendingResponse(staleRequestStarted, staleCompletion, observeCancellation: false);

            view.Source = staleSource;

            await staleRequestStarted.Task;

            view.Source = activeSource;

            await activeSource.WaitForRequestCountAsync(1);
            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [42]);

            staleCompletion.SetResult(staleSource.CreateTile(overviewKey));

            await staleSource.AssertRequestCountSettlesAtAsync(1);
            await activeSource.AssertRequestCountSettlesAtAsync(1);
            await SurfaceChartTestHelpers.AssertLoadedTileValuesStayAsync(view, [42]);
        });
    }

    internal static SurfaceMetadata CreateMetadata()
    {
        return new SurfaceMetadata(
            width: 1024,
            height: 1024,
            new SurfaceAxisDescriptor("X", unit: null, minimum: 0, maximum: 1023),
            new SurfaceAxisDescriptor("Y", unit: null, minimum: 0, maximum: 1023),
            new SurfaceValueRange(0, 100));
    }
}

internal sealed class RecordingSurfaceTileSource : ISurfaceTileSource
{
    private readonly object _sync = new();
    private readonly List<SurfaceTileKey> _requestLog = [];
    private readonly List<TaskCompletionSource<bool>> _waiters = [];

    public RecordingSurfaceTileSource(SurfaceMetadata metadata)
    {
        Metadata = metadata;
    }

    public SurfaceMetadata Metadata { get; }

    public IReadOnlyList<SurfaceTileKey> RequestLog
    {
        get
        {
            lock (_sync)
            {
                return _requestLog.ToArray();
            }
        }
    }

    public ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            _requestLog.Add(tileKey);
            for (var index = _waiters.Count - 1; index >= 0; index--)
            {
                _waiters[index].TrySetResult(true);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<SurfaceTile?>(CreateTile(tileKey));
    }

    public async Task WaitForRequestCountAsync(int expectedCount, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(2);
        var deadline = Stopwatch.GetTimestamp() + (long)(timeout.Value.TotalSeconds * Stopwatch.Frequency);

        while (GetRequestCount() < expectedCount)
        {
            var waiter = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_sync)
            {
                if (_requestLog.Count >= expectedCount)
                {
                    return;
                }

                _waiters.Add(waiter);
            }

            var remaining = deadline - Stopwatch.GetTimestamp();
            if (remaining <= 0)
            {
                throw new TimeoutException($"Timed out waiting for {expectedCount} requests. Actual count: {GetRequestCount()}.");
            }

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds((double)remaining / Stopwatch.Frequency));
            using var registration = timeoutCts.Token.Register(static state => ((TaskCompletionSource<bool>)state!).TrySetCanceled(), waiter);

            try
            {
                await waiter.Task.ConfigureAwait(false);
            }
            catch (TaskCanceledException ex)
            {
                throw new TimeoutException($"Timed out waiting for {expectedCount} requests. Actual count: {GetRequestCount()}.", ex);
            }
            finally
            {
                lock (_sync)
                {
                    _waiters.Remove(waiter);
                }
            }
        }
    }

    public async Task AssertRequestCountSettlesAtAsync(int expectedCount, TimeSpan? settlingDelay = null)
    {
        settlingDelay ??= TimeSpan.FromMilliseconds(100);
        await Task.Delay(settlingDelay.Value).ConfigureAwait(false);
        GetRequestCount().Should().Be(expectedCount);
    }

    private int GetRequestCount()
    {
        lock (_sync)
        {
            return _requestLog.Count;
        }
    }

    private SurfaceTile CreateTile(SurfaceTileKey key)
    {
        return SurfaceChartTestHelpers.CreateTile(Metadata, key, tileValue: key.LevelX + key.LevelY + key.TileX + key.TileY);
    }
}

internal sealed class ScriptedSurfaceTileSource : ISurfaceTileSource
{
    private readonly object _sync = new();
    private readonly Queue<Func<SurfaceTileKey, CancellationToken, Task<SurfaceTile?>>> _responses = [];
    private readonly List<SurfaceTileKey> _requestLog = [];
    private readonly List<TaskCompletionSource<bool>> _waiters = [];
    private readonly float _defaultTileValue;

    public ScriptedSurfaceTileSource(SurfaceMetadata metadata, float defaultTileValue)
    {
        Metadata = metadata;
        _defaultTileValue = defaultTileValue;
    }

    public SurfaceMetadata Metadata { get; }

    public IReadOnlyList<SurfaceTileKey> RequestLog
    {
        get
        {
            lock (_sync)
            {
                return _requestLog.ToArray();
            }
        }
    }

    public void EnqueueResponse(Func<SurfaceTileKey, CancellationToken, Task<SurfaceTile?>> response)
    {
        ArgumentNullException.ThrowIfNull(response);

        lock (_sync)
        {
            _responses.Enqueue(response);
        }
    }

    public void EnqueueSuccessResponse()
    {
        EnqueueResponse((key, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<SurfaceTile?>(CreateTile(key));
        });
    }

    public void EnqueuePendingResponse(
        TaskCompletionSource<bool> started,
        TaskCompletionSource<SurfaceTile?> completion,
        bool observeCancellation)
    {
        ArgumentNullException.ThrowIfNull(started);
        ArgumentNullException.ThrowIfNull(completion);

        EnqueueResponse(async (_, cancellationToken) =>
        {
            started.TrySetResult(true);

            if (!observeCancellation)
            {
                return await completion.Task.ConfigureAwait(false);
            }

            using var registration = cancellationToken.Register(
                static state => ((TaskCompletionSource<SurfaceTile?>)state!).TrySetCanceled(),
                completion);

            return await completion.Task.ConfigureAwait(false);
        });
    }

    public SurfaceTile CreateTile(SurfaceTileKey key)
    {
        return SurfaceChartTestHelpers.CreateTile(Metadata, key, _defaultTileValue);
    }

    public ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
    {
        Func<SurfaceTileKey, CancellationToken, Task<SurfaceTile?>>? response = null;

        lock (_sync)
        {
            _requestLog.Add(tileKey);
            for (var index = _waiters.Count - 1; index >= 0; index--)
            {
                _waiters[index].TrySetResult(true);
            }

            if (_responses.Count > 0)
            {
                response = _responses.Dequeue();
            }
        }

        if (response is null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult<SurfaceTile?>(CreateTile(tileKey));
        }

        return new ValueTask<SurfaceTile?>(response(tileKey, cancellationToken));
    }

    public Task WaitForRequestCountAsync(int expectedCount, TimeSpan? timeout = null)
    {
        return SurfaceChartTestHelpers.WaitForRequestCountAsync(_sync, _requestLog, _waiters, expectedCount, timeout);
    }

    public Task AssertRequestCountSettlesAtAsync(int expectedCount, TimeSpan? settlingDelay = null)
    {
        return SurfaceChartTestHelpers.AssertRequestCountSettlesAtAsync(_sync, _requestLog, expectedCount, settlingDelay);
    }
}

internal static class SurfaceChartTestHelpers
{
    public static SurfaceTile CreateTile(SurfaceMetadata metadata, SurfaceTileKey key, float tileValue)
    {
        var tileCountX = 1 << key.LevelX;
        var tileCountY = 1 << key.LevelY;
        var startX = (metadata.Width * key.TileX) / tileCountX;
        var endX = (metadata.Width * (key.TileX + 1)) / tileCountX;
        var startY = (metadata.Height * key.TileY) / tileCountY;
        var endY = (metadata.Height * (key.TileY + 1)) / tileCountY;
        var width = endX - startX;
        var height = endY - startY;
        var bounds = new SurfaceTileBounds(startX, startY, width, height);
        var values = new float[width * height];
        Array.Fill(values, tileValue);
        return new SurfaceTile(key, width, height, bounds, values, metadata.ValueRange);
    }

    public static async Task WaitForLoadedTileValuesAsync(SurfaceChartView view, float[] expectedValues, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(2);
        var deadline = Stopwatch.GetTimestamp() + (long)(timeout.Value.TotalSeconds * Stopwatch.Frequency);

        while (Stopwatch.GetTimestamp() < deadline)
        {
            if (GetLoadedTileValues(view).SequenceEqual(expectedValues))
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
        }

        GetLoadedTileValues(view).Should().Equal(expectedValues);
    }

    public static async Task AssertLoadedTileValuesStayAsync(SurfaceChartView view, float[] expectedValues, TimeSpan? settlingDelay = null)
    {
        settlingDelay ??= TimeSpan.FromMilliseconds(100);
        await Task.Delay(settlingDelay.Value).ConfigureAwait(false);
        GetLoadedTileValues(view).Should().Equal(expectedValues);
    }

    public static async Task WaitForRequestCountAsync(
        object sync,
        List<SurfaceTileKey> requestLog,
        List<TaskCompletionSource<bool>> waiters,
        int expectedCount,
        TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(2);
        var deadline = Stopwatch.GetTimestamp() + (long)(timeout.Value.TotalSeconds * Stopwatch.Frequency);

        while (GetRequestCount(sync, requestLog) < expectedCount)
        {
            var waiter = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (sync)
            {
                if (requestLog.Count >= expectedCount)
                {
                    return;
                }

                waiters.Add(waiter);
            }

            var remaining = deadline - Stopwatch.GetTimestamp();
            if (remaining <= 0)
            {
                throw new TimeoutException($"Timed out waiting for {expectedCount} requests. Actual count: {GetRequestCount(sync, requestLog)}.");
            }

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds((double)remaining / Stopwatch.Frequency));
            using var registration = timeoutCts.Token.Register(static state => ((TaskCompletionSource<bool>)state!).TrySetCanceled(), waiter);

            try
            {
                await waiter.Task.ConfigureAwait(false);
            }
            catch (TaskCanceledException ex)
            {
                throw new TimeoutException($"Timed out waiting for {expectedCount} requests. Actual count: {GetRequestCount(sync, requestLog)}.", ex);
            }
            finally
            {
                lock (sync)
                {
                    waiters.Remove(waiter);
                }
            }
        }
    }

    public static async Task AssertRequestCountSettlesAtAsync(
        object sync,
        List<SurfaceTileKey> requestLog,
        int expectedCount,
        TimeSpan? settlingDelay = null)
    {
        settlingDelay ??= TimeSpan.FromMilliseconds(100);
        await Task.Delay(settlingDelay.Value).ConfigureAwait(false);
        GetRequestCount(sync, requestLog).Should().Be(expectedCount);
    }

    private static int GetRequestCount(object sync, List<SurfaceTileKey> requestLog)
    {
        lock (sync)
        {
            return requestLog.Count;
        }
    }

    private static float[] GetLoadedTileValues(SurfaceChartView view)
    {
        var tileCacheField = typeof(SurfaceChartView).GetField("_tileCache", BindingFlags.Instance | BindingFlags.NonPublic);
        tileCacheField.Should().NotBeNull();

        var tileCache = tileCacheField!.GetValue(view);
        tileCache.Should().NotBeNull();

        var getLoadedTilesMethod = tileCache!.GetType().GetMethod("GetLoadedTiles", BindingFlags.Instance | BindingFlags.Public);
        getLoadedTilesMethod.Should().NotBeNull();

        var tiles = (IReadOnlyList<SurfaceTile>)getLoadedTilesMethod!.Invoke(tileCache, null)!;
        return tiles.SelectMany(static tile => tile.Values.Span.ToArray()).Distinct().ToArray();
    }
}

internal enum ScriptedRequestOutcome
{
    Canceled = 0,
    Faulted = 1,
    Null = 2
}
