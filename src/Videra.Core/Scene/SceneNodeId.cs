namespace Videra.Core.Scene;

public readonly record struct SceneNodeId(Guid Value)
{
    public static SceneNodeId New() => new(Guid.NewGuid());
}
