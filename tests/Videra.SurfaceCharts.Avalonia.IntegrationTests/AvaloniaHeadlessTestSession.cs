using Avalonia;
using Avalonia.Headless;
using System.Threading;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

internal static class AvaloniaHeadlessTestSession
{
    private static readonly Lazy<HeadlessUnitTestSession> Session = new(
        static () => HeadlessUnitTestSession.StartNew(typeof(HeadlessTestApplication)));

    public static void Run(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        Session.Value.Dispatch(action, default).GetAwaiter().GetResult();
    }

    public static Task RunAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return Session.Value.Dispatch(
            async () =>
            {
                await action().ConfigureAwait(true);
                }, default);
    }

    private sealed class HeadlessTestApplication : Application
    {
    }
}
