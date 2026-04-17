namespace Videra.Avalonia.Rendering;

internal sealed class InteractiveFrameLease : IDisposable
{
    private readonly Action _release;
    private bool _disposed;

    public InteractiveFrameLease(Action release)
    {
        _release = release ?? throw new ArgumentNullException(nameof(release));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _release();
    }
}
