using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public sealed class SurfaceMaskTests
{
    [Fact]
    public void Ctor_RejectsValueCountThatDoesNotMatchShape()
    {
        var act = () => new SurfaceMask(
            width: 2,
            height: 2,
            values: new bool[] { true, false, true });

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "values");
    }

    [Fact]
    public void SurfaceTileStatisticsFromValues_IgnoresMaskedAndNonFiniteSamples()
    {
        var statistics = SurfaceTileStatistics.FromValues(
            values: new float[] { 1f, 2f, float.PositiveInfinity, 4f },
            isExact: true,
            mask: new SurfaceMask(width: 2, height: 2, values: new bool[] { true, false, true, true }));

        statistics.Range.Should().Be(new SurfaceValueRange(1d, 4d));
        statistics.Average.Should().Be(2.5d);
        statistics.SampleCount.Should().Be(4);
        statistics.IsExact.Should().BeTrue();
    }

    [Fact]
    public void SurfaceTileStatisticsFromValues_ReturnsZeroPlaceholderStatsWhenAllSamplesAreInvalid()
    {
        var statistics = SurfaceTileStatistics.FromValues(
            values: new float[] { float.NaN, float.PositiveInfinity },
            isExact: false,
            mask: new SurfaceMask(width: 2, height: 1, values: new bool[] { true, true }));

        statistics.Range.Should().Be(new SurfaceValueRange(0d, 0d));
        statistics.Average.Should().Be(0d);
        statistics.SampleCount.Should().Be(2);
        statistics.IsExact.Should().BeFalse();
    }
}
