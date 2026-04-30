# MultiPlot3D

Create an N×M subplot grid with mixed chart types and linked camera.

## Quick Start

```csharp
var grid = new MultiPlot3D(2, 2);

grid.GetPlot(0, 0).Add.Surface(surfaceMatrix, "Surface");
grid.GetPlot(0, 1).Add.Bar(barData, "Bar");
grid.GetPlot(1, 0).Add.Line(xs, ys, zs, "Line");
grid.GetPlot(1, 1).Add.Contour(contourField, "Contour");

grid.FitAllToData();
using var link = grid.LinkAll(SurfaceChartLinkPolicy.CameraOnly);
```

## Key APIs

- `MultiPlot3D(rows, cols)` — creates the grid container
- `GetPlot(row, col)` — returns the `Plot3D` at a cell position
- `this[row, col]` — returns the `VideraChartView` at a cell position
- `FitAllToData()` — fits camera for all cells
- `LinkAll(policy)` — links all cells (CameraOnly, AxisOnly, or FullViewState)
- `LinkRow(row, policy)` / `LinkColumn(col, policy)` — link a single row or column
- `CaptureSnapshotAsync(path, width, height)` — renders entire grid as one PNG

## Link Policies

| Policy | Camera | Axes | Probe |
|--------|--------|------|-------|
| `CameraOnly` | Synced | Independent | Independent |
| `AxisOnly` | Independent | Synced | Independent |
| `FullViewState` | Synced | Synced | Synced |

## Mixed Chart Types

Each cell is independent — mix Surface, Bar, Line, Contour, Scatter,
Ribbon, VectorField, HeatmapSlice, BoxPlot freely across the grid.

## Notes

- `MultiPlot3D` extends `Grid` and manages its own `SurfaceChartWorkspace`.
- Disposing the grid cleans up all cells and link groups.
- `LinkRow` and `LinkColumn` can be combined for selective linking.
