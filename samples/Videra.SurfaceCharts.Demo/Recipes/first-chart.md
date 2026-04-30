# First Chart Recipe

This recipe builds the smallest Videra-native surface chart that still uses the same tiled data path as the demo. The host creates a `SurfaceMatrix`, turns it into an `ISurfaceTileSource` with `SurfacePyramidBuilder`, adds it through `VideraChartView.Plot.Add.Surface`, then lets `FitToData()` establish the first `ViewState`.

## Project References

Use the SurfaceCharts control package and the core package it brings with it:

```xml
<ProjectReference Include="..\..\src\Videra.SurfaceCharts.Avalonia\Videra.SurfaceCharts.Avalonia.csproj" />
```

## Minimal Setup

```csharp
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;

var chart = new VideraChartView();

var values = new float[]
{
    0.0f, 0.4f, 0.8f,
    0.2f, 0.7f, 1.0f,
    0.1f, 0.5f, 0.9f,
};

var metadata = new SurfaceMetadata(
    width: 3,
    height: 3,
    horizontalAxis: new SurfaceAxisDescriptor("Time", "s", 0d, 2d),
    verticalAxis: new SurfaceAxisDescriptor("Band", "Hz", 0d, 2d),
    valueRange: new SurfaceValueRange(values.Min(), values.Max()));

var matrix = new SurfaceMatrix(metadata, values);
var source = new SurfacePyramidBuilder(maxTileWidth: 64, maxTileHeight: 64).Build(matrix);

chart.Plot.Add.Surface(source, "First surface");
chart.Plot.Axes.X.Label = "Time";
chart.Plot.Axes.X.Unit = "s";
chart.Plot.Axes.Y.Label = "Amplitude";
chart.Plot.Axes.Y.Unit = "mm";
chart.Plot.Axes.Z.Label = "Band";
chart.Plot.Axes.Z.Unit = "Hz";
chart.FitToData();
```

`chart.ViewState` is the persisted camera and data-window contract after `FitToData()`. Store it in host settings if the user should reopen the same view later:

```csharp
SurfaceViewState savedViewState = chart.ViewState;
chart.ViewState = savedViewState;
```

## Attach To Avalonia

In XAML, host the control directly:

```xml
<controls:VideraChartView x:Name="ChartView" />
```

In code-behind, keep all data loading on the plot API:

```csharp
ChartView.Plot.Clear();
ChartView.Plot.Add.Surface(source, "First surface");
ChartView.FitToData();
```

## Support Evidence

When filing support evidence for a first-chart issue, include:

- the recipe name, `First Chart Recipe`
- the series name passed to `Plot.Add.Surface`
- `chart.ViewState.DataWindow`
- `chart.ViewState.Camera`
- `chart.RenderingStatus.ActiveBackend`
- `chart.RenderingStatus.IsReady`
- `chart.RenderingStatus.ResidentTileCount`
- `chart.RenderingStatus.VisibleTileCount`

Those fields prove which Videra control, plot path, data window, camera, and renderer state were active when the chart was observed.
