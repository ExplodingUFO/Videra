---
phase: 407
bead: Videra-xq1
title: "Phase 407 Interaction Handoff Polish"
status: planned
scope: "bounded docs/tests/support evidence for existing SurfaceCharts interaction APIs"
---

# Phase 407 Plan

## Owned Child Beads

- `Videra-0yp` - Phase 407 interaction snippet parity and host wiring
- `Videra-ots` - Phase 407 probe selection draggable handoff

## Success Criteria

- `src/Videra.SurfaceCharts.Avalonia/README.md` documents minimal imports and
  host button/context-menu wiring for `SurfaceChartInteractionProfile`,
  `SurfaceChartCommand`, and `TryExecuteChartCommand(...)`.
- The same README documents bounded probe evidence, selection report, and
  host-owned draggable marker/range overlay recipes using the shipped APIs.
- Focused integration tests assert the handoff wording and evidence terms are
  present and do not imply chart-owned product selection or built-in drag editor
  behavior.
- Validation passes with the Phase 407 focused test filter, snapshot scope
  guard script, `git diff --check`, and final branch status.

## Non-Goals

- No command framework, mouse remapping framework, chart-owned selection state,
  backend/export expansion, hidden downshift path, generic plotting workbench,
  or runtime behavior changes.
