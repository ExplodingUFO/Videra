namespace Videra.Avalonia.Runtime.Scene;

internal readonly record struct SceneResidencyDiagnostics(
    int SceneDocumentVersion,
    int PendingUploads,
    int ResidentObjects,
    int DirtyObjects,
    int FailedUploads);
