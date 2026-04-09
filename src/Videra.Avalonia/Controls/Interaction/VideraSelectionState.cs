namespace Videra.Avalonia.Controls.Interaction;

public sealed class VideraSelectionState
{
    private IReadOnlyList<Guid> _objectIds = Array.Empty<Guid>();

    public IReadOnlyList<Guid> ObjectIds
    {
        get => _objectIds;
        set => _objectIds = value ?? Array.Empty<Guid>();
    }

    public Guid? PrimaryObjectId { get; set; }
}
