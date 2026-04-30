# Phase 246 Context - SurfaceCharts Columnar Scatter and Streaming Entry

## Goal

Add the first columnar/streaming data entry for SurfaceCharts scatter without adding new chart families or merging SurfaceCharts into `VideraView`.

## Scope

- Implement a narrow columnar scatter data shape for X/Y/Z, size, and color columns.
- Support `ReplaceRange` and `AppendRange`.
- Keep high-volume columnar paths non-pickable by default and expose the count in diagnostics.
- Keep interaction-quality refine behavior deferred; the current scatter control does not expose `InteractionQuality`.

## Constraints

- No broad chart architecture rewrite.
- No new chart families.
- No compatibility/fallback layer.
- Keep the renderer bridge direct and maintainable.
