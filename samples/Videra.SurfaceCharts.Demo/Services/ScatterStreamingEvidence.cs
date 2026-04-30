using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Demo.Services;

public sealed record ScatterStreamingScenarioEvidence(
    string ScenarioId,
    ScatterStreamingUpdateMode UpdateMode,
    int InitialPointCount,
    int UpdatePointCount,
    int? FifoCapacity,
    bool Pickable,
    int ColumnarSeriesCount,
    int RetainedPointCount,
    int PickablePointCount,
    int ReplaceBatchCount,
    int AppendBatchCount,
    long DroppedFifoPointCount,
    int LastDroppedFifoPointCount,
    bool EvidenceOnly);

public sealed record ScatterStreamingDataLoggerEvidence(
    int? FifoCapacity,
    bool Pickable,
    int RetainedPointCount,
    int ReplaceBatchCount,
    int AppendBatchCount,
    long AppendedPointCount,
    long DroppedFifoPointCount,
    int LastDroppedFifoPointCount,
    DataLogger3DLiveViewEvidence LiveViewEvidence,
    bool EvidenceOnly);

public static class ScatterStreamingEvidence
{
    public static IReadOnlyList<ScatterStreamingScenarioEvidence> CreateScenarioEvidence()
    {
        var evidence = new ScatterStreamingScenarioEvidence[ScatterStreamingScenarios.All.Count];
        for (var index = 0; index < ScatterStreamingScenarios.All.Count; index++)
        {
            evidence[index] = CreateScenarioEvidence(ScatterStreamingScenarios.All[index]);
        }

        return evidence;
    }

    public static ScatterStreamingDataLoggerEvidence CreateDataLoggerEvidence()
    {
        var stream = new DataLogger3D(0xFF2F80EDu, label: "Live scatter", fifoCapacity: 4);
        stream.Append(ScatterStreamingScenarios.CreateData(0, 3));
        stream.Append(ScatterStreamingScenarios.CreateData(3, 3));
        stream.UseLatestWindow(pointCount: 2);

        return new ScatterStreamingDataLoggerEvidence(
            stream.FifoCapacity,
            stream.Series.Pickable,
            stream.Count,
            stream.ReplaceBatchCount,
            stream.AppendBatchCount,
            stream.TotalAppendedPointCount,
            stream.TotalDroppedPointCount,
            stream.LastDroppedPointCount,
            stream.CreateLiveViewEvidence(),
            EvidenceOnly: true);
    }

    private static ScatterStreamingScenarioEvidence CreateScenarioEvidence(ScatterStreamingScenario scenario)
    {
        var series = ScatterStreamingScenarios.CreateSeries(scenario);
        var data = new ScatterChartData(CreateMetadata(), [], [series]);

        return new ScatterStreamingScenarioEvidence(
            scenario.Id,
            scenario.UpdateMode,
            scenario.InitialPointCount,
            scenario.UpdatePointCount,
            scenario.FifoCapacity,
            scenario.Pickable,
            data.ColumnarSeriesCount,
            data.ColumnarPointCount,
            data.PickablePointCount,
            data.StreamingReplaceBatchCount,
            data.StreamingAppendBatchCount,
            data.StreamingDroppedPointCount,
            data.LastStreamingDroppedPointCount,
            EvidenceOnly: true);
    }

    private static ScatterChartMetadata CreateMetadata()
    {
        return new ScatterChartMetadata(
            new SurfaceAxisDescriptor("Sample", "index", minimum: 0d, maximum: 2_000d),
            new SurfaceAxisDescriptor("Depth", "a.u.", minimum: -15d, maximum: 15d),
            new SurfaceValueRange(-25d, 25d));
    }
}
