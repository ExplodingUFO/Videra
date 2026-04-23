using FluentAssertions;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Core.Tests.Scene;

public sealed class Texture2DTests
{
    [Fact]
    public void Constructor_ClonesContentBytes()
    {
        var sourceBytes = new byte[] { 1, 2, 3, 4 };
        var pixelBytes = new byte[] { 5, 6, 7, 8 };

        var texture = new Texture2D(
            Texture2DId.New(),
            "Texture",
            1,
            1,
            TextureImageFormat.Png,
            sourceBytes,
            pixelBytes);

        sourceBytes[0] = 99;
        pixelBytes[0] = 100;

        texture.ContentBytes.ToArray().Should().Equal(1, 2, 3, 4);
        texture.PixelBytes.ToArray().Should().Equal(5, 6, 7, 8);
    }
}
