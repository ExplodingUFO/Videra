using Microsoft.Extensions.Logging;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Scene;

public static class SceneObjectUploader
{
    public static void Upload(Object3D sceneObject, IResourceFactory factory, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(sceneObject);
        ArgumentNullException.ThrowIfNull(factory);

        sceneObject.RecreateGraphicsResources(factory, logger);
    }
}
