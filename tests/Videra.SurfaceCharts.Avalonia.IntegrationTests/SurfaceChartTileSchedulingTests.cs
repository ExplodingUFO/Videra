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
            source.RequestLog.Skip(1).Should().ContainInOrder(expectedKeys);
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
}
