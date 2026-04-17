using System.Numerics;
using Videra.Core.Cameras;
using Videra.Core.Graphics;

namespace Videra.Core.Selection;

public sealed class SelectionBoxService
{
    private readonly SceneBoxSelectionService _boxSelectionService = new();

    public SceneBoxSelectionResult Select(
        OrbitCamera camera,
        Vector2 viewportSize,
        Vector2 startPoint,
        Vector2 endPoint,
        IReadOnlyList<Object3D> objects,
        SceneBoxSelectionMode mode = SceneBoxSelectionMode.Touch)
    {
        ArgumentNullException.ThrowIfNull(camera);
        ArgumentNullException.ThrowIfNull(objects);

        return _boxSelectionService.Select(new SceneBoxSelectionQuery(
            camera,
            viewportSize,
            startPoint,
            endPoint,
            objects,
            mode));
    }
}
