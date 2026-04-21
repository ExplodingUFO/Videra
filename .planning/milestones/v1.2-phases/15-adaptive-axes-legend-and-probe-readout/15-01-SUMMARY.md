---
phase: 15-adaptive-axes-legend-and-probe-readout
plan: 01
subsystem: ui
tags: [surface-charts, avalonia, axes, legend, projection]
requires:
  - phase: 14-built-in-interaction-and-camera-workflow
    provides: built-in chart interaction shell and chart-local control lifecycle
provides:
  - shared chart projection helper for painter and overlay layout
  - explicit axis/legend overlay state contracts and presenters
  - integration coverage for metadata labels, legend range truth, and yaw-driven edge switching
affects: [15-02, 15-03, 16-rendering-host-seam-and-gpu-main-path]
tech-stack:
  added: []
  patterns: [shared chart projection, explicit overlay presenter state]
key-files:
  created:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartProjection.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayState.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayState.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayPresenter.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayPresenter.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceAxisOverlayTests.cs
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceScenePainter.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceCameraController.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs
key-decisions:
  - "Keep overlay layout chart-local by adding dedicated axis and legend presenters in Videra.SurfaceCharts.Avalonia."
  - "Share painter and overlay transforms through SurfaceChartProjection instead of duplicating screen-space math."
patterns-established:
  - "Overlay presenters compute explicit state first, then render from that state."
  - "Chart projection anchors include full chart bounds so painter and overlays fit the same scene envelope."
requirements-completed: [AXIS-01, AXIS-02]
duration: 5 min
completed: 2026-04-14
---

# Phase 15 Plan 01: Axis layout engine, tick generation, and legend/value-scale contract Summary

**Shared chart projection plus adaptive X/Y/Z axis and legend overlays wired directly into SurfaceChartView**

## Performance

- **Duration:** 5 min
- **Started:** 2026-04-14T17:19:57+08:00
- **Completed:** 2026-04-14T17:25:22Z
- **Tasks:** 2
- **Files modified:** 10

## Accomplishments
- Added `SurfaceChartProjection` so the software painter and overlay layer use the same screen-space transform.
- Introduced explicit axis and legend overlay state models plus dedicated presenters for layout and rendering.
- Added integration tests that lock metadata labels, legend range truth, and yaw-based edge selection behavior.

## Task Commits

Each task was committed atomically:

1. **Task 1: Extract reusable chart projection helpers for overlay layout** - `caa50e0` (`feat`)
2. **Task 2: Render adaptive axes and legend through SurfaceChartView overlay** - `413e78a` (`feat`)

## Files Created/Modified
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartProjection.cs` - Shared chart projection and chart-bounds anchor helpers.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayState.cs` - Explicit axis line/title/tick state contracts.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayState.cs` - Explicit legend swatch and label state contracts.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayPresenter.cs` - Adaptive axis layout, tick generation, and rendering.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayPresenter.cs` - Truthful legend range selection and rendering.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceScenePainter.cs` - Painter switched to the shared projector.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceCameraController.cs` - Internal projection settings path for yaw-driven overlay tests.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs` - Overlay invalidation now computes axis, legend, probe, and shared projection state together.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs` - Rendering path now consumes the stored shared projection.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceAxisOverlayTests.cs` - Integration coverage for labels, legend ranges, and edge switching.

## Decisions Made
- Kept the new overlay contracts inside `Videra.SurfaceCharts.Avalonia` so Phase 15 does not couple chart overlays back to `VideraView`.
- Used `ColorMap.Range` only when the control has an explicit `ColorMap`, and fell back to `Source.Metadata.ValueRange` when the fallback color map is active.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Plan referenced a later camera/runtime seam than this checkout provides**
- **Found during:** Task 2 (Render adaptive axes and legend through `SurfaceChartView` overlay)
- **Issue:** The plan text assumed `ViewState.Camera` and `_runtime.GetLoadedTiles()`, but this branch still uses `Viewport`, `_tileCache`, and `SurfaceCameraController`.
- **Fix:** Implemented the same overlay behavior against the current control seams and added an internal projection-settings path on `SurfaceCameraController` to support yaw-driven overlay selection.
- **Files modified:** `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceCameraController.cs`, `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs`, `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs`, `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayPresenter.cs`, `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayPresenter.cs`
- **Verification:** `dotnet build tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release`; `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceAxisOverlayTests"`
- **Committed in:** `413e78a`

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** The shipped behavior matches the plan intent, but it is implemented against the current branch’s pre-`ViewState` seams.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Ready for `15-02` probe/readout work on top of the shared projection and chart-local overlay state pattern.
- Current axis/legend behavior is locked by integration tests, but the internal projection-settings seam is still an implementation detail until the later camera contract lands.

---
*Phase: 15-adaptive-axes-legend-and-probe-readout*
*Completed: 2026-04-14*
