using System.Numerics;
using FluentAssertions;
using Videra.SurfaceCharts.Core.Rendering;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests.Rendering;

public class SvgChartRendererTests
{
    // ── Scatter ────────────────────────────────────────────────────

    [Fact]
    public void RenderScatter_ProducesValidSvgWithCircleElements()
    {
        var scene = CreateScatterScene();

        var svg = SvgChartRenderer.RenderScatter(scene, 800, 600);

        svg.Should().Contain("<svg");
        svg.Should().Contain("</svg>");
        svg.Should().Contain("xmlns=\"http://www.w3.org/2000/svg\"");
        svg.Should().Contain("<circle");
    }

    [Fact]
    public void RenderScatter_IncludesSeriesLabel()
    {
        var scene = CreateScatterScene();

        var svg = SvgChartRenderer.RenderScatter(scene, 800, 600);

        svg.Should().Contain("data-label=\"Test Series\"");
    }

    [Fact]
    public void RenderScatter_ConnectedPointsProducesPolyline()
    {
        var points = new List<ScatterRenderPoint>
        {
            new(new Vector3(0, 0, 0), 0xFF0000),
            new(new Vector3(5, 0, 5), 0xFF0000),
            new(new Vector3(10, 0, 10), 0xFF0000),
        };
        var series = new ScatterRenderSeries(points, 0xFF0000, "Connected", connectPoints: true);
        var metadata = new ScatterChartMetadata(
            new SurfaceAxisDescriptor("X", null, 0, 10),
            new SurfaceAxisDescriptor("Z", null, 0, 10),
            new SurfaceValueRange(0, 10));
        var scene = new ScatterRenderScene(metadata, [series]);

        var svg = SvgChartRenderer.RenderScatter(scene, 800, 600);

        svg.Should().Contain("<polyline");
    }

    // ── Line ───────────────────────────────────────────────────────

    [Fact]
    public void RenderLine_ProducesValidSvgWithLineElements()
    {
        var scene = CreateLineScene();

        var svg = SvgChartRenderer.RenderLine(scene, 800, 600);

        svg.Should().Contain("<svg");
        svg.Should().Contain("<line");
        svg.Should().Contain("</svg>");
    }

    // ── Bar ────────────────────────────────────────────────────────

    [Fact]
    public void RenderBar_ProducesValidSvgWithRectElements()
    {
        var scene = CreateBarScene();

        var svg = SvgChartRenderer.RenderBar(scene, 800, 600);

        svg.Should().Contain("<svg");
        svg.Should().Contain("<rect");
        svg.Should().Contain("</svg>");
    }

    // ── Pie ────────────────────────────────────────────────────────

    [Fact]
    public void RenderPie_ProducesValidSvgWithPathElements()
    {
        var scene = CreatePieScene();

        var svg = SvgChartRenderer.RenderPie(scene, 800, 600);

        svg.Should().Contain("<svg");
        svg.Should().Contain("<path");
        svg.Should().Contain("</svg>");
    }

    [Fact]
    public void RenderPie_DonutProducesInnerArcs()
    {
        var slices = new List<PieRenderSlice>
        {
            new(Vector3.Zero, 0.3f, 1f, 0, 180, 0, 0xFF0000),
            new(Vector3.Zero, 0.3f, 1f, 180, 180, 0, 0x0000FF),
        };
        var scene = new PieRenderScene(2, 0.3, slices);

        var svg = SvgChartRenderer.RenderPie(scene, 800, 600);

        // Donut should produce path elements (not circles)
        svg.Should().Contain("<path");
    }

    // ── Histogram ──────────────────────────────────────────────────

    [Fact]
    public void RenderHistogram_ProducesValidSvgWithRectElements()
    {
        var scene = CreateHistogramScene();

        var svg = SvgChartRenderer.RenderHistogram(scene, 800, 600);

        svg.Should().Contain("<svg");
        svg.Should().Contain("<rect");
        svg.Should().Contain("</svg>");
    }

    // ── Box Plot ───────────────────────────────────────────────────

    [Fact]
    public void RenderBoxPlot_ProducesValidSvgWithBoxesAndWhiskers()
    {
        var scene = CreateBoxPlotScene();

        var svg = SvgChartRenderer.RenderBoxPlot(scene, 800, 600);

        svg.Should().Contain("<svg");
        svg.Should().Contain("<rect");
        svg.Should().Contain("<line");
        svg.Should().Contain("</svg>");
    }

    [Fact]
    public void RenderBoxPlot_WithOutliers_ProducesCircleElements()
    {
        var boxes = new List<BoxPlotRenderBox>
        {
            new(new Vector3(0, 5, 0), new Vector3(0.6f, 4, 0.4f), 0xFF4488CC),
        };
        var whiskers = new List<BoxPlotRenderWhisker>
        {
            new(new Vector3(0, 2, 0), new Vector3(0, 5, 0), 0xFF4488CC),
            new(new Vector3(0, 9, 0), new Vector3(0, 12, 0), 0xFF4488CC),
            new(new Vector3(-0.3f, 7, 0), new Vector3(0.3f, 7, 0), 0xFFFFFFFF),
        };
        var outliers = new List<BoxPlotRenderOutlier>
        {
            new(new Vector3(0, 1, 0), 0xFF4488CC),
            new(new Vector3(0, 15, 0), 0xFF4488CC),
        };
        var scene = new BoxPlotRenderScene(1, boxes, whiskers, outliers);

        var svg = SvgChartRenderer.RenderBoxPlot(scene, 800, 600);

        svg.Should().Contain("<circle");
    }

    // ── OHLC ───────────────────────────────────────────────────────

    [Fact]
    public void RenderOHLC_Candlestick_ProducesValidSvg()
    {
        var scene = CreateOHLCScene(OHLCStyle.Candlestick);

        var svg = SvgChartRenderer.RenderOHLC(scene, 800, 600);

        svg.Should().Contain("<svg");
        svg.Should().Contain("<rect");
        svg.Should().Contain("<line");
        svg.Should().Contain("</svg>");
    }

    [Fact]
    public void RenderOHLC_Standard_ProducesTickMarks()
    {
        var scene = CreateOHLCScene(OHLCStyle.OHLC);

        var svg = SvgChartRenderer.RenderOHLC(scene, 800, 600);

        svg.Should().Contain("<svg");
        svg.Should().Contain("<line");
        svg.Should().Contain("</svg>");
    }

    // ── General ────────────────────────────────────────────────────

    [Fact]
    public void RenderScatter_ContainsViewBox()
    {
        var scene = CreateScatterScene();

        var svg = SvgChartRenderer.RenderScatter(scene, 1024, 768);

        svg.Should().Contain("width=\"1024\"");
        svg.Should().Contain("height=\"768\"");
        svg.Should().Contain("viewBox=\"0 0 1024 768\"");
    }

    [Fact]
    public void RenderScatter_ThrowsOnNull()
    {
        var act = () => SvgChartRenderer.RenderScatter(null!, 800, 600);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RenderLine_ThrowsOnNull()
    {
        var act = () => SvgChartRenderer.RenderLine(null!, 800, 600);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RenderBar_ThrowsOnNull()
    {
        var act = () => SvgChartRenderer.RenderBar(null!, 800, 600);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RenderPie_ThrowsOnNull()
    {
        var act = () => SvgChartRenderer.RenderPie(null!, 800, 600);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Helpers ────────────────────────────────────────────────────

    private static ScatterRenderScene CreateScatterScene()
    {
        var metadata = new ScatterChartMetadata(
            new SurfaceAxisDescriptor("X", null, 0, 10),
            new SurfaceAxisDescriptor("Z", null, 0, 10),
            new SurfaceValueRange(0, 10));
        var points = new List<ScatterRenderPoint>
        {
            new(new Vector3(1, 0, 2), 0xFF0000, 5f),
            new(new Vector3(3, 0, 5), 0xFF0000, 5f),
            new(new Vector3(5, 0, 4), 0xFF0000, 5f),
            new(new Vector3(7, 0, 8), 0xFF0000, 5f),
        };
        var series = new ScatterRenderSeries(points, 0xFF0000, "Test Series");
        return new ScatterRenderScene(metadata, [series]);
    }

    private static LineRenderScene CreateLineScene()
    {
        var segments = new List<LineRenderSegment>
        {
            new(new Vector3(0, 0, 0), new Vector3(1, 1, 0), 2f, 0xFF0000),
            new(new Vector3(1, 1, 0), new Vector3(2, 0.5f, 0), 2f, 0xFF0000),
            new(new Vector3(2, 0.5f, 0), new Vector3(3, 2, 0), 2f, 0xFF0000),
        };
        return new LineRenderScene(1, segments);
    }

    private static BarRenderScene CreateBarScene()
    {
        var bars = new List<BarRenderBar>
        {
            new(new Vector3(0, 2.5f, 0), new Vector3(0.8f, 5, 0.48f), 0xFF38BDF8),
            new(new Vector3(1, 4, 0), new Vector3(0.8f, 8, 0.48f), 0xFFF97316),
            new(new Vector3(2, 1.5f, 0), new Vector3(0.8f, 3, 0.48f), 0xFF2DD4BF),
        };
        return new BarRenderScene(3, 1, BarChartLayout.Grouped, bars);
    }

    private static PieRenderScene CreatePieScene()
    {
        var slices = new List<PieRenderSlice>
        {
            new(Vector3.Zero, 0, 1f, 0, 126, 0, 0xFF38BDF8),
            new(Vector3.Zero, 0, 1f, 126, 90, 0, 0xFFF97316),
            new(Vector3.Zero, 0, 1f, 216, 72, 0, 0xFF2DD4BF),
            new(Vector3.Zero, 0, 1f, 288, 54, 0, 0xFF8B5CF6),
            new(Vector3.Zero, 0, 1f, 342, 18, 0.1f, 0xFFFF6B6B),
        };
        return new PieRenderScene(5, 0, slices);
    }

    private static HistogramRenderScene CreateHistogramScene()
    {
        var bins = new List<HistogramRenderBin>();
        for (var i = 0; i < 10; i++)
        {
            bins.Add(new HistogramRenderBin(
                new Vector3(i * 0.5f, 0, 0),
                new Vector3(0.5f, 0.1f * (i + 1), 0.5f),
                0xFF38BDF8));
        }

        return new HistogramRenderScene(10, HistogramMode.Count, bins);
    }

    private static BoxPlotRenderScene CreateBoxPlotScene()
    {
        var boxes = new List<BoxPlotRenderBox>
        {
            new(new Vector3(0, 5, 0), new Vector3(0.6f, 4, 0.4f), 0xFF4488CC),
            new(new Vector3(1, 7, 0), new Vector3(0.6f, 3, 0.4f), 0xFFF97316),
        };
        var whiskers = new List<BoxPlotRenderWhisker>
        {
            new(new Vector3(0, 2, 0), new Vector3(0, 5, 0), 0xFF4488CC),
            new(new Vector3(0, 9, 0), new Vector3(0, 12, 0), 0xFF4488CC),
            new(new Vector3(-0.3f, 7, 0), new Vector3(0.3f, 7, 0), 0xFFFFFFFF),
            new(new Vector3(1, 4, 0), new Vector3(1, 7, 0), 0xFFF97316),
            new(new Vector3(1, 10, 0), new Vector3(1, 13, 0), 0xFFF97316),
            new(new Vector3(0.7f, 8.5f, 0), new Vector3(1.3f, 8.5f, 0), 0xFFFFFFFF),
        };
        return new BoxPlotRenderScene(2, boxes, whiskers, []);
    }

    private static OHLCRenderScene CreateOHLCScene(OHLCStyle style)
    {
        var bars = new List<OHLCRenderBar>
        {
            new(new Vector3(-0.3f, 100, 0), new Vector3(0.3f, 105, 0), new Vector3(0, 108, 0), new Vector3(0, 98, 0), 0xFF00FF00, true),
            new(new Vector3(0.7f, 106, 0), new Vector3(1.3f, 101, 0), new Vector3(1, 107, 0), new Vector3(1, 100, 0), 0xFFFF0000, false),
        };
        return new OHLCRenderScene(2, style, bars);
    }
}
