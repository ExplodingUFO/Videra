namespace Videra.Avalonia.Controls.Interaction;

public sealed class SelectionRequestedEventArgs : EventArgs
{
    private readonly IReadOnlyList<Guid> _objectIds;

    public SelectionRequestedEventArgs(VideraSelectionState selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        _objectIds = Array.AsReadOnly(selection.ObjectIds.ToArray());
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
