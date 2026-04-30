# Heatmap Slice Chart

## Recipe: Volumetric Slice

Create a heatmap slice from a 2D scalar field at a specified axis position.

```csharp
var values = new double[16, 16];
for (int y = 0; y < 16; y++)
    for (int x = 0; x < 16; x++)
        values[x, y] = Math.Sin(x * 0.4) * Math.Cos(y * 0.3);

chart.Plot.Clear();
chart.Plot.Add.HeatmapSlice(values, HeatmapSliceAxis.Z, 0.5, "XY slice");
chart.FitToData();
```

## Features

- Configurable axis (X, Y, Z)
- Configurable position (0..1)
- Colormap support
- Probe shows interpolated value
