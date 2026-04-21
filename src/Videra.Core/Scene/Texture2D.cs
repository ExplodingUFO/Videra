namespace Videra.Core.Scene;

public sealed class Texture2D
{
    private readonly byte[] _pixelBytes;

    public Texture2D(
        Texture2DId id,
        string name,
        int width,
        int height,
        TextureImageFormat contentFormat,
        byte[] contentBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentNullException.ThrowIfNull(contentBytes);

        Id = id;
        Name = name;
        Width = width;
        Height = height;
        ContentFormat = contentFormat;
        _pixelBytes = (byte[])contentBytes.Clone();
    }

    public Texture2DId Id { get; }

    public string Name { get; }

    public int Width { get; }

    public int Height { get; }

    public TextureImageFormat ContentFormat { get; }

    public ReadOnlyMemory<byte> ContentBytes => _pixelBytes;
}

public enum TextureImageFormat
{
    Png = 0,
    Jpeg = 1
}
