using System.Numerics;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Demo.Services;

// --- Surface family ---

internal sealed class SurfaceStartRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.StartId;
    public string Group => "Surface";
    public string Title => "Start here: In-memory first chart";
    public string Description => "Generated at runtime from a dense matrix.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.SurfaceChartView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        chart.Plot.Add.Surface(context.InMemorySource, "In-memory");
        chart.Plot.ColorMap = SampleDataFactory.CreateColorMap(context.InMemorySource.Metadata.ValueRange);
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "In-memory 64x64 Gaussian surface.", "No additional assets.");
    }
}

internal sealed class WaterfallRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.WaterfallId;
    public string Group => "Surface";
    public string Title => "Waterfall proof";
    public string Description => "Waterfall presentation with strip spacing.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.WaterfallChartView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        chart.Plot.Add.Waterfall(context.WaterfallSource, "Waterfall");
        chart.Plot.ColorMap = SampleDataFactory.CreateColorMap(context.WaterfallSource.Metadata.ValueRange);
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "Waterfall from in-memory source with explicit strip spacing.", "No additional assets.");
    }
}

internal sealed class AnalyticsRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.AnalyticsId;
    public string Group => "Surface";
    public string Title => "Analytics proof";
    public string Description => "Explicit coordinates and independent ColorField.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.SurfaceChartView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        chart.Plot.Add.Surface(context.AnalyticsProofSource, "Analytics");
        chart.Plot.ColorMap = SampleDataFactory.CreateColorMap(context.AnalyticsProofSource.Metadata.ValueRange);
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "Analytics proof with explicit coordinates.", "No additional assets.");
    }
}

// --- Scatter family ---

internal sealed class ScatterRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.ScatterId;
    public string Group => "Scatter";
    public string Title => "Scatter proof";
    public string Description => "3D scatter with streaming scenarios.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.ScatterChartView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        var data = SampleDataFactory.CreateSampleScatterData();
        chart.Plot.Add.Scatter(data, "Scatter");
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "Scatter with 50 random 3D points.", "No additional assets.");
    }
}

// --- Bar family ---

internal sealed class BarRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.BarId;
    public string Group => "Bar";
    public string Title => "Bar chart proof";
    public string Description => "Grouped bar chart with 3 series.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.BarChartView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        chart.Plot.Add.Bar(SampleDataFactory.CreateSampleBarData(), "Bar");
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "BarChartData with 3 BarSeries of 5 values each.", "No additional assets.");
    }
}

// --- Contour ---

internal sealed class ContourRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.ContourId;
    public string Group => "Contour";
    public string Title => "Contour plot proof";
    public string Description => "Marching squares iso-line extraction.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.ContourPlotView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        chart.Plot.Add.Contour(SampleDataFactory.CreateSampleContourField(), "Contour");
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "32x32 radial scalar field with 10 contour levels.", "No additional assets.");
    }
}

// --- Line family ---

internal sealed class LineRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.LineId;
    public string Group => "Line";
    public string Title => "Line chart proof";
    public string Description => "3D polyline from coordinate arrays.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.LinePlotView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        chart.Plot.Add.Line(
            SampleDataFactory.CreateSampleLineXs(),
            SampleDataFactory.CreateSampleLineYs(),
            SampleDataFactory.CreateSampleLineZs(),
            "Line");
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "30-point helix polyline with sinusoidal Y values.", "No additional assets.");
    }
}

internal sealed class RibbonRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.RibbonId;
    public string Group => "Line";
    public string Title => "Ribbon chart proof";
    public string Description => "3D ribbon/tube geometry.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.RibbonPlotView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        chart.Plot.Add.Ribbon(
            SampleDataFactory.CreateSampleLineXs(),
            SampleDataFactory.CreateSampleLineYs(),
            SampleDataFactory.CreateSampleLineZs(),
            radius: 0.15f,
            "Ribbon");
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "30-point helix path with 0.15 tube radius.", "No additional assets.");
    }
}

// --- Vector field ---

internal sealed class VectorFieldRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.VectorFieldId;
    public string Group => "Vector Field";
    public string Title => "Vector field proof";
    public string Description => "3D vector field with arrow rendering.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.VectorFieldPlotView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        chart.Plot.Add.VectorField(
            SampleDataFactory.CreateSampleVectorFieldXs(),
            SampleDataFactory.CreateSampleVectorFieldYs(),
            SampleDataFactory.CreateSampleVectorFieldZs(),
            SampleDataFactory.CreateSampleVectorFieldDxs(),
            SampleDataFactory.CreateSampleVectorFieldDys(),
            SampleDataFactory.CreateSampleVectorFieldDzs(),
            "VectorField");
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "6x6 grid of arrows with radial direction pattern.", "No additional assets.");
    }
}

// --- Heatmap ---

internal sealed class HeatmapSliceRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.HeatmapSliceId;
    public string Group => "Heatmap";
    public string Title => "Heatmap slice proof";
    public string Description => "Heatmap slice from 2D scalar field.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.HeatmapSlicePlotView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        chart.Plot.Add.HeatmapSlice(
            SampleDataFactory.CreateSampleHeatmapValues(),
            HeatmapSliceAxis.Z,
            0.5,
            "HeatmapSlice");
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "24x24 sinusoidal scalar field at Z=0.5.", "No additional assets.");
    }
}

// --- Statistical ---

internal sealed class BoxPlotRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.BoxPlotId;
    public string Group => "Statistical";
    public string Title => "Box plot proof";
    public string Description => "3D box plot with statistical distribution.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.BoxPlotView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        chart.Plot.Add.BoxPlot(SampleDataFactory.CreateSampleBoxPlotData(), "BoxPlot");
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "4 categories with min/Q1/median/Q3/max and outliers.", "No additional assets.");
    }
}

internal sealed class HistogramRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.HistogramId;
    public string Group => "Statistical";
    public string Title => "Histogram proof";
    public string Description => "Histogram with configurable bins and mode.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.HistogramPlotView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        chart.Plot.Add.Histogram(SampleDataFactory.CreateSampleHistogramValues(), binCount: 25, mode: HistogramMode.Count, name: "Histogram");
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "500 Box-Muller normal samples with 25 bins.", "No additional assets.");
    }
}

internal sealed class FunctionPlotRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.FunctionPlotId;
    public string Group => "Statistical";
    public string Title => "Function plot proof";
    public string Description => "Function plot evaluating y = sin(x) * exp(-0.1x).";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.FunctionPlotView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        chart.Plot.Add.Function(
            x => Math.Sin(x) * Math.Exp(-x * 0.1),
            xMin: 0, xMax: 20, sampleCount: 300,
            name: "Function");
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "Damped sine over [0, 20] with 300 samples.", "No additional assets.");
    }
}

// --- Pie ---

internal sealed class PieRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.PieId;
    public string Group => "Pie";
    public string Title => "Pie/Donut proof";
    public string Description => "Donut chart with 5 labeled slices.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.PiePlotView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        chart.Plot.Add.Pie(
            new[]
            {
                new PieSlice(35, 0xFF38BDF8u, "Engineering"),
                new PieSlice(25, 0xFFF97316u, "Design"),
                new PieSlice(20, 0xFF2DD4BFu, "Marketing"),
                new PieSlice(15, 0xFF8B5CF6u, "Sales"),
                new PieSlice(5, 0xFFFF6B6B, "Other"),
            },
            holeRatio: 0.4,
            name: "Pie");
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "Department budget distribution with 40% donut hole.", "No additional assets.");
    }
}

// --- Financial ---

internal sealed class OHLCRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.OHLCId;
    public string Group => "Financial";
    public string Title => "OHLC/Candlestick proof";
    public string Description => "Candlestick chart with 20 random bars.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.OHLCPlotView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();

        var rng = new Random(42);
        var bars = new OHLCBar[20];
        var price = 100d;
        for (var i = 0; i < bars.Length; i++)
        {
            var open = price;
            var change = (rng.NextDouble() - 0.48) * 6;
            var close = open + change;
            var high = Math.Max(open, close) + (rng.NextDouble() * 3);
            var low = Math.Min(open, close) - (rng.NextDouble() * 3);
            bars[i] = new OHLCBar(open, high, low, close, i);
            price = close;
        }

        chart.Plot.Add.OHLC(bars, OHLCStyle.Candlestick, name: "OHLC");
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "20 generated candlestick bars with random walk.", "No additional assets.");
    }
}

// --- Distribution ---

internal sealed class ViolinRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.ViolinId;
    public string Group => "Distribution";
    public string Title => "Violin proof";
    public string Description => "Violin plot with 3 groups.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.ViolinPlotView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();

        var groups = new[]
        {
            new ViolinGroup(new[] { 1.2, 1.5, 1.8, 2.1, 2.3, 2.5, 2.8, 3.0, 3.2, 3.5 }, 0xFF38BDF8u, "Group A"),
            new ViolinGroup(new[] { 2.0, 2.3, 2.6, 2.9, 3.1, 3.4, 3.7, 4.0, 4.2, 4.5 }, 0xFFF97316u, "Group B"),
            new ViolinGroup(new[] { 0.8, 1.0, 1.3, 1.6, 1.9, 2.2, 2.5, 2.8, 3.1, 3.4 }, 0xFF2DD4BFu, "Group C"),
        };

        chart.Plot.Add.Violin(groups, name: "Violin");
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "3 groups with 10 samples each, KDE shapes.", "No additional assets.");
    }
}

internal sealed class PolygonRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.PolygonId;
    public string Group => "Distribution";
    public string Title => "Polygon proof";
    public string Description => "Filled polygon with 5 vertices.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.PolygonPlotView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();

        var vertices = new Vector3[]
        {
            new(0, 0, 0), new(4, 0, 0), new(4, 0, 4),
            new(2, 0, 6), new(0, 0, 4),
        };

        chart.Plot.Add.Polygon(vertices, fillColor: 0x8038BDF8u, name: "Polygon");
        chart.FitToData();
        return new RecipeResult(Title, Description,
            "Pentagon shape with configurable fill.", "No additional assets.");
    }
}

// --- Annotations & References ---

internal sealed class AnnotationRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.AnnotationId;
    public string Group => "Annotation";
    public string Title => "Annotation proof";
    public string Description => "Text and arrow annotations on surface.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.SurfaceChartView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        chart.Plot.Add.Surface(context.InMemorySource, name: "Base Surface");

        chart.Plot.Add.Text(new Vector3(16, 0.6f, 12), "Peak", color: 0xFFFF6B6Bu, fontSize: 14d);
        chart.Plot.Add.Text(new Vector3(48, 0.3f, 36), "Valley", color: 0xFF38BDF8u, fontSize: 14d);
        chart.Plot.Add.Arrow(new Vector3(10, 0.5f, 10), new Vector3(20, 0.7f, 20), color: 0xFF2DD4BFu, label: "Gradient");

        chart.FitToData();
        return new RecipeResult(Title, Description,
            "Text labels and arrows on a surface chart.", "No additional assets.");
    }
}

internal sealed class ReferenceRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.ReferenceId;
    public string Group => "Reference";
    public string Title => "Reference lines & shapes";
    public string Description => "Reference lines, spans, and shape annotations.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.SurfaceChartView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();
        chart.Plot.Add.Surface(context.InMemorySource, name: "Base Surface");

        chart.Plot.Add.ReferenceLine(ReferenceAxis.Y, 0.5, color: 0xFFFF0000u, lineWidth: 2d, label: "Threshold");
        chart.Plot.Add.ReferenceSpan(ReferenceAxis.X, 20, 40, color: 0x4000FF00u, label: "Region of Interest");
        chart.Plot.Add.Rectangle(new Vector3(32, 0.4f, 32), 10, 10, fillColor: 0x40FFA500u, label: "Zone A");
        chart.Plot.Add.Ellipse(new Vector3(48, 0.3f, 16), 12, 8, fillColor: 0x409B59B6u, label: "Zone B");

        chart.FitToData();
        return new RecipeResult(Title, Description,
            "Threshold lines, region spans, and shape overlays.", "No additional assets.");
    }
}
