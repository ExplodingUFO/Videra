namespace Videra.Core.Scene;

public sealed class Texture2D
{
    private readonly byte[] _contentBytes;
    private readonly byte[] _pixelBytes;

    public Texture2D(
        Texture2DId id,
        string name,
        int width,
        int height,
        TextureImageFormat contentFormat,
        byte[] contentBytes,
        byte[] pixelBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentNullException.ThrowIfNull(contentBytes);
        ArgumentNullException.ThrowIfNull(pixelBytes);

        if (pixelBytes.Length != width * height * 4)
        {
            throw new ArgumentException(
                "Texture pixel payload must contain width*height RGBA32 bytes.",
                nameof(pixelBytes));
        }

        Id = id;
        Name = name;
        Width = width;
        Height = height;
        ContentFormat = contentFormat;
        _contentBytes = (byte[])contentBytes.Clone();
        _pixelBytes = (byte[])pixelBytes.Clone();
    }

    public Texture2DId Id { get; }

    public string Name { get; }

    public int Width { get; }

    public int Height { get; }

    public TextureImageFormat ContentFormat { get; }

    public ReadOnlyMemory<byte> ContentBytes => _contentBytes;

    public ReadOnlyMemory<byte> PixelBytes => _pixelBytes;
}

public enum TextureImageFormat
{
    Png = 0,
    Jpeg = 1
}
