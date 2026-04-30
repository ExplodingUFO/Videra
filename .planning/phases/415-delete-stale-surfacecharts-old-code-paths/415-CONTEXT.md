# Phase 415 Context: Delete Stale SurfaceCharts Old-Code Paths

## Beads

- Parent phase: `Videra-9vg`
- Child beads:
  - `Videra-sva`: remove chart-local render fallback/downshift
  - `Videra-1wq`: remove compatibility camera-frame backfill and fallback naming
  - `Videra-avu`: clean stale compatibility vocabulary in tests

## Boundary

This phase owns code/API cleanup in SurfaceCharts rendering and focused
integration tests. It must not edit the demo/cookbook simplification write set
owned by Phase 416.

## Required Outcomes

- `SurfaceChartRenderHost` must stop silently downshifting from GPU to software
  rendering on exceptions or default GPU backend resolution failure.
- `SurfaceChartRenderInputs` must stop using compatibility camera-frame
  backfill.
- Default color-map behavior must not use fallback terminology.
- Test names/comments must not preserve misleading compatibility vocabulary.

## Non-Goals

- No compatibility wrapper.
- No fallback/downshift preservation.
- No old public chart controls.
- No direct public `VideraChartView.Source`.
- No demo workbench expansion.
