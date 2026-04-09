namespace Videra.Avalonia.Controls.Interaction;

public sealed class SelectionRequestedEventArgs : EventArgs
{
    private readonly Guid[] _objectIds;

    public SelectionRequestedEventArgs(VideraSelectionState selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        _objectIds = selection.ObjectIds.ToArray();
        PrimaryObjectId = selection.PrimaryObjectId;
    }

    public IReadOnlyList<Guid> ObjectIds => _objectIds;

    public Guid? PrimaryObjectId { get; }

    public VideraSelectionState Selection => new()
    {
        ObjectIds = _objectIds,
        PrimaryObjectId = PrimaryObjectId
    };
}
