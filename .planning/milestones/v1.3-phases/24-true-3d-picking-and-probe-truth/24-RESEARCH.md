# Phase 24 Research

## Problem

The chart now has a shared camera-frame projection spine, but probe truth still comes from linear viewport remapping. That breaks camera correctness immediately:

- hover hits drift when yaw/pitch changes
- wheel zoom anchors are only accidentally correct
- pinned probes are not grounded in a real 3D surface hit

## Codebase Findings

- `SurfaceProbeService` currently uses `SurfaceViewport` + `Size` to derive sample coordinates directly.
- `SurfaceProbeInfo` only stores sample/axis/value truth plus `IsApproximate`; it has no world position or tile provenance.
- `SurfaceProbeOverlayPresenter` and pinned-probe rendering already know how to display richer information if the core contract carries it.
- `SurfaceChartInteractionController` already consumes the hovered probe for dolly anchoring, so a better hovered-probe result automatically improves wheel zoom.

## Chosen Direction

1. Add core picking contracts: pick ray, pick hit, heightfield picker.
2. Generate screen rays from the Phase 23 camera frame.
3. Resolve hover probes from ray-heightfield hits instead of viewport-linear remapping.
4. Extend `SurfaceProbeInfo` with world-space/tile-depth truth so pinned markers and later refine upgrades have the right data.

## Verification Shape

- Core tests:
  - screen-ray hit remains stable across multiple yaw/pitch views of the same peak
  - finer tiles win over coarse overlapping tiles
- Avalonia integration tests:
  - hover probe tracks a projected world peak under different camera angles
  - pinned probes remain stable through orbit and refine-facing updates
