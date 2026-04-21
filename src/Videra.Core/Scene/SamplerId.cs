namespace Videra.Core.Scene;

public readonly record struct SamplerId(Guid Value)
{
    public static SamplerId New() => new(Guid.NewGuid());
}
