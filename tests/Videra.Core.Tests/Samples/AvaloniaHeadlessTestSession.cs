using Avalonia;
using Avalonia.Headless;
using System.Runtime.ExceptionServices;

namespace Videra.Core.Tests.Samples;

internal static class AvaloniaHeadlessTestSession
{
    private static readonly Lazy<HeadlessUnitTestSession> Session = new(
        static () => HeadlessUnitTestSession.StartNew(typeof(HeadlessTestApplication)));

    public static void Run(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        ExceptionDispatchInfo? capturedException = null;

        Session.Value.Dispatch(
            () =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    capturedException = ExceptionDispatchInfo.Capture(exception);
                }
            },
            default).GetAwaiter().GetResult();

        capturedException?.Throw();
    }

    public static async Task RunAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        ExceptionDispatchInfo? capturedException = null;

        await Session.Value.Dispatch(
            async () =>
            {
                try
                {
                    await action().ConfigureAwait(true);
                }
                catch (Exception exception)
                {
                    capturedException = ExceptionDispatchInfo.Capture(exception);
                }
            },
            default).ConfigureAwait(false);

        capturedException?.Throw();
    }

    private sealed class HeadlessTestApplication : Application
    {
        public override void Initialize()
        {
        }
    }
}
