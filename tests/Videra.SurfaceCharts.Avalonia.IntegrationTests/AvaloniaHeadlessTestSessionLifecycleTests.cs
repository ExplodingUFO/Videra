using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class AvaloniaHeadlessTestSessionLifecycleTests
{
    [Fact]
    public async Task Run_WhenDispatchCannotEnterBeforeTimeout_ReportsLifecycleContext()
    {
        var blockerEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseBlocker = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var blockingDispatch = StartBlockingDispatch(blockerEntered, releaseBlocker);

        try
        {
            await blockerEntered.Task.WaitAsync(TimeSpan.FromSeconds(1));

            var act = () => AvaloniaHeadlessTestSession.Run(
                static () => { },
                TimeSpan.FromMilliseconds(50),
                "surface chart synchronous lifecycle guardrail test");

            var exception = Assert.Throws<TimeoutException>(act);

            exception.Message.Should().Contain("Timed out");
            exception.Message.Should().Contain("surface chart synchronous lifecycle guardrail test");
            exception.Message.Should().Contain("Avalonia headless dispatch");
        }
        finally
        {
            releaseBlocker.TrySetResult(true);
            await blockingDispatch.WaitAsync(TimeSpan.FromSeconds(1));
        }
    }

    [Fact]
    public async Task RunAsync_WhenDispatchCannotEnterBeforeTimeout_ReportsLifecycleContext()
    {
        var blockerEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseBlocker = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var blockingDispatch = StartBlockingDispatch(blockerEntered, releaseBlocker);

        try
        {
            await blockerEntered.Task.WaitAsync(TimeSpan.FromSeconds(1));

            var act = () => AvaloniaHeadlessTestSession.RunAsync(
                () => Task.CompletedTask,
                TimeSpan.FromMilliseconds(50),
                "surface chart lifecycle guardrail test");

            var exception = await Assert.ThrowsAsync<TimeoutException>(act);

            exception.Message.Should().Contain("Timed out");
            exception.Message.Should().Contain("surface chart lifecycle guardrail test");
            exception.Message.Should().Contain("Avalonia headless dispatch");
        }
        finally
        {
            releaseBlocker.TrySetResult(true);
            await blockingDispatch.WaitAsync(TimeSpan.FromSeconds(1));
        }
    }

    private static Task StartBlockingDispatch(
        TaskCompletionSource<bool> blockerEntered,
        TaskCompletionSource<bool> releaseBlocker)
    {
        return Task.Run(() =>
            AvaloniaHeadlessTestSession.Run(
                () =>
                {
                    blockerEntered.TrySetResult(true);
                    releaseBlocker.Task.GetAwaiter().GetResult();
                },
                TimeSpan.FromSeconds(5),
                "surface chart lifecycle guardrail dispatcher blocker"));
    }
}
