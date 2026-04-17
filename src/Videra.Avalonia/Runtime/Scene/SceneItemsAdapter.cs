using System.Collections;
using System.Collections.Specialized;
using Videra.Core.Graphics;
using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime.Scene;

internal sealed class SceneItemsAdapter
{
    private readonly SceneDocumentMutator _mutator;

    public SceneItemsAdapter(SceneDocumentMutator mutator)
    {
        _mutator = mutator ?? throw new ArgumentNullException(nameof(mutator));
    }

    public SceneDocument Rebuild(SceneDocument previous, IEnumerable? items)
    {
        ArgumentNullException.ThrowIfNull(previous);
        return _mutator.RebuildPreservingEntries(previous, EnumerateSceneObjects(items));
    }

    public SceneDocument ApplyChange(SceneDocument previous, NotifyCollectionChangedEventArgs change, IEnumerable? source)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(change);

        return change.Action switch
        {
            NotifyCollectionChangedAction.Add => ApplyAdd(previous, change),
            NotifyCollectionChangedAction.Remove => ApplyRemove(previous, change),
            NotifyCollectionChangedAction.Replace => ApplyReplace(previous, change),
            NotifyCollectionChangedAction.Move => ApplyMove(previous, change),
            NotifyCollectionChangedAction.Reset => Rebuild(previous, source),
            _ => Rebuild(previous, source)
        };
    }

    private SceneDocument ApplyAdd(SceneDocument previous, NotifyCollectionChangedEventArgs change)
    {
        if (change.NewItems is null || change.NewItems.Count == 0)
        {
            return previous;
        }

        var next = previous;
        var insertIndex = change.NewStartingIndex >= 0 ? change.NewStartingIndex : next.SceneObjects.Count;
        foreach (var sceneObject in change.NewItems.OfType<Object3D>())
        {
            next = _mutator.Add(
                next,
                sceneObject,
                ownership: SceneOwnership.ExternalObject,
                index: insertIndex++);
        }

        return next;
    }

    private SceneDocument ApplyRemove(SceneDocument previous, NotifyCollectionChangedEventArgs change)
    {
        if (change.OldItems is null || change.OldItems.Count == 0)
        {
            return previous;
        }

        var next = previous;
        foreach (var sceneObject in change.OldItems.OfType<Object3D>())
        {
            if (!next.TryGetEntry(sceneObject, out var entry))
            {
                continue;
            }

            next = _mutator.Remove(next, entry.Id);
        }

        return next;
    }

    private SceneDocument ApplyReplace(SceneDocument previous, NotifyCollectionChangedEventArgs change)
    {
        if (change.NewItems is null || change.OldItems is null || change.NewStartingIndex < 0)
        {
            return previous;
        }

        var entries = previous.Entries.ToList();
        var replaceIndex = change.NewStartingIndex;

        foreach (var oldItem in change.OldItems.OfType<Object3D>())
        {
            var entryIndex = entries.FindIndex(entry => ReferenceEquals(entry.SceneObject, oldItem));
            if (entryIndex >= 0)
            {
                entries.RemoveAt(entryIndex);
            }
        }

        foreach (var newItem in change.NewItems.OfType<Object3D>())
        {
            entries.Insert(
                Math.Min(replaceIndex++, entries.Count),
                _mutator.CreateExternalEntry(newItem));
        }

        return _mutator.ReplaceEntries(previous, entries);
    }

    private SceneDocument ApplyMove(SceneDocument previous, NotifyCollectionChangedEventArgs change)
    {
        if (change.OldStartingIndex < 0 || change.NewStartingIndex < 0 || change.OldItems?.Count != 1)
        {
            return previous;
        }

        return _mutator.Move(previous, change.OldStartingIndex, change.NewStartingIndex);
    }

    private static IReadOnlyList<Object3D> EnumerateSceneObjects(IEnumerable? sequence)
    {
        return sequence is null
            ? Array.Empty<Object3D>()
            : sequence.OfType<Object3D>().ToArray();
    }
}
