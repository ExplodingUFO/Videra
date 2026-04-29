---
gsd_state_version: 1.0
milestone: v2.54
milestone_name: "Chart Interactivity"
status: active
stopped_at: "Roadmap created"
last_updated: "2026-04-29T21:53:00+08:00"
last_activity: 2026-04-29
progress:
  total_phases: 5
  completed_phases: 1
  total_plans: 1
  completed_plans: 1
  percent: 20
---

# Project State

## Project Reference

See: `.planning/PROJECT.md`

**Core value:** 跨平台 3D 渲染引擎的可靠性
**Current focus:** v2.54 Chart Interactivity

## Current Position

Milestone: `v2.54 Chart Interactivity`
Phase: 372 of 375 (Enhanced Tooltips)
Plan: 371-PLAN complete
Status: Phase 371 Crosshair Overlay complete. Ready for Phase 372.
Last activity: 2026-04-29 - Completed Phase 371 Crosshair Overlay with 6 tests.

Progress: [██░░░░░░░░] 20%

## Initial Scope Summary

v2.54 starts from the v2.53 chart type expansion baseline:

- `VideraChartView` is the single shipped chart control with Surface, Waterfall, Scatter, Bar, and Contour series.
- Log/DateTime axes with custom tick formatters are operational.
- Multi-series legend overlay with kind-specific indicators is shipped.
- Chart-local PNG/bitmap snapshot export via `Plot3D.CaptureSnapshotAsync` is operational.

The scope adds interactivity:

- Crosshair overlay: projected ground-plane guidelines with axis-value pills following mouse
- Enhanced tooltips: multi-series-aware, edge-avoidance positioning, configurable offset
- Series probe strategies: `ISeriesProbeStrategy` with scatter/bar/contour implementations
- Keyboard shortcuts: +/- zoom, arrow pan, Home reset, F fit-to-data
- Zoom/pan toolbar buttons as overlay controls
- Cursor feedback during hover, drag, zoom operations
- Integration with snapshot export and probe evidence contracts

## Decisions

- Phase 371–374 are independent (all depend only on existing infrastructure) — can be parallelized
- Phase 375 depends on all of 371–374 — integration and verification last
- Crosshair uses separate lightweight render path (not full overlay coordinator rebuild)
- Crosshair projects onto XZ ground plane (not screen-space H/V lines) because Videra is 3D
- Axis-value pills positioned at outer endpoints following ScottPlot pattern
- Default crosshair visibility is ON for immediate user feedback
- Probe strategies use `ISeriesProbeStrategy` interface for extensibility
- All features are presentation-layer overlays — no backend/Core changes
- Start at Phase 371 (continuing from v2.53's Phase 370)

## Known Residuals

- Full CI can lag; user often prioritizes fast local progress unless CI is explicitly requested.
- `.planning` remains local-only unless specific files are already tracked.
- Docker-backed Dolt SQL Server is the Beads remote path; use direct Docker Dolt push when needed.
- v2.54 must not restore old chart view APIs, reintroduce direct public `Source`, add PDF/vector export, add compatibility wrappers, add hidden fallback/downshift behavior, expand backend/runtime scope, create a generic plotting engine, or create a god-code workbench.

## Session Continuity

Last session: `2026-04-29 +08:00`
Stopped at: Completed Phase 371 Crosshair Overlay
Next action: `$gsd-plan-phase 372` to plan Phase 372 Enhanced Tooltips
