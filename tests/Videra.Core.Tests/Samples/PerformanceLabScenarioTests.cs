using FluentAssertions;
using Videra.Demo.Services;
using Videra.SurfaceCharts.Demo.Services;
using Xunit;

namespace Videra.Core.Tests.Samples;

public class PerformanceLabScenarioTests
{
    [Fact]
    public void ViewerScenarios_ShouldExposeDeterministicInstanceBatchDatasets()
    {
        PerformanceLabViewerScenarios.All.Select(static scenario => scenario.Id).Should().Equal(
            "viewer-instance-small",
            "viewer-instance-medium",
            "viewer-instance-large");

        var small = PerformanceLabViewerScenarios.Get("viewer-instance-small");
        var large = PerformanceLabViewerScenarios.Get("viewer-instance-large");

        small.ObjectCount.Should().Be(1000);
        small.Pickable.Should().BeTrue();
        large.ObjectCount.Should().Be(10000);
        large.Pickable.Should().BeFalse();
    }

    [Fact]
    public void ViewerScenarioData_ShouldBeStableAcrossCalls()
    {
        var scenario = PerformanceLabViewerScenarios.Get("viewer-instance-medium");

        var firstTransforms = PerformanceLabViewerScenarios.CreateTransforms(scenario);
        var secondTransforms = PerformanceLabViewerScenarios.CreateTransforms(scenario);
        var firstObjectIds = PerformanceLabViewerScenarios.CreateObjectIds(scenario);
        var secondObjectIds = PerformanceLabViewerScenarios.CreateObjectIds(scenario);
        var colors = PerformanceLabViewerScenarios.CreateColors(scenario);

        firstTransforms.Should().Equal(secondTransforms);
        firstObjectIds.Should().Equal(secondObjectIds);
        firstTransforms.Should().HaveCount(scenario.ObjectCount);
        firstObjectIds.Should().HaveCount(scenario.ObjectCount);
        colors.Should().HaveCount(scenario.ObjectCount);
    }

    [Fact]
    public void ScatterStreamingScenarios_ShouldExposeReplaceAppendAndFifoTrimPaths()
    {
        ScatterStreamingScenarios.All.Select(static scenario => scenario.Id).Should().Equal(
            "scatter-replace-100k",
            "scatter-append-100k",
            "scatter-fifo-trim-100k");

        var replace = ScatterStreamingScenarios.CreateSeries(ScatterStreamingScenarios.Get("scatter-replace-100k"));
        var append = ScatterStreamingScenarios.CreateSeries(ScatterStreamingScenarios.Get("scatter-append-100k"));
        var fifo = ScatterStreamingScenarios.CreateSeries(ScatterStreamingScenarios.Get("scatter-fifo-trim-100k"));

        replace.Count.Should().Be(100_000);
        replace.ReplaceBatchCount.Should().Be(2);
        replace.AppendBatchCount.Should().Be(0);

        append.Count.Should().Be(125_000);
        append.ReplaceBatchCount.Should().Be(1);
        append.AppendBatchCount.Should().Be(1);

        fifo.Count.Should().Be(100_000);
        fifo.FifoCapacity.Should().Be(100_000);
        fifo.TotalDroppedPointCount.Should().Be(50_000);
        fifo.LastDroppedPointCount.Should().Be(50_000);
    }
}
