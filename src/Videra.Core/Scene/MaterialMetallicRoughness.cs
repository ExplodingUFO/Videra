namespace Videra.Core.Scene;

public readonly record struct MaterialMetallicRoughness(
    float MetallicFactor,
    float RoughnessFactor,
    MaterialTextureBinding? Texture = null)
{
    public static MaterialMetallicRoughness Default { get; } = new(1f, 1f);
}
