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
        return sceneObject;
    }
}
