using Videra.Core.Graphics;
using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime.Scene;

internal static class SceneEngineApplicator
{
    public static void ApplyRemovals(VideraEngine engine, IReadOnlyList<SceneDocumentEntry> removed, SceneResidencyRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(removed);
        ArgumentNullException.ThrowIfNull(registry);

        foreach (var entry in removed)
        {
            foreach (var runtimeObject in entry.RuntimeObjects)
            {
                engine.RemoveObject(runtimeObject, disposeObject: entry.Ownership == SceneOwnership.RuntimeOwnedImported);
            }

            registry.MarkDetached(entry.Id);
        }
    }

    public static void ApplyReadyAdds(VideraEngine engine, IReadOnlyList<SceneResidencyRecord> readyRecords, SceneResidencyRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(readyRecords);
        ArgumentNullException.ThrowIfNull(registry);

        foreach (var record in readyRecords)
        {
            foreach (var runtimeObject in record.RuntimeObjects)
            {
                engine.AddObject(runtimeObject, uploadIfPossible: false);
            }

            registry.MarkAttached(record.Id);
        }
    }
}
