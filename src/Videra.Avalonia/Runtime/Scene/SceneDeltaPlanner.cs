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
        var reuploadRequired = new List<SceneDocumentEntry>();

        foreach (var entry in next.Entries)
        {
            if (!previousById.TryGetValue(entry.Id, out var previousEntry))
            {
                added.Add(entry);
                continue;
            }

            retained.Add(entry);
            if (!HaveSameRuntimeObjects(previousEntry, entry) ||
                !ReferenceEquals(previousEntry.ImportedAsset, entry.ImportedAsset))
            {
                reuploadRequired.Add(entry);
            }
        }

        var removed = previous.Entries
            .Where(entry => !nextById.ContainsKey(entry.Id))
            .ToArray();

        return new SceneDelta(added, removed, retained, reuploadRequired);
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
