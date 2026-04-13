using System.Diagnostics;
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
        var act = () =>
        {
            var view = new SurfaceChartView();
            view.Measure(new Size(320, 180));
            view.Arrange(new Rect(0, 0, 320, 180));
            view.Viewport = new SurfaceViewport(0, 0, 128, 128);
        };

        act.Should().NotThrow();
    }

    [Fact]
    public async Task SourceAssignment_IssuesOverviewFirstRequest()
    {
        var view = new SurfaceChartView();
        var source = new RecordingSurfaceTileSource(CreateMetadata());

        view.Source = source;

        await source.WaitForRequestCountAsync(1);
        await source.AssertRequestCountSettlesAtAsync(1);

        source.RequestLog.Should().Equal(new SurfaceTileKey(0, 0, 0, 0));
    }

    [Fact]
    public async Task ViewportChangesBeforeSourceAssignment_DoNotRequestTiles_AndSourceStillStartsWithOverview()
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
        var tileCountX = 1 << key.LevelX;
        var tileCountY = 1 << key.LevelY;
        var startX = (Metadata.Width * key.TileX) / tileCountX;
        var endX = (Metadata.Width * (key.TileX + 1)) / tileCountX;
        var startY = (Metadata.Height * key.TileY) / tileCountY;
        var endY = (Metadata.Height * (key.TileY + 1)) / tileCountY;
        var width = endX - startX;
        var height = endY - startY;
        var bounds = new SurfaceTileBounds(startX, startY, width, height);
        var values = new float[width * height];

        for (var index = 0; index < values.Length; index++)
        {
            values[index] = key.LevelX + key.LevelY + key.TileX + key.TileY;
        }

        return new SurfaceTile(key, width, height, bounds, values, Metadata.ValueRange);
    }
}
