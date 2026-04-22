using Videra.Core.Graphics;

namespace Videra.Core.Scene;

public sealed class SceneDocumentEntry
{
    internal SceneDocumentEntry(
        SceneEntryId id,
        string name,
        Object3D sceneObject,
        ImportedSceneAsset? importedAsset,
        SceneOwnership ownership)
    {
        Id = id;
        Name = name;
        SceneObject = sceneObject;
        ImportedAsset = importedAsset;
        Ownership = ownership;
    }

    public SceneEntryId Id { get; }

    public string Name { get; }

    internal Object3D SceneObject { get; }

    public ImportedSceneAsset? ImportedAsset { get; }

    public SceneOwnership Ownership { get; }
}
