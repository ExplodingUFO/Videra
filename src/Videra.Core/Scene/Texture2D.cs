namespace Videra.Core.Scene;

public sealed class Texture2D
{
    private readonly byte[] _pixelBytes;

    public Texture2D(
        Texture2DId id,
        string name,
        int width,
        int height,
        Texture2DPixelFormat pixelFormat,
        byte[] pixelBytes,
        bool isSrgb)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentNullException.ThrowIfNull(pixelBytes);

        Id = id;
        Name = name;
        Width = width;
        Height = height;
        PixelFormat = pixelFormat;
        _pixelBytes = (byte[])pixelBytes.Clone();
        IsSrgb = isSrgb;
    }

    public Texture2DId Id { get; }

    public string Name { get; }

    public int Width { get; }

    public int Height { get; }

    public Texture2DPixelFormat PixelFormat { get; }

    public IReadOnlyList<byte> PixelBytes => _pixelBytes;

    public bool IsSrgb { get; }
}

public enum Texture2DPixelFormat
{
    Rgba8Unorm = 0,
    Rgba8Srgb = 1
}
