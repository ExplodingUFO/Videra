using System.Numerics;

namespace Videra.Core.Scene;

public readonly record struct MaterialEmissive(
    Vector3 Factor,
    MaterialTextureBinding? Texture = null)
{
    public static MaterialEmissive Default { get; } = new(Vector3.Zero);
}
