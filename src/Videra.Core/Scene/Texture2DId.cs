namespace Videra.Core.Scene;

public readonly record struct Texture2DId(Guid Value)
{
    public static Texture2DId New() => new(Guid.NewGuid());
}
