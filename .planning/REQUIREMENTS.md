# Requirements: v2.67 ScottPlot5 Feature Parity — Charts, Axes, Annotations, Export

**Milestone goal:** Push Videra's 3D chart module toward ScottPlot5 feature parity
in chart type breadth, axis flexibility, annotation/overlay richness, export
capabilities, and demo framework quality. ScottPlot5 has zero 3D support —
Videra already leads in 3D surface/scatter/wireframe. This milestone adds the
2D-inspired features that translate well to 3D (new chart types, axis upgrades,
annotations, export) and refactors the demo into a maintainable recipe-driven
architecture.

## Category 1: New Chart Types

- [ ] **CHART-01**: Histogram chart type with configurable bin count, density
  mode, cumulative mode, and bar/line rendering. `Plot.Add.Histogram(values)`.
- [ ] **CHART-02**: Error Bar overlay on scatter plots with configurable
  X/Y symmetric and asymmetric error bars. `Plot.Add.ErrorBars(xs, ys, xErr, yErr)`.
- [ ] **CHART-03**: Pie/Donut chart type with configurable slice colors, labels,
  explode offset, and donut hole ratio. `Plot.Add.Pie(slices)`.
- [ ] **CHART-04**: OHLC/Candlestick chart type for financial data with
  configurable up/down colors and wick rendering. `Plot.Add.Candlestick(data)`.
- [ ] **CHART-05**: Violin plot type showing kernel density estimation with
  configurable bandwidth and mirror rendering. `Plot.Add.Violin(data)`.
- [ ] **CHART-06**: Function plot type that evaluates `Func<double, double>` over
  a domain with configurable sample count. `Plot.Add.Function(fn, xMin, xMax)`.
- [ ] **CHART-07**: Polygon chart type for arbitrary 2D/3D filled or outlined
  polygons. `Plot.Add.Polygon(xs, ys, zs)`.

## Category 2: Axis System Upgrade

- [ ] **AXIS-01**: DateTime axis support with auto-formatted ticks (seconds,
  minutes, hours, days, months, years). `Plot.Axes.X.IsDateTime = true`.
- [ ] **AXIS-02**: Logarithmic axis scale with configurable base (default 10).
  `Plot.Axes.X.Scale = AxisScale.Logarithmic`.
- [ ] **AXIS-03**: Tick label format strings (numeric, scientific, custom).
  `Plot.Axes.X.TickFormat = "F2"` or `Plot.Axes.X.TickFormatter = fn`.
- [ ] **AXIS-04**: Minor tick marks with configurable count between major ticks.
  `Plot.Axes.X.MinorTickCount = 5`.
- [ ] **AXIS-05**: Axis inversion (flip direction).
  `Plot.Axes.X.IsInverted = true`.
- [ ] **AXIS-06**: Multiple independent Y axes (left/right) for overlaying
  different scales on the same chart. `Plot.Axes.AddY("right", unit)`.

## Category 3: Annotations & Overlays

- [ ] **ANNO-01**: Text annotation anchored to data coordinates with
  configurable font, color, rotation, and background.
  `Plot.Add.Text("label", x, y, z)`.
- [ ] **ANNO-02**: Arrow annotation from one data point to another with
  configurable head style and color. `Plot.Add.Arrow(x1, y1, z1, x2, y2, z2)`.
- [ ] **ANNO-03**: Reference line (horizontal/vertical/diagonal) at a fixed
  value with configurable style and label.
  `Plot.Add.HorizontalLine(y)` / `Plot.Add.VerticalLine(x)`.
- [ ] **ANNO-04**: Reference region (span/band) between two values with
  configurable fill and border. `Plot.Add.HorizontalSpan(y1, y2)`.
- [ ] **ANNO-05**: Shape annotation (rectangle, ellipse) anchored to data
  coordinates with configurable fill and border.
  `Plot.Add.Rectangle(x1, y1, x2, y2)`.

## Category 4: Export & Rendering

- [ ] **EXPORT-01**: SVG export producing resolution-independent vector output.
  `chart.Plot.SaveSvgAsync(path, width, height)`.
- [ ] **EXPORT-02**: Batch export API for exporting multiple charts or a
  MultiPlot3D grid to a specified directory with naming patterns.
  `SurfaceChartExporter.ExportBatch(requests)`.
- [ ] **EXPORT-03**: Export manifest (JSON) describing exported files, chart
  metadata, and timestamps for CI integration.

## Category 5: Demo Framework Refactor

- [ ] **DEMO-01**: Refactor MainWindow into recipe-driven architecture where
  each recipe is a self-contained class implementing `IChartRecipe`.
- [ ] **DEMO-02**: Recipe group navigation (Basics, Charts, Annotations,
  Export, Streaming) in sidebar with group expand/collapse.
- [ ] **DEMO-03**: Recipe parameter panel auto-generated from recipe metadata
  (sliders, dropdowns, checkboxes) with live preview.
- [ ] **DEMO-04**: MainWindow reduced to recipe dispatch, shared infrastructure,
  and theme management (<500 lines).

## Category 6: Cookbook Recipes for New Features

- [ ] **COOK-05**: Cookbook recipes for all 7 new chart types (Histogram, Error
  Bar, Pie, Candlestick, Violin, Function, Polygon).
- [ ] **COOK-06**: Cookbook recipe for DateTime axis and log scale axis.
- [ ] **COOK-07**: Cookbook recipe for annotations (text, arrow, reference line,
  reference region, shape).
- [ ] **COOK-08**: Cookbook recipe for SVG and batch export workflows.
