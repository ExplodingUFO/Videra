---
phase: 386
title: "Selection, Probe, and Draggable Overlay Recipes"
status: completed
bead: Videra-v256.4
---

# Phase 386 Summary

Phase 386 added small chart-local interaction recipes on top of the Phase 385
interaction profile/command surface.

## Implementation Summary

- Added `VideraChartView.TryResolveProbe(...)` for pointer-to-probe resolution
  without changing hover or pinned probe state.
- Added `VideraChartView.TryCreateSelectionReport(...)` for host-owned click
  and rectangle reports in screen, sample, and axis space.
- Added bounded immutable draggable marker/range overlay recipes via
  `SurfaceChartDraggableOverlayRecipes`.
- Added deterministic interaction recipe evidence via
  `SurfaceChartInteractionRecipeEvidenceFormatter`.
- Added focused tests covering probe helper state ownership, selection reports,
  draggable overlay bounds, view helper creation, and evidence formatting.

## Evidence

status: passed

- Focused probe/pinned/interaction tests passed: 67/67.
- Snapshot export scope guardrail passed.
- `git diff --check` passed.

## Requirement Closure

- `PICK-01`: completed by `TryResolveProbe(...)` and screen-to-sample/axis
  selection helpers.
- `PICK-02`: completed by host-owned click/rectangle `SurfaceChartSelectionReport`
  without chart-owned selected state.
- `PICK-03`: completed by bounded marker/range overlay recipe helpers.
- `PICK-04`: completed by deterministic recipe evidence and immutable helper
  return values that do not mutate source data.

## Merge Notes

Phase 386 touched only overlay/control helpers, focused interaction tests, Phase
386 planning state, and Beads/generated roadmap artifacts. It intentionally did
not touch `Controls/Plot/*`, axis/linking/live helper files, or sample demo code
owned by later phases.
