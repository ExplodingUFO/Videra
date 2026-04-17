using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime.Scene;

internal sealed record SceneDelta(
    IReadOnlyList<SceneDocumentEntry> Added,
    IReadOnlyList<SceneDocumentEntry> Removed,
    IReadOnlyList<SceneDocumentEntry> Retained,
    IReadOnlyList<SceneDocumentEntry> ReuploadRequired)
{
    public static SceneDelta Empty { get; } = new(
        Array.Empty<SceneDocumentEntry>(),
        Array.Empty<SceneDocumentEntry>(),
        Array.Empty<SceneDocumentEntry>(),
        Array.Empty<SceneDocumentEntry>());

    public bool HasChanges =>
        Added.Count > 0 ||
        Removed.Count > 0 ||
        ReuploadRequired.Count > 0;

    public bool RequiresOverlayRefresh => HasChanges;
}
