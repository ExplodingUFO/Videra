# Phase 420 Context: Native 3D Feature Convenience APIs

## Beads

- Parent: `Videra-kyy`
- Parallel candidate: `Videra-kyy.1` - bar category labels
- Parallel candidate: `Videra-kyy.2` - contour explicit levels
- Deferred until shared API files settle: `Videra-kyy.3` - minimal series style handles

## Current Surface

Bar chart data is immutable and currently carries:

- `BarChartData.Series`
- `BarChartData.Layout`
- `BarChartData.SeriesCount`
- `BarChartData.CategoryCount`
- `BarSeries.Values`
- `BarSeries.Color`
- `BarSeries.Label`

Contour chart data is immutable and currently carries:

- `ContourChartData.Field`
- `ContourChartData.Mask`
- `ContourChartData.LevelCount`

`ContourExtractor.ExtractAll()` derives evenly spaced levels from field range and
`LevelCount`. There is no explicit level list.

`Plot3DAddApi` is the native authoring facade for both chart families. It is the
only expected shared edit point for 420A and 420B.

`Plot3DDatasetEvidence` is the truth surface for dataset metadata. Both 420A and
420B may touch it, so integration must review evidence strings and properties
directly rather than accepting blind merges.

## Boundaries

- Do not add compatibility wrappers or alternate chart controls.
- Do not add hidden fallback/downshift behavior.
- Do not add a categorical plotting framework; bar category labels are metadata
  and evidence for the current grouped/stacked model.
- Do not add contour labels, contour annotation placement, or per-level visual
  style in 420B.
- Do not start 420C until 420A/420B integration is clean.
