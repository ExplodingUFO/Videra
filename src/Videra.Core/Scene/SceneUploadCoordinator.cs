using Microsoft.Extensions.Logging;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Scene;

public static class SceneUploadCoordinator
{
    public static Object3D Upload(ImportedSceneAsset asset, IResourceFactory factory, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(factory);

        var sceneObject = new Object3D
        {
            Name = asset.Name
        };
        sceneObject.Initialize(factory, asset.MeshData, logger);
        return sceneObject;
    }
}
