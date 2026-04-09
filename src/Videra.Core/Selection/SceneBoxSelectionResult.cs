using Videra.Core.Graphics;

namespace Videra.Core.Selection;

public sealed class SceneBoxSelectionResult
{
    public static SceneBoxSelectionResult Empty { get; } = new([], []);

    public SceneBoxSelectionResult(IReadOnlyList<Guid> objectIds, IReadOnlyList<Object3D> objects)
    {
        ObjectIds = objectIds ?? throw new ArgumentNullException(nameof(objectIds));
        Objects = objects ?? throw new ArgumentNullException(nameof(objects));
    }

    public IReadOnlyList<Guid> ObjectIds { get; }

    public IReadOnlyList<Object3D> Objects { get; }
}
