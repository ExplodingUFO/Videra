using Videra.Core.Graphics;

namespace Videra.Core.Scene;

internal sealed record SceneDocumentEntry(
    SceneEntryId Id,
    Object3D SceneObject,
    ImportedSceneAsset? ImportedAsset,
    SceneOwnership Ownership);
