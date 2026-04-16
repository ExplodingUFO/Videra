using System.Reflection;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartTileSchedulingTests
{
    [Fact]
    public Task SurfaceChartRuntime_ViewStateUpdatesDriveSchedulerThroughCompatibilityViewport()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
            var source = new RecordingSurfaceTileSource(metadata);
            var view = new SurfaceChartView();
            var expectedViewState = SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(256, 128, 512, 256));
            var expectedSelection = SurfaceLodPolicy.Default.Select(
                new SurfaceViewportRequest(metadata, expectedViewState.DataWindow, outputWidth: 256, outputHeight: 128));
            var expectedKeys = expectedSelection.EnumerateTileKeys().ToArray();

            view.Measure(new Size(256, 128));
            view.Arrange(new Rect(0, 0, 256, 128));
            view.Source = source;

            await source.WaitForRequestCountAsync(1);

            var runtime = SurfaceChartTestHelpers.GetRuntime(view);
            runtime.UpdateViewState(expectedViewState);

            await source.WaitForRequestCountAsync(1 + expectedKeys.Length);

            runtime.CurrentViewport.Should().Be(expectedViewState.ToViewport());
            runtime.ViewState.Should().Be(expectedViewState);
            source.RequestLog.Skip(1).Should().BeEquivalentTo(expectedKeys);
        });
    }

    [Fact]
    public Task ArrangedViewport_RequestsAdditionalTilesThroughScheduler()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
            var source = new RecordingSurfaceTileSource(metadata);
            var view = new SurfaceChartView
            {
                Source = source
            };

            await source.WaitForRequestCountAsync(1);

            view.Measure(new Size(256, 128));
            view.Arrange(new Rect(0, 0, 256, 128));

            var viewport = new SurfaceViewport(256, 128, 512, 256);
            var expectedSelection = SurfaceLodPolicy.Default.Select(
                new SurfaceViewportRequest(metadata, viewport, outputWidth: 256, outputHeight: 128));
            var expectedKeys = expectedSelection.EnumerateTileKeys().ToArray();

            view.Viewport = viewport;

            await source.WaitForRequestCountAsync(1 + expectedKeys.Length);

            source.RequestLog[0].Should().Be(new SurfaceTileKey(0, 0, 0, 0));
            source.RequestLog.Skip(1).Should().BeEquivalentTo(expectedKeys);
        });
    }

    [Fact]
    public Task ViewportPipeline_PrioritizesVisibleTilesBeforeOuterNeighborhood()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
            var source = new RecordingSurfaceTileSource(metadata);
            var view = new SurfaceChartView();

            view.Measure(new Size(128, 128));
            view.Arrange(new Rect(0, 0, 128, 128));

            var viewport = new SurfaceViewport(512, 512, 256, 256);
            var expectedOrderedKeys = GetPrioritizedKeys(metadata, viewport, outputWidth: 128, outputHeight: 128);
            view.Viewport = viewport;

            source.RequestLog.Should().BeEmpty();
            view.Source = source;

            await source.WaitForRequestCountAsync(1 + expectedOrderedKeys.Length);

            source.RequestLog[0].Should().Be(new SurfaceTileKey(0, 0, 0, 0));
            source.RequestLog.Skip(1).Should().Equal(expectedOrderedKeys);
        });
    }

    [Fact]
    public Task ViewportPipeline_DoesNotExceedMaxConcurrentRequests()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
            var source = new ConcurrencyTrackingSurfaceTileSource(
                metadata,
                defaultTileValue: 5,
                requestDelay: TimeSpan.FromMilliseconds(75));
            var view = new SurfaceChartView();

            view.Measure(new Size(128, 128));
            view.Arrange(new Rect(0, 0, 128, 128));

            var viewport = new SurfaceViewport(256, 256, 512, 512);
            var expectedSelection = SurfaceLodPolicy.Default.Select(
                new SurfaceViewportRequest(metadata, viewport, outputWidth: 128, outputHeight: 128));
            var expectedDetailRequestCount = expectedSelection.EnumerateTileKeys().Count();
            view.Viewport = viewport;

            source.RequestLog.Should().BeEmpty();
            view.Source = source;

            await source.WaitForRequestCountAsync(1 + expectedDetailRequestCount, timeout: TimeSpan.FromSeconds(5));
            await source.WaitForIdleAsync();

            source.MaxObservedConcurrency.Should().Be(4);
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public Task NonCommittedOverviewRequest_IsRetriedByNextPipeline(int outcome)
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 7);
            var firstRequestStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            switch ((ScriptedRequestOutcome)outcome)
            {
                case ScriptedRequestOutcome.Canceled:
                {
                    var completion = new TaskCompletionSource<SurfaceTile?>(TaskCreationOptions.RunContinuationsAsynchronously);
                    source.EnqueuePendingResponse(firstRequestStarted, completion, observeCancellation: true);
                    break;
                }

                case ScriptedRequestOutcome.Faulted:
                    source.EnqueueResponse(static (_, _) => Task.FromException<SurfaceTile?>(new InvalidOperationException("boom")));
                    break;

                case ScriptedRequestOutcome.Null:
                    source.EnqueueResponse(static (_, _) => Task.FromResult<SurfaceTile?>(null));
                    break;

                default:
                    throw new InvalidOperationException($"Unexpected scripted request outcome value {outcome}.");
            }

            source.EnqueueSuccessResponse();

            var view = new SurfaceChartView
            {
                Source = source
            };

            await source.WaitForRequestCountAsync(1);

            if ((ScriptedRequestOutcome)outcome == ScriptedRequestOutcome.Canceled)
            {
                await firstRequestStarted.Task;
            }

            view.Viewport = new SurfaceViewport(64, 64, 128, 128);

            await source.WaitForRequestCountAsync(2);
            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [7]);

            source.RequestLog.Should().Equal(
                new SurfaceTileKey(0, 0, 0, 0),
                new SurfaceTileKey(0, 0, 0, 0));
        });
    }

    [Fact]
    public async Task ViewportChange_PrunesToCurrentNeighborhoodInsteadOfDroppingAllDetailTiles()
    {
        var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
        var firstViewport = new SurfaceViewport(256, 256, 256, 256);
        var secondViewport = new SurfaceViewport(512, 256, 256, 256);
        var outputSize = new Size(64, 64);
        var firstSelectionKeys = GetSelectionKeys(metadata, firstViewport, outputWidth: 64, outputHeight: 64);
        var secondSelectionKeys = GetSelectionKeys(metadata, secondViewport, outputWidth: 64, outputHeight: 64);
        var retainedDetailKeys = firstSelectionKeys
            .Intersect(secondSelectionKeys)
            .OrderBy(static key => key.LevelY)
            .ThenBy(static key => key.LevelX)
            .ThenBy(static key => key.TileY)
            .ThenBy(static key => key.TileX)
            .ToArray();
        var overviewKey = new SurfaceTileKey(0, 0, 0, 0);
        var tileCache = new SurfaceTileCache();
        var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 2f);
        var blockedRequestStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var blockedCompletion = new TaskCompletionSource<SurfaceTile?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var controller = new SurfaceChartController(
            new SurfaceCameraController(firstViewport),
            tileCache,
            new SurfaceTileScheduler(tileCache, static () => { }),
            static () => { },
            static () => { });

        controller.UpdateViewSize(outputSize);
        controller.UpdateSource(source);
        await source.WaitForRequestCountAsync(1 + firstSelectionKeys.Length);
        await Task.Delay(100);

        var blockedRequestCount = secondSelectionKeys.Except(retainedDetailKeys).Count();
        for (var index = 0; index < blockedRequestCount; index++)
        {
            source.EnqueuePendingResponse(blockedRequestStarted, blockedCompletion, observeCancellation: true);
        }

        controller.UpdateViewport(secondViewport);
        await blockedRequestStarted.Task;

        var expectedKeys = new[] { overviewKey }.Concat(retainedDetailKeys).ToArray();
        tileCache.GetLoadedTiles().Select(static tile => tile.Key).Should().Equal(expectedKeys);
        retainedDetailKeys.Should().NotBeEmpty();

        controller.UpdateSource(null);
        await Task.Delay(100);
    }

    [Fact]
    public Task ViewportChange_PruneStaleDetailTiles_OnlyOverviewAndCurrentSelectionNeighborhoodRemain()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
            var source = new RecordingSurfaceTileSource(metadata);
            var view = new SurfaceChartView();

            view.Measure(new Size(256, 256));
            view.Arrange(new Rect(0, 0, 256, 256));
            view.Source = source;

            await source.WaitForRequestCountAsync(1);

            view.Viewport = new SurfaceViewport(0, 0, 512, 512);

            var firstSelection = SurfaceLodPolicy.Default.Select(
                new SurfaceViewportRequest(metadata, new SurfaceViewport(0, 0, 512, 512), 256, 256));
            var firstKeys = firstSelection.EnumerateTileKeys().ToArray();
            await source.WaitForRequestCountAsync(1 + firstKeys.Length);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view,
                SurfaceChartTestHelpers.GetLoadedTileKeys(view).Select(k => (float)(k.LevelX + k.LevelY + k.TileX + k.TileY)).Distinct().ToArray());

            view.Viewport = new SurfaceViewport(512, 512, 512, 512);

            var secondSelection = SurfaceLodPolicy.Default.Select(
                new SurfaceViewportRequest(metadata, new SurfaceViewport(512, 512, 512, 512), 256, 256));
            var secondKeys = secondSelection.EnumerateTileKeys().ToArray();
            await source.WaitForRequestCountAsync(1 + firstKeys.Length + 1 + secondKeys.Length);

            await Task.Delay(200).ConfigureAwait(false);

            var loadedKeys = SurfaceChartTestHelpers.GetLoadedTileKeys(view);
            var expectedKeys = new List<SurfaceTileKey> { new(0, 0, 0, 0) };
            expectedKeys.AddRange(secondKeys);
            loadedKeys.Should().BeEquivalentTo(expectedKeys, opts => opts.WithStrictOrdering());
        });
    }

    [Fact]
    public Task ViewSizeChange_PruneStaleDetailTiles_OnlyOverviewAndCurrentSelectionNeighborhoodRemain()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
            var source = new RecordingSurfaceTileSource(metadata);
            var view = new SurfaceChartView();

            view.Measure(new Size(256, 256));
            view.Arrange(new Rect(0, 0, 256, 256));
            view.Source = source;

            await source.WaitForRequestCountAsync(1);

            view.Viewport = new SurfaceViewport(0, 0, 512, 512);

            var firstSelection = SurfaceLodPolicy.Default.Select(
                new SurfaceViewportRequest(metadata, new SurfaceViewport(0, 0, 512, 512), 256, 256));
            var firstKeys = firstSelection.EnumerateTileKeys().ToArray();
            await source.WaitForRequestCountAsync(1 + firstKeys.Length);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view,
                SurfaceChartTestHelpers.GetLoadedTileKeys(view).Select(k => (float)(k.LevelX + k.LevelY + k.TileX + k.TileY)).Distinct().ToArray());

            view.Measure(new Size(512, 512));
            view.Arrange(new Rect(0, 0, 512, 512));

            await Task.Delay(300).ConfigureAwait(false);

            var secondSelection = SurfaceLodPolicy.Default.Select(
                new SurfaceViewportRequest(metadata, new SurfaceViewport(0, 0, 512, 512), 512, 512));
            var secondKeys = secondSelection.EnumerateTileKeys().ToArray();
            await source.WaitForRequestCountAsync(1 + firstKeys.Length + 1 + secondKeys.Length);

            await Task.Delay(200).ConfigureAwait(false);

            var loadedKeys = SurfaceChartTestHelpers.GetLoadedTileKeys(view);
            var expectedKeys = new List<SurfaceTileKey> { new(0, 0, 0, 0) };
            expectedKeys.AddRange(secondKeys);
            loadedKeys.Should().BeEquivalentTo(expectedKeys, opts => opts.WithStrictOrdering());
        });
    }

    [Fact]
    public void ViewSizeChange_SameSize_IsNoOp()
    {
        var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
        var overviewKey = new SurfaceTileKey(0, 0, 0, 0);
        var detailKey = new SurfaceTileKey(1, 1, 0, 0);
        var tileCache = new SurfaceTileCache();
        var clearFailureCount = 0;
        var invalidateCount = 0;
        var controller = new SurfaceChartController(
            new SurfaceCameraController(new SurfaceViewport(0, 0, 512, 512)),
            tileCache,
            new SurfaceTileScheduler(tileCache, static () => { }),
            () => clearFailureCount++,
            () => invalidateCount++);

        controller.UpdateViewSize(new Size(256, 256));

        tileCache.TryMarkRequested(overviewKey, requestGeneration: 1).Should().BeTrue();
        tileCache.TryStore(SurfaceChartTestHelpers.CreateTile(metadata, overviewKey, tileValue: 1f), requestGeneration: 1).Should().BeTrue();
        tileCache.TryMarkRequested(detailKey, requestGeneration: 1).Should().BeTrue();
        tileCache.TryStore(SurfaceChartTestHelpers.CreateTile(metadata, detailKey, tileValue: 2f), requestGeneration: 1).Should().BeTrue();

        clearFailureCount = 0;
        invalidateCount = 0;

        controller.UpdateViewSize(new Size(256, 256));

        tileCache.GetLoadedTiles().Select(static tile => tile.Key).Should().Equal(overviewKey, detailKey);
        clearFailureCount.Should().Be(0);
        invalidateCount.Should().Be(0);
    }

    [Fact]
    public void ViewSizeChange_ToZero_PrunesStaleDetailTiles_AndInvalidatesRenderScene()
    {
        var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
        var overviewKey = new SurfaceTileKey(0, 0, 0, 0);
        var detailKey = new SurfaceTileKey(1, 1, 0, 0);
        var tileCache = new SurfaceTileCache();
        var invalidateCount = 0;
        var controller = new SurfaceChartController(
            new SurfaceCameraController(new SurfaceViewport(0, 0, 512, 512)),
            tileCache,
            new SurfaceTileScheduler(tileCache, static () => { }),
            static () => { },
            () => invalidateCount++);

        controller.UpdateViewSize(new Size(256, 256));
        invalidateCount = 0;

        tileCache.TryMarkRequested(overviewKey, requestGeneration: 1).Should().BeTrue();
        tileCache.TryStore(SurfaceChartTestHelpers.CreateTile(metadata, overviewKey, tileValue: 1f), requestGeneration: 1).Should().BeTrue();
        tileCache.TryMarkRequested(detailKey, requestGeneration: 1).Should().BeTrue();
        tileCache.TryStore(SurfaceChartTestHelpers.CreateTile(metadata, detailKey, tileValue: 2f), requestGeneration: 1).Should().BeTrue();

        controller.UpdateViewSize(new Size(0, 0));

        tileCache.GetLoadedTiles().Select(static tile => tile.Key).Should().Equal(overviewKey);
        invalidateCount.Should().Be(1);
    }

    [Fact]
    public async Task InteractiveQuality_RequestPlanUsesCoarserSelectionThanRefine()
    {
        var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
        var viewport = new SurfaceViewport(0d, 0d, metadata.Width, metadata.Height);
        var outputSize = new Size(256, 256);
        var refineSource = new RecordingSurfaceTileSource(metadata);
        var interactiveSource = new RecordingSurfaceTileSource(metadata);
        var refineTileCache = new SurfaceTileCache();
        var interactiveTileCache = new SurfaceTileCache();
        var qualityMethod = typeof(SurfaceChartController).GetMethod(
            "UpdateInteractionQuality",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        qualityMethod.Should().NotBeNull("Phase 20 should expose an explicit interaction-quality seam on the controller.");
        var qualityType = qualityMethod!.GetParameters().Single().ParameterType;
        var interactiveQuality = Enum.Parse(qualityType, "Interactive");

        var refineController = new SurfaceChartController(
            new SurfaceCameraController(viewport),
            refineTileCache,
            new SurfaceTileScheduler(refineTileCache, static () => { }),
            static () => { },
            static () => { });
        var interactiveController = new SurfaceChartController(
            new SurfaceCameraController(viewport),
            interactiveTileCache,
            new SurfaceTileScheduler(interactiveTileCache, static () => { }),
            static () => { },
            static () => { });

        refineController.UpdateViewSize(outputSize);
        refineController.UpdateSource(refineSource);

        var refineExpectedKeys = GetPrioritizedKeys(metadata, viewport, outputWidth: 256, outputHeight: 256);
        await refineSource.WaitForRequestCountAsync(1 + refineExpectedKeys.Length);

        interactiveController.UpdateViewSize(outputSize);
        qualityMethod.Invoke(interactiveController, [interactiveQuality]);
        interactiveController.UpdateSource(interactiveSource);

        var interactiveExpectedKeys = GetPrioritizedKeys(metadata, viewport, outputWidth: 512, outputHeight: 512);
        await interactiveSource.WaitForRequestCountAsync(1 + interactiveExpectedKeys.Length);

        refineSource.RequestLog.Skip(1).Should().Equal(refineExpectedKeys);
        interactiveSource.RequestLog.Skip(1).Should().Equal(interactiveExpectedKeys);
        interactiveExpectedKeys.Length.Should().BeLessThan(refineExpectedKeys.Length);
    }

    [Fact]
    public Task BatchCapableSource_UsesOrderedGetTilesAsyncForDetailRequests()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
            var source = new BatchRecordingSurfaceTileSource(metadata);
            var view = new SurfaceChartView();

            view.Measure(new Size(128, 128));
            view.Arrange(new Rect(0, 0, 128, 128));

            var viewport = new SurfaceViewport(512, 512, 256, 256);
            var expectedOrderedKeys = GetPrioritizedKeys(metadata, viewport, outputWidth: 128, outputHeight: 128);
            view.Viewport = viewport;

            view.Source = source;

            await source.WaitForTotalRequestCountAsync(1 + expectedOrderedKeys.Length);

            source.SingleRequestLog.Should().Equal(new SurfaceTileKey(0, 0, 0, 0));
            source.BatchRequestLog.Should().NotBeEmpty();
            source.BatchRequestLog.SelectMany(static batch => batch).Should().Equal(expectedOrderedKeys);
        });
    }

    [Fact]
    public async Task BatchCapableSource_SplitsDetailRequestsToSchedulerBatchSize()
    {
        var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
        var viewport = new SurfaceViewport(0, 0, metadata.Width, metadata.Height);
        var outputSize = new Size(256, 256);
        var expectedOrderedKeys = GetPrioritizedKeys(metadata, viewport, outputWidth: 256, outputHeight: 256);
        var source = new BatchRecordingSurfaceTileSource(metadata);
        var tileCache = new SurfaceTileCache();
        var scheduler = new SurfaceTileScheduler(tileCache, static () => { }, maxConcurrentRequests: 3);
        var controller = new SurfaceChartController(
            new SurfaceCameraController(viewport),
            tileCache,
            scheduler,
            static () => { },
            static () => { });

        controller.UpdateViewSize(outputSize);
        controller.UpdateSource(source);

        await source.WaitForTotalRequestCountAsync(1 + expectedOrderedKeys.Length);

        source.SingleRequestLog.Should().Equal(new SurfaceTileKey(0, 0, 0, 0));
        source.BatchRequestLog.Should().NotBeEmpty();
        source.BatchRequestLog.Should().OnlyContain(batch => batch.Count <= 3);
        source.BatchRequestLog.SelectMany(static batch => batch).Should().Equal(expectedOrderedKeys);
    }

    private static SurfaceTileKey[] GetSelectionKeys(
        SurfaceMetadata metadata,
        SurfaceViewport viewport,
        int outputWidth,
        int outputHeight)
    {
        var selection = SurfaceLodPolicy.Default.Select(
            new SurfaceViewportRequest(metadata, viewport, outputWidth, outputHeight));
        return selection.EnumerateTileKeys().ToArray();
    }

    private static SurfaceTileKey[] GetPrioritizedKeys(
        SurfaceMetadata metadata,
        SurfaceViewport viewport,
        int outputWidth,
        int outputHeight)
    {
        var request = new SurfaceViewportRequest(metadata, viewport, outputWidth, outputHeight);
        var selection = SurfaceLodPolicy.Default.Select(request);
        var clampedViewport = request.ClampedViewport;
        var tileCountX = 1 << selection.LevelX;
        var tileCountY = 1 << selection.LevelY;
        var visibleTileXStart = GetTileIndexStart(clampedViewport.StartX, metadata.Width, tileCountX);
        var visibleTileXEnd = GetTileIndexEnd(clampedViewport.EndXExclusive, metadata.Width, tileCountX);
        var visibleTileYStart = GetTileIndexStart(clampedViewport.StartY, metadata.Height, tileCountY);
        var visibleTileYEnd = GetTileIndexEnd(clampedViewport.EndYExclusive, metadata.Height, tileCountY);
        var centerTileX = GetTileIndexStart(clampedViewport.StartX + (clampedViewport.Width / 2d), metadata.Width, tileCountX);
        var centerTileY = GetTileIndexStart(clampedViewport.StartY + (clampedViewport.Height / 2d), metadata.Height, tileCountY);

        return selection.EnumerateTileKeys()
            .Select(static (key, sequence) => new { key, sequence })
            .OrderBy(entry => IsVisible(entry.key, visibleTileXStart, visibleTileXEnd, visibleTileYStart, visibleTileYEnd) ? 0 : 1)
            .ThenBy(entry => Math.Abs(entry.key.TileX - centerTileX) + Math.Abs(entry.key.TileY - centerTileY))
            .ThenBy(entry => entry.sequence)
            .Select(static entry => entry.key)
            .ToArray();
    }

    private static bool IsVisible(
        SurfaceTileKey key,
        int visibleTileXStart,
        int visibleTileXEnd,
        int visibleTileYStart,
        int visibleTileYEnd)
    {
        return key.TileX >= visibleTileXStart &&
               key.TileX <= visibleTileXEnd &&
               key.TileY >= visibleTileYStart &&
               key.TileY <= visibleTileYEnd;
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
}

internal sealed class BatchRecordingSurfaceTileSource : ISurfaceTileSource, ISurfaceTileBatchSource
{
    private readonly object _sync = new();
    private readonly List<SurfaceTileKey> _allRequestLog = [];
    private readonly List<SurfaceTileKey> _singleRequestLog = [];
    private readonly List<IReadOnlyList<SurfaceTileKey>> _batchRequestLog = [];
    private readonly List<TaskCompletionSource<bool>> _waiters = [];

    public BatchRecordingSurfaceTileSource(SurfaceMetadata metadata)
    {
        Metadata = metadata;
    }

    public SurfaceMetadata Metadata { get; }

    public IReadOnlyList<SurfaceTileKey> SingleRequestLog
    {
        get
        {
            lock (_sync)
            {
                return _singleRequestLog.ToArray();
            }
        }
    }

    public IReadOnlyList<IReadOnlyList<SurfaceTileKey>> BatchRequestLog
    {
        get
        {
            lock (_sync)
            {
                return _batchRequestLog.Select(static batch => (IReadOnlyList<SurfaceTileKey>)batch.ToArray()).ToArray();
            }
        }
    }

    public ValueTask<SurfaceTile?> GetTileAsync(SurfaceTileKey tileKey, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            _singleRequestLog.Add(tileKey);
            _allRequestLog.Add(tileKey);
            NotifyWaiters();
        }

        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<SurfaceTile?>(CreateTile(tileKey));
    }

    public ValueTask<IReadOnlyList<SurfaceTile?>> GetTilesAsync(
        IReadOnlyList<SurfaceTileKey> tileKeys,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tileKeys);

        lock (_sync)
        {
            var copiedKeys = tileKeys.ToArray();
            _batchRequestLog.Add(copiedKeys);
            _allRequestLog.AddRange(copiedKeys);
            NotifyWaiters();
        }

        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IReadOnlyList<SurfaceTile?>>(tileKeys.Select(CreateTile).Cast<SurfaceTile?>().ToArray());
    }

    public Task WaitForTotalRequestCountAsync(int expectedCount, TimeSpan? timeout = null)
    {
        return SurfaceChartTestHelpers.WaitForRequestCountAsync(_sync, _allRequestLog, _waiters, expectedCount, timeout);
    }

    private SurfaceTile CreateTile(SurfaceTileKey key)
    {
        return SurfaceChartTestHelpers.CreateTile(Metadata, key, tileValue: key.LevelX + key.LevelY + key.TileX + key.TileY);
    }

    private void NotifyWaiters()
    {
        for (var index = _waiters.Count - 1; index >= 0; index--)
        {
            _waiters[index].TrySetResult(true);
        }
    }
}
