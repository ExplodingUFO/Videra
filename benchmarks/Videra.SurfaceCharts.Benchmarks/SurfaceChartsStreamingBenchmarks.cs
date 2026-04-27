using BenchmarkDotNet.Attributes;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Benchmarks;

[MemoryDiagnoser]
public class SurfaceChartsStreamingBenchmarks
{
    private const int BatchPointCount = 1_024;
    private const int RetainedPointCount = 8_192;

    private ScatterColumnarData _appendBatch;
    private ScatterColumnarData _fifoAppendBatch;
    private ScatterColumnarData _initialRetainedBatch;
    private ScatterChartMetadata _metadata = null!;
    private ScatterColumnarSeries _diagnosticSeries = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _appendBatch = CreateBatch(0, BatchPointCount);
        _fifoAppendBatch = CreateBatch(RetainedPointCount, BatchPointCount);
        _initialRetainedBatch = CreateBatch(0, RetainedPointCount);
        _metadata = new ScatterChartMetadata(
            new SurfaceAxisDescriptor("Horizontal", null, 0d, 20_000d),
            new SurfaceAxisDescriptor("Depth", null, 0d, 20_000d),
            new SurfaceValueRange(0d, 20_000d));

        _diagnosticSeries = CreateFifoSeries();
        _diagnosticSeries.ReplaceRange(_initialRetainedBatch);
        _diagnosticSeries.AppendRange(_fifoAppendBatch);
    }

    [Benchmark]
    public int AppendColumnarBatch()
    {
        var series = CreateUnboundedSeries();
        series.AppendRange(_appendBatch);
        return series.Count;
    }

    [Benchmark]
    public int AppendColumnarBatch_WithFifoTrim()
    {
        var series = CreateFifoSeries();
        series.ReplaceRange(_initialRetainedBatch);
        series.AppendRange(_fifoAppendBatch);
        return series.Count + series.LastDroppedPointCount;
    }

    [Benchmark]
    public int StreamingDiagnosticsAggregation()
    {
        var data = new ScatterChartData(_metadata, [], [_diagnosticSeries]);
        return data.ColumnarPointCount
            + data.StreamingAppendBatchCount
            + data.StreamingReplaceBatchCount
            + data.ConfiguredFifoCapacity
            + data.LastStreamingDroppedPointCount;
    }

    private static ScatterColumnarSeries CreateUnboundedSeries()
    {
        return new ScatterColumnarSeries(
            0xFF22C55Eu,
            "Streaming",
            isSortedX: true,
            pickable: false);
    }

    private static ScatterColumnarSeries CreateFifoSeries()
    {
        return new ScatterColumnarSeries(
            0xFF22C55Eu,
            "Streaming FIFO",
            isSortedX: true,
            pickable: false,
            fifoCapacity: RetainedPointCount);
    }

    private static ScatterColumnarData CreateBatch(int start, int count)
    {
        var x = new float[count];
        var y = new float[count];
        var z = new float[count];
        var size = new float[count];

        for (var index = 0; index < count; index++)
        {
            var value = start + index;
            x[index] = value;
            y[index] = value * 0.5f;
            z[index] = value * 0.25f;
            size[index] = 1f + (index % 4);
        }

        return new ScatterColumnarData(x, y, z, size);
    }
}
