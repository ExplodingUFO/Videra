namespace Videra.Avalonia.Runtime.Scene;

internal readonly record struct SceneResidencyDiagnostics(
    int SceneDocumentVersion,
    int PendingUploads,
    long PendingUploadBytes,
    int ResidentObjects,
    long ResidentObjectBytes,
    int DirtyObjects,
    int FailedUploads,
    int LastUploadedObjects,
    long LastUploadedBytes,
    int LastUploadFailures,
    TimeSpan LastUploadDuration,
    int LastBudgetMaxObjects,
    long LastBudgetMaxBytes);
