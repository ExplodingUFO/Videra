namespace Videra.Core.Scene;

public sealed class MaterialTextureBinding
{
    public MaterialTextureBinding(
        Texture2DId textureId,
        SamplerId samplerId,
        int coordinateSet,
        TextureColorSpace colorSpace,
        MaterialTextureTransform transform = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(coordinateSet);

        TextureId = textureId;
        SamplerId = samplerId;
        CoordinateSet = coordinateSet;
        ColorSpace = colorSpace;
        Transform = transform;
    }

    public Texture2DId TextureId { get; }

    public SamplerId SamplerId { get; }

    public int CoordinateSet { get; }

    public TextureColorSpace ColorSpace { get; }

    public MaterialTextureTransform Transform { get; }
}

public enum TextureColorSpace
{
    Linear = 0,
    Srgb = 1
}
