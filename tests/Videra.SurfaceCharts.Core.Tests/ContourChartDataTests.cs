using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public sealed class ContourChartDataTests
{
    [Fact]
    public void Constructor_WithValidField_CreatesInstance()
    {
        var field = CreateField(3, 3, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f]);

        var data = new ContourChartData(field);

        data.Field.Should().BeSameAs(field);
        data.Mask.Should().BeNull();
        data.LevelCount.Should().Be(10);
    }

    [Fact]
    public void Constructor_WithCustomLevelCount_UsesSpecifiedCount()
    {
        var field = CreateField(3, 3, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f]);

        var data = new ContourChartData(field, levelCount: 5);

        data.LevelCount.Should().Be(5);
        data.HasExplicitLevels.Should().BeFalse();
        data.ExplicitLevels.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithExplicitLevels_PreservesOrderAndCopiesLevels()
    {
        var field = CreateField(3, 3, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f]);
        var levels = new[] { 6f, 2f, 4f };

        var data = new ContourChartData(field, levels);
        levels[0] = 100f;

        data.LevelCount.Should().Be(3);
        data.HasExplicitLevels.Should().BeTrue();
        data.ExplicitLevels.Should().Equal(6f, 2f, 4f);
    }

    [Fact]
    public void Constructor_WithMask_PreservesMask()
    {
        var field = CreateField(3, 3, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f]);
        var mask = new SurfaceMask(3, 3, new ReadOnlyMemory<bool>([true, true, true, true, true, true, true, true, true]));

        var data = new ContourChartData(field, mask);

        data.Mask.Should().BeSameAs(mask);
    }

    [Fact]
    public void Constructor_WithNullField_ThrowsArgumentNullException()
    {
        var action = () => new ContourChartData(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithZeroLevelCount_ThrowsArgumentOutOfRangeException()
    {
        var field = CreateField(3, 3, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f]);

        var action = () => new ContourChartData(field, levelCount: 0);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_WithNegativeLevelCount_ThrowsArgumentOutOfRangeException()
    {
        var field = CreateField(3, 3, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f]);

        var action = () => new ContourChartData(field, levelCount: -1);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_WithEmptyExplicitLevels_ThrowsArgumentException()
    {
        var field = CreateField(3, 3, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f]);

        var action = () => new ContourChartData(field, Array.Empty<float>());
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithNonFiniteExplicitLevels_ThrowsArgumentOutOfRangeException()
    {
        var field = CreateField(3, 3, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f]);

        var action = () => new ContourChartData(field, [1f, float.NaN]);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static SurfaceScalarField CreateField(int width, int height, float[] values)
    {
        var min = values.Min();
        var max = values.Max();
        return new SurfaceScalarField(width, height, values, new SurfaceValueRange(min, max));
    }
}
