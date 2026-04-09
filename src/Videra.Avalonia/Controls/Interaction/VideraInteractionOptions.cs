namespace Videra.Avalonia.Controls.Interaction;

public sealed class VideraInteractionOptions
{
    public bool AllowCameraNavigation { get; set; } = true;

    public VideraEmptySpaceSelectionBehavior EmptySpaceSelectionBehavior { get; set; } = VideraEmptySpaceSelectionBehavior.ClearSelection;
}
