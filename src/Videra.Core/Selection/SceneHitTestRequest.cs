using System.Numerics;
using Videra.Core.Cameras;
using Videra.Core.Graphics;
using Videra.Core.Scene;

namespace Videra.Core.Selection;

public sealed record SceneHitTestRequest
{
    public SceneHitTestRequest(
        OrbitCamera camera,
        Vector2 viewportSize,
        Vector2 screenPoint,
        IReadOnlyList<Object3D> objects,
        IReadOnlyList<InstanceBatchEntry>? instanceBatches = null)
    {
        Camera = camera ?? throw new ArgumentNullException(nameof(camera));
        Objects = objects ?? throw new ArgumentNullException(nameof(objects));
        InstanceBatches = instanceBatches ?? Array.Empty<InstanceBatchEntry>();
        ViewportSize = viewportSize;
        ScreenPoint = screenPoint;
    }

    public OrbitCamera Camera { get; init; }

    public Vector2 ViewportSize { get; init; }

    public Vector2 ScreenPoint { get; init; }

    public IReadOnlyList<Object3D> Objects { get; init; }

    public IReadOnlyList<InstanceBatchEntry> InstanceBatches { get; init; }
}
