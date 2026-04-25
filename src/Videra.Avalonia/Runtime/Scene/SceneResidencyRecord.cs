using Videra.Core.Graphics;
using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime.Scene;

/** Residency state for one scene entry. <see cref="RuntimeObjects"/> holds all
 *  primitive-level objects; <see cref="SceneObject"/> is a convenience accessor to
 *  the first primitive and must not be treated as the only one. */
internal sealed record SceneResidencyRecord(
    SceneEntryId Id,
    // First primitive; never assume this is the only one.
    Object3D SceneObject,
    // All primitives owned by this entry.
    IReadOnlyList<Object3D> RuntimeObjects,
    ImportedSceneAsset? ImportedAsset,
    SceneOwnership Ownership,
    SceneResidencyState State,
    long ApproximateUploadBytes,
    ulong ResourceEpoch,
    bool IsAttachedToEngine,
    Exception? LastError);
