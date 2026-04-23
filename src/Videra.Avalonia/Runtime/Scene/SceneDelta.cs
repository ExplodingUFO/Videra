using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime.Scene;

internal enum SceneDeltaChangeKind
{
    RuntimeObjectsChanged = 0,
    ImportedAssetChanged = 1
}

internal sealed record SceneDeltaChange(
    SceneDocumentEntry Entry,
    SceneDeltaChangeKind Kind);

internal sealed record SceneDelta(
    IReadOnlyList<SceneDocumentEntry> Added,
    IReadOnlyList<SceneDocumentEntry> Removed,
    IReadOnlyList<SceneDocumentEntry> Retained,
    IReadOnlyList<SceneDeltaChange> Changed)
{
    public static SceneDelta Empty { get; } = new(
        Array.Empty<SceneDocumentEntry>(),
        Array.Empty<SceneDocumentEntry>(),
        Array.Empty<SceneDocumentEntry>(),
        Array.Empty<SceneDeltaChange>());

    public bool HasChanges =>
        Added.Count > 0 ||
        Removed.Count > 0 ||
        Changed.Count > 0;

    public bool RequiresOverlayRefresh => HasChanges;
}
