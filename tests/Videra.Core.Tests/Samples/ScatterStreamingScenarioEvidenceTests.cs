using FluentAssertions;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Demo.Services;
using Xunit;

namespace Videra.Core.Tests.Samples;

public class ScatterStreamingScenarioEvidenceTests
{
    [Fact]
    public void CreateScenarioEvidence_ReportsColumnarReplaceAppendAndFifoTruth()
    {
        var evidence = ScatterStreamingEvidence.CreateScenarioEvidence();

        evidence.Select(static item => item.ScenarioId).Should().Equal(
            "scatter-replace-100k",
            "scatter-append-100k",
            "scatter-fifo-trim-100k");

        evidence.Should().OnlyContain(static item => item.ColumnarSeriesCount == 1);
        evidence.Should().OnlyContain(static item => !item.Pickable);
        evidence.Should().OnlyContain(static item => item.PickablePointCount == 0);
        evidence.Should().OnlyContain(static item => item.EvidenceOnly);

        evidence[0].Should().BeEquivalentTo(new ScatterStreamingScenarioEvidence(
            "scatter-replace-100k",
            ScatterStreamingUpdateMode.Replace,
            InitialPointCount: 100_000,
            UpdatePointCount: 100_000,
            FifoCapacity: null,
            Pickable: false,
            ColumnarSeriesCount: 1,
            RetainedPointCount: 100_000,
            PickablePointCount: 0,
            ReplaceBatchCount: 2,
            AppendBatchCount: 0,
            DroppedFifoPointCount: 0,
            LastDroppedFifoPointCount: 0,
            EvidenceOnly: true));

        evidence[1].Should().BeEquivalentTo(new ScatterStreamingScenarioEvidence(
            "scatter-append-100k",
            ScatterStreamingUpdateMode.Append,
            InitialPointCount: 100_000,
            UpdatePointCount: 25_000,
            FifoCapacity: null,
            Pickable: false,
            ColumnarSeriesCount: 1,
            RetainedPointCount: 125_000,
            PickablePointCount: 0,
            ReplaceBatchCount: 1,
            AppendBatchCount: 1,
            DroppedFifoPointCount: 0,
            LastDroppedFifoPointCount: 0,
            EvidenceOnly: true));

        evidence[2].Should().BeEquivalentTo(new ScatterStreamingScenarioEvidence(
            "scatter-fifo-trim-100k",
            ScatterStreamingUpdateMode.FifoTrim,
            InitialPointCount: 100_000,
            UpdatePointCount: 50_000,
            FifoCapacity: 100_000,
            Pickable: false,
            ColumnarSeriesCount: 1,
            RetainedPointCount: 100_000,
            PickablePointCount: 0,
            ReplaceBatchCount: 1,
            AppendBatchCount: 1,
            DroppedFifoPointCount: 50_000,
            LastDroppedFifoPointCount: 50_000,
            EvidenceOnly: true));
    }

    [Fact]
    public void CreateDataLoggerEvidence_ReportsDefaultNonPickableFifoLiveStream()
    {
        var evidence = ScatterStreamingEvidence.CreateDataLoggerEvidence();

        evidence.Should().Be(new ScatterStreamingDataLoggerEvidence(
            FifoCapacity: 4,
            Pickable: false,
            RetainedPointCount: 4,
            ReplaceBatchCount: 0,
            AppendBatchCount: 2,
            AppendedPointCount: 6,
            DroppedFifoPointCount: 2,
            LastDroppedFifoPointCount: 2,
            LiveViewEvidence: new DataLogger3DLiveViewEvidence(
                DataLogger3DLiveViewMode.LatestWindow,
                AppendedPointCount: 6,
                DroppedPointCount: 2,
                RetainedPointCount: 4,
                VisibleStartIndex: 2,
                VisiblePointCount: 2,
                AutoscaleDecision: DataLogger3DAutoscaleDecision.LatestWindow),
            EvidenceOnly: true));
    }
}
