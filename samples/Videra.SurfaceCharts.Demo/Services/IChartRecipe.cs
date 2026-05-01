using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Demo.Services;

/// <summary>
/// Describes the result of applying a chart recipe.
/// </summary>
public sealed record RecipeResult(
    string Heading,
    string Details,
    string DatasetSummary,
    string AssetSummary,
    VideraChartView? ActiveChartView = null);

/// <summary>
/// Provides chart views and shared state to recipe implementations.
/// </summary>
public sealed class RecipeContext
{
    public RecipeContext(
        VideraChartView surfaceChartView,
        VideraChartView waterfallChartView,
        VideraChartView scatterChartView,
        VideraChartView barChartView,
        VideraChartView contourPlotView,
        VideraChartView linePlotView,
        VideraChartView ribbonPlotView,
        VideraChartView vectorFieldPlotView,
        VideraChartView heatmapSlicePlotView,
        VideraChartView boxPlotView,
        VideraChartView histogramPlotView,
        VideraChartView functionPlotView,
        VideraChartView piePlotView,
        VideraChartView ohlcPlotView,
        VideraChartView violinPlotView,
        VideraChartView polygonPlotView,
        ISurfaceTileSource inMemorySource,
        ISurfaceTileSource analyticsProofSource,
        ISurfaceTileSource waterfallSource,
        Action<VideraChartView> setActiveChartView)
    {
        SurfaceChartView = surfaceChartView;
        WaterfallChartView = waterfallChartView;
        ScatterChartView = scatterChartView;
        BarChartView = barChartView;
        ContourPlotView = contourPlotView;
        LinePlotView = linePlotView;
        RibbonPlotView = ribbonPlotView;
        VectorFieldPlotView = vectorFieldPlotView;
        HeatmapSlicePlotView = heatmapSlicePlotView;
        BoxPlotView = boxPlotView;
        HistogramPlotView = histogramPlotView;
        FunctionPlotView = functionPlotView;
        PiePlotView = piePlotView;
        OHLCPlotView = ohlcPlotView;
        ViolinPlotView = violinPlotView;
        PolygonPlotView = polygonPlotView;
        InMemorySource = inMemorySource;
        AnalyticsProofSource = analyticsProofSource;
        WaterfallSource = waterfallSource;
        SetActiveChartView = setActiveChartView;
    }

    public VideraChartView SurfaceChartView { get; }
    public VideraChartView WaterfallChartView { get; }
    public VideraChartView ScatterChartView { get; }
    public VideraChartView BarChartView { get; }
    public VideraChartView ContourPlotView { get; }
    public VideraChartView LinePlotView { get; }
    public VideraChartView RibbonPlotView { get; }
    public VideraChartView VectorFieldPlotView { get; }
    public VideraChartView HeatmapSlicePlotView { get; }
    public VideraChartView BoxPlotView { get; }
    public VideraChartView HistogramPlotView { get; }
    public VideraChartView FunctionPlotView { get; }
    public VideraChartView PiePlotView { get; }
    public VideraChartView OHLCPlotView { get; }
    public VideraChartView ViolinPlotView { get; }
    public VideraChartView PolygonPlotView { get; }
    public ISurfaceTileSource InMemorySource { get; }
    public ISurfaceTileSource AnalyticsProofSource { get; }
    public ISurfaceTileSource WaterfallSource { get; }
    public Action<VideraChartView> SetActiveChartView { get; }
}

/// <summary>
/// A self-contained chart demo recipe that can be applied to a chart view.
/// </summary>
public interface IChartRecipe
{
    /// <summary>
    /// Gets the scenario ID this recipe handles.
    /// </summary>
    string ScenarioId { get; }

    /// <summary>
    /// Gets the recipe group name for sidebar navigation.
    /// </summary>
    string Group { get; }

    /// <summary>
    /// Gets the recipe title.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the recipe description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Applies the recipe to the appropriate chart view.
    /// </summary>
    /// <returns>The result describing what was applied.</returns>
    RecipeResult Apply(RecipeContext context);
}
