using Avalonia;
using Avalonia.Headless;
using System.Threading;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

internal static class AvaloniaHeadlessTestSession
{
    private static readonly TimeSpan DispatchTimeout = TimeSpan.FromSeconds(30);

    private static readonly Lazy<HeadlessUnitTestSession> Session = new(
        static () => HeadlessUnitTestSession.StartNew(typeof(HeadlessTestApplication)));

    public static void Run(Action action)
    {
        Run(action, DispatchTimeout, "SurfaceCharts lifecycle test");
    }

    internal static void Run(Action action, TimeSpan timeout, string lifecycleContext)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var cancellationSource = new CancellationTokenSource(timeout);
        var dispatchTask = Session.Value.Dispatch(action, cancellationSource.Token);

        try
        {
            dispatchTask.WaitAsync(cancellationSource.Token).GetAwaiter().GetResult();
        }
        catch (OperationCanceledException ex) when (cancellationSource.IsCancellationRequested && !dispatchTask.IsCompleted)
        {
            throw CreateTimeoutException(timeout, lifecycleContext, ex);
        }
    }

    public static Task RunAsync(Func<Task> action)
    {
        return RunAsync(action, DispatchTimeout, "SurfaceCharts lifecycle test");
    }

    internal static async Task RunAsync(Func<Task> action, TimeSpan timeout, string lifecycleContext)
    {
        ArgumentNullException.ThrowIfNull(action);

        using var cancellationSource = new CancellationTokenSource(timeout);
        var dispatchTask = Session.Value.Dispatch(
            async () =>
            {
                await action().ConfigureAwait(true);
            },
            cancellationSource.Token);

        try
        {
            await dispatchTask.WaitAsync(cancellationSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (cancellationSource.IsCancellationRequested && !dispatchTask.IsCompleted)
        {
            throw CreateTimeoutException(timeout, lifecycleContext, ex);
        }
    }

    private static TimeoutException CreateTimeoutException(
        TimeSpan timeout,
        string lifecycleContext,
        Exception innerException)
    {
        return new TimeoutException(
            $"Timed out after {timeout.TotalSeconds:N1}s during Avalonia headless dispatch for {lifecycleContext}. " +
            "This usually indicates a SurfaceCharts lifecycle test did not finish on the headless UI thread.",
            innerException);
    }

    private sealed class HeadlessTestApplication : Application
    {
    }
}
