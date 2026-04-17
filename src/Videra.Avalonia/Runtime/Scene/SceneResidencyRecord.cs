using Videra.Core.Graphics;
using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime.Scene;

internal sealed record SceneResidencyRecord(
    SceneEntryId Id,
    Object3D SceneObject,
    ImportedSceneAsset? ImportedAsset,
    SceneOwnership Ownership,
    SceneResidencyState State,
    long ApproximateUploadBytes,
    ulong ResourceEpoch,
    bool IsAttachedToEngine,
    Exception? LastError);
