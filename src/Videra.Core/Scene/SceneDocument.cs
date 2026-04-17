using Videra.Core.Graphics;

namespace Videra.Core.Scene;

public sealed class SceneDocument
{
    public static SceneDocument Empty { get; } = new(Array.Empty<Object3D>());

    public SceneDocument(IEnumerable<Object3D> sceneObjects)
    {
        ArgumentNullException.ThrowIfNull(sceneObjects);
        SceneObjects = sceneObjects.ToArray();
    }

    public IReadOnlyList<Object3D> SceneObjects { get; }
}
