---
phase: 385
title: "Interaction Profile and Command Surface"
status: completed
bead: Videra-v256.3
---

# Phase 385 Summary

Phase 385 added a small public chart-local interaction profile and bounded
command surface for `VideraChartView`.

## Implementation Summary

- Added `SurfaceChartInteractionProfile` with explicit switches for built-in
  orbit, pan, dolly/zoom, reset camera, fit-to-data, keyboard shortcut, toolbar,
  focus selection, probe pinning, and pointer-focus behavior.
- Added `SurfaceChartCommand` for bounded host wiring of common chart actions:
  zoom in/out, pan left/right/up/down, reset camera, and fit to data.
- Added `VideraChartView.InteractionProfile` and
  `VideraChartView.TryExecuteChartCommand(...)`.
- Routed existing pointer, wheel, keyboard, and toolbar behavior through the
  profile while preserving the fully enabled default profile.
- Disabled behavior is deterministic: disabled commands return `false`, disabled
  gestures do not capture or mutate view state, and disabled toolbar hits are
  consumed without falling through into chart gestures.

## Evidence

status: passed

- Focused interaction tests passed: 27/27.
- Snapshot export scope guardrail passed.
- `git diff --check` passed.

## Requirement Closure

- `INT-01`: completed by `SurfaceChartInteractionProfile`.
- `INT-02`: completed by explicit disabled-profile checks and deterministic
  no-op behavior.
- `INT-03`: completed by bounded `SurfaceChartCommand` and
  `TryExecuteChartCommand`.
- `INT-04`: completed by routing toolbar commands through the same command
  surface; host context menus can call the same bounded method.

## Merge Notes

Phase 385 touched only the assigned interaction/input/test surfaces plus Phase
385 planning state. It does not overlap with Phase 384's `Controls/Plot/*`
ownership unless generated planning or Beads files need normal merge ordering.
