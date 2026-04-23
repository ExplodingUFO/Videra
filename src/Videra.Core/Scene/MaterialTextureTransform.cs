using System.Numerics;

namespace Videra.Core.Scene;

public readonly record struct MaterialTextureTransform(
    Vector2 Offset,
    Vector2 Scale,
    float Rotation)
{
    public static MaterialTextureTransform Identity { get; } = new(Vector2.Zero, Vector2.One, 0f);
}
