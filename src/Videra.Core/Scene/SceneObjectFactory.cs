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
        sceneObject.PrepareDeferredMesh(asset.Payload);
        sceneObject.ApplyMaterialAlpha(ResolveMaterialAlpha(asset.Payload));
        return sceneObject;
    }

    internal static IReadOnlyList<Object3D> CreateDeferredRuntimeObjects(ImportedSceneAsset asset)
    {
        return ImportedSceneRuntimeObjectBuilder.CreateDeferredObjects(asset);
    }

    private static MaterialAlphaSettings ResolveMaterialAlpha(MeshPayload payload)
    {
        var segments = payload.Segments;
        if (segments.Length == 0)
        {
            return MaterialAlphaSettings.Opaque;
        }

        var hasBlend = false;
        var hasNonBlend = false;
        var resolved = segments[0].Alpha;
        for (var i = 1; i < segments.Length; i++)
        {
            var segmentAlpha = segments[i].Alpha;
            if (segmentAlpha.Mode == MaterialAlphaMode.Blend)
            {
                hasBlend = true;
            }
            else
            {
                hasNonBlend = true;
            }
        }

        if (resolved.Mode == MaterialAlphaMode.Blend)
        {
            hasBlend = true;
        }
        else
        {
            hasNonBlend = true;
        }

        if (hasBlend && hasNonBlend)
        {
            throw new InvalidOperationException(
                "Imported scene asset mixes Blend and non-Blend material segments, which the current object-level transparent ordering cannot safely preserve.");
        }

        return resolved;
    }
}
