using System.Numerics;
using BenchmarkDotNet.Attributes;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Benchmarks;

[MemoryDiagnoser]
public class SurfaceChartsNewChartTypeBenchmarks
{
    private const int HistogramValueCount = 1_000;
    private const int FunctionPlotSampleCount = 300;
    private const int PieSliceCount = 5;
    private const int OHLCBarCount = 100;
    private const int ViolinGroupValueCount = 200;
    private const int ViolinGroupCount = 3;
    private const int PolygonVertexCount = 8;

    private double[] _histogramValues = null!;
    private Func<double, double> _function = null!;
    private PieSlice[] _pieSlices = null!;
    private double[] _violinValues = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var random = new Random(42);

        _histogramValues = new double[HistogramValueCount];
        for (var i = 0; i < HistogramValueCount; i++)
        {
            _histogramValues[i] = random.NextDouble() * 100d;
        }

        _function = x => Math.Sin(x) * Math.Exp(-0.1 * x);

        _pieSlices = new PieSlice[PieSliceCount];
        var colors = new uint[] { 0xFF4DA3FFu, 0xFFFF6B6Bu, 0xFF2DD4BFu, 0xFFFFD93Du, 0xFF9B59B6u };
        var labels = new string[] { "Alpha", "Beta", "Gamma", "Delta", "Epsilon" };
        for (var i = 0; i < PieSliceCount; i++)
        {
            _pieSlices[i] = new PieSlice(
                value: 20d + (i * 15d),
                color: colors[i],
                label: labels[i]);
        }

        _violinValues = new double[ViolinGroupValueCount];
        for (var i = 0; i < ViolinGroupValueCount; i++)
        {
            _violinValues[i] = (random.NextDouble() * 2d - 1d) * 3d;
        }
    }

    [Benchmark]
    public int HistogramCreateBins()
    {
        var data = new HistogramData(_histogramValues, binCount: 20, HistogramMode.Count);
        return data.Bins.Count;
    }

    [Benchmark]
    public double FunctionPlotEvaluate()
    {
        var data = new FunctionPlotData(_function, xMin: -10d, xMax: 10d, sampleCount: FunctionPlotSampleCount);
        return data.Ys[FunctionPlotSampleCount / 2];
    }

    [Benchmark]
    public double PieSliceLayout()
    {
        var data = new PieChartData(_pieSlices, holeRatio: 0.3d);
        return data.TotalValue;
    }

    [Benchmark]
    public int OHLCBarCreate()
    {
        var random = new Random(42);
        var price = 100d;
        var bars = new OHLCBar[OHLCBarCount];

        for (var i = 0; i < OHLCBarCount; i++)
        {
            var open = price;
            var change = (random.NextDouble() - 0.5d) * 4d;
            var close = open + change;
            var high = Math.Max(open, close) + random.NextDouble() * 2d;
            var low = Math.Min(open, close) - random.NextDouble() * 2d;
            bars[i] = new OHLCBar(open, high, low, close, x: i);
            price = close;
        }

        var data = new OHLCData(bars, OHLCStyle.Candlestick);
        return data.BarCount;
    }

    [Benchmark]
    public int ViolinKdeEstimate()
    {
        var groups = new ViolinGroup[ViolinGroupCount];
        var colors = new uint[] { 0xFF4DA3FFu, 0xFFFF6B6Bu, 0xFF2DD4BFu };
        var labels = new string[] { "Group A", "Group B", "Group C" };

        for (var i = 0; i < ViolinGroupCount; i++)
        {
            groups[i] = new ViolinGroup(
                _violinValues,
                color: colors[i],
                label: labels[i],
                bandwidth: 0.5d);
        }

        var data = new ViolinData(groups);
        return data.GroupCount;
    }

    [Benchmark]
    public int PolygonTriangulate()
    {
        var vertices = new Vector3[PolygonVertexCount];
        for (var i = 0; i < PolygonVertexCount; i++)
        {
            var angle = (2d * Math.PI * i) / PolygonVertexCount;
            vertices[i] = new Vector3(
                (float)Math.Cos(angle),
                (float)Math.Sin(angle),
                (float)(i * 0.25d));
        }

        var data = new PolygonData(vertices, fillColor: 0x404DA3FFu, strokeColor: 0xFF4DA3FFu, strokeWidth: 2d);
        return data.VertexCount;
    }
}
