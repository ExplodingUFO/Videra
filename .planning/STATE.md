---
gsd_state_version: 1.0
milestone: v2.56
milestone_name: "ScottPlot 5 Interaction and Cookbook Experience"
status: active
stopped_at: "Phase 387 completed in v2.56-phase387 worktree; Phase 386 remains concurrent in separate worktree"
last_updated: "2026-04-30T03:30:00+08:00"
last_activity: 2026-04-30
progress:
  total_phases: 7
  completed_phases: 4
  total_plans: 7
  completed_plans: 4
  percent: 57
---

# Project State

## Project Reference

See: `.planning/PROJECT.md`

**Core value:** 跨平台 3D 渲染引擎的可靠性
**Current focus:** v2.56 ScottPlot 5 Interaction and Cookbook Experience — Phase 387 complete; Phase 386 concurrent

## Current Position

Milestone: `v2.56 ScottPlot 5 Interaction and Cookbook Experience`
Phase: 387 complete; 386 remains concurrent
Plan: Phase 387 axis/live/linking complete
Status: Axis rules, explicit two-chart view linking, and DataLogger3D live view evidence are implemented and verified in `v2.56-phase387`. Phase 386 is owned by another worker in a separate worktree.
Last activity: 2026-04-30 — Completed Phase 387 axis rules, linked views, and live view management

Progress: [██████░░░░] 57%

## Initial Scope Summary

v2.56 starts from the completed v2.55 baseline:

- `VideraChartView` remains the single shipped chart control.
- `Plot.Add.*` raw overloads, typed plottable handles, `Plot.Axes`, `Plot.SavePngAsync`, `DataLogger3D`, and same-type multi-series composition are available.
- v2.54 chart interaction foundations are available: crosshair, enhanced tooltips, probe strategies, keyboard shortcuts, toolbar overlay, and snapshot chrome suppression.

This milestone improves the next layer of ScottPlot-inspired experience:

- concise Plot lifecycle/code operations
- configurable interaction profile and bounded chart commands
- selection, probe, and draggable overlay recipes
- axis rules, linked chart views, and live view management
- cookbook-style demo/gallery and README recipes

## Decisions

- ScottPlot 5 is an ergonomics reference, not a compatibility target.
- Keep the implementation chart-local unless a small explicit bridge is required.
- Use Beads as the task spine for phase status, ownership, dependencies, and handoff.
- Use isolated worktrees and branches only for disjoint implementation phases.
- Do not restore old chart controls, public direct `Source`, compatibility wrappers, PDF/vector export, backend expansion, hidden fallback/downshift, or god-code demo editor behavior.

## Known Residuals

- Full CI can lag; use focused local verification first unless CI is explicitly requested.
- `.planning` phase artifacts are force-tracked only when needed because `.planning/` is ignored by default.
- Docker-backed Dolt SQL Server is the Beads remote path; if `bd dolt push` hits host path issues, use `scripts/Push-BeadsDoltViaHost.ps1`.

## Session Continuity

Last session: `2026-04-30 +08:00`
Stopped at: Phase 387 complete
Next action: Integrate Phase 386 after its separate worktree closes, then proceed to Phase 388 when dependencies are satisfied.

## Accumulated Context

From v2.55: 27 requirements delivered across 7 phases. SurfaceCharts now has short raw `Plot.Add.*` overloads, typed plottables, `Plot.Axes`, `Plot.SavePngAsync`, `DataLogger3D`, same-type multi-series composition, cookbook seed docs, and closure guardrails.

From v2.54: 22 interactivity requirements delivered across 5 phases. Chart now has crosshair, tooltips, probe strategies, keyboard shortcuts, and toolbar.
