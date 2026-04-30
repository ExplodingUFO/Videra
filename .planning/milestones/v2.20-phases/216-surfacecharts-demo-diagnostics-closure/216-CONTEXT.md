# Phase 216 Context

## Goal

Add diagnostics and fallback visibility to `Videra.SurfaceCharts.Demo` while preserving the chart package family's independence from `VideraView`.

## Scope

- Keep changes inside the SurfaceCharts demo, its README, and existing sample/repository tests.
- Do not move chart semantics into viewer contracts.
- Do not add compatibility fallback behavior or force backend selection from the demo.
- Surface the active runtime path truth instead of claiming exhaustive GPU host/fallback coverage.

## Success Criteria

- SurfaceCharts demo exposes `RenderingStatus`, fallback status, and relevant host/backend state in a diagnostic surface.
- Support summary captures the same rendering diagnostics for bug reports.
- Docs describe what the demo proves and what it does not cover across GPU host/fallback combinations.
- `SurfaceChartView` remains independent from `VideraView`.
