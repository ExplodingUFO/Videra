using System.Linq;
using Videra.Core.Graphics;

namespace Videra.Core.Scene;

public static class SceneObjectFactory
{
    public static Object3D CreateDeferred(ImportedSceneAsset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);

        var sceneObject = new Object3D
        {
            Name = asset.Name
        };
        sceneObject.ApplyMaterialAlpha(ResolveMaterialAlpha(asset));
        sceneObject.PrepareDeferredMesh(asset.Payload);
        return sceneObject;
    }

    private static MaterialAlphaSettings ResolveMaterialAlpha(ImportedSceneAsset asset)
    {
        if (asset.Primitives.Count == 0 || asset.Materials.Count == 0)
        {
            return MaterialAlphaSettings.Opaque;
        }

        var materialById = asset.Materials.ToDictionary(static material => material.Id);
        MaterialAlphaSettings? resolved = null;

        foreach (var primitive in asset.Primitives)
        {
            var alpha = primitive.MaterialId is { } materialId && materialById.TryGetValue(materialId, out var material)
                ? material.Alpha
                : MaterialAlphaSettings.Opaque;

            if (resolved is null)
            {
                resolved = alpha;
                continue;
            }

            if (resolved.Value != alpha)
            {
                throw new InvalidOperationException(
                    $"Imported scene asset '{asset.Name}' flattens primitives with conflicting alpha semantics into one render object.");
            }
        }

        return resolved ?? MaterialAlphaSettings.Opaque;
    }
}
