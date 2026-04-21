---
phase: 15-adaptive-axes-legend-and-probe-readout
plan: 02
subsystem: ui
tags: [surface-charts, avalonia, probe, interaction]
requires:
  - phase: 15-adaptive-axes-legend-and-probe-readout
    provides: shared chart projection and chart-local overlay presenters from 15-01
provides:
  - axis-space probe contract with approximate/exact truth
  - chart-local probe service separated from overlay rendering
  - built-in hover plus Shift+LeftClick pinned-probe workflow
affects: [15-03, 16-rendering-host-seam-and-gpu-main-path]
tech-stack:
  added: []
  patterns: [probe service, pinned probe requests, chart-local pointer seam]
key-files:
  created:
    - src/Videra.SurfaceCharts.Core/Picking/SurfaceProbeInfo.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeService.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartInteractionController.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Input.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartPinnedProbeTests.cs
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayState.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Properties.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartProbeOverlayTests.cs
key-decisions:
  - "Pinned probes are stored as sample-space requests on SurfaceChartView so hover/pin truth recomputes when loaded tile density changes."
  - "The current checkout lacked the later-branch chart input partial and interaction controller, so 15-02 adds the narrowest chart-local pointer seam needed for hover and Shift+LeftClick pinning."
patterns-established:
  - "Probe computation lives in SurfaceProbeService while the presenter only formats and renders overlay state."
  - "Pinned probe state is chart-local and never routes through VideraView selection or annotation overlays."
requirements-completed: [PROBE-01, PROBE-02]
duration: 1 min
completed: 2026-04-14
---

# Phase 15 Plan 02: Probe/readout service with hover and pinned-point workflows Summary

**Chart-local probe service with axis-space hover readouts, approximate/exact truth, and Shift+LeftClick pinned comparisons**

## Performance

- **Duration:** 1 min
- **Started:** 2026-04-14T17:41:25+08:00
- **Completed:** 2026-04-14T17:42:42+08:00
- **Tasks:** 2
- **Files modified:** 10

## Accomplishments
- Added `SurfaceProbeInfo` and `SurfaceProbeService` so probe resolution now exposes sample-space, axis-space, value, and approximate/exact truth through a reusable chart-local seam.
- Extended the overlay state/presenter so hover and pinned probes render from explicit probe models instead of coupled probe math plus one-off bubble text.
- Added built-in pointer hover plus `Shift + LeftClick` pin toggling with integration coverage for axis conversion, coarse-tile approximation, and pinned-probe toggling.

## Task Commits

Each task was committed atomically:

1. **Task 1: Split probe computation from rendering and enrich the probe contract** - `a400a5a` (`feat`)
2. **Task 2: Add built-in hover and pinned-point workflows** - `69c0b9c` (`feat`)

**Plan metadata:** not committed (`commit_docs: false`; `.planning/` is gitignored in this checkout).

## Files Created/Modified
- `src/Videra.SurfaceCharts.Core/Picking/SurfaceProbeInfo.cs` - Public probe contract with sample-space, axis-space, value, and approximation truth.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeService.cs` - Reusable probe resolver shared by hover and pinned workflows.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayState.cs` - Hovered and pinned probe overlay state with compatibility projections for existing tests.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs` - Hover/pinned readout rendering with `Approx`/`Exact` labels and projected pin markers.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs` - Chart-local probe service wiring plus pinned-probe request storage.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartInteractionController.cs` - Minimal chart-local gesture helper for hover updates and pin-toggle click detection.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Input.cs` - Pointer overrides that route hover and Shift+LeftClick pin gestures through the control.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Properties.cs` - Source changes clear pinned probe state alongside the active hover point.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartProbeOverlayTests.cs` - Regression coverage for axis conversion and coarse-tile approximation flags.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartPinnedProbeTests.cs` - Pinned-probe workflow regression coverage.

## Decisions Made
- Stored pinned probes as sample-space requests rather than frozen probe results so future tile refinements can upgrade pinned readouts from coarse to exact without host help.
- Kept the new gesture seam inside `SurfaceChartView` instead of reaching into `VideraView` overlays or importing the later branch's broader interaction contract.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Plan referenced later-branch interaction files that do not exist in this checkout**
- **Found during:** Task 2 (Add built-in hover and pinned-point workflows)
- **Issue:** The plan assumed `SurfaceChartView.Input.cs` and `SurfaceChartInteractionController.cs` already existed on this branch, but the current checkout still uses the earlier `Viewport` / `_tileCache` shell and had no chart input partial at all.
- **Fix:** Added the narrowest chart-local input partial and interaction helper needed for truthful hover and `Shift + LeftClick` pin toggling, without expanding into a broader Phase-14 interaction rewrite.
- **Files modified:** `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartInteractionController.cs`, `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Input.cs`, `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs`, `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Properties.cs`
- **Verification:** `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartProbeOverlayTests|FullyQualifiedName~SurfaceChartPinnedProbeTests"`
- **Committed in:** `69c0b9c`

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** The shipped probe/readout behavior matches the plan intent on this branch, but it is implemented through a smaller chart-local input seam than the later branch text assumed.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Ready for `15-03` correctness regression work with explicit probe service, pinned-probe state, and gesture coverage in place.
- Remaining risk: this branch still uses the pre-later-branch chart shell, so `15-03` should validate the shipped `SurfaceChartView` seam rather than assume the later interaction/runtime file layout.

---
*Phase: 15-adaptive-axes-legend-and-probe-readout*
*Completed: 2026-04-14*
