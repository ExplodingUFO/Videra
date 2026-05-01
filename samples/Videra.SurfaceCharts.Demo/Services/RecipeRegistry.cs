namespace Videra.SurfaceCharts.Demo.Services;

/// <summary>
/// Provides access to all registered chart recipes.
/// </summary>
public static class RecipeRegistry
{
    private static readonly List<IChartRecipe> Recipes = [];
    private static bool _initialized;

    /// <summary>
    /// Gets all registered recipes.
    /// </summary>
    public static IReadOnlyList<IChartRecipe> All
    {
        get
        {
            EnsureInitialized();
            return Recipes;
        }
    }

    /// <summary>
    /// Gets a recipe by scenario ID.
    /// </summary>
    public static IChartRecipe? Get(string scenarioId)
    {
        EnsureInitialized();
        return Recipes.FirstOrDefault(r =>
            string.Equals(r.ScenarioId, scenarioId, StringComparison.Ordinal));
    }

    /// <summary>
    /// Gets recipes grouped by their group name.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<IChartRecipe>> GetByGroup()
    {
        EnsureInitialized();
        return Recipes
            .GroupBy(r => r.Group)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<IChartRecipe>)g.ToList());
    }

    private static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        RegisterAll();
    }

    private static void RegisterAll()
    {
        // Surface family
        Recipes.Add(new SurfaceStartRecipe());
        Recipes.Add(new WaterfallRecipe());
        Recipes.Add(new AnalyticsRecipe());

        // Scatter family
        Recipes.Add(new ScatterRecipe());

        // Bar family
        Recipes.Add(new BarRecipe());

        // Contour
        Recipes.Add(new ContourRecipe());

        // Line family
        Recipes.Add(new LineRecipe());
        Recipes.Add(new RibbonRecipe());

        // Vector field
        Recipes.Add(new VectorFieldRecipe());

        // Heatmap
        Recipes.Add(new HeatmapSliceRecipe());

        // Statistical
        Recipes.Add(new BoxPlotRecipe());
        Recipes.Add(new HistogramRecipe());
        Recipes.Add(new FunctionPlotRecipe());

        // Pie
        Recipes.Add(new PieRecipe());

        // Financial
        Recipes.Add(new OHLCRecipe());

        // Distribution
        Recipes.Add(new ViolinRecipe());
        Recipes.Add(new PolygonRecipe());

        // Annotations & References
        Recipes.Add(new AnnotationRecipe());
        Recipes.Add(new ReferenceRecipe());
    }
}
