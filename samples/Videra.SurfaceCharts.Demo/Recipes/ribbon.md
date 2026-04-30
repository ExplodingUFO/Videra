# Ribbon Chart

## Recipe: Tube Geometry

Create a 3D ribbon/tube from coordinate arrays with configurable radius.

```csharp
var xs = new double[] { 0, 1, 2, 3, 4, 5 };
var ys = new double[] { 0, 1.0, 0.5, 1.5, 0.8, 2.0 };
var zs = new double[] { 0, 0.5, 1.0, 1.5, 2.0, 2.5 };

chart.Plot.Clear();
chart.Plot.Add.Ribbon(xs, ys, zs, radius: 0.15f, "Helix ribbon");
chart.FitToData();
```

## Features

- Configurable tube radius
- Colormap support
- Probe integration
- Legend support
