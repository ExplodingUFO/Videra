namespace Videra.Core.Scene;

public readonly record struct MaterialAlphaSettings(
    MaterialAlphaMode Mode,
    float Cutoff,
    bool DoubleSided)
{
    public static MaterialAlphaSettings Opaque { get; } = new(MaterialAlphaMode.Opaque, 0.5f, false);
}

public enum MaterialAlphaMode
{
    Opaque = 0,
    Mask = 1,
    Blend = 2
}
