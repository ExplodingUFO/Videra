namespace Videra.Core.Scene;

public readonly record struct MaterialInstanceId(Guid Value)
{
    public static MaterialInstanceId New() => new(Guid.NewGuid());
}
