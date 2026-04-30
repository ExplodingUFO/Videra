# Phase 425 Context: Analysis Workspace and Streaming Inventory

**Gathered:** 2026-04-30
**Status:** Ready for planning
**Mode:** Autonomous smart discuss with user constraints
**Bead:** `Videra-7tqx.1`

## Phase Boundary

Phase 425 is an inventory-only phase for v2.64. It must map the real
SurfaceCharts surfaces for multi-chart analysis, linked interaction,
streaming/high-density data, cookbook/package templates, CI, and
release-readiness before any implementation phase changes APIs, demo behavior,
or validation scripts.

## User Constraints

- Beads are the task, state, and handoff spine.
- Split tasks small and identify dependencies before implementation.
- Use isolated worktrees and branches for parallel beads when write scopes do
  not block each other.
- Every worker must have a responsibility boundary, write scope, validation
  command, and handoff notes.
- Avoid god code.
- Do not add compatibility layers, downshift behavior, fallback behavior, old
  chart controls, or fake validation evidence.
- Keep implementation simple and direct.

## Initial Code Context

- `VideraChartView` remains the single shipped SurfaceCharts control.
- `VideraChartView.Plot.Add.*` remains the data-loading and authoring route.
- `VideraChartView.LinkedViews.cs` already provides pairwise view-state
  linking.
- `DataLogger3D` and `SurfaceChartsStreamingBenchmarks` already provide
  streaming evidence surfaces.
- `smoke/Videra.SurfaceCharts.ConsumerSmoke` and
  `scripts/Invoke-ReleaseReadinessValidation.ps1` are the package-consumer and
  release-readiness truth paths.
- The repository already has cookbook recipes, generated public roadmap tests,
  and snapshot scope guardrails that should be extended rather than bypassed.

## Parallel Inventory Beads

| Bead | Scope | Worktree | Output |
| --- | --- | --- | --- |
| `Videra-7tqx.1.1` | API/workspace seams | `.worktrees/v264-425-api` | `425A-API-WORKSPACE-INVENTORY.md` |
| `Videra-7tqx.1.2` | Demo/cookbook/package templates | `.worktrees/v264-425-demo` | `425B-DEMO-COOKBOOK-TEMPLATE-INVENTORY.md` |
| `Videra-7tqx.1.3` | Streaming/high-density/performance truth | `.worktrees/v264-425-stream` | `425C-STREAMING-PERFORMANCE-INVENTORY.md` |
| `Videra-7tqx.1.4` | CI/release-readiness/guardrails | `.worktrees/v264-425-ci` | `425D-CI-RELEASE-GUARDRAIL-INVENTORY.md` |

## Deferred

Implementation belongs to later phases:

- Phase 426 owns native multi-chart analysis workspace changes.
- Phase 427 owns linked interaction and selection propagation.
- Phase 428 owns high-density and streaming data evidence.
- Phase 429 owns cookbook and package templates.
- Phase 430 owns CI/release-readiness truth.
