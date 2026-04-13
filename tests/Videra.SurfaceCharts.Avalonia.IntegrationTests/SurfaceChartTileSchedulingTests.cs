using System.Reflection;
using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
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
                    throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null);
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
    public Task ViewportChange_PruneStaleDetailTiles_OnlyOverviewAndCurrentSelectionRemain()
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
    public Task ViewSizeChange_PruneStaleDetailTiles_OnlyOverviewRemains()
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
}
