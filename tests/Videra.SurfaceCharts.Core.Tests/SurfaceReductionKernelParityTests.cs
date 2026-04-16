using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public sealed class SurfaceReductionKernelParityTests
{
    [Fact]
    public void ManagedKernel_MatchesCurrentAverageReduction()
    {
        ISurfaceTileReductionKernel kernel = new ManagedSurfaceTileReductionKernel();
        var sourceValues = new float[]
        {
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16
        };

        var statistics = kernel.ReduceRegion(sourceValues, sourceWidth: 4, startX: 0, startY: 0, width: 2, height: 2);

        statistics.Average.Should().Be(3.5d);
        statistics.SampleCount.Should().Be(4);
        statistics.IsExact.Should().BeFalse();
    }

    [Fact]
    public void ManagedKernel_PreservesRangeAndAverageForTileStatistics()
    {
        ISurfaceTileReductionKernel kernel = new ManagedSurfaceTileReductionKernel();
        var sourceValues = new float[]
        {
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16
        };

        var statistics = kernel.ReduceRegion(sourceValues, sourceWidth: 4, startX: 0, startY: 0, width: 4, height: 4);

        statistics.Range.Should().Be(new SurfaceValueRange(1, 16));
        statistics.Average.Should().Be(8.5d);
        statistics.SampleCount.Should().Be(16);
        statistics.IsExact.Should().BeFalse();
    }
}
