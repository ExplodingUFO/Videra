using System.Diagnostics;
using System.Reflection;
using System.Collections.Concurrent;
using Avalonia;
using Avalonia.Threading;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartViewLifecycleTests
{
    [Fact]
    public void SurfaceChartView_DelegatesAuthoritativeStateToSurfaceChartRuntime()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var runtimeField = typeof(SurfaceChartView).GetField("_runtime", BindingFlags.Instance | BindingFlags.NonPublic);
            runtimeField.Should().NotBeNull();
            runtimeField!.FieldType.Should().Be(typeof(SurfaceChartRuntime));
            typeof(SurfaceChartView).GetField("_tileCache", BindingFlags.Instance | BindingFlags.NonPublic).Should().BeNull();

            var overlayCoordinatorField = typeof(SurfaceChartView).GetField("_overlayCoordinator", BindingFlags.Instance | BindingFlags.NonPublic);
            overlayCoordinatorField.Should().NotBeNull();
            overlayCoordinatorField!.FieldType.Should().Be(typeof(SurfaceChartOverlayCoordinator));

            var view = new SurfaceChartView();
            var runtime = SurfaceChartTestHelpers.GetRuntime(view);
            var overlayCoordinator = SurfaceChartTestHelpers.GetOverlayCoordinator(view);

            runtime.ViewState.DataWindow.Should().Be(view.Viewport.ToDataWindow());
            runtime.GetLoadedTiles().Should().BeEmpty();
            overlayCoordinator.AxisState.Should().BeSameAs(SurfaceAxisOverlayState.Empty);
            overlayCoordinator.LegendState.Should().BeSameAs(SurfaceLegendOverlayState.Empty);
            overlayCoordinator.ProbeState.Should().BeSameAs(SurfaceProbeOverlayState.Empty);
        });
    }

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
    public Task SameSizeArrange_DoesNotAppendRequestsOrClearLastTileFailure()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = CreateMetadata();
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 7);
            source.EnqueueSuccessResponse();
            source.EnqueueResponse(static (_, _) => Task.FromException<SurfaceTile?>(new InvalidOperationException("detail tile fault")));

            var view = new SurfaceChartView();
            view.Measure(new Size(256, 256));
            view.Arrange(new Rect(0, 0, 256, 256));
            view.Source = source;

            await source.WaitForRequestCountAsync(1);

            view.Viewport = new SurfaceViewport(0, 0, 512, 512);

            await SurfaceChartTestHelpers.WaitForFailureAsync(view);

            var requestCount = source.RequestLog.Count;
            var failure = view.LastTileFailure;

            failure.Should().NotBeNull();
            failure!.Exception.Message.Should().Be("detail tile fault");

            view.Measure(new Size(256, 256));
            view.Arrange(new Rect(0, 0, 256, 256));

            view.LastTileFailure.Should().BeSameAs(failure);
            view.LastTileFailure!.Exception.Message.Should().Be("detail tile fault");
            await source.AssertRequestCountSettlesAtAsync(requestCount);
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

    [Fact]
    public Task SourceSwap_DoesNotAllowStaleRequestsToRepopulateActiveTiles()
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

    [Fact]
    public Task RapidSourceReplacement_DoesNotPublishLateFailuresFromSupersededSource()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = CreateMetadata();
            var staleSource = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 11);
            var activeSource = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 42);
            var staleRequestStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var staleCompletion = new TaskCompletionSource<SurfaceTile?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var staleRequestCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var failureEvents = new List<SurfaceChartTileRequestFailedEventArgs>();
            var view = new SurfaceChartView();
            view.TileRequestFailed += (_, args) => failureEvents.Add(args);
            view.Measure(new Size(256, 256));
            view.Arrange(new Rect(0, 0, 256, 256));
            view.Viewport = new SurfaceViewport(0, 0, 512, 512);

            staleSource.EnqueueSuccessResponse();

            staleSource.EnqueueResponse(async (_, _) =>
            {
                staleRequestStarted.TrySetResult(true);

                try
                {
                    return await staleCompletion.Task.ConfigureAwait(false);
                }
                finally
                {
                    staleRequestCompleted.TrySetResult(true);
                }
            });

            view.Source = staleSource;

            await staleRequestStarted.Task;

            view.Source = activeSource;

            await activeSource.WaitForRequestCountAsync(1);
            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [42]);

            staleCompletion.SetException(new InvalidOperationException("stale fault"));

            await staleRequestCompleted.Task.ConfigureAwait(false);
            await staleSource.AssertRequestCountSettlesAtAsync(1);
            await activeSource.AssertRequestCountSettlesAtAsync(1);
            await Task.Delay(100).ConfigureAwait(false);

            failureEvents.Should().BeEmpty();
            view.LastTileFailure.Should().BeNull();
            await SurfaceChartTestHelpers.AssertLoadedTileValuesStayAsync(view, [42]);
        });
    }

    [Fact]
    public async Task ControllerSourceReplacement_DoesNotPublishLateFailuresFromSupersededGeneration()
    {
        var metadata = CreateMetadata();
        var expectedPipelineRequestCount = GetCameraAwarePipelineRequestCount(
            metadata,
            new SurfaceViewport(0, 0, 512, 512),
            new Size(256, 256));
        var staleSource = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 11);
        var activeSource = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 42);
        var staleRequestStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var staleCompletion = new TaskCompletionSource<SurfaceTile?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var staleRequestCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var failures = new ConcurrentQueue<(SurfaceTileKey TileKey, Exception Exception)>();
        var tileCache = new SurfaceTileCache();
        var scheduler = new SurfaceTileScheduler(tileCache, static () => { }, (key, exception) => failures.Enqueue((key, exception)));
        var controller = new SurfaceChartController(
            new SurfaceCameraController(new SurfaceViewport(0, 0, 512, 512)),
            tileCache,
            scheduler,
            static () => { },
            static () => { });

        controller.UpdateViewSize(new Size(256, 256));

        staleSource.EnqueueSuccessResponse();
        staleSource.EnqueueResponse(async (_, _) =>
        {
            staleRequestStarted.TrySetResult(true);

            try
            {
                return await staleCompletion.Task.ConfigureAwait(false);
            }
            finally
            {
                staleRequestCompleted.TrySetResult(true);
            }
        });

        controller.UpdateSource(staleSource);

        await staleRequestStarted.Task;

        controller.UpdateSource(activeSource);

        await activeSource.WaitForRequestCountAsync(expectedPipelineRequestCount);

        staleCompletion.SetException(new InvalidOperationException("stale fault"));

        await staleRequestCompleted.Task;
        await staleSource.AssertRequestCountSettlesAtAsync(expectedPipelineRequestCount);
        await Task.Delay(100);

        failures.Should().BeEmpty();
    }

    [Fact]
    public async Task ControllerResizeSupersession_IgnoresLateFailuresDuringResizeTransition()
    {
        var metadata = CreateMetadata();
        var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 11);
        var staleRequestStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var staleCompletion = new TaskCompletionSource<SurfaceTile?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var staleRequestCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var failures = new ConcurrentQueue<(SurfaceTileKey TileKey, Exception Exception)>();
        var allowResizeToContinue = new ManualResetEventSlim(false);
        var resizeEnteredInvalidate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var shouldBlockInvalidate = 0;
        var tileCache = new SurfaceTileCache();
        var scheduler = new SurfaceTileScheduler(tileCache, static () => { }, (key, exception) => failures.Enqueue((key, exception)));
        var controller = new SurfaceChartController(
            new SurfaceCameraController(new SurfaceViewport(0, 0, 512, 512)),
            tileCache,
            scheduler,
            static () => { },
            () =>
            {
                if (Volatile.Read(ref shouldBlockInvalidate) == 0)
                {
                    return;
                }

                resizeEnteredInvalidate.TrySetResult(true);
                allowResizeToContinue.Wait(TimeSpan.FromSeconds(2)).Should().BeTrue();
            });

        controller.UpdateViewSize(new Size(256, 256));

        source.EnqueueSuccessResponse();
        source.EnqueueResponse(async (_, _) =>
        {
            staleRequestStarted.TrySetResult(true);

            try
            {
                return await staleCompletion.Task.ConfigureAwait(false);
            }
            finally
            {
                staleRequestCompleted.TrySetResult(true);
            }
        });

        controller.UpdateSource(source);

        await staleRequestStarted.Task;

        Volatile.Write(ref shouldBlockInvalidate, 1);
        var resizeTask = Task.Run(() => controller.UpdateViewSize(new Size(384, 384)));

        await resizeEnteredInvalidate.Task;

        staleCompletion.SetException(new InvalidOperationException("stale resize fault"));

        await staleRequestCompleted.Task;
        await Task.Delay(100);

        failures.Should().BeEmpty();

        allowResizeToContinue.Set();
        await resizeTask;
        await Task.Delay(100);
        failures.Should().BeEmpty();
    }

    [Fact]
    public Task FailingOverviewRequest_SetsLastTileFailureAndRaisesEvent()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = CreateMetadata();
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 5);
            source.EnqueueResponse(static (_, _) => Task.FromException<SurfaceTile?>(new InvalidOperationException("tile fault")));
            var view = new SurfaceChartView
            {
                Source = source
            };

            await SurfaceChartTestHelpers.WaitForFailureAsync(view);

            view.LastTileFailure.Should().NotBeNull();
            view.LastTileFailure!.TileKey.Should().Be(new SurfaceTileKey(0, 0, 0, 0));
            view.LastTileFailure.Exception.Should().BeOfType<InvalidOperationException>();
        });
    }

    [Fact]
    public void FailureDiagnostics_AreNotExposedAsPublicControlApi()
    {
        var members = typeof(SurfaceChartView)
            .GetMembers(BindingFlags.Instance | BindingFlags.Public)
            .Select(static member => member.Name)
            .ToArray();

        members.Should().NotContain("TileRequestFailed");
        members.Should().NotContain("LastTileFailure");
        typeof(SurfaceChartTileRequestFailedEventArgs).IsPublic.Should().BeFalse();
    }

    [Fact]
    public Task FailingOverviewRequest_PublishesFailureOnUiThread()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var failureMethod = typeof(SurfaceChartView).GetMethod("OnTileRequestFailed", BindingFlags.Instance | BindingFlags.NonPublic);
            var eventRaised = new TaskCompletionSource<SurfaceChartTileRequestFailedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
            var uiThreadId = Environment.CurrentManagedThreadId;
            var callbackThreadId = -1;
            var view = new SurfaceChartView();
            view.TileRequestFailed += (_, args) =>
            {
                callbackThreadId = Environment.CurrentManagedThreadId;
                Dispatcher.UIThread.CheckAccess().Should().BeTrue();
                view.LastTileFailure.Should().BeSameAs(args);
                eventRaised.TrySetResult(args);
            };

            failureMethod.Should().NotBeNull();

            var worker = new Thread(() => failureMethod!.Invoke(view, [new SurfaceTileKey(0, 0, 0, 0), new InvalidOperationException("tile fault")]));
            worker.Start();

            var failureArgs = await eventRaised.Task.WaitAsync(TimeSpan.FromSeconds(2));
            worker.Join();

            callbackThreadId.Should().Be(uiThreadId);
            view.LastTileFailure.Should().BeSameAs(failureArgs);
        });
    }

    [Fact]
    public Task SuccessfulRequestAfterFailure_ClearsLastTileFailure()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = CreateMetadata();
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 9);
            source.EnqueueResponse(static (_, _) => Task.FromException<SurfaceTile?>(new InvalidOperationException("tile fault")));
            source.EnqueueSuccessResponse();
            var view = new SurfaceChartView
            {
                Source = source
            };

            await SurfaceChartTestHelpers.WaitForFailureAsync(view);

            view.LastTileFailure.Should().NotBeNull();

            view.Viewport = new SurfaceViewport(0, 0, 512, 512);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [9]);

            view.LastTileFailure.Should().BeNull();
        });
    }

    [Fact]
    public Task SameGenerationSuccessNotification_AfterFailure_PreservesLastTileFailure()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var failureMethod = typeof(SurfaceChartView).GetMethod("OnTileRequestFailed", BindingFlags.Instance | BindingFlags.NonPublic);
            var tilesChangedMethod = typeof(SurfaceChartView).GetMethod("NotifyTilesChanged", BindingFlags.Instance | BindingFlags.NonPublic);
            var failurePublished = new TaskCompletionSource<SurfaceChartTileRequestFailedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
            var view = new SurfaceChartView();
            view.TileRequestFailed += (_, args) => failurePublished.TrySetResult(args);

            failureMethod.Should().NotBeNull();
            tilesChangedMethod.Should().NotBeNull();

            failureMethod!.Invoke(view, [new SurfaceTileKey(1, 1, 0, 0), new InvalidOperationException("detail tile fault")]);

            var failureArgs = await failurePublished.Task.WaitAsync(TimeSpan.FromSeconds(2));
            view.LastTileFailure.Should().BeSameAs(failureArgs);

            tilesChangedMethod!.Invoke(view, null);

            view.LastTileFailure.Should().BeSameAs(failureArgs);
            view.LastTileFailure!.TileKey.Should().Be(new SurfaceTileKey(1, 1, 0, 0));
            view.LastTileFailure.Exception.Should().BeOfType<InvalidOperationException>();
        });
    }

    [Fact]
    public Task SourceReplacement_ClearsLastTileFailure()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = CreateMetadata();
            var faultingSource = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 3);
            faultingSource.EnqueueResponse(static (_, _) => Task.FromException<SurfaceTile?>(new InvalidOperationException("tile fault")));
            var view = new SurfaceChartView
            {
                Source = faultingSource
            };

            await SurfaceChartTestHelpers.WaitForFailureAsync(view);

            view.LastTileFailure.Should().NotBeNull();

            var freshSource = new RecordingSurfaceTileSource(metadata);
            view.Source = freshSource;

            await Task.Delay(100).ConfigureAwait(false);

            view.LastTileFailure.Should().BeNull();
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

    private static int GetCameraAwarePipelineRequestCount(
        SurfaceMetadata metadata,
        SurfaceViewport viewport,
        Size viewSize,
        SurfaceChartInteractionQuality interactionQuality = SurfaceChartInteractionQuality.Refine)
    {
        var tileCache = new SurfaceTileCache();
        var scheduler = new SurfaceTileScheduler(tileCache, static () => { });
        scheduler.UpdateSource(new RecordingSurfaceTileSource(metadata));

        var viewState = new SurfaceCameraController(viewport).CurrentViewState;
        var cameraFrame = SurfaceProjectionMath.CreateCameraFrame(metadata, viewState, viewSize.Width, viewSize.Height, 1f);
        return scheduler.CreateRequestPlan(viewState, cameraFrame, viewSize, interactionQuality).OrderedKeys.Count;
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

internal sealed class ConcurrencyTrackingSurfaceTileSource : ISurfaceTileSource
{
    private static readonly SurfaceTileKey OverviewKey = new(0, 0, 0, 0);

    private readonly object _sync = new();
    private readonly List<SurfaceTileKey> _requestLog = [];
    private readonly List<TaskCompletionSource<bool>> _waiters = [];
    private readonly TimeSpan _requestDelay;
    private readonly float _defaultTileValue;
    private int _inFlightRequests;
    private int _maxObservedConcurrency;

    public ConcurrencyTrackingSurfaceTileSource(
        SurfaceMetadata metadata,
        float defaultTileValue,
        TimeSpan requestDelay)
    {
        Metadata = metadata;
        _defaultTileValue = defaultTileValue;
        _requestDelay = requestDelay;
    }

    public SurfaceMetadata Metadata { get; }

    public int MaxObservedConcurrency => Volatile.Read(ref _maxObservedConcurrency);

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

    public async ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            _requestLog.Add(tileKey);
            for (var index = _waiters.Count - 1; index >= 0; index--)
            {
                _waiters[index].TrySetResult(true);
            }
        }

        if (tileKey == OverviewKey)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return CreateTile(tileKey);
        }

        var currentConcurrency = Interlocked.Increment(ref _inFlightRequests);
        UpdateMaxObservedConcurrency(currentConcurrency);

        try
        {
            await Task.Delay(_requestDelay, cancellationToken).ConfigureAwait(false);
            return CreateTile(tileKey);
        }
        finally
        {
            Interlocked.Decrement(ref _inFlightRequests);
        }
    }

    public Task WaitForRequestCountAsync(int expectedCount, TimeSpan? timeout = null)
    {
        return SurfaceChartTestHelpers.WaitForRequestCountAsync(_sync, _requestLog, _waiters, expectedCount, timeout);
    }

    public async Task WaitForIdleAsync(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(5);
        var deadline = Stopwatch.GetTimestamp() + (long)(timeout.Value.TotalSeconds * Stopwatch.Frequency);

        while (Volatile.Read(ref _inFlightRequests) != 0 && Stopwatch.GetTimestamp() < deadline)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
        }

        Volatile.Read(ref _inFlightRequests).Should().Be(0);
    }

    private SurfaceTile CreateTile(SurfaceTileKey key)
    {
        return SurfaceChartTestHelpers.CreateTile(Metadata, key, _defaultTileValue);
    }

    private void UpdateMaxObservedConcurrency(int currentConcurrency)
    {
        while (true)
        {
            var observed = Volatile.Read(ref _maxObservedConcurrency);
            if (currentConcurrency <= observed)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref _maxObservedConcurrency, currentConcurrency, observed) == observed)
            {
                return;
            }
        }
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
    public static SurfaceChartRuntime GetRuntime(SurfaceChartView view)
    {
        var runtimeField = typeof(SurfaceChartView).GetField("_runtime", BindingFlags.Instance | BindingFlags.NonPublic);
        runtimeField.Should().NotBeNull();
        return (SurfaceChartRuntime)runtimeField!.GetValue(view)!;
    }

    public static SurfaceChartOverlayCoordinator GetOverlayCoordinator(SurfaceChartView view)
    {
        var coordinatorField = typeof(SurfaceChartView).GetField("_overlayCoordinator", BindingFlags.Instance | BindingFlags.NonPublic);
        coordinatorField.Should().NotBeNull();
        return (SurfaceChartOverlayCoordinator)coordinatorField!.GetValue(view)!;
    }

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

    public static SurfaceTileKey[] GetLoadedTileKeys(SurfaceChartView view)
    {
        var tiles = GetRuntime(view).GetLoadedTiles();
        return tiles.Select(static tile => tile.Key).ToArray();
    }

    public static SurfaceTileKey[] GetRenderSceneTileKeys(SurfaceChartView view)
    {
        var scene = GetRenderSceneFromView(view);
        if (scene is null)
        {
            return [];
        }

        var tilesField = typeof(SurfaceRenderScene).GetProperty("Tiles", BindingFlags.Instance | BindingFlags.Public);
        tilesField.Should().NotBeNull();

        var tiles = (IReadOnlyList<SurfaceRenderTile>)tilesField!.GetValue(scene)!;
        return tiles.Select(static tile => tile.Key).ToArray();
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

    public static async Task WaitForFailureAsync(SurfaceChartView view, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(2);
        var deadline = Stopwatch.GetTimestamp() + (long)(timeout.Value.TotalSeconds * Stopwatch.Frequency);

        while (Stopwatch.GetTimestamp() < deadline)
        {
            if (view.LastTileFailure is not null)
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
        }

        view.LastTileFailure.Should().NotBeNull("a tile failure should have been reported within the timeout");
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
        var tiles = GetRuntime(view).GetLoadedTiles();
        return tiles.SelectMany(static tile => tile.Values.Span.ToArray()).Distinct().ToArray();
    }

    private static SurfaceRenderScene? GetRenderSceneFromView(SurfaceChartView view)
    {
        var field = typeof(SurfaceChartView).GetField("_renderScene", BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();
        return (SurfaceRenderScene?)field!.GetValue(view);
    }
}

internal enum ScriptedRequestOutcome
{
    Canceled = 0,
    Faulted = 1,
    Null = 2
}
