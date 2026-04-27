using System.Collections.Generic;
using System.Numerics;
using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests.Rendering;

public class ScatterRendererTests
{
    [Fact]
    public void ScatterChartDataCtor_RejectsPointsOutsideMetadataBounds()
    {
        var metadata = CreateMetadata();
        var series = new ScatterSeries(
            [
                new ScatterPoint(horizontal: 10d, value: 30d, depth: 80d),
                new ScatterPoint(horizontal: 125d, value: 35d, depth: 90d)
            ],
            color: 0xFF203040u,
            label: "Series A");

        var act = () => new ScatterChartData(metadata, [series]);

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "series");
    }

    [Fact]
    public void BuildScene_MapsScatterPointsIntoRenderReadySeries()
    {
        var data = new ScatterChartData(
            CreateMetadata(),
            [
                new ScatterSeries(
                    [
                        new ScatterPoint(horizontal: 10d, value: 30d, depth: 80d),
                        new ScatterPoint(horizontal: 25d, value: 45d, depth: 100d, color: 0xFFABCDEFu)
                    ],
                    color: 0xFF102030u,
                    label: "Series A"),
                new ScatterSeries(
                    [
                        new ScatterPoint(horizontal: 60d, value: 20d, depth: 140d)
                    ],
                    color: 0xFF405060u,
                    label: "Series B")
            ]);

        var scene = ScatterRenderer.BuildScene(data);

        scene.Metadata.Should().BeSameAs(data.Metadata);
        scene.Series.Should().HaveCount(2);
        scene.Series[0].Label.Should().Be("Series A");
        scene.Series[0].Color.Should().Be(0xFF102030u);
        scene.Series[0].ConnectPoints.Should().BeFalse();
        scene.Series[0].Points.Should().Equal(
            new ScatterRenderPoint(new Vector3(10f, 30f, 80f), 0xFF102030u),
            new ScatterRenderPoint(new Vector3(25f, 45f, 100f), 0xFFABCDEFu));
        scene.Series[1].Label.Should().Be("Series B");
        scene.Series[1].Color.Should().Be(0xFF405060u);
        scene.Series[1].ConnectPoints.Should().BeFalse();
        scene.Series[1].Points.Should().Equal(
            new ScatterRenderPoint(new Vector3(60f, 20f, 140f), 0xFF405060u));
    }

    [Fact]
    public void ScatterSeriesCtor_DefaultsToPointOnlyPresentation()
    {
        var series = new ScatterSeries(
            [new ScatterPoint(horizontal: 10d, value: 30d, depth: 80d)],
            color: 0xFF102030u);

        series.ConnectPoints.Should().BeFalse();
    }

    [Fact]
    public void BuildScene_PropagatesConnectedPointPresentation()
    {
        var data = new ScatterChartData(
            CreateMetadata(),
            [
                new ScatterSeries(
                    [
                        new ScatterPoint(horizontal: 10d, value: 30d, depth: 80d),
                        new ScatterPoint(horizontal: 25d, value: 45d, depth: 100d, color: 0xFFABCDEFu)
                    ],
                    color: 0xFF102030u,
                    label: "Series A",
                    connectPoints: true)
            ]);

        var scene = ScatterRenderer.BuildScene(data);

        scene.Series.Should().ContainSingle();
        scene.Series[0].Color.Should().Be(0xFF102030u);
        scene.Series[0].ConnectPoints.Should().BeTrue();
        scene.Series[0].Points.Should().Equal(
            new ScatterRenderPoint(new Vector3(10f, 30f, 80f), 0xFF102030u),
            new ScatterRenderPoint(new Vector3(25f, 45f, 100f), 0xFFABCDEFu));
    }

    [Fact]
    public void ScatterColumnarSeries_DefaultsToNonPickableHighVolumePath()
    {
        var series = new ScatterColumnarSeries(0xFF102030u);

        series.Pickable.Should().BeFalse();
        series.Count.Should().Be(0);
    }

    [Fact]
    public void ScatterColumnarSeries_ReplaceRange_StoresColumnarData()
    {
        var series = new ScatterColumnarSeries(0xFF102030u, label: "Columnar");
        var data = new ScatterColumnarData(
            new float[] { 10f, 25f },
            new float[] { 30f, 45f },
            new float[] { 80f, 100f },
            new float[] { 1f, 2f },
            new uint[] { 0xFFABCDEFu, 0xFF405060u });

        series.ReplaceRange(data);

        series.Count.Should().Be(2);
        series.X.ToArray().Should().Equal(10f, 25f);
        series.Y.ToArray().Should().Equal(30f, 45f);
        series.Z.ToArray().Should().Equal(80f, 100f);
        series.Size.ToArray().Should().Equal(1f, 2f);
        series.PointColor.ToArray().Should().Equal(0xFFABCDEFu, 0xFF405060u);
        series.ReplaceBatchCount.Should().Be(1);
        series.AppendBatchCount.Should().Be(0);
        series.LastDroppedPointCount.Should().Be(0);
        series.TotalDroppedPointCount.Should().Be(0);
    }

    [Fact]
    public void ScatterColumnarSeries_AppendRange_TrimsOldestPointsWhenFifoCapacityIsSet()
    {
        var series = new ScatterColumnarSeries(0xFF102030u, fifoCapacity: 3);
        series.AppendRange(new ScatterColumnarData(
            new float[] { 10f, 20f },
            new float[] { 30f, 40f },
            new float[] { 80f, 90f }));

        series.AppendRange(new ScatterColumnarData(
            new float[] { 50f, 60f },
            new float[] { 70f, 80f },
            new float[] { 100f, 110f }));

        series.Count.Should().Be(3);
        series.X.ToArray().Should().Equal(20f, 50f, 60f);
        series.Y.ToArray().Should().Equal(40f, 70f, 80f);
        series.Z.ToArray().Should().Equal(90f, 100f, 110f);
        series.AppendBatchCount.Should().Be(2);
        series.TotalAppendedPointCount.Should().Be(4);
        series.LastDroppedPointCount.Should().Be(1);
        series.TotalDroppedPointCount.Should().Be(1);
    }

    [Fact]
    public void ScatterColumnarSeries_ReplaceRange_TrimsIncomingDataToFifoCapacity()
    {
        var series = new ScatterColumnarSeries(0xFF102030u, fifoCapacity: 2);

        series.ReplaceRange(new ScatterColumnarData(
            new float[] { 10f, 20f, 30f },
            new float[] { 30f, 40f, 50f },
            new float[] { 80f, 90f, 100f }));

        series.Count.Should().Be(2);
        series.X.ToArray().Should().Equal(20f, 30f);
        series.LastDroppedPointCount.Should().Be(1);
        series.TotalDroppedPointCount.Should().Be(1);
        series.ReplaceBatchCount.Should().Be(1);
    }

    [Fact]
    public void ScatterColumnarDataCtor_RejectsMismatchedColumnLengths()
    {
        var act = () => new ScatterColumnarData(
            new float[] { 10f, 20f },
            new float[] { 30f },
            new float[] { 80f, 90f });

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "y");
    }

    [Fact]
    public void ScatterColumnarSeries_RejectsNaNCoordinatesByDefault()
    {
        var series = new ScatterColumnarSeries(0xFF102030u);
        var data = new ScatterColumnarData(
            new float[] { 10f, float.NaN },
            new float[] { 30f, 45f },
            new float[] { 80f, 100f });

        var act = () => series.ReplaceRange(data);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "X");
    }

    [Fact]
    public void ScatterColumnarSeries_IsSortedX_RejectsOutOfOrderAppendBatches()
    {
        var series = new ScatterColumnarSeries(0xFF102030u, isSortedX: true);
        series.AppendRange(new ScatterColumnarData(
            new float[] { 10f, 20f },
            new float[] { 30f, 35f },
            new float[] { 80f, 90f }));

        var act = () => series.AppendRange(new ScatterColumnarData(
            new float[] { 19f, 30f },
            new float[] { 40f, 45f },
            new float[] { 100f, 110f }));

        act.Should().Throw<ArgumentException>()
            .WithMessage("*non-decreasing*");
    }

    [Fact]
    public void ScatterColumnarSeries_IsSortedX_AllowsReplacementBelowPreviousRange()
    {
        var series = new ScatterColumnarSeries(0xFF102030u, isSortedX: true);
        series.AppendRange(new ScatterColumnarData(
            new float[] { 50f, 60f },
            new float[] { 30f, 35f },
            new float[] { 80f, 90f }));

        series.ReplaceRange(new ScatterColumnarData(
            new float[] { 10f, 20f },
            new float[] { 40f, 45f },
            new float[] { 100f, 110f }));

        series.X.ToArray().Should().Equal(10f, 20f);
        series.ReplaceBatchCount.Should().Be(1);
        series.AppendBatchCount.Should().Be(1);
    }

    [Fact]
    public void BuildScene_MapsColumnarSeriesIntoRenderReadySeries()
    {
        var columnar = new ScatterColumnarSeries(0xFF102030u, label: "Columnar", pickable: true);
        columnar.ReplaceRange(new ScatterColumnarData(
            new float[] { 10f, 25f },
            new float[] { 30f, 45f },
            new float[] { 80f, 100f },
            new float[] { 1f, 2f },
            new uint[] { 0xFFABCDEFu, 0xFF405060u }));
        var data = new ScatterChartData(CreateMetadata(), [], [columnar]);

        var scene = ScatterRenderer.BuildScene(data);

        data.SeriesCount.Should().Be(1);
        data.ColumnarSeriesCount.Should().Be(1);
        data.ColumnarPointCount.Should().Be(2);
        data.PointCount.Should().Be(2);
        data.PickablePointCount.Should().Be(2);
        data.StreamingReplaceBatchCount.Should().Be(1);
        data.StreamingAppendBatchCount.Should().Be(0);
        data.ConfiguredFifoCapacity.Should().Be(0);
        scene.Series.Should().ContainSingle();
        scene.Series[0].Label.Should().Be("Columnar");
        scene.Series[0].Points.Should().Equal(
            new ScatterRenderPoint(new Vector3(10f, 30f, 80f), 0xFFABCDEFu, 1f),
            new ScatterRenderPoint(new Vector3(25f, 45f, 100f), 0xFF405060u, 2f));
    }

    [Fact]
    public void ScatterChartData_ReportsMutableColumnarStreamingCounters()
    {
        var columnar = new ScatterColumnarSeries(0xFF102030u, fifoCapacity: 3);
        columnar.AppendRange(new ScatterColumnarData(
            new float[] { 10f, 20f },
            new float[] { 30f, 40f },
            new float[] { 80f, 90f }));
        var data = new ScatterChartData(CreateMetadata(), [], [columnar]);

        columnar.AppendRange(new ScatterColumnarData(
            new float[] { 25f, 30f },
            new float[] { 42f, 45f },
            new float[] { 95f, 100f }));

        data.PointCount.Should().Be(3);
        data.ColumnarPointCount.Should().Be(3);
        data.StreamingAppendBatchCount.Should().Be(2);
        data.StreamingDroppedPointCount.Should().Be(1);
        data.LastStreamingDroppedPointCount.Should().Be(1);
        data.ConfiguredFifoCapacity.Should().Be(3);
    }

    [Fact]
    public void BuildScene_SkipsColumnarNaNGapsWhenSeriesAllowsThem()
    {
        var columnar = new ScatterColumnarSeries(0xFF102030u, containsNaN: true);
        columnar.ReplaceRange(new ScatterColumnarData(
            new float[] { 10f, float.NaN, 25f },
            new float[] { 30f, 45f, 45f },
            new float[] { 80f, 100f, 100f }));
        var data = new ScatterChartData(CreateMetadata(), [], [columnar]);

        var scene = ScatterRenderer.BuildScene(data);

        data.PointCount.Should().Be(3);
        scene.Series[0].Points.Should().Equal(
            new ScatterRenderPoint(new Vector3(10f, 30f, 80f), 0xFF102030u),
            new ScatterRenderPoint(new Vector3(25f, 45f, 100f), 0xFF102030u));
    }

    [Fact]
    public void ScatterSeriesCtor_DoesNotAllowMutatingExposedPoints()
    {
        var series = new ScatterSeries(
            [new ScatterPoint(horizontal: 10d, value: 30d, depth: 80d)],
            color: 0xFF102030u);

        var act = () => ((IList<ScatterPoint>)series.Points)[0] =
            new ScatterPoint(horizontal: 99d, value: 99d, depth: 99d);

        act.Should().Throw<NotSupportedException>();
        series.Points[0].Should().Be(new ScatterPoint(horizontal: 10d, value: 30d, depth: 80d));
    }

    [Fact]
    public void ScatterRenderSceneCtor_DoesNotAllowMutatingExposedSeries()
    {
        var firstSeries = new ScatterRenderSeries(
            [new ScatterRenderPoint(new Vector3(10f, 30f, 80f), 0xFF102030u)],
            0xFF102030u,
            label: "Series A");
        var scene = new ScatterRenderScene(CreateMetadata(), [firstSeries]);
        var replacementSeries = new ScatterRenderSeries(
            [new ScatterRenderPoint(new Vector3(20f, 40f, 90f), 0xFF405060u)],
            0xFF405060u,
            label: "Series B");

        var act = () => ((IList<ScatterRenderSeries>)scene.Series)[0] = replacementSeries;

        act.Should().Throw<NotSupportedException>();
        scene.Series[0].Label.Should().Be("Series A");
    }

    [Theory]
    [InlineData(double.NaN, 1d, 2d, "horizontal")]
    [InlineData(1d, double.NaN, 2d, "value")]
    [InlineData(1d, 2f, double.PositiveInfinity, "depth")]
    public void ScatterPointCtor_RejectsNonFiniteCoordinates(
        double horizontal,
        double value,
        double depth,
        string expectedParamName)
    {
        var act = () => new ScatterPoint(horizontal, value, depth);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == expectedParamName);
    }

    [Fact]
    public void BuildScene_RejectsCoordinatesThatDoNotFitSinglePrecisionRenderSpace()
    {
        var metadata = new ScatterChartMetadata(
            new SurfaceAxisDescriptor("Time", "s", minimum: 0d, maximum: (double)float.MaxValue * 2d),
            new SurfaceAxisDescriptor("Frequency", "Hz", minimum: 50d, maximum: 150d),
            new SurfaceValueRange(10d, 50d));
        var data = new ScatterChartData(
            metadata,
            [
                new ScatterSeries(
                    [new ScatterPoint(horizontal: (double)float.MaxValue * 2d, value: 30d, depth: 80d)],
                    color: 0xFF102030u)
            ]);

        var act = () => ScatterRenderer.BuildScene(data);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .Where(ex => ex.ParamName == "Horizontal");
    }

    private static ScatterChartMetadata CreateMetadata()
    {
        return new ScatterChartMetadata(
            new SurfaceAxisDescriptor("Time", "s", minimum: 0d, maximum: 100d),
            new SurfaceAxisDescriptor("Frequency", "Hz", minimum: 50d, maximum: 150d),
            new SurfaceValueRange(10d, 50d));
    }
}
