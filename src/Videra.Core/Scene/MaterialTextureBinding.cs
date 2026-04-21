namespace Videra.Core.Scene;

public sealed class MaterialTextureBinding
{
    public MaterialTextureBinding(
        Texture2DId textureId,
        SamplerId samplerId,
        int coordinateSet,
        TextureColorSpace colorSpace)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(coordinateSet);

        TextureId = textureId;
        SamplerId = samplerId;
        CoordinateSet = coordinateSet;
        ColorSpace = colorSpace;
    }

    public Texture2DId TextureId { get; }

    public SamplerId SamplerId { get; }

    public int CoordinateSet { get; }

    public TextureColorSpace ColorSpace { get; }
}

public enum TextureColorSpace
{
    Linear = 0,
    Srgb = 1
}
