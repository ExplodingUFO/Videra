namespace Videra.Avalonia.Controls.Interaction;

public sealed class VideraSelectionRequest
{
    private readonly IReadOnlyList<Guid> _objectIds;

    public VideraSelectionRequest(
        VideraSelectionOperation operation,
        IReadOnlyList<Guid> objectIds,
        Guid? primaryObjectId,
        VideraEmptySpaceSelectionBehavior emptySpaceSelectionBehavior)
    {
        _objectIds = Array.AsReadOnly((objectIds ?? Array.Empty<Guid>()).ToArray());
        Operation = operation;
        PrimaryObjectId = primaryObjectId;
        EmptySpaceSelectionBehavior = emptySpaceSelectionBehavior;
    }

    public VideraSelectionOperation Operation { get; }

    public IReadOnlyList<Guid> ObjectIds => _objectIds;

    public Guid? PrimaryObjectId { get; }

    public VideraEmptySpaceSelectionBehavior EmptySpaceSelectionBehavior { get; }
}
