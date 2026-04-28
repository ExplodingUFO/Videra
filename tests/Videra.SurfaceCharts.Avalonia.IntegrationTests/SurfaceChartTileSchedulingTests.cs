using System.Reflection;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartTileSchedulingTests
{
    [Fact]
    public Task SurfaceChartRuntime_ViewStateUpdatesDriveScheduler()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
            var source = new RecordingSurfaceTileSource(metadata);
            var view = new VideraChartView();
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
            var view = new VideraChartView
            {
                Source = source
            };

            await source.WaitForRequestCountAsync(1);

            view.Measure(new Size(256, 128));
            view.Arrange(new Rect(0, 0, 256, 128));

            var viewport = new SurfaceViewport(256, 128, 512, 256);
            var expectedSelection = SurfaceLodPolicy.Default.Select(
                new SurfaceViewportRequest(metadata, viewport.ToDataWindow(), outputWidth: 256, outputHeight: 128));
            var expectedKeys = expectedSelection.EnumerateTileKeys().ToArray();

            view.ViewState = new SurfaceViewState((viewport).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);

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
            var view = new VideraChartView();

            view.Measure(new Size(128, 128));
            view.Arrange(new Rect(0, 0, 128, 128));

            var viewport = new SurfaceViewport(512, 512, 256, 256);
            var expectedOrderedKeys = GetPrioritizedKeys(metadata, viewport, outputWidth: 128, outputHeight: 128);
            view.ViewState = new SurfaceViewState((viewport).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);

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
            var view = new VideraChartView();

            view.Measure(new Size(128, 128));
            view.Arrange(new Rect(0, 0, 128, 128));

            var viewport = new SurfaceViewport(256, 256, 512, 512);
            var expectedSelection = SurfaceLodPolicy.Default.Select(
                new SurfaceViewportRequest(metadata, viewport.ToDataWindow(), outputWidth: 128, outputHeight: 128));
            var expectedDetailRequestCount = expectedSelection.EnumerateTileKeys().Count();
            view.ViewState = new SurfaceViewState((viewport).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);

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

            var view = new VideraChartView
            {
                Source = source
            };

            await source.WaitForRequestCountAsync(1);

            if ((ScriptedRequestOutcome)outcome == ScriptedRequestOutcome.Canceled)
            {
                await firstRequestStarted.Task;
            }

            view.ViewState = new SurfaceViewState((new SurfaceViewport(64, 64, 128, 128)).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);

            await source.WaitForRequestCountAsync(2);
            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [7]);

            source.RequestLog.Should().Equal(
                new SurfaceTileKey(0, 0, 0, 0),
                new SurfaceTileKey(0, 0, 0, 0));
        });
    }

    [Fact]
    public async Task ViewportChange_PrunesToCurrentPlanRetainedKeys()
    {
        var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
        var firstViewport = new SurfaceViewport(256, 256, 256, 256);
        var secondViewport = new SurfaceViewport(384, 256, 256, 256);
        var outputSize = new Size(64, 64);
        var initialViewState = SurfaceViewState.CreateDefault(metadata, firstViewport.ToDataWindow());
        var firstPlan = GetCameraAwareRequestPlan(metadata, initialViewState, outputSize, SurfaceChartInteractionQuality.Refine);
        var secondViewState = new SurfaceViewState(secondViewport.ToDataWindow(), initialViewState.Camera);
        var secondPlan = GetCameraAwareRequestPlan(metadata, secondViewState, outputSize, SurfaceChartInteractionQuality.Refine);
        var firstSelectionKeys = GetDetailOrderedKeys(firstPlan);
        var secondSelectionKeys = GetDetailOrderedKeys(secondPlan);
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
            new SurfaceCameraController(initialViewState),
            tileCache,
            new SurfaceTileScheduler(tileCache, static () => { }),
            static () => { },
            static () => { });

        controller.UpdateViewSize(outputSize);
        controller.UpdateSource(source);
        await source.WaitForRequestCountAsync(firstPlan.OrderedKeys.Count);
        await Task.Delay(100);

        var blockedRequestCount = secondSelectionKeys.Except(retainedDetailKeys).Count();
        for (var index = 0; index < blockedRequestCount; index++)
        {
            source.EnqueuePendingResponse(blockedRequestStarted, blockedCompletion, observeCancellation: true);
        }

        controller.UpdateDataWindow(secondViewport.ToDataWindow());
        await blockedRequestStarted.Task;

        var expectedKeys = new[] { overviewKey }.Concat(retainedDetailKeys).ToArray();
        tileCache.GetLoadedTiles().Select(static tile => tile.Key).Should().Equal(expectedKeys);

        controller.UpdateSource(null);
        await Task.Delay(100);
    }

    [Fact]
    public void InteractiveCameraNudge_StaysWithinStablePriorityBuckets()
    {
        var metadata = CreateLargeMetadata();
        var dataWindow = new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height);
        var source = new RecordingSurfaceTileSource(metadata);
        var tileCache = new SurfaceTileCache();
        var scheduler = new SurfaceTileScheduler(tileCache, static () => { });
        var baseCamera = SurfaceCameraPose.CreateDefault(metadata, dataWindow);
        var nudgedCamera = new SurfaceCameraPose(
            baseCamera.Target,
            baseCamera.YawDegrees + 0.25d,
            baseCamera.PitchDegrees,
            baseCamera.Distance * 1.01d,
            baseCamera.FieldOfViewDegrees);
        var baseViewState = new SurfaceViewState(dataWindow, baseCamera);
        var nudgedViewState = new SurfaceViewState(dataWindow, nudgedCamera);
        var baseFrame = SurfaceProjectionMath.CreateCameraFrame(metadata, baseViewState, 256d, 256d, 1f);
        var nudgedFrame = SurfaceProjectionMath.CreateCameraFrame(metadata, nudgedViewState, 256d, 256d, 1f);

        scheduler.UpdateSource(source);

        var basePlan = scheduler.CreateRequestPlan(baseViewState, baseFrame, new Size(256, 256), SurfaceChartInteractionQuality.Interactive);
        var nudgedPlan = scheduler.CreateRequestPlan(nudgedViewState, nudgedFrame, new Size(256, 256), SurfaceChartInteractionQuality.Interactive);

        nudgedPlan.Should().BeEquivalentTo(basePlan);
    }

    [Fact]
    public Task ViewportChange_PruneStaleDetailTiles_OnlyOverviewAndCurrentSelectionNeighborhoodRemain()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
            var source = new RecordingSurfaceTileSource(metadata);
            var view = new VideraChartView();

            view.Measure(new Size(256, 256));
            view.Arrange(new Rect(0, 0, 256, 256));
            view.Source = source;

            await source.WaitForRequestCountAsync(1);

            view.ViewState = new SurfaceViewState((new SurfaceViewport(0, 0, 512, 512)).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);

            var firstSelection = SurfaceLodPolicy.Default.Select(
                new SurfaceViewportRequest(metadata, new SurfaceViewport(0, 0, 512, 512).ToDataWindow(), 256, 256));
            var firstKeys = firstSelection.EnumerateTileKeys().ToArray();
            await source.WaitForRequestCountAsync(1 + firstKeys.Length);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view,
                SurfaceChartTestHelpers.GetLoadedTileKeys(view).Select(k => (float)(k.LevelX + k.LevelY + k.TileX + k.TileY)).Distinct().ToArray());

            view.ViewState = new SurfaceViewState((new SurfaceViewport(512, 512, 512, 512)).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);

            var secondSelection = SurfaceLodPolicy.Default.Select(
                new SurfaceViewportRequest(metadata, new SurfaceViewport(512, 512, 512, 512).ToDataWindow(), 256, 256));
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
            var view = new VideraChartView();

            view.Measure(new Size(256, 256));
            view.Arrange(new Rect(0, 0, 256, 256));
            view.Source = source;

            await source.WaitForRequestCountAsync(1);

            view.ViewState = new SurfaceViewState((new SurfaceViewport(0, 0, 512, 512)).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);

            var firstSelection = SurfaceLodPolicy.Default.Select(
                new SurfaceViewportRequest(metadata, new SurfaceViewport(0, 0, 512, 512).ToDataWindow(), 256, 256));
            var firstKeys = firstSelection.EnumerateTileKeys().ToArray();
            await source.WaitForRequestCountAsync(1 + firstKeys.Length);

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view,
                SurfaceChartTestHelpers.GetLoadedTileKeys(view).Select(k => (float)(k.LevelX + k.LevelY + k.TileX + k.TileY)).Distinct().ToArray());

            view.Measure(new Size(512, 512));
            view.Arrange(new Rect(0, 0, 512, 512));

            await Task.Delay(300).ConfigureAwait(false);

            var secondSelection = SurfaceLodPolicy.Default.Select(
                new SurfaceViewportRequest(metadata, new SurfaceViewport(0, 0, 512, 512).ToDataWindow(), 512, 512));
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
            new SurfaceCameraController(SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(0d, 0d, 512d, 512d))),
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
            new SurfaceCameraController(SurfaceViewState.CreateDefault(metadata, new SurfaceDataWindow(0d, 0d, 512d, 512d))),
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
        var viewState = SurfaceViewState.CreateDefault(metadata, viewport.ToDataWindow());

        var refineController = new SurfaceChartController(
            new SurfaceCameraController(viewState),
            refineTileCache,
            new SurfaceTileScheduler(refineTileCache, static () => { }),
            static () => { },
            static () => { });
        var interactiveController = new SurfaceChartController(
            new SurfaceCameraController(viewState),
            interactiveTileCache,
            new SurfaceTileScheduler(interactiveTileCache, static () => { }),
            static () => { },
            static () => { });

        refineController.UpdateViewSize(outputSize);
        refineController.UpdateSource(refineSource);

        var refineViewState = viewState;
        var refinePlan = GetCameraAwareRequestPlan(metadata, refineViewState, outputSize, SurfaceChartInteractionQuality.Refine);
        await refineSource.WaitForRequestCountAsync(refinePlan.OrderedKeys.Count);

        interactiveController.UpdateViewSize(outputSize);
        qualityMethod.Invoke(interactiveController, [interactiveQuality]);
        interactiveController.UpdateSource(interactiveSource);

        var interactivePlan = GetCameraAwareRequestPlan(metadata, refineViewState, new Size(512, 512), SurfaceChartInteractionQuality.Interactive);
        await interactiveSource.WaitForRequestCountAsync(interactivePlan.OrderedKeys.Count);

        refineSource.RequestLog.Should().Equal(refinePlan.OrderedKeys);
        interactiveSource.RequestLog.Should().Equal(interactivePlan.OrderedKeys);
        GetDetailOrderedKeys(interactivePlan).Length.Should().BeLessThan(GetDetailOrderedKeys(refinePlan).Length);
    }

    [Fact]
    public Task BatchCapableSource_UsesOrderedGetTilesAsyncForDetailRequests()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = SurfaceChartViewLifecycleTests.CreateMetadata();
            var source = new BatchRecordingSurfaceTileSource(metadata);
            var view = new VideraChartView();

            view.Measure(new Size(128, 128));
            view.Arrange(new Rect(0, 0, 128, 128));

            var viewport = new SurfaceViewport(512, 512, 256, 256);
            var expectedOrderedKeys = GetPrioritizedKeys(metadata, viewport, outputWidth: 128, outputHeight: 128);
            view.ViewState = new SurfaceViewState((viewport).ToDataWindow(), view.ViewState.Camera, view.ViewState.DisplaySpace);

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
        var viewState = SurfaceViewState.CreateDefault(metadata, viewport.ToDataWindow());
        var expectedPlan = GetCameraAwareRequestPlan(metadata, viewState, outputSize, SurfaceChartInteractionQuality.Refine);
        var expectedOrderedKeys = GetDetailOrderedKeys(expectedPlan);
        var source = new BatchRecordingSurfaceTileSource(metadata);
        var tileCache = new SurfaceTileCache();
        var scheduler = new SurfaceTileScheduler(tileCache, static () => { }, maxConcurrentRequests: 3);
        var controller = new SurfaceChartController(
            new SurfaceCameraController(viewState),
            tileCache,
            scheduler,
            static () => { },
            static () => { });

        controller.UpdateViewSize(outputSize);
        controller.UpdateSource(source);

        await source.WaitForTotalRequestCountAsync(expectedPlan.OrderedKeys.Count);

        source.SingleRequestLog.Should().Equal(new SurfaceTileKey(0, 0, 0, 0));
        source.BatchRequestLog.Should().NotBeEmpty();
        source.BatchRequestLog.Should().OnlyContain(batch => batch.Count <= 3);
        source.BatchRequestLog.SelectMany(static batch => batch).Should().Equal(expectedOrderedKeys);
    }

    [Fact]
    public void CameraAwareRequestPlan_FartherCameraChangesSelection()
    {
        var metadata = CreateLargeMetadata();
        var dataWindow = new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height);
        var source = new RecordingSurfaceTileSource(metadata);
        var tileCache = new SurfaceTileCache();
        var scheduler = new SurfaceTileScheduler(tileCache, static () => { });
        var nearCamera = SurfaceCameraPose.CreateDefault(metadata, dataWindow);
        var farCamera = new SurfaceCameraPose(
            nearCamera.Target,
            nearCamera.YawDegrees,
            nearCamera.PitchDegrees,
            nearCamera.Distance * 4d,
            nearCamera.FieldOfViewDegrees);
        var nearViewState = new SurfaceViewState(dataWindow, nearCamera);
        var farViewState = new SurfaceViewState(dataWindow, farCamera);
        var nearFrame = SurfaceProjectionMath.CreateCameraFrame(metadata, nearViewState, 256d, 256d, 1f);
        var farFrame = SurfaceProjectionMath.CreateCameraFrame(metadata, farViewState, 256d, 256d, 1f);

        scheduler.UpdateSource(source);

        var nearPlan = scheduler.CreateRequestPlan(nearViewState, nearFrame, new Size(256, 256), SurfaceChartInteractionQuality.Refine);
        var farPlan = scheduler.CreateRequestPlan(farViewState, farFrame, new Size(256, 256), SurfaceChartInteractionQuality.Refine);

        farPlan.OrderedKeys.Should().NotEqual(nearPlan.OrderedKeys);
        farPlan.RetainedKeys.SetEquals(nearPlan.RetainedKeys).Should().BeFalse();
    }

    [Fact]
    public async Task CameraOnlyViewStateChange_ReplansWhenProjectedFootprintChanges()
    {
        var metadata = CreateLargeMetadata();
        var dataWindow = new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height);
        var source = new RecordingSurfaceTileSource(metadata);
        var tileCache = new SurfaceTileCache();
        var cameraController = new SurfaceCameraController(new SurfaceViewState(dataWindow, SurfaceCameraPose.CreateDefault(metadata, dataWindow)));
        var controller = new SurfaceChartController(
            cameraController,
            tileCache,
            new SurfaceTileScheduler(tileCache, static () => { }),
            static () => { },
            static () => { });

        controller.UpdateViewSize(new Size(256, 256));
        controller.UpdateSource(source);
        await source.WaitForRequestCountAsync(2, timeout: TimeSpan.FromSeconds(5));
        await source.AssertRequestCountSettlesAtAsync(source.RequestLog.Count, settlingDelay: TimeSpan.FromMilliseconds(100));

        var initialRequestCount = source.RequestLog.Count;
        var currentCamera = cameraController.CurrentViewState.Camera;
        var fartherCamera = new SurfaceCameraPose(
            currentCamera.Target,
            currentCamera.YawDegrees,
            currentCamera.PitchDegrees,
            currentCamera.Distance * 4d,
            currentCamera.FieldOfViewDegrees);

        controller.UpdateViewState(new SurfaceViewState(dataWindow, fartherCamera));

        await source.WaitForRequestCountAsync(initialRequestCount + 1, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CameraAwareRequestPlan_ObliqueCameraReordersDetailPriorityWithinSameSelection()
    {
        var metadata = new SurfaceMetadata(
            1024,
            512,
            new SurfaceAxisDescriptor("Time", "s", 0d, 1023d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 0d, 511d),
            new SurfaceValueRange(0d, 100d));
        var dataWindow = new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height);
        var source = new RecordingSurfaceTileSource(metadata);
        var tileCache = new SurfaceTileCache();
        var scheduler = new SurfaceTileScheduler(tileCache, static () => { });
        var baseCamera = SurfaceCameraPose.CreateDefault(metadata, dataWindow);
        var obliqueCamera = new SurfaceCameraPose(
            baseCamera.Target,
            yawDegrees: 15d,
            pitchDegrees: 10d,
            baseCamera.Distance,
            baseCamera.FieldOfViewDegrees);
        var baseViewState = new SurfaceViewState(dataWindow, baseCamera);
        var obliqueViewState = new SurfaceViewState(dataWindow, obliqueCamera);
        var baseFrame = SurfaceProjectionMath.CreateCameraFrame(metadata, baseViewState, 256d, 256d, 1f);
        var obliqueFrame = SurfaceProjectionMath.CreateCameraFrame(metadata, obliqueViewState, 256d, 256d, 1f);

        scheduler.UpdateSource(source);

        var basePlan = scheduler.CreateRequestPlan(baseViewState, baseFrame, new Size(256, 256), SurfaceChartInteractionQuality.Refine);
        var obliquePlan = scheduler.CreateRequestPlan(obliqueViewState, obliqueFrame, new Size(256, 256), SurfaceChartInteractionQuality.Refine);

        obliquePlan.RetainedKeys.SetEquals(basePlan.RetainedKeys).Should().BeTrue();
        obliquePlan.OrderedKeys.Should().NotEqual(basePlan.OrderedKeys);
    }

    private static SurfaceTileKey[] GetPrioritizedKeys(
        SurfaceMetadata metadata,
        SurfaceViewport viewport,
        int outputWidth,
        int outputHeight)
    {
        var request = new SurfaceViewportRequest(metadata, viewport.ToDataWindow(), outputWidth, outputHeight);
        var selection = SurfaceLodPolicy.Default.Select(request);
        var clampedViewport = request.ClampedDataWindow.ToViewport();
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

    private static SurfaceTileRequestPlan GetCameraAwareRequestPlan(
        SurfaceMetadata metadata,
        SurfaceViewState viewState,
        Size outputSize,
        SurfaceChartInteractionQuality interactionQuality)
    {
        var tileCache = new SurfaceTileCache();
        var scheduler = new SurfaceTileScheduler(tileCache, static () => { });
        scheduler.UpdateSource(new RecordingSurfaceTileSource(metadata));
        var cameraFrame = SurfaceProjectionMath.CreateCameraFrame(metadata, viewState, outputSize.Width, outputSize.Height, 1f);
        return scheduler.CreateRequestPlan(viewState, cameraFrame, outputSize, interactionQuality);
    }

    private static SurfaceTileKey[] GetDetailOrderedKeys(SurfaceTileRequestPlan plan)
    {
        return plan.OrderedKeys
            .Where(static key => key != new SurfaceTileKey(0, 0, 0, 0))
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

    private static SurfaceMetadata CreateLargeMetadata()
    {
        return new SurfaceMetadata(
            4096,
            2048,
            new SurfaceAxisDescriptor("Time", "s", 0d, 4095d),
            new SurfaceAxisDescriptor("Frequency", "Hz", 0d, 2047d),
            new SurfaceValueRange(0d, 100d));
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
