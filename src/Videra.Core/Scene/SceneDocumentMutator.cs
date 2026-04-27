using System.Collections;
using Videra.Core.Graphics;

namespace Videra.Core.Scene;

internal sealed class SceneDocumentMutator
{
    public SceneDocument Add(
        SceneDocument document,
        Object3D sceneObject,
        ImportedSceneAsset? importedAsset = null,
        SceneOwnership ownership = SceneOwnership.ExternalObject,
        int? index = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(sceneObject);

        var entries = document.Entries.ToList();
        var entry = ownership == SceneOwnership.RuntimeOwnedImported
            ? CreateImportedEntry(sceneObject, importedAsset)
            : CreateExternalEntry(sceneObject);

        if (index is >= 0 and <= int.MaxValue)
        {
            entries.Insert(Math.Min(index.Value, entries.Count), entry);
        }
        else
        {
            entries.Add(entry);
        }

        return document.WithEntries(entries, document.Version + 1);
    }

    public SceneDocument ReplaceEntries(SceneDocument document, IEnumerable<SceneDocumentEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(entries);
        return document.WithEntries(entries, document.Version + 1);
    }

    public SceneDocument Remove(SceneDocument document, SceneEntryId id)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.WithEntries(
            document.Entries.Where(entry => entry.Id != id),
            document.Version + 1);
    }

    public SceneDocument Move(SceneDocument document, int oldIndex, int newIndex)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (oldIndex == newIndex)
        {
            return document;
        }

        var entries = document.Entries.ToList();
        if (oldIndex < 0 || oldIndex >= entries.Count || newIndex < 0 || newIndex >= entries.Count)
        {
            return document;
        }

        var entry = entries[oldIndex];
        entries.RemoveAt(oldIndex);
        entries.Insert(newIndex, entry);
        return document.WithEntries(entries, document.Version + 1);
    }

    public SceneDocument Clear(SceneDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.WithEntries(
            Array.Empty<SceneDocumentEntry>(),
            document.Version + 1,
            Array.Empty<InstanceBatchEntry>());
    }

    public SceneDocument RebuildPreservingEntries(SceneDocument previous, IEnumerable<Object3D> sceneObjects)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(sceneObjects);

        var knownEntries = new Dictionary<Object3D, SceneDocumentEntry>(ReferenceEqualityComparer.Instance);
        foreach (var entry in previous.Entries)
        {
            knownEntries[entry.SceneObject] = entry;
        }

        var entries = new List<SceneDocumentEntry>();
        foreach (var sceneObject in sceneObjects)
        {
            if (knownEntries.TryGetValue(sceneObject, out var existing))
            {
                entries.Add(new SceneDocumentEntry(
                    existing.Id,
                    sceneObject.Name,
                    [sceneObject],
                    existing.ImportedAsset,
                    existing.Ownership));
                continue;
            }

            entries.Add(CreateExternalEntry(sceneObject));
        }

        return previous.WithEntries(entries, previous.Version + 1);
    }

    internal SceneDocumentEntry CreateExternalEntry(Object3D sceneObject)
    {
        ArgumentNullException.ThrowIfNull(sceneObject);

        return new SceneDocumentEntry(
            SceneEntryId.New(),
            sceneObject.Name,
            [sceneObject],
            importedAsset: null,
            SceneOwnership.ExternalObject);
    }

    internal SceneDocumentEntry CreateImportedEntry(Object3D sceneObject, ImportedSceneAsset? importedAsset)
    {
        return CreateImportedEntry([sceneObject], importedAsset);
    }

    internal SceneDocumentEntry CreateImportedEntry(IEnumerable<Object3D> sceneObjects, ImportedSceneAsset? importedAsset)
    {
        ArgumentNullException.ThrowIfNull(importedAsset);

        return new SceneDocumentEntry(
            SceneEntryId.New(),
            importedAsset.Name,
            sceneObjects,
            importedAsset,
            SceneOwnership.RuntimeOwnedImported);
    }
}
