using Videra.Core.Graphics;

namespace Videra.Core.Selection;

public sealed class SceneHitTestResult
{
    public static SceneHitTestResult Empty { get; } = new([]);

    public SceneHitTestResult(IReadOnlyList<SceneHit> hits)
    {
        Hits = hits ?? throw new ArgumentNullException(nameof(hits));
    }

    public IReadOnlyList<SceneHit> Hits { get; }

    public SceneHit? PrimaryHit => Hits.Count > 0 ? Hits[0] : null;

    public sealed record SceneHit(Guid ObjectId, Object3D Object, float Distance);
}
