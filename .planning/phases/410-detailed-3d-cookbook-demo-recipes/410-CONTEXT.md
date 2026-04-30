---
phase: 410
bead: Videra-2de
title: "Detailed 3D Cookbook Demo Recipes Context"
status: complete
created_at: 2026-04-30
---

# Phase 410 Context

Phase 410 expands the SurfaceCharts demo cookbook into detailed Videra-native
recipe files while preserving the shipped control boundary:

- `VideraChartView` remains the single chart control.
- Data enters through `Plot.Add.*` and existing chart-domain models.
- Cache-backed surface examples stay on `SurfaceMatrix`,
  `SurfacePyramidBuilder`, `SurfaceCacheReader`, and `SurfaceCacheTileSource`.
- Scatter/live examples stay on `DataLogger3D`, columnar data, FIFO evidence,
  and support-summary truth.
- Snapshot/export examples stay PNG-only and chart-local.

The phase deliberately keeps the detailed recipes as separate markdown files
instead of growing `MainWindow.axaml.cs` or turning the sample into a generic
workbench.
