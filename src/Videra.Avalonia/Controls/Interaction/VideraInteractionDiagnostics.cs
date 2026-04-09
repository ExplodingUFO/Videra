namespace Videra.Avalonia.Controls.Interaction;

public sealed class VideraInteractionDiagnostics
{
    public static VideraInteractionDiagnostics CreateDefault()
    {
        return new VideraInteractionDiagnostics
        {
            SupportsControlledSelection = true,
            SupportsControlledAnnotations = true,
            SupportsIntentEvents = true,
            IsInputBehaviorAttached = false
        };
    }

    public bool SupportsControlledSelection { get; init; }

    public bool SupportsControlledAnnotations { get; init; }

    public bool SupportsIntentEvents { get; init; }

    public bool IsInputBehaviorAttached { get; init; }
}
