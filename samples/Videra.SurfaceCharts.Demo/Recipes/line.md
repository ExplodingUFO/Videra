# Line Chart

## Recipe: 3D Polyline

Create a 3D line plot from coordinate arrays.

```csharp
var xs = new double[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
var ys = new double[] { 0, 1.5, 0.5, 2.0, 1.0, 2.5, 1.5, 3.0, 2.0 };
var zs = new double[] { 0, 0.5, 1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0 };

chart.Plot.Clear();
chart.Plot.Add.Line(xs, ys, zs, "Sine wave");
chart.FitToData();
```

## Features

- Configurable color and width
- Per-segment colormap coloring
- Probe integration
- Legend support
