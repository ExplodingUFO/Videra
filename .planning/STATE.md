---
gsd_state_version: 1.0
milestone: v2.62
milestone_name: "Native SurfaceCharts Cleanup and Old-Code Removal"
status: active
stopped_at: "Phase 415 and Phase 416 ready for parallel execution"
last_updated: "2026-04-30T17:18:00+08:00"
last_activity: 2026-04-30
progress:
  total_phases: 5
  completed_phases: 1
  total_plans: 5
  completed_plans: 1
  percent: 20
---

# Project State

## Project Reference

See: `.planning/PROJECT.md`

**Core value:** 跨平台 3D 渲染引擎的可靠性
**Current focus:** v2.62 Native SurfaceCharts Cleanup and Old-Code Removal

## Current Position

Milestone: `v2.62 Native SurfaceCharts Cleanup and Old-Code Removal`
Phase: 415/416 of 418 - READY
Plan: Phase 415 and Phase 416 can run in parallel
Status: Phase 414 read-only inventory is complete. True cleanup candidates are split into Phase 415 code/API work, Phase 416 demo/cookbook simplification, and Phase 417 guardrail/CI hardening.
Last activity: 2026-04-30 - Completed Phase 414 inventory, created child beads, corrected dependencies, and prepared Phase 415/416 parallel handoff.

Progress: [##--------] 20%

## Initial Scope Summary

v2.62 continues from the completed v2.61 native cookbook and CI truth milestone:

- SurfaceCharts has detailed Videra-native cookbook recipes for shipped 3D chart families and workflows.
- SurfaceCharts has focused native performance evidence and CI truth gates for cookbook/demo/support drift.
- Beads and generated roadmap state are synchronized through v2.61 closure.

v2.62 focuses on:

- native cleanup and old-code inventory
- deletion or direct replacement of true stale SurfaceCharts old-code paths
- bounded cookbook/demo simplification without god-code workbench expansion
- no-compat/no-old-code guardrails and CI truth hardening
- final integrated validation and clean Beads/Git/Dolt handoff

## Decisions

- Beads remain the single task spine for phase status, dependencies, ownership, and handoff.
- Phase 414 must classify residual old-code/compat/fallback/downshift hits before deletion.
- Phase 415 and Phase 416 may run in parallel after Phase 414 if their write sets are disjoint.
- Phase 415 owns true stale old-code deletion or direct replacement.
- Phase 416 owns bounded cookbook/demo simplification and must avoid god-code workbench expansion.
- Phase 417 owns no-compat/no-old-code guardrails and focused CI truth after Phase 415/416.
- Shared Beads export, generated roadmap, final scope guardrails, archive, push, and cleanup are Phase 418 responsibilities.
- Keep implementation simple and direct; do not add ScottPlot compatibility adapters, migration shims, old chart controls, hidden fallback/downshift behavior, backend expansion, broad demo frameworks, or fake validation evidence.
- v2.60 artifacts are archived under `.planning/milestones/v2.60-phases`.
- v2.61 artifacts are archived under `.planning/milestones/v2.61-phases`.

## Known Residuals

- Existing analyzer warnings remain in pre-existing SurfaceCharts/demo files.
- `.planning` phase artifacts are force-tracked only when needed because `.planning/` is ignored by default.
- Docker-backed Dolt SQL Server is the Beads remote path; if `bd dolt push` hits host path issues, use `scripts/Push-BeadsDoltViaHost.ps1`.

## Session Continuity

Last session: `2026-04-30 +08:00`
Stopped at: Phase 415/416 ready
Next action: execute Phase 415 (`Videra-9vg`) and Phase 416 (`Videra-w2t`) in isolated worktrees with their child beads.

## Accumulated Context

From v2.58: controlled release cutover package completed with release-readiness validation, manual-gated release actions, support docs, and archive.

From v2.57: SurfaceCharts release-readiness evidence covers package-only consumer smoke, public API/package metadata, validation scripts, release-candidate docs/support handoff, final guardrails, and clean Beads state.

From v2.56: SurfaceCharts has Plot lifecycle/code polish, interaction profile commands, selection/probe/draggable overlay recipes, axis rules, linked views, live view management, cookbook demo/gallery docs, and closure guardrails.

From v2.55: SurfaceCharts has short raw `Plot.Add.*` overloads, typed plottables, `Plot.Axes`, `Plot.SavePngAsync`, `DataLogger3D`, same-type multi-series composition, cookbook seed docs, and closure guardrails.
