using System.Numerics;
using FluentAssertions;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests.Picking;

public sealed class SeriesProbeStrategyTests
{
    #region ScatterProbeStrategy Tests

    [Fact]
    public void ScatterProbeStrategy_FindNearestPoint_InPointSeries()
    {
        var metadata = CreateScatterMetadata();
        var points = new[]
        {
            new ScatterPoint(10d, 100d, 20d),
            new ScatterPoint(15d, 150d, 25d),
            new ScatterPoint(20d, 200d, 30d),
        };
        var series = new ScatterSeries(points, 0xFF0000);
        var data = new ScatterChartData(metadata, new[] { series });
        var strategy = new ScatterProbeStrategy(data);

        // Probe near the second point (15, 25)
        var result = strategy.TryResolve(14.5d, 24.5d, CreateSurfaceMetadata());

        result.Should().NotBeNull();
        var probe = result!.Value;
        probe.Value.Should().Be(150d);
        probe.AxisX.Should().Be(15d);
        probe.AxisY.Should().Be(25d);
    }

    [Fact]
    public void ScatterProbeStrategy_FindNearestPoint_InColumnarSeries()
    {
        var metadata = CreateScatterMetadata();
        var columnar = new ScatterColumnarSeries(0xFF0000, containsNaN: true);
        columnar.AppendRange(new ScatterColumnarData(
            x: new float[] { 10f, 15f, 20f },
            y: new float[] { 100f, 150f, 200f },
            z: new float[] { 20f, 25f, 30f }));
        var data = new ScatterChartData(metadata, [], new[] { columnar });
        var strategy = new ScatterProbeStrategy(data);

        // Probe near the first point (10, 20)
        var result = strategy.TryResolve(10.2d, 19.8d, CreateSurfaceMetadata());

        result.Should().NotBeNull();
        var probe = result!.Value;
        probe.Value.Should().Be(100d);
        probe.AxisX.Should().Be(10d);
        probe.AxisY.Should().Be(20d);
    }

    [Fact]
    public void ScatterProbeStrategy_ReturnsNull_ForEmptySeries()
    {
        var metadata = CreateScatterMetadata();
        var data = new ScatterChartData(metadata, []);
        var strategy = new ScatterProbeStrategy(data);

        var result = strategy.TryResolve(15d, 25d, CreateSurfaceMetadata());

        result.Should().BeNull();
    }

    [Fact]
    public void ScatterProbeStrategy_SkipsNaN_InColumnarSeries()
    {
        var metadata = CreateScatterMetadata();
        var columnar = new ScatterColumnarSeries(0xFF0000, containsNaN: true);
        columnar.AppendRange(new ScatterColumnarData(
            x: new float[] { 10f, float.NaN, 20f },
            y: new float[] { 100f, float.NaN, 200f },
            z: new float[] { 20f, float.NaN, 30f }));
        var data = new ScatterChartData(metadata, [], new[] { columnar });
        var strategy = new ScatterProbeStrategy(data);

        // Probe near second point (which is NaN) — should find first point
        var result = strategy.TryResolve(12d, 22d, CreateSurfaceMetadata());

        result.Should().NotBeNull();
        result!.Value.Value.Should().Be(100d);
    }

    #endregion

    #region BarProbeStrategy Tests

    [Fact]
    public void BarProbeStrategy_HitBar_ReturnsBarValue()
    {
        var bars = new[]
        {
            new BarRenderBar(new Vector3(5f, 0f, 10f), new Vector3(2f, 50f, 2f), 0xFF0000),
            new BarRenderBar(new Vector3(10f, 0f, 10f), new Vector3(2f, 75f, 2f), 0x00FF00),
        };
        var scene = new BarRenderScene(categoryCount: 2, seriesCount: 1, BarChartLayout.Grouped, bars);
        var strategy = new BarProbeStrategy(scene);

        // Hit the second bar (center at 10, 10)
        var result = strategy.TryResolve(10d, 10d, CreateSurfaceMetadata());

        result.Should().NotBeNull();
        var probe = result!.Value;
        probe.Value.Should().Be(75d); // Position.Y + Size.Y = 0 + 75
        probe.AxisX.Should().Be(10d);
        probe.AxisY.Should().Be(10d);
    }

    [Fact]
    public void BarProbeStrategy_OutsideBar_ReturnsNull()
    {
        var bars = new[]
        {
            new BarRenderBar(new Vector3(5f, 0f, 10f), new Vector3(2f, 50f, 2f), 0xFF0000),
        };
        var scene = new BarRenderScene(categoryCount: 1, seriesCount: 1, BarChartLayout.Grouped, bars);
        var strategy = new BarProbeStrategy(scene);

        // Probe outside bar footprint
        var result = strategy.TryResolve(20d, 20d, CreateSurfaceMetadata());

        result.Should().BeNull();
    }

    [Fact]
    public void BarProbeStrategy_OverlappingBars_PrefersClosest()
    {
        var bars = new[]
        {
            new BarRenderBar(new Vector3(5f, 0f, 10f), new Vector3(4f, 50f, 4f), 0xFF0000),
            new BarRenderBar(new Vector3(6f, 0f, 10f), new Vector3(4f, 75f, 4f), 0x00FF00),
        };
        var scene = new BarRenderScene(categoryCount: 2, seriesCount: 1, BarChartLayout.Grouped, bars);
        var strategy = new BarProbeStrategy(scene);

        // Probe at 5.5 — both bars overlap, should prefer closer to center
        var result = strategy.TryResolve(5.5d, 10d, CreateSurfaceMetadata());

        result.Should().NotBeNull();
        var probe = result!.Value;
        // The bar at (5,10) is closer to (5.5,10) than (6,10)
        probe.AxisX.Should().Be(5d);
        probe.Value.Should().Be(50d);
    }

    [Fact]
    public void BarProbeStrategy_EmptyBars_ReturnsNull()
    {
        var scene = new BarRenderScene(categoryCount: 1, seriesCount: 1, BarChartLayout.Grouped, []);
        var strategy = new BarProbeStrategy(scene);

        var result = strategy.TryResolve(5d, 10d, CreateSurfaceMetadata());

        result.Should().BeNull();
    }

    #endregion

    #region ContourProbeStrategy Tests

    [Fact]
    public void ContourProbeStrategy_NearSegment_ReturnsIsoValue()
    {
        var segments = new[]
        {
            new ContourSegment(new Vector3(0f, 0f, 0f), new Vector3(10f, 0f, 0f)),
        };
        var line = new ContourLine(isoValue: 42f, segments);
        var scene = new ContourRenderScene(CreateSurfaceMetadata(), new[] { line });
        var strategy = new ContourProbeStrategy(scene, snapRadius: 1.0);

        // Probe near the segment (y=0 line from x=0 to x=10)
        var result = strategy.TryResolve(5d, 0.1d, CreateSurfaceMetadata());

        result.Should().NotBeNull();
        result!.Value.Value.Should().Be(42d);
    }

    [Fact]
    public void ContourProbeStrategy_OutsideSnapRadius_ReturnsNull()
    {
        var segments = new[]
        {
            new ContourSegment(new Vector3(0f, 0f, 0f), new Vector3(10f, 0f, 0f)),
        };
        var line = new ContourLine(isoValue: 42f, segments);
        var scene = new ContourRenderScene(CreateSurfaceMetadata(), new[] { line });
        var strategy = new ContourProbeStrategy(scene, snapRadius: 0.5);

        // Probe far from any segment
        var result = strategy.TryResolve(5d, 5d, CreateSurfaceMetadata());

        result.Should().BeNull();
    }

    [Fact]
    public void ContourProbeStrategy_EmptyLines_ReturnsNull()
    {
        var scene = new ContourRenderScene(CreateSurfaceMetadata(), []);
        var strategy = new ContourProbeStrategy(scene);

        var result = strategy.TryResolve(5d, 5d, CreateSurfaceMetadata());

        result.Should().BeNull();
    }

    [Fact]
    public void ContourProbeStrategy_ProjectsOntoSegment()
    {
        var segments = new[]
        {
            new ContourSegment(new Vector3(0f, 0f, 0f), new Vector3(10f, 0f, 0f)),
        };
        var line = new ContourLine(isoValue: 42f, segments);
        var scene = new ContourRenderScene(CreateSurfaceMetadata(), new[] { line });
        var strategy = new ContourProbeStrategy(scene, snapRadius: 1.0);

        // Probe at x=3, y=0.1 — should project to x=3 on the segment
        var result = strategy.TryResolve(3d, 0.1d, CreateSurfaceMetadata());

        result.Should().NotBeNull();
        var probe = result!.Value;
        probe.SampleX.Should().BeApproximately(3d, 0.01d);
        probe.SampleY.Should().BeApproximately(0d, 0.01d);
    }

    #endregion

    #region Helper Methods

    private static ScatterChartMetadata CreateScatterMetadata()
    {
        return new ScatterChartMetadata(
            new SurfaceAxisDescriptor("X", null, 0d, 30d),
            new SurfaceAxisDescriptor("Z", null, 0d, 40d),
            new SurfaceValueRange(0d, 300d));
    }

    private static SurfaceMetadata CreateSurfaceMetadata()
    {
        return new SurfaceMetadata(
            width: 10,
            height: 10,
            new SurfaceAxisDescriptor("X", null, 0d, 30d),
            new SurfaceAxisDescriptor("Z", null, 0d, 40d),
            new SurfaceValueRange(0d, 300d));
    }

    #endregion
}
