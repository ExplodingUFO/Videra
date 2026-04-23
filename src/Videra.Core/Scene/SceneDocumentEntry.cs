using Videra.Core.Graphics;

namespace Videra.Core.Scene;

public sealed class SceneDocumentEntry
{
    private readonly Object3D[] _runtimeObjects;

    internal SceneDocumentEntry(
        SceneEntryId id,
        string name,
        IEnumerable<Object3D> runtimeObjects,
        ImportedSceneAsset? importedAsset,
        SceneOwnership ownership)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(runtimeObjects);

        _runtimeObjects = runtimeObjects.ToArray();
        if (_runtimeObjects.Length == 0)
        {
            throw new ArgumentException("Scene document entries must own at least one runtime object.", nameof(runtimeObjects));
        }

        Id = id;
        Name = name;
        SceneObject = _runtimeObjects[0];
        ImportedAsset = importedAsset;
        Ownership = ownership;
    }

    public SceneEntryId Id { get; }

    public string Name { get; }

    internal Object3D SceneObject { get; }

    internal IReadOnlyList<Object3D> RuntimeObjects => _runtimeObjects;

    public ImportedSceneAsset? ImportedAsset { get; }

    public SceneOwnership Ownership { get; }
}
