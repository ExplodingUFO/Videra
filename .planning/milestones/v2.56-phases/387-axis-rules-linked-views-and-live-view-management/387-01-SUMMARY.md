---
phase: 387
title: "Axis Rules, Linked Views, and Live View Management"
status: completed
bead: Videra-v256.5
---

# Phase 387 Summary

Phase 387 added explicit chart-local axis rules, deterministic two-chart view linking, and core-local live view evidence.

## Implementation Summary

- `Plot.Axes.X/Y/Z` now support `IsLocked`, `SetBounds(...)`, `ClearBounds()`, and `Bounds`.
- Explicit limit and autoscale calls are constrained by the per-axis rules. Locked axes preserve current limits; bounds clamp requested and autoscaled limits.
- `VideraChartView.LinkViewWith(...)` returns a disposable two-view link that synchronizes `ViewState` in both directions and disconnects deterministically.
- `DataLogger3D` now exposes full-data/latest-window live view mode plus `DataLogger3DLiveViewEvidence` for appended points, dropped points, retained count, visible window, and autoscale decision.

## Evidence

status: passed

- Focused DataLogger3D tests passed: 5/5.
- Focused Axis/Plot API integration tests passed: 47/47.
- Snapshot export scope guardrail passed.
- `git diff --check` passed.

## Requirement Closure

- `AXIS-01`: completed by per-axis locks and min/max bounds constraining `Plot.Axes` limit/autoscale calls.
- `AXIS-02`: completed by explicit disposable `VideraChartView.LinkViewWith(...)`.
- `LIVE-01`: completed by `DataLogger3D` full-data/latest-window live view mode.
- `LIVE-02`: completed by deterministic `DataLogger3DLiveViewEvidence`.

## Merge Notes

Phase 387 touched the assigned axis/live/linking surfaces and focused tests. It did not edit overlay, probe, selection, demo, or Phase 386-owned files.
