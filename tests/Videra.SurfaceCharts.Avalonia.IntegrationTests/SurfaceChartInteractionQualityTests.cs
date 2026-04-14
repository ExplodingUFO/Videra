using Avalonia;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartInteractionQualityTests
{
    [Fact]
    public async Task InteractiveQualityMode_StaysInteractiveUntilIdleDelayCompletes()
    {
        var delayGate = new DeterministicDelayGate();
        var runtime = CreateRuntime(delayGate, source: null);

        runtime.EnterInteractiveMode();
        runtime.ScheduleRefineMode();

        runtime.CurrentInteractionQualityMode.Should().Be(SurfaceInteractionQualityMode.Interactive);
        runtime.WaitForPendingQualityTransitionAsync().IsCompleted.Should().BeFalse();

        delayGate.Release();
        await runtime.WaitForPendingQualityTransitionAsync();

        runtime.CurrentInteractionQualityMode.Should().Be(SurfaceInteractionQualityMode.Refine);
    }

    [Fact]
    public async Task InteractiveQualityMode_UsesOverviewDuringMotionAndRestoresDetailAfterRefine()
    {
        var delayGate = new DeterministicDelayGate();
        var source = new RecordingSurfaceTileSource(SurfaceChartViewLifecycleTests.CreateMetadata());
        var runtime = CreateRuntime(delayGate, source);

        await WaitForLoadedTileCountAsync(runtime, minimumCount: 2);
        runtime.GetLoadedTiles().Select(static tile => tile.Key).Should().Contain(new SurfaceTileKey(0, 0, 0, 0));

        runtime.EnterInteractiveMode();

        runtime.CurrentInteractionQualityMode.Should().Be(SurfaceInteractionQualityMode.Interactive);
        runtime.GetLoadedTiles().Select(static tile => tile.Key).Should().Equal(new SurfaceTileKey(0, 0, 0, 0));

        runtime.ScheduleRefineMode();
        runtime.WaitForPendingQualityTransitionAsync().IsCompleted.Should().BeFalse();

        delayGate.Release();
        await runtime.WaitForPendingQualityTransitionAsync();
        await WaitForLoadedTileCountAsync(runtime, minimumCount: 2);

        runtime.CurrentInteractionQualityMode.Should().Be(SurfaceInteractionQualityMode.Refine);
        runtime.GetLoadedTiles().Count.Should().BeGreaterThan(1);
    }

    private static SurfaceChartRuntime CreateRuntime(DeterministicDelayGate delayGate, RecordingSurfaceTileSource? source)
    {
        var runtime = new SurfaceChartRuntime(
            new SurfaceCameraController(new SurfaceViewport(0, 0, 512, 512)),
            new SurfaceTileCache(),
            static () => { },
            tileFailed: null,
            static () => { },
            static () => { },
            TimeSpan.FromMilliseconds(1),
            delayGate.DelayAsync);

        runtime.UpdateViewSize(new Size(256, 256));
        if (source is not null)
        {
            runtime.UpdateSource(source);
        }

        return runtime;
    }

    private static async Task WaitForLoadedTileCountAsync(SurfaceChartRuntime runtime, int minimumCount, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(2);
        var deadline = DateTime.UtcNow + timeout.Value;

        while (DateTime.UtcNow < deadline)
        {
            if (runtime.GetLoadedTiles().Count >= minimumCount)
            {
                return;
            }

            await Task.Delay(10).ConfigureAwait(false);
        }

        runtime.GetLoadedTiles().Count.Should().BeGreaterThanOrEqualTo(minimumCount);
    }

    private sealed class DeterministicDelayGate
    {
        private readonly TaskCompletionSource<bool> _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            _ = delay;
            return _release.Task.WaitAsync(cancellationToken);
        }

        public void Release()
        {
            _release.TrySetResult(true);
        }
    }
}
