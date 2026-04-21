namespace Videra.Core.Scene;

public sealed class MaterialNormalTextureBinding
{
    public MaterialNormalTextureBinding(
        MaterialTextureBinding texture,
        float scale)
    {
        Texture = texture ?? throw new ArgumentNullException(nameof(texture));
        Scale = scale;
    }

    public MaterialTextureBinding Texture { get; }

    public float Scale { get; }
}
