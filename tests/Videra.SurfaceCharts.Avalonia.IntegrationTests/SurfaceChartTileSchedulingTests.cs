using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartTileSchedulingTests
{
    [Fact]
    public async Task ArrangedViewport_RequestsAdditionalTilesThroughScheduler()
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
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task NonCommittedOverviewRequest_IsRetriedByNextPipeline(int outcome)
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
    }
}
