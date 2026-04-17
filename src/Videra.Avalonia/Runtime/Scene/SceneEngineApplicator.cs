using Videra.Core.Graphics;
using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime.Scene;

internal sealed class SceneEngineApplicator
{
    public void ApplyRemovals(VideraEngine engine, IReadOnlyList<SceneDocumentEntry> removed, SceneResidencyRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(removed);
        ArgumentNullException.ThrowIfNull(registry);

        foreach (var entry in removed)
        {
            engine.RemoveObject(entry.SceneObject, disposeObject: entry.Ownership == SceneOwnership.RuntimeOwnedImported);
            registry.MarkDetached(entry.Id);
        }
    }

    public void ApplyReadyAdds(VideraEngine engine, IReadOnlyList<SceneResidencyRecord> readyRecords, SceneResidencyRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(readyRecords);
        ArgumentNullException.ThrowIfNull(registry);

        foreach (var record in readyRecords)
        {
            engine.AddObject(record.SceneObject, uploadIfPossible: false);
            registry.MarkAttached(record.Id);
        }
    }
}
