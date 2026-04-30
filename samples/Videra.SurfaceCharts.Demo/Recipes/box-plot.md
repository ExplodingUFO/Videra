# Box Plot Chart

## Recipe: Statistical Distribution

Create a 3D box plot showing statistical distributions with grouped layout.

```csharp
var data = new BoxPlotData([
    new BoxPlotCategory("Group A", min: 2, q1: 5, median: 8, q3: 12, max: 18, outliers: [1, 20]),
    new BoxPlotCategory("Group B", min: 4, q1: 7, median: 10, q3: 14, max: 19),
    new BoxPlotCategory("Group C", min: 1, q1: 3, median: 6, q3: 9, max: 15, outliers: [0, 17]),
]);

chart.Plot.Clear();
chart.Plot.Add.BoxPlot(data, "Distribution comparison");
chart.FitToData();
```

## Features

- Grouped layout for multi-category comparison
- IQR boxes with median lines
- Whisker lines for min/max
- Outlier points
- Probe shows min/Q1/median/Q3/max
