using Videra.Core.Graphics;

namespace Videra.Core.Scene;

/**
 * Represents one logical scene entry that may expand into multiple primitive-level
 * runtime objects. The primitive-first runtime path treats each <see cref="Object3D"/>
 * in <see cref="RuntimeObjects"/> as an independently uploadable and renderable unit.
 */
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

    /// <summary>Convenience accessor for the first primitive; never assume this is the only one.</summary>
    internal Object3D SceneObject { get; }

    /// <summary>All primitive-level runtime objects owned by this entry. Count may be > 1.</summary>
    internal IReadOnlyList<Object3D> RuntimeObjects => _runtimeObjects;

    public ImportedSceneAsset? ImportedAsset { get; }

    public SceneOwnership Ownership { get; }
}
