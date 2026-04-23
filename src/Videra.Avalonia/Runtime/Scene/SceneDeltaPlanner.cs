using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime.Scene;

internal static class SceneDeltaPlanner
{
    public static SceneDelta Diff(SceneDocument previous, SceneDocument next)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(next);

        if (ReferenceEquals(previous, next))
        {
            return SceneDelta.Empty;
        }

        var previousById = previous.Entries.ToDictionary(static entry => entry.Id);
        var nextById = next.Entries.ToDictionary(static entry => entry.Id);

        var added = new List<SceneDocumentEntry>();
        var retained = new List<SceneDocumentEntry>();
        var changed = new List<SceneDeltaChange>();

        foreach (var entry in next.Entries)
        {
            if (!previousById.TryGetValue(entry.Id, out var previousEntry))
            {
                added.Add(entry);
                continue;
            }

            retained.Add(entry);
            if (!ReferenceEquals(previousEntry.ImportedAsset, entry.ImportedAsset))
            {
                changed.Add(new SceneDeltaChange(entry, SceneDeltaChangeKind.ImportedAssetChanged));
                continue;
            }

            if (!HaveSameRuntimeObjects(previousEntry, entry))
            {
                changed.Add(new SceneDeltaChange(entry, SceneDeltaChangeKind.RuntimeObjectsChanged));
            }
        }

        var removed = previous.Entries
            .Where(entry => !nextById.ContainsKey(entry.Id))
            .ToArray();

        return new SceneDelta(added, removed, retained, changed);
    }

    private static bool HaveSameRuntimeObjects(SceneDocumentEntry previous, SceneDocumentEntry next)
    {
        if (previous.RuntimeObjects.Count != next.RuntimeObjects.Count)
        {
            return false;
        }

        for (var i = 0; i < previous.RuntimeObjects.Count; i++)
        {
            if (!ReferenceEquals(previous.RuntimeObjects[i], next.RuntimeObjects[i]))
            {
                return false;
            }
        }

        return true;
    }
}
