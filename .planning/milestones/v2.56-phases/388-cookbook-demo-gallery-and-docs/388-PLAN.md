---
phase: 388
title: "Cookbook Demo Gallery and Docs"
status: completed
bead: Videra-v256.6
---

# Phase 388 Plan

## Goal

Restructure the SurfaceCharts demo/docs into a bounded cookbook/gallery model
for first chart, styling, interactions, live data, linked axes, and export
recipes while keeping the sample a reference app, not a generic editor.

## Assumptions

- Existing Phase 384-387 APIs are available: Plot lifecycle, interaction
  profile/commands, probe helpers, axis bounds/locks, linked views, and
  `DataLogger3D` live view evidence.
- Demo recipe results should reuse existing explicit setup paths wherever
  possible instead of adding product runtime APIs or a broad demo editor.
- ScottPlot 5 should be named as an ergonomics inspiration only, with explicit
  rejection of compatibility or parity.

## Implementation Steps

1. Add a compact cookbook gallery panel to `Videra.SurfaceCharts.Demo`.
   - Verify: demo project builds.
2. Map recipe groups to existing explicit demo paths and visible snippets.
   - Verify: docs/config test asserts first chart, styling, interactions, live
     data, linked axes, and export recipe coverage.
3. Update root and demo README cookbook sections.
   - Verify: docs/config test asserts ScottPlot 5 inspiration and Videra 3D
     boundary wording.
4. Update Phase 388 planning/requirements state and generated Beads roadmap.
   - Verify: exports complete and worktree contains only Phase 388 changes.

## Boundaries

- No product runtime API changes.
- No compatibility layer, direct public `Source` API, PDF/vector export, backend
  expansion, hidden fallback/downshift behavior, or god-code demo editor.
