using Videra.Core.Scene;

namespace Videra.Avalonia.Runtime.Scene;

internal sealed class SceneDocumentStore
{
    public SceneDocumentStore(SceneDocument initial)
    {
        Current = initial ?? SceneDocument.Empty;
    }

    public SceneDocument Current { get; private set; }

    public (SceneDocument Previous, SceneDocument Current) Publish(SceneDocument next)
    {
        ArgumentNullException.ThrowIfNull(next);

        var previous = Current;
        Current = next;
        return (previous, Current);
    }
}
