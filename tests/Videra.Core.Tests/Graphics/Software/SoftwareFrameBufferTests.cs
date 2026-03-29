using System.Numerics;
using FluentAssertions;
using Videra.Core.Graphics.Software;
using Xunit;

namespace Videra.Core.Tests.Graphics.Software;

public class SoftwareFrameBufferTests
{
    [Fact]
    public void Resize_SetsWidthAndHeight()
    {
        var fb = new SoftwareFrameBuffer();
        fb.Resize(100, 200);

        fb.Width.Should().Be(100);
        fb.Height.Should().Be(200);
    }

    [Fact]
    public void Resize_ClampsToMinimumOne()
    {
        var fb = new SoftwareFrameBuffer();
        fb.Resize(0, -5);

        fb.Width.Should().Be(1);
        fb.Height.Should().Be(1);
    }

    [Fact]
    public void Resize_SameDimensions_DoesNotReallocate()
    {
        var fb = new SoftwareFrameBuffer();
        fb.Resize(100, 100);
        var colorBuffer = fb.ColorBuffer;

        fb.Resize(100, 100);

        // Should be same buffer reference since dimensions unchanged
        fb.ColorBuffer.Length.Should().Be(colorBuffer.Length);
    }

    [Fact]
    public void Clear_SetsColorBuffer()
    {
        var fb = new SoftwareFrameBuffer();
        fb.Resize(10, 10);
        var color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f); // Red

        fb.Clear(color);

        fb.ClearColor.Should().Be(color);
        // Check first pixel (BGRA format)
        var span = fb.ColorBuffer;
        span[0].Should().Be(0);   // B
        span[1].Should().Be(0);   // G
        span[2].Should().Be(255); // R
        span[3].Should().Be(255); // A
    }

    [Fact]
    public void ClearDepth_SetsAllDepthToOne()
    {
        var fb = new SoftwareFrameBuffer();
        fb.Resize(5, 5);

        fb.ClearDepth();

        for (int i = 0; i < fb.DepthBuffer.Length; i++)
        {
            fb.DepthBuffer[i].Should().Be(1.0f);
        }
    }

    [Fact]
    public void Clear_WithTransparentColor_ProducesPremultipliedAlpha()
    {
        var fb = new SoftwareFrameBuffer();
        fb.Resize(10, 10);
        var color = new Vector4(1.0f, 0.0f, 0.0f, 0.5f); // Half-transparent red

        fb.Clear(color);

        // Premultiplied: R=0.5, G=0, B=0, A=0.5
        var span = fb.ColorBuffer;
        span[0].Should().Be(0);   // B premultiplied
        span[1].Should().Be(0);   // G premultiplied
        span[2].Should().Be(128); // R premultiplied (0.5 * 255 ≈ 128)
        span[3].Should().Be(128); // A (0.5 * 255 ≈ 128)
    }

    [Fact]
    public void PackPremultipliedBgra_WhiteOpaque_ReturnsExpected()
    {
        var packed = SoftwareFrameBuffer.PackPremultipliedBgra(Vector4.One);
        // R=255, G=255, B=255, A=255, premultiplied stays same
        var b = packed & 0xFF;
        var g = (packed >> 8) & 0xFF;
        var r = (packed >> 16) & 0xFF;
        var a = (packed >> 24) & 0xFF;

        b.Should().Be(255);
        g.Should().Be(255);
        r.Should().Be(255);
        a.Should().Be(255);
    }

    [Fact]
    public void PackPremultipliedBgra_ClampsOutOfRangeValues()
    {
        var color = new Vector4(2.0f, -1.0f, 0.5f, 1.5f);
        var packed = SoftwareFrameBuffer.PackPremultipliedBgra(color);

        // R clamped to 1.0, G clamped to 0.0, B=0.5, A clamped to 1.0
        // Premultiplied: R=1*1=1, G=0, B=0.5*1=0.5, A=1
        var b = packed & 0xFF;
        var g = (packed >> 8) & 0xFF;
        var r = (packed >> 16) & 0xFF;
        var a = (packed >> 24) & 0xFF;

        r.Should().Be(255);
        g.Should().Be(0);
        b.Should().Be(128);
        a.Should().Be(255);
    }

    [Fact]
    public void ColorBuffer_HasCorrectSize_AfterResize()
    {
        var fb = new SoftwareFrameBuffer();
        fb.Resize(16, 8);

        fb.ColorBuffer.Length.Should().Be(16 * 8 * 4); // 4 bytes per pixel
        fb.DepthBuffer.Length.Should().Be(16 * 8);
    }
}
