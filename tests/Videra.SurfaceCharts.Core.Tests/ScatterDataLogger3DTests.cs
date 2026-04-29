using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class ScatterDataLogger3DTests
{
    [Fact]
    public void Append_TrimsOldestPointsThroughColumnarFifoSemantics()
    {
        var stream = new DataLogger3D(0xFF102030u, fifoCapacity: 3);

        stream.Append(new ScatterColumnarData(
            new float[] { 10f, 20f },
            new float[] { 30f, 40f },
            new float[] { 80f, 90f }));
        stream.Append(new ScatterColumnarData(
            new float[] { 50f, 60f },
            new float[] { 70f, 80f },
            new float[] { 100f, 110f }));

        stream.Count.Should().Be(3);
        stream.X.ToArray().Should().Equal(20f, 50f, 60f);
        stream.Y.ToArray().Should().Equal(40f, 70f, 80f);
        stream.Z.ToArray().Should().Equal(90f, 100f, 110f);
        stream.AppendBatchCount.Should().Be(2);
        stream.ReplaceBatchCount.Should().Be(0);
        stream.TotalAppendedPointCount.Should().Be(4);
        stream.LastDroppedPointCount.Should().Be(1);
        stream.TotalDroppedPointCount.Should().Be(1);
    }

    [Fact]
    public void Replace_TrimsIncomingDataThroughColumnarFifoSemantics()
    {
        var stream = new DataLogger3D(0xFF102030u, fifoCapacity: 2);

        stream.Replace(new ScatterColumnarData(
            new float[] { 10f, 20f, 30f },
            new float[] { 30f, 40f, 50f },
            new float[] { 80f, 90f, 100f },
            new float[] { 1f, 2f, 3f },
            new uint[] { 0xFF111111u, 0xFF222222u, 0xFF333333u }));

        stream.Count.Should().Be(2);
        stream.X.ToArray().Should().Equal(20f, 30f);
        stream.Y.ToArray().Should().Equal(40f, 50f);
        stream.Z.ToArray().Should().Equal(90f, 100f);
        stream.Size.ToArray().Should().Equal(2f, 3f);
        stream.PointColor.ToArray().Should().Equal(0xFF222222u, 0xFF333333u);
        stream.AppendBatchCount.Should().Be(0);
        stream.ReplaceBatchCount.Should().Be(1);
        stream.LastDroppedPointCount.Should().Be(1);
        stream.TotalDroppedPointCount.Should().Be(1);
    }

    [Fact]
    public void Series_ExposesChartDataCountersForEvidence()
    {
        var stream = new DataLogger3D(0xFF102030u, pickable: true, fifoCapacity: 3);
        stream.Append(new ScatterColumnarData(
            new float[] { 10f, 20f },
            new float[] { 30f, 40f },
            new float[] { 80f, 90f }));
        stream.Append(new ScatterColumnarData(
            new float[] { 25f, 30f },
            new float[] { 42f, 45f },
            new float[] { 95f, 100f }));
        var data = new ScatterChartData(CreateMetadata(), [], [stream.Series]);

        data.ColumnarPointCount.Should().Be(3);
        data.PickablePointCount.Should().Be(3);
        data.StreamingAppendBatchCount.Should().Be(2);
        data.StreamingReplaceBatchCount.Should().Be(0);
        data.StreamingDroppedPointCount.Should().Be(1);
        data.LastStreamingDroppedPointCount.Should().Be(1);
        data.ConfiguredFifoCapacity.Should().Be(3);
    }

    [Fact]
    public void Ctor_WrapsExistingColumnarSeriesWithoutReplacingItsCounters()
    {
        var series = new ScatterColumnarSeries(0xFF102030u, fifoCapacity: 2);
        series.AppendRange(new ScatterColumnarData(
            new float[] { 10f, 20f, 30f },
            new float[] { 30f, 40f, 50f },
            new float[] { 80f, 90f, 100f }));

        var stream = new DataLogger3D(series);

        stream.Series.Should().BeSameAs(series);
        stream.Count.Should().Be(2);
        stream.AppendBatchCount.Should().Be(1);
        stream.TotalAppendedPointCount.Should().Be(3);
        stream.TotalDroppedPointCount.Should().Be(1);
    }

    [Fact]
    public void LiveViewEvidence_ReportsFullDataAndLatestWindowDecisions()
    {
        var stream = new DataLogger3D(0xFF102030u, fifoCapacity: 5);
        stream.Append(new ScatterColumnarData(
            new float[] { 10f, 20f, 30f },
            new float[] { 30f, 40f, 50f },
            new float[] { 80f, 90f, 100f }));

        stream.LiveViewMode.Should().Be(DataLogger3DLiveViewMode.FullData);
        stream.CreateLiveViewEvidence().Should().Be(new DataLogger3DLiveViewEvidence(
            DataLogger3DLiveViewMode.FullData,
            AppendedPointCount: 3,
            DroppedPointCount: 0,
            RetainedPointCount: 3,
            VisibleStartIndex: 0,
            VisiblePointCount: 3,
            AutoscaleDecision: DataLogger3DAutoscaleDecision.FullData));

        stream.UseLatestWindow(pointCount: 2);
        stream.Append(new ScatterColumnarData(
            new float[] { 40f, 50f, 60f, 70f },
            new float[] { 60f, 70f, 80f, 90f },
            new float[] { 110f, 120f, 130f, 140f }));

        stream.CreateLiveViewEvidence().Should().Be(new DataLogger3DLiveViewEvidence(
            DataLogger3DLiveViewMode.LatestWindow,
            AppendedPointCount: 7,
            DroppedPointCount: 2,
            RetainedPointCount: 5,
            VisibleStartIndex: 3,
            VisiblePointCount: 2,
            AutoscaleDecision: DataLogger3DAutoscaleDecision.LatestWindow));

        stream.UseFullDataView();

        stream.CreateLiveViewEvidence().VisibleStartIndex.Should().Be(0);
        stream.CreateLiveViewEvidence().VisiblePointCount.Should().Be(5);
        stream.CreateLiveViewEvidence().AutoscaleDecision.Should().Be(DataLogger3DAutoscaleDecision.FullData);
    }

    private static ScatterChartMetadata CreateMetadata()
    {
        return new ScatterChartMetadata(
            new SurfaceAxisDescriptor("Time", "s", minimum: 0d, maximum: 100d),
            new SurfaceAxisDescriptor("Frequency", "Hz", minimum: 50d, maximum: 150d),
            new SurfaceValueRange(10d, 50d));
    }
}
