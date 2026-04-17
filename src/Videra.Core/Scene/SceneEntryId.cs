namespace Videra.Core.Scene;

internal readonly record struct SceneEntryId(Guid Value)
{
    public static SceneEntryId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
