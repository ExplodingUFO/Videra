using Videra.Avalonia.Controls;
using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime;

internal sealed partial class VideraViewRuntime
{
    public VideraInspectionBundleAssetManifest CaptureInspectionBundleAssetManifest()
    {
        var importedEntries = _sceneDocument.Entries
            .Where(static entry => entry.ImportedAsset is not null)
            .ToArray();
        var hasExternalObjects = importedEntries.Length != _sceneDocument.Entries.Count;

        return new VideraInspectionBundleAssetManifest
        {
            CanReplayScene = !hasExternalObjects,
            ReplayLimitation = hasExternalObjects
                ? "The current scene includes host-owned objects that were added outside LoadModelAsync/LoadModelsAsync, so the bundle cannot fully replay the scene on import."
                : null,
            Entries = importedEntries
                .Select(static entry => new VideraInspectionBundleAssetEntry
                {
                    OriginalObjectId = entry.SceneObject.Id,
                    FilePath = entry.ImportedAsset!.FilePath,
                    Name = entry.SceneObject.Name,
                    Position = BundleVector3.From(entry.SceneObject.Position),
                    Rotation = BundleVector3.From(entry.SceneObject.Rotation),
                    Scale = BundleVector3.From(entry.SceneObject.Scale)
                })
                .ToArray()
        };
    }
}
