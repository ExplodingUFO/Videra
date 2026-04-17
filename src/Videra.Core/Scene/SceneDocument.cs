using Videra.Core.Graphics;

namespace Videra.Core.Scene;

public sealed class SceneDocument
{
    private readonly DocumentEntry[] _entries;

    public static SceneDocument Empty { get; } = new(Array.Empty<DocumentEntry>());

    public SceneDocument(IEnumerable<Object3D> sceneObjects)
        : this(CreateEntries(sceneObjects))
    {
    }

    internal SceneDocument(IEnumerable<DocumentEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        _entries = entries.ToArray();
        SceneObjects = _entries.Select(static entry => entry.SceneObject).ToArray();
    }

    public IReadOnlyList<Object3D> SceneObjects { get; }

    internal IReadOnlyList<DocumentEntry> Entries => _entries;

    internal SceneDocument Add(Object3D sceneObject, ImportedSceneAsset? importedAsset = null)
    {
        ArgumentNullException.ThrowIfNull(sceneObject);
        return new SceneDocument(_entries.Append(new DocumentEntry(sceneObject, importedAsset)));
    }

    internal SceneDocument Replace(IEnumerable<DocumentEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        return new SceneDocument(entries);
    }

    internal sealed record DocumentEntry(
        Object3D SceneObject,
        ImportedSceneAsset? ImportedAsset);

    private static IEnumerable<DocumentEntry> CreateEntries(IEnumerable<Object3D> sceneObjects)
    {
        ArgumentNullException.ThrowIfNull(sceneObjects);
        return sceneObjects.Select(static sceneObject => new DocumentEntry(sceneObject, ImportedAsset: null));
    }
}
