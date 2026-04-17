namespace Videra.Avalonia.Rendering;

internal sealed class FrameScheduler
{
    private RenderInvalidationKinds _pendingInvalidations;
    private int _interactiveLeaseCount;

    public bool HasPendingFrame => _pendingInvalidations != RenderInvalidationKinds.None;

    public bool HasInteractiveLease => _interactiveLeaseCount > 0;

    public InteractiveFrameLease AcquireInteractiveLease(Action onReleased)
    {
        _interactiveLeaseCount++;
        return new InteractiveFrameLease(() =>
        {
            if (_interactiveLeaseCount > 0)
            {
                _interactiveLeaseCount--;
            }

            onReleased();
        });
    }

    public void Invalidate(RenderInvalidationKinds flags)
    {
        _pendingInvalidations |= flags;
    }

    public RenderInvalidationKinds ConsumeInvalidations()
    {
        var flags = _pendingInvalidations;
        _pendingInvalidations = RenderInvalidationKinds.None;
        return flags;
    }
}
