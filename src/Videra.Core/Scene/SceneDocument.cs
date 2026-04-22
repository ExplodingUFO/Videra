using Videra.Core.Graphics;

namespace Videra.Core.Scene;

public sealed class SceneDocument
{
    private readonly SceneDocumentEntry[] _entries;

    public static SceneDocument Empty { get; } = new(version: 0, Array.Empty<SceneDocumentEntry>());

    internal SceneDocument(IEnumerable<SceneDocumentEntry> entries)
        : this(version: 1, entries)
    {
    }

    internal SceneDocument(int version, IEnumerable<SceneDocumentEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        Version = version;
        _entries = entries.ToArray();
        SceneObjects = _entries.Select(static entry => entry.SceneObject).ToArray();
    }

    public int Version { get; }

    public IReadOnlyList<SceneDocumentEntry> Entries => _entries;

    internal IReadOnlyList<Object3D> SceneObjects { get; }

    internal bool TryGetEntry(Object3D sceneObject, out SceneDocumentEntry entry)
    {
        ArgumentNullException.ThrowIfNull(sceneObject);

        for (var i = 0; i < _entries.Length; i++)
        {
            if (ReferenceEquals(_entries[i].SceneObject, sceneObject))
            {
                entry = _entries[i];
                return true;
            }
        }

        entry = default!;
        return false;
    }

    internal bool TryGetEntry(SceneEntryId id, out SceneDocumentEntry entry)
    {
        for (var i = 0; i < _entries.Length; i++)
        {
            if (_entries[i].Id == id)
            {
                entry = _entries[i];
                return true;
            }
        }

        entry = default!;
        return false;
    }

    internal SceneDocument WithEntries(IEnumerable<SceneDocumentEntry> entries, int version)
    {
        return new SceneDocument(version, entries);
    }
}
