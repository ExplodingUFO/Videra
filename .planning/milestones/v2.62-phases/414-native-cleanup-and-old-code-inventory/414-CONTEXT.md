# Phase 414 Context: Native Cleanup and Old-Code Inventory

## Bead

- Phase bead: `Videra-2wb`
- Milestone epic: `Videra-6zx`
- Status: claimed and executed as a read-only inventory phase.

## Goal

Classify residual SurfaceCharts old-code, compatibility, fallback/downshift,
docs/demo/test, and guardrail surfaces before deletion or refactoring.

## Decisions

- Do not delete code during Phase 414.
- Treat Beads as the task spine; downstream work is split into smaller child
  beads.
- Preserve intentional negative guardrail text where it prevents old public
  controls, direct public `Source`, hidden fallback/downshift, or fake evidence
  from returning.
- Product-truth fallback outside this SurfaceCharts cleanup scope remains out of
  scope unless it creates chart-local downshift behavior.
- Phase 415 and Phase 416 can run in parallel after Phase 414 if isolated in
  worktrees because their write sets are disjoint.

## Evidence Sources

- Code/API scan: `src/Videra.SurfaceCharts.*`, especially render host, render
  inputs, chart view rendering/overlay, Linux host, and Plot evidence code.
- Docs/demo scan: `docs/**`, `samples/Videra.SurfaceCharts.Demo/**`.
- Tests/CI scan: `tests/**`, `scripts/**`, `.github/**`.

## Non-Goals

- No compatibility layer.
- No downshift/fallback preservation.
- No old chart controls.
- No direct public `VideraChartView.Source`.
- No backend expansion.
- No generic demo/workbench expansion.
