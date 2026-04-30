# Phase 246 Plan - SurfaceCharts Columnar Scatter and Streaming Entry

## Success Criteria

1. SurfaceCharts exposes a columnar scatter data shape using contiguous columns for X/Y/Z, size, and color.
2. API supports `ReplaceRange` and `AppendRange` with clear validation and optional FIFO retention.
3. `Pickable` defaults to false for high-volume columnar paths and is reflected in diagnostics.
4. Demo/docs show the columnar path without adding a new chart family.

## Tasks

1. Add `ScatterColumnarData` and `ScatterColumnarSeries` in `Videra.SurfaceCharts.Core`.
2. Extend `ScatterChartData` and `ScatterRenderer` to include columnar series in render snapshots.
3. Extend `ScatterChartRenderingStatus` and `ScatterChartView` bounds/status projection.
4. Update `Videra.SurfaceCharts.Demo` scatter proof and README.
5. Add focused Core and Avalonia integration tests.
