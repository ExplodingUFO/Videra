namespace Videra.Avalonia.Runtime.Scene;

internal enum SceneResidencyState
{
    PendingUpload = 0,
    Uploading = 1,
    Resident = 2,
    Dirty = 3,
    Failed = 4
}
