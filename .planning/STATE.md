---
gsd_state_version: 1.0
milestone: v2.55
milestone_name: "ScottPlot-like Plot API"
status: active
stopped_at: "Phase 376 complete"
last_updated: "2026-04-29T23:59:00+08:00"
last_activity: 2026-04-29
progress:
  total_phases: 7
  completed_phases: 1
  total_plans: 7
  completed_plans: 1
  percent: 14
---

# Project State

## Project Reference

See: `.planning/PROJECT.md`

**Core value:** 跨平台 3D 渲染引擎的可靠性
**Current focus:** v2.55 ScottPlot-like Plot API — phase execution ready

## Current Position

Milestone: `v2.55 ScottPlot-like Plot API`
Phase: 376 of 382 (Plot API Inventory and Beads Coordination) — COMPLETE
Plan: 376-PLAN complete
Status: Requirements, roadmap, and beads chain prepared. Phases 377-379 ready for parallel execution.
Last activity: 2026-04-29 — Completed Phase 376 coordination gate

Progress: [█░░░░░░░░░] 14%

## Initial Scope Summary

v2.55 starts from the v2.54 chart interactivity baseline:

- `VideraChartView` is the single shipped chart control with Surface, Waterfall, Scatter, Bar, and Contour series.
- Crosshair overlay, enhanced tooltips, series probe strategies, keyboard/toolbar controls are operational.
- Snapshot export via `Plot3D.CaptureSnapshotAsync` is operational.
- Log/DateTime axes, custom formatters, multi-series legend are shipped.

The scope adds ScottPlot-like API ergonomics:

- Plot.Add overloads: Surface/Scatter/Waterfall/Bar/Contour from raw arrays without internal types
- Typed plottable returns: IPlottable3D with Label, IsVisible, Color/ColorMap
- Plot.Axes facade: X/Y/Z.Label, SetLimits, AutoScale, SetLabels
- SavePngAsync convenience method
- DataLogger3D / ScatterStream for live streaming data
- Cookbook-style demo and README rewrite
- Multi-series rendering (same-type series compose)

## Decisions

- Facade layer only — no renderer changes, no new chart types, no broad architecture rewrites
- Internal types (SurfaceMatrix, ScatterChartData, etc.) stay as advanced path
- P0: Plot.Add overloads + typed plottable returns + Plot.Axes facade
- P1: SavePngAsync + DataLogger3D + cookbook/README rewrite
- P2: Multi-series rendering (same-type series compose)
- Phase 376 is the coordination gate; Phases 377, 378, and 379 can run in parallel after 376 if each uses its own worktree/branch and disjoint write set.
- Phase 380 depends on Phase 377; Phase 381 depends on Phases 377-379; Phase 382 closes integration after 380 and 381.

## Known Residuals

- Full CI can lag; user often prioritizes fast local progress unless CI is explicitly requested.
- `.planning` remains local-only unless specific files are already tracked.
- Docker-backed Dolt SQL Server is the Beads remote path; use direct Docker Dolt push when needed.
- v2.55 must not restore old chart view APIs, reintroduce direct public `Source`, add PDF/vector export, add compatibility wrappers, add hidden fallback/downshift behavior, expand backend/runtime scope, or create a god-code workbench.

## Session Continuity

Last session: `2026-04-29 +08:00`
Stopped at: Phase 376 complete
Next action: Claim Phases 377-379 beads and dispatch them in isolated worktrees if the branch/worktree state can be made safe.

## Accumulated Context

From v2.54: 22 interactivity requirements delivered across 5 phases. Chart now has crosshair, tooltips, probe strategies, keyboard shortcuts, and toolbar. All 200 integration tests pass.
