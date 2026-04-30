# Vector Field Chart

## Recipe: Arrow Field

Create a 3D vector field with arrow rendering and magnitude-based coloring.

```csharp
var xs = new double[] { 0, 1, 2, 0, 1, 2, 0, 1, 2 };
var ys = new double[] { 0, 0, 0, 1, 1, 1, 2, 2, 2 };
var zs = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
var dxs = new double[] { 0.5, 0, -0.5, 0.5, 0, -0.5, 0.5, 0, -0.5 };
var dys = new double[] { 0, 0.5, 0, 0, 0.5, 0, 0, 0.5, 0 };
var dzs = new double[] { 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3 };

chart.Plot.Clear();
chart.Plot.Add.VectorField(xs, ys, zs, dxs, dys, dzs, "Wind field");
chart.FitToData();
```

## Features

- Arrow length proportional to magnitude
- Magnitude-based blue-to-red colormap
- Configurable scale
- Probe shows vector value and magnitude
