namespace Videra.Core.Scene;

public sealed class Sampler
{
    public Sampler(
        SamplerId id,
        string name,
        TextureFilter minFilter,
        TextureFilter magFilter,
        TextureWrapMode wrapU,
        TextureWrapMode wrapV)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
        MinFilter = minFilter;
        MagFilter = magFilter;
        WrapU = wrapU;
        WrapV = wrapV;
    }

    public SamplerId Id { get; }

    public string Name { get; }

    public TextureFilter MinFilter { get; }

    public TextureFilter MagFilter { get; }

    public TextureWrapMode WrapU { get; }

    public TextureWrapMode WrapV { get; }
}

public enum TextureFilter
{
    Nearest = 0,
    Linear = 1
}

public enum TextureWrapMode
{
    Repeat = 0,
    ClampToEdge = 1,
    MirroredRepeat = 2
}
