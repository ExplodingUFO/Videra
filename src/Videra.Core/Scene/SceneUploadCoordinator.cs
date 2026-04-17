using Microsoft.Extensions.Logging;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Scene;

public static class SceneUploadCoordinator
{
    public static Object3D CreateDeferredObject(ImportedSceneAsset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);

        var sceneObject = new Object3D
        {
            Name = asset.Name
        };
        sceneObject.PrepareDeferredMesh(asset.MeshData);
        return sceneObject;
    }

    public static Object3D Upload(ImportedSceneAsset asset, IResourceFactory factory, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(factory);

        var sceneObject = CreateDeferredObject(asset);
        Upload(sceneObject, factory, logger);
        return sceneObject;
    }

    public static void Upload(Object3D sceneObject, IResourceFactory factory, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(sceneObject);
        ArgumentNullException.ThrowIfNull(factory);

        sceneObject.RecreateGraphicsResources(factory, logger);
    }
}
