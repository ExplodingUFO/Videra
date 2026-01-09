using System.Numerics;
using System.Runtime.InteropServices;

namespace Videra.Core.Graphics.Software;

internal sealed class SoftwareFrameBuffer
{
    private byte[] _colorBuffer = Array.Empty<byte>();
    private float[] _depthBuffer = Array.Empty<float>();

    public int Width { get; private set; }
    public int Height { get; private set; }
    public Vector4 ClearColor { get; set; } = new(0f, 0f, 0f, 1f);

    public Span<byte> ColorBuffer => _colorBuffer;
    public Span<float> DepthBuffer => _depthBuffer;
    internal byte[] ColorBufferArray => _colorBuffer;

    public void Resize(int width, int height)
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);

        if (width == Width && height == Height)
            return;

        Width = width;
        Height = height;

        _colorBuffer = new byte[Width * Height * 4];
        _depthBuffer = new float[Width * Height];
    }

    public void Clear(Vector4 color)
    {
        ClearColor = color;
        ClearDepth();

        var packed = PackPremultipliedBgra(color);
        var pixels = MemoryMarshal.Cast<byte, uint>(_colorBuffer);
        pixels.Fill(packed);
    }

    public void ClearDepth()
    {
        Array.Fill(_depthBuffer, 1f);
    }

    public static uint PackPremultipliedBgra(Vector4 color)
    {
        var a = Math.Clamp(color.W, 0f, 1f);
        var r = Math.Clamp(color.X, 0f, 1f) * a;
        var g = Math.Clamp(color.Y, 0f, 1f) * a;
        var b = Math.Clamp(color.Z, 0f, 1f) * a;

        byte br = (byte)Math.Round(r * 255f);
        byte bg = (byte)Math.Round(g * 255f);
        byte bb = (byte)Math.Round(b * 255f);
        byte ba = (byte)Math.Round(a * 255f);

        return (uint)(bb | (bg << 8) | (br << 16) | (ba << 24));
    }
}
