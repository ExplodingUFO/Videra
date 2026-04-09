using System.Numerics;
using Videra.Core.Cameras;
using Videra.Core.Graphics;

namespace Videra.Core.Selection;

public sealed record SceneBoxSelectionQuery
{
    public SceneBoxSelectionQuery(
        OrbitCamera camera,
        Vector2 viewportSize,
        Vector2 startPoint,
        Vector2 endPoint,
        IReadOnlyList<Object3D> objects,
        SceneBoxSelectionMode mode = SceneBoxSelectionMode.Touch)
    {
        Camera = camera ?? throw new ArgumentNullException(nameof(camera));
        Objects = objects ?? throw new ArgumentNullException(nameof(objects));
        ViewportSize = viewportSize;
        StartPoint = startPoint;
        EndPoint = endPoint;
        Mode = mode;
    }

    public OrbitCamera Camera { get; init; }

    public Vector2 ViewportSize { get; init; }

    public Vector2 StartPoint { get; init; }

    public Vector2 EndPoint { get; init; }

    public IReadOnlyList<Object3D> Objects { get; init; }

    public SceneBoxSelectionMode Mode { get; init; }
}
