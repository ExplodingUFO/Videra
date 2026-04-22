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
        scene.Series[0].ConnectPoints.Should().BeFalse();
        scene.Series[0].Points.Should().Equal(
            new ScatterRenderPoint(new Vector3(10f, 30f, 80f), 0xFF102030u),
            new ScatterRenderPoint(new Vector3(25f, 45f, 100f), 0xFFABCDEFu));
        scene.Series[1].Label.Should().Be("Series B");
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
        scene.Series[0].ConnectPoints.Should().BeTrue();
        scene.Series[0].Points.Should().Equal(
            new ScatterRenderPoint(new Vector3(10f, 30f, 80f), 0xFF102030u),
            new ScatterRenderPoint(new Vector3(25f, 45f, 100f), 0xFFABCDEFu));
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
            label: "Series A");
        var scene = new ScatterRenderScene(CreateMetadata(), [firstSeries]);
        var replacementSeries = new ScatterRenderSeries(
            [new ScatterRenderPoint(new Vector3(20f, 40f, 90f), 0xFF405060u)],
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
