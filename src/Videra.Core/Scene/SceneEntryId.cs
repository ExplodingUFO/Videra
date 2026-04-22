namespace Videra.Core.Scene;

public readonly record struct SceneEntryId(Guid Value)
{
    public static SceneEntryId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
