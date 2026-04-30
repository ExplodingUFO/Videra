# Phase 333 Context: Breaking Chart API Deletion Inventory

## Bead

- `Videra-rhb`

## Goal

Inventory every old chart View component reference before replacing the multi-view SurfaceCharts public model with a single `VideraChartView` and `Plot.Add.*` authoring API.

## Constraints

- Delete `SurfaceChartView`, `WaterfallChartView`, and `ScatterChartView` as View components.
- Do not preserve compatibility wrappers.
- Do not internalize old Views.
- Do not add fallback/downshift behavior.
- Do not widen backend, chart-family, or renderer scope.
- Avoid god renderer or god facade.
