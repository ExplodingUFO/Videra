using Microsoft.Extensions.Logging;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Scene;

public static class SceneUploadCoordinator
{
    public static Object3D CreateDeferredObject(ImportedSceneAsset asset)
    {
        return SceneObjectFactory.CreateDeferred(asset);
    }

    public static Object3D Upload(ImportedSceneAsset asset, IResourceFactory factory, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(factory);

        var sceneObject = SceneObjectFactory.CreateDeferred(asset);
        SceneObjectUploader.Upload(sceneObject, factory, logger);
        return sceneObject;
    }

    public static void Upload(Object3D sceneObject, IResourceFactory factory, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(sceneObject);
        ArgumentNullException.ThrowIfNull(factory);

        SceneObjectUploader.Upload(sceneObject, factory, logger);
    }
}
