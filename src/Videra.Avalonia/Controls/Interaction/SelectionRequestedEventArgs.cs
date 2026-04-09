namespace Videra.Avalonia.Controls.Interaction;

public sealed class SelectionRequestedEventArgs : EventArgs
{
    public SelectionRequestedEventArgs(VideraSelectionRequest request)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
    }

    public VideraSelectionRequest Request { get; }

    public VideraSelectionOperation Operation => Request.Operation;

    public IReadOnlyList<Guid> ObjectIds => Request.ObjectIds;

    public Guid? PrimaryObjectId => Request.PrimaryObjectId;

    public VideraEmptySpaceSelectionBehavior EmptySpaceSelectionBehavior => Request.EmptySpaceSelectionBehavior;
}
