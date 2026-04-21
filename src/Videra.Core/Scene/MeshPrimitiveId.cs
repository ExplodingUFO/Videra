namespace Videra.Core.Scene;

public readonly record struct MeshPrimitiveId(Guid Value)
{
    public static MeshPrimitiveId New() => new(Guid.NewGuid());
}
