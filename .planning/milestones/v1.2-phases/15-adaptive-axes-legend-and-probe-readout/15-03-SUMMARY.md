---
phase: 15-adaptive-axes-legend-and-probe-readout
plan: 03
subsystem: testing
tags: [surface-charts, regression, probe, overlay, repository-guard]
requires:
  - phase: 15-adaptive-axes-legend-and-probe-readout
    provides: shared projection, axis/legend overlays, and chart-local probe workflows from 15-01 and 15-02
provides:
  - core probe mapping seam with unit coverage for sample-space to axis-space truth
  - integration regressions for axis monotonicity, legend range truth, clamped viewport probe mapping, and pinned probe stability
  - repository guard that keeps Phase 15 overlay symbols out of VideraView
affects: [16-rendering-host-seam-and-gpu-main-path, 17-large-dataset-residency-cache-evolution-and-optional-rust-spike, 18-demo-docs-and-repository-truth-for-professional-charts]
tech-stack:
  added: []
  patterns: [core-owned probe mapping truth, chart-local overlay boundary guard]
key-files:
  created:
    - tests/Videra.SurfaceCharts.Core.Tests/Picking/SurfaceProbeInfoTests.cs
  modified:
    - src/Videra.SurfaceCharts.Core/Videra.SurfaceCharts.Core.csproj
    - src/Videra.SurfaceCharts.Core/Picking/SurfaceProbeInfo.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeService.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceAxisOverlayTests.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartProbeOverlayTests.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartPinnedProbeTests.cs
    - tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs
key-decisions:
  - "Move sample-space to axis-space probe mapping into SurfaceProbeInfo so the truth can be unit-tested in the core assembly instead of staying buried in the Avalonia probe service."
  - "Lock the shipped branch seam through Viewport and UpdateProjectionSettings-based regressions rather than inventing later-branch ZoomTo or ResetCamera APIs."
patterns-established:
  - "Probe mapping truth now lives in Videra.SurfaceCharts.Core and is reused by the Avalonia overlay service."
  - "Repository boundary guards read VideraView sibling files directly and fail on chart overlay symbol leakage."
requirements-completed: [AXIS-01, AXIS-02, PROBE-01, PROBE-02]
duration: 2 min
completed: 2026-04-14
---

# Phase 15 Plan 03: Coordinate, bounds, and probe-correctness regression matrix Summary

**Core probe mapping truth plus integration and repository regressions that lock Phase 15 axis, legend, and pinned-probe behavior on the shipped chart-local seams**

## Performance

- **Duration:** 2 min
- **Started:** 2026-04-14T17:52:05+08:00
- **Completed:** 2026-04-14T17:53:43+08:00
- **Tasks:** 2
- **Files modified:** 8

## Accomplishments
- Added `SurfaceProbeInfoTests` plus a core mapping seam so sample-space to axis-space conversion and coarse-vs-exact truth are unit-tested in `Videra.SurfaceCharts.Core`.
- Expanded the integration matrix to cover monotonic axis ticks under projection changes, legend labels tied to `ColorMap.Range`, clamped viewport probe truth, viewport-focus recomputation, and pinned probe stability across viewport and projection changes.
- Added a repository guard that keeps `SurfaceAxisOverlayPresenter`, `SurfaceLegendOverlayPresenter`, `SurfaceProbeService`, `SurfaceProbeInfo`, and `Videra.SurfaceCharts` references out of `VideraView` and `VideraView.Overlay`.

## Task Commits

Each task was committed atomically:

1. **Task 1: Build the coordinate and probe-correctness regression matrix** - `ce13b0b` (`test`), `196f46f` (`feat`)
2. **Task 2: Guard the sibling boundary while overlay features grow** - `905a08c` (`test`)

**Plan metadata:** not committed (`commit_docs: false`; `.planning/` is gitignored in this checkout).

## Files Created/Modified
- `tests/Videra.SurfaceCharts.Core.Tests/Picking/SurfaceProbeInfoTests.cs` - Core coverage for min/mid/max sample-to-axis mapping and approximate/exact derivation.
- `src/Videra.SurfaceCharts.Core/Videra.SurfaceCharts.Core.csproj` - Internals visibility for the core probe-mapping seam.
- `src/Videra.SurfaceCharts.Core/Picking/SurfaceProbeInfo.cs` - Internal core factory that maps sample-space probes into axis-space truth.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeService.cs` - Avalonia probe resolution now reuses the core mapping seam.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceAxisOverlayTests.cs` - Projection-aware monotonic axis tick and legend-range regressions.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartProbeOverlayTests.cs` - Clamped viewport and focused-window probe regressions.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartPinnedProbeTests.cs` - Pinned probe stability checks across viewport and projection changes.
- `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs` - Sibling-boundary guard for Phase 15 overlay symbols.

## Decisions Made
- Pulled probe mapping into `Videra.SurfaceCharts.Core` so the coordinate truth does not depend on an Avalonia-only test harness.
- Treated the current branch's `Viewport` plus internal `UpdateProjectionSettings(...)` seam as the shipped contract for 15-03 regression coverage.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Core regression coverage needed a small production seam**
- **Found during:** Task 1 (Build the coordinate and probe-correctness regression matrix)
- **Issue:** Sample-space to axis-space conversion only existed as private Avalonia service logic, so the requested core `SurfaceProbeInfoTests` could not verify the truth without duplicating implementation details.
- **Fix:** Added an internal `SurfaceProbeInfo.FromResolvedSample(...)` helper in `Videra.SurfaceCharts.Core` and routed `SurfaceProbeService` through it.
- **Files modified:** `src/Videra.SurfaceCharts.Core/Videra.SurfaceCharts.Core.csproj`, `src/Videra.SurfaceCharts.Core/Picking/SurfaceProbeInfo.cs`, `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeService.cs`
- **Verification:** `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceProbeInfoTests"`; `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceAxisOverlayTests|FullyQualifiedName~SurfaceChartProbeOverlayTests|FullyQualifiedName~SurfaceChartPinnedProbeTests"`
- **Committed in:** `196f46f`

**2. [Rule 3 - Blocking] The original plan text referenced later public camera APIs that do not exist on this checkout**
- **Found during:** Task 1 (Build the coordinate and probe-correctness regression matrix)
- **Issue:** The plan text called out `ZoomTo(...)` and `ResetCamera()`, but the shipped branch still exposes viewport/focus changes through `SurfaceChartView.Viewport` and projection changes through the internal `UpdateProjectionSettings(...)` seam.
- **Fix:** Adapted the regression matrix to the shipped `Viewport` + `UpdateProjectionSettings(...)` path and locked pinned-probe stability there.
- **Files modified:** `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceAxisOverlayTests.cs`, `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartProbeOverlayTests.cs`, `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartPinnedProbeTests.cs`
- **Verification:** `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceAxisOverlayTests|FullyQualifiedName~SurfaceChartProbeOverlayTests|FullyQualifiedName~SurfaceChartPinnedProbeTests"`
- **Committed in:** `196f46f`

---

**Total deviations:** 2 auto-fixed (2 blocking)
**Impact on plan:** Both deviations tightened the regression matrix around the real shipped seams without widening feature scope.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Phase 15 now has deterministic core, integration, and repository coverage for the axis/legend/probe truth introduced in 15-01 and 15-02.
- Phase-level verification/closeout can run next; renderer and large-data phases can build on a locked correctness baseline instead of re-deriving overlay semantics.

---
*Phase: 15-adaptive-axes-legend-and-probe-readout*
*Completed: 2026-04-14*
