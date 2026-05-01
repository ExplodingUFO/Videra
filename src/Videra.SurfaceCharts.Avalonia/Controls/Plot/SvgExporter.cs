using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Exports Plot3D chart data to SVG format by building render scenes from chart data
/// and delegating to <see cref="SvgChartRenderer"/>.
/// </summary>
internal static class SvgExporter
{
    /// <summary>
    /// Exports the active series of a Plot3D to SVG.
    /// </summary>
    internal static string ExportPlot(Plot3D plot, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(plot);

        var activeSeries = plot.ActiveSeries;
        if (activeSeries is null)
        {
            return RenderEmptySvg(width, height);
        }

        return activeSeries.Kind switch
        {
            Plot3DSeriesKind.Scatter => ExportScatter(plot, width, height),
            Plot3DSeriesKind.Line => ExportLine(plot, width, height),
            Plot3DSeriesKind.Bar => ExportBar(plot, width, height),
            Plot3DSeriesKind.Pie => ExportPie(plot, width, height),
            Plot3DSeriesKind.Histogram => ExportHistogram(plot, width, height),
            Plot3DSeriesKind.BoxPlot => ExportBoxPlot(plot, width, height),
            Plot3DSeriesKind.OHLC => ExportOHLC(plot, width, height),
            _ => RenderEmptySvg(width, height),
        };
    }

    private static string ExportScatter(Plot3D plot, int width, int height)
    {
        var scatterData = plot.ActiveScatterData;
        if (scatterData is null)
        {
            return RenderEmptySvg(width, height);
        }

        var scene = ScatterRenderer.BuildScene(scatterData);
        return SvgChartRenderer.RenderScatter(scene, width, height);
    }

    private static string ExportLine(Plot3D plot, int width, int height)
    {
        var lineData = plot.ActiveLineData;
        if (lineData is null)
        {
            return RenderEmptySvg(width, height);
        }

        var scene = LineRenderer.BuildScene(lineData);
        return SvgChartRenderer.RenderLine(scene, width, height);
    }

    private static string ExportBar(Plot3D plot, int width, int height)
    {
        var barData = plot.ActiveBarData;
        if (barData is null)
        {
            return RenderEmptySvg(width, height);
        }

        var scene = BarRenderer.BuildScene(barData);
        return SvgChartRenderer.RenderBar(scene, width, height);
    }

    private static string ExportPie(Plot3D plot, int width, int height)
    {
        var pieData = plot.ActivePieData;
        if (pieData is null)
        {
            return RenderEmptySvg(width, height);
        }

        var scene = PieRenderer.Render(pieData);
        return SvgChartRenderer.RenderPie(scene, width, height);
    }

    private static string ExportHistogram(Plot3D plot, int width, int height)
    {
        var histData = plot.ActiveHistogramData;
        if (histData is null)
        {
            return RenderEmptySvg(width, height);
        }

        var scene = HistogramRenderer.Render(histData);
        return SvgChartRenderer.RenderHistogram(scene, width, height);
    }

    private static string ExportBoxPlot(Plot3D plot, int width, int height)
    {
        var boxData = plot.ActiveBoxPlotData;
        if (boxData is null)
        {
            return RenderEmptySvg(width, height);
        }

        var scene = BoxPlotRenderer.BuildScene(boxData);
        return SvgChartRenderer.RenderBoxPlot(scene, width, height);
    }

    private static string ExportOHLC(Plot3D plot, int width, int height)
    {
        var ohlcData = plot.ActiveOHLCData;
        if (ohlcData is null)
        {
            return RenderEmptySvg(width, height);
        }

        var scene = OHLCRenderer.Render(ohlcData);
        return SvgChartRenderer.RenderOHLC(scene, width, height);
    }

    private static string RenderEmptySvg(int width, int height)
    {
        return $"""
            <svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">
              <text x="{width / 2}" y="{height / 2}" text-anchor="middle" fill="#999">No chart data</text>
            </svg>
            """;
    }
}
