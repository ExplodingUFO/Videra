namespace Videra.Core.Scene;

public sealed class MaterialOcclusionTextureBinding
{
    public MaterialOcclusionTextureBinding(
        MaterialTextureBinding texture,
        float strength)
    {
        Texture = texture ?? throw new ArgumentNullException(nameof(texture));
        Strength = strength;
    }

    public MaterialTextureBinding Texture { get; }

    public float Strength { get; }
}
