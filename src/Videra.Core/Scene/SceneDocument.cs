using Videra.Core.Graphics;

namespace Videra.Core.Scene;

public sealed class SceneDocument
{
    private readonly SceneDocumentEntry[] _entries;
    private readonly InstanceBatchEntry[] _instanceBatches;

    public static SceneDocument Empty { get; } = new(version: 0, Array.Empty<SceneDocumentEntry>(), Array.Empty<InstanceBatchEntry>());

    public SceneDocument(IEnumerable<Object3D> sceneObjects)
        : this(version: 1, CreateExternalEntries(sceneObjects))
    {
    }

    internal SceneDocument(IEnumerable<SceneDocumentEntry> entries)
        : this(version: 1, entries, Array.Empty<InstanceBatchEntry>())
    {
    }

    internal SceneDocument(int version, IEnumerable<SceneDocumentEntry> entries)
        : this(version, entries, Array.Empty<InstanceBatchEntry>())
    {
    }

    private SceneDocument(int version, IEnumerable<SceneDocumentEntry> entries, IEnumerable<InstanceBatchEntry> instanceBatches)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(instanceBatches);
        Version = version;
        _entries = entries.ToArray();
        _instanceBatches = instanceBatches.ToArray();
        SceneObjects = _entries.SelectMany(static entry => entry.RuntimeObjects).ToArray();
    }

    public int Version { get; }

    public IReadOnlyList<SceneDocumentEntry> Entries => _entries;

    public IReadOnlyList<InstanceBatchEntry> InstanceBatches => _instanceBatches;

    internal IReadOnlyList<Object3D> SceneObjects { get; }

    internal bool TryGetEntry(Object3D sceneObject, out SceneDocumentEntry entry)
    {
        ArgumentNullException.ThrowIfNull(sceneObject);

        for (var i = 0; i < _entries.Length; i++)
        {
            if (_entries[i].RuntimeObjects.Any(runtimeObject => ReferenceEquals(runtimeObject, sceneObject)))
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

    internal SceneDocument WithEntries(
        IEnumerable<SceneDocumentEntry> entries,
        int version,
        IEnumerable<InstanceBatchEntry>? instanceBatches = null)
    {
        return new SceneDocument(version, entries, instanceBatches ?? _instanceBatches);
    }

    public SceneDocument AddInstanceBatch(InstanceBatchDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        var instanceBatches = _instanceBatches
            .Append(new InstanceBatchEntry(SceneEntryId.New(), descriptor));
        return new SceneDocument(Version + 1, _entries, instanceBatches);
    }

    private static IEnumerable<SceneDocumentEntry> CreateExternalEntries(IEnumerable<Object3D> sceneObjects)
    {
        ArgumentNullException.ThrowIfNull(sceneObjects);

        return sceneObjects.Select(static sceneObject =>
        {
            ArgumentNullException.ThrowIfNull(sceneObject);

            return new SceneDocumentEntry(
                SceneEntryId.New(),
                sceneObject.Name,
                [sceneObject],
                importedAsset: null,
                SceneOwnership.ExternalObject);
        });
    }
}
