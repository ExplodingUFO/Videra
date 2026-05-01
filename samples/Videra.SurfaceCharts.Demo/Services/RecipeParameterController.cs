using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Demo.Services;

/// <summary>
/// Manages the recipe parameter panel visibility and slider callbacks.
/// </summary>
internal sealed class RecipeParameterController
{
    private readonly Border _recipeParameterPanel;
    private readonly StackPanel _lineParamGroup;
    private readonly StackPanel _ribbonParamGroup;
    private readonly StackPanel _vectorFieldParamGroup;
    private readonly StackPanel _heatmapParamGroup;
    private readonly TextBlock _lineWidthText;
    private readonly TextBlock _ribbonRadiusText;
    private readonly TextBlock _vectorFieldScaleText;
    private readonly TextBlock _heatmapPositionText;

    private LinePlot3DSeries? _activeLineSeries;
    private RibbonPlot3DSeries? _activeRibbonSeries;
    private VectorFieldPlot3DSeries? _activeVectorFieldSeries;
    private HeatmapSlicePlot3DSeries? _activeHeatmapSliceSeries;

    public RecipeParameterController(
        Border recipeParameterPanel,
        StackPanel lineParamGroup,
        StackPanel ribbonParamGroup,
        StackPanel vectorFieldParamGroup,
        StackPanel heatmapParamGroup,
        TextBlock lineWidthText,
        TextBlock ribbonRadiusText,
        TextBlock vectorFieldScaleText,
        TextBlock heatmapPositionText)
    {
        _recipeParameterPanel = recipeParameterPanel;
        _lineParamGroup = lineParamGroup;
        _ribbonParamGroup = ribbonParamGroup;
        _vectorFieldParamGroup = vectorFieldParamGroup;
        _heatmapParamGroup = heatmapParamGroup;
        _lineWidthText = lineWidthText;
        _ribbonRadiusText = ribbonRadiusText;
        _vectorFieldScaleText = vectorFieldScaleText;
        _heatmapPositionText = heatmapPositionText;
    }

    public void UpdateParameterPanel(string scenarioId)
    {
        _activeLineSeries = null;
        _activeRibbonSeries = null;
        _activeVectorFieldSeries = null;
        _activeHeatmapSliceSeries = null;

        _lineParamGroup.IsVisible = false;
        _ribbonParamGroup.IsVisible = false;
        _vectorFieldParamGroup.IsVisible = false;
        _heatmapParamGroup.IsVisible = false;

        var hasParams = scenarioId switch
        {
            _ when scenarioId == SurfaceDemoScenarios.LineId => ShowLineParams(),
            _ when scenarioId == SurfaceDemoScenarios.RibbonId => ShowRibbonParams(),
            _ when scenarioId == SurfaceDemoScenarios.VectorFieldId => ShowVectorFieldParams(),
            _ when scenarioId == SurfaceDemoScenarios.HeatmapSliceId => ShowHeatmapParams(),
            _ => false,
        };

        _recipeParameterPanel.IsVisible = hasParams;
    }

    private bool ShowLineParams()
    {
        _lineParamGroup.IsVisible = true;
        return true;
    }

    private bool ShowRibbonParams()
    {
        _ribbonParamGroup.IsVisible = true;
        return true;
    }

    private bool ShowVectorFieldParams()
    {
        _vectorFieldParamGroup.IsVisible = true;
        return true;
    }

    private bool ShowHeatmapParams()
    {
        _heatmapParamGroup.IsVisible = true;
        return true;
    }

    public void OnLineWidthChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_activeLineSeries is null) return;
        _activeLineSeries.SetWidth((float)e.NewValue);
        _lineWidthText.Text = e.NewValue.ToString("0");
    }

    public void OnRibbonRadiusChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_activeRibbonSeries is null) return;
        _activeRibbonSeries.SetRadius((float)e.NewValue);
        _ribbonRadiusText.Text = e.NewValue.ToString("0.00");
    }

    public void OnVectorFieldScaleChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_activeVectorFieldSeries is null) return;
        _activeVectorFieldSeries.SetScale((float)e.NewValue);
        _vectorFieldScaleText.Text = e.NewValue.ToString("0.0");
    }

    public void OnHeatmapPositionChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_activeHeatmapSliceSeries is null) return;
        _activeHeatmapSliceSeries.SetPosition(e.NewValue);
        _heatmapPositionText.Text = e.NewValue.ToString("0.0");
    }
}
