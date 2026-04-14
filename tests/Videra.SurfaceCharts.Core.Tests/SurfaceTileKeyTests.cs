using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class SurfaceTileKeyTests
{
    [Fact]
    public void Equality_IsStableForSameCoordinates()
    {
        var left = new SurfaceTileKey(2, 1, 3, 4);
        var right = new SurfaceTileKey(2, 1, 3, 4);

        left.Should().Be(right);
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equality_DistinguishesDifferentLevels()
    {
        var lowerLevel = new SurfaceTileKey(1, 1, 3, 4);
        var higherLevel = new SurfaceTileKey(2, 1, 3, 4);

        lowerLevel.Should().NotBe(higherLevel);
    }

    [Theory]
    [InlineData(-1, 0, "startX")]
    [InlineData(0, -1, "startY")]
    public void Bounds_Ctor_RejectsNegativeStartCoordinates(int startX, int startY, string paramName)
    {
        var act = () => new SurfaceTileBounds(startX, startY, 1, 1);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == paramName);
    }

    [Theory]
    [InlineData(int.MaxValue, 0, 1, 1, "startX")]
    [InlineData(0, int.MaxValue, 1, 1, "startY")]
    public void Bounds_Ctor_RejectsOverflowingEndCoordinates(int startX, int startY, int width, int height, string paramName)
    {
        var act = () => new SurfaceTileBounds(startX, startY, width, height);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == paramName);
    }

    [Theory]
    [InlineData(-1, 0, 0, 0, "levelX")]
    [InlineData(0, -1, 0, 0, "levelY")]
    [InlineData(0, 0, -1, 0, "tileX")]
    [InlineData(0, 0, 0, -1, "tileY")]
    public void Ctor_RejectsNegativeCoordinates(int levelX, int levelY, int tileX, int tileY, string paramName)
    {
        var act = () => new SurfaceTileKey(levelX, levelY, tileX, tileY);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == paramName);
    }

    [Fact]
    public void Tile_PreservesKeyAndShape()
    {
        var key = new SurfaceTileKey(3, 2, 5, 7);
        var bounds = new SurfaceTileBounds(10, 20, 2, 3);
        var tile = new SurfaceTile(key, 2, 3, bounds, new float[] { 1, 2, 3, 4, 5, 6 }, new SurfaceValueRange(1, 6));

        tile.Key.Should().Be(key);
        tile.Width.Should().Be(2);
        tile.Height.Should().Be(3);
        tile.Bounds.Should().Be(bounds);
        tile.Bounds.EndXExclusive.Should().Be(12);
        tile.Bounds.EndYExclusive.Should().Be(23);
        tile.Values.Length.Should().Be(6);
    }
}
