---
phase: 16-rendering-host-seam-and-gpu-main-path
plan: 03
subsystem: rendering
tags: [surface-charts, rendering, residency, gpu, docs]
requires:
  - phase: 16-rendering-host-seam-and-gpu-main-path
    provides: chart-local GPU/software backend selection, native-host shell, and control-visible rendering status from plan 16-02
provides:
  - resident render-state keyed by tile with explicit full-reset, residency, color, and projection dirty buckets
  - shared dirty-truth contract consumed by both software and GPU render backends
  - README/demo/repository truth for GPU-first rendering, software fallback, and Linux XWayland host limits
affects: [17-large-dataset-residency-cache-evolution-and-optional-rust-spike, 18-demo-docs-and-repository-truth-for-professional-charts]
tech-stack:
  added: []
  patterns: [resident-tile render state, shared render change-set contract, renderer truth guarded by docs and repository tests]
key-files:
  created:
    - src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderState.cs
    - src/Videra.SurfaceCharts.Rendering/SurfaceChartResidentTile.cs
    - src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderChangeSet.cs
    - tests/Videra.SurfaceCharts.Core.Tests/Rendering/SurfaceChartRenderStateTests.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartIncrementalRenderingTests.cs
  modified:
    - src/Videra.SurfaceCharts.Rendering/ISurfaceChartRenderBackend.cs
    - src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderHost.cs
    - src/Videra.SurfaceCharts.Rendering/SurfaceChartGpuRenderBackend.cs
    - src/Videra.SurfaceCharts.Rendering/Software/SurfaceChartSoftwareRenderBackend.cs
    - src/Videra.SurfaceCharts.Avalonia/README.md
    - samples/Videra.SurfaceCharts.Demo/README.md
    - tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs
key-decisions:
  - "Keep resident-tile geometry/sample positions, colors, and residency membership in a render-state object owned by the render host."
  - "Pass a shared change-set contract into both GPU and software backends so dirty semantics stay aligned."
  - "Freeze Linux chart-host truth as X11/XWayland compatibility rather than implying compositor-native Wayland embedding."
patterns-established:
  - "Projection-only view changes reuse resident geometry and only mark the projection path dirty."
  - "Color-map changes recolor resident tiles without rebuilding sample-position mapping."
  - "README/demo truth and repository guards now lock the chart-local renderer seam, GPU-first path, software fallback, and Linux host limit wording together."
requirements-completed: [REND-02]
duration: 10 min
completed: 2026-04-14
---

# Phase 16 Plan 03: Incremental render-state, residency path, and renderer truth Summary

**Resident tile render-state with shared dirty change sets, projection-only updates, and guarded GPU-first/software-fallback documentation truth**

## Performance

- **Duration:** 10 min
- **Started:** 2026-04-14T11:05:06Z
- **Completed:** 2026-04-14T11:15:24Z
- **Tasks:** 3
- **Files modified:** 12

## Accomplishments
- Added `SurfaceChartRenderState`, `SurfaceChartResidentTile`, and `SurfaceChartRenderChangeSet` so the renderer distinguishes full reset, residency, color, and projection dirtiness explicitly.
- Moved both the software and GPU backends onto the same render-state/change-set contract, which stops viewport-only updates from rebuilding resident software scenes and keeps GPU resource updates aligned with the same dirty truth.
- Updated the Avalonia/demo README text and repository guards so the shipped chart-local renderer seam, `GPU-first` path, `software fallback`, and Linux `XWayland compatibility` limit are described consistently.

## Task Commits

Each task was committed atomically:

1. **Task 1: 先补 incremental render-state 的 failing tests** - `d1843f9` (`test`)
2. **Task 2: 实现 resident render-state / change-set，并让 renderer 按 dirty boundary 增量更新** - `fff5373` (`feat`)
3. **Task 3: 更新 README/demo truth，并用 repository guards 固定 renderer/fallback 边界** - `ff60d13` (`docs`)

**Plan metadata:** not committed (`commit_docs: false`; `.planning/` stays local in this checkout).

## Files Created/Modified
- `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderState.cs` - Host-owned resident render-state and dirty-boundary computation.
- `src/Videra.SurfaceCharts.Rendering/SurfaceChartResidentTile.cs` - Resident tile model with explicit geometry, sample positions, sample values, and colors.
- `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderChangeSet.cs` - Shared delta contract for full reset, residency, color, and projection updates.
- `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderHost.cs` - Host now computes one shared change set before routing to GPU or software rendering.
- `src/Videra.SurfaceCharts.Rendering/Software/SurfaceChartSoftwareRenderBackend.cs` - Software scene rebuilds only when residency/color/full-reset changes require it.
- `src/Videra.SurfaceCharts.Rendering/SurfaceChartGpuRenderBackend.cs` - GPU resource updates now follow the same dirty buckets as the software path.
- `tests/Videra.SurfaceCharts.Core.Tests/Rendering/SurfaceChartRenderStateTests.cs` - Regression coverage for viewport, color-map, residency, and full-reset boundaries.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartIncrementalRenderingTests.cs` - Regression coverage for repeated viewport updates, resident deltas, and shared backend dirty truth.
- `src/Videra.SurfaceCharts.Avalonia/README.md` - Updated renderer/fallback/host-limit truth for the shipped control.
- `samples/Videra.SurfaceCharts.Demo/README.md` - Updated demo truth for the shipped renderer path and current host-coverage limits.
- `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs` - Repository guard for chart-renderer sibling boundaries and exact doc truth.

## Decisions Made
- The render host now owns a resident-tile state instead of treating `LoadedTiles` as a full-scene snapshot on every update.
- Backend dirtiness is expressed once through `SurfaceChartRenderChangeSet`; GPU and software renderers consume the same buckets instead of maintaining backend-specific semantics.
- Linux chart-host documentation now states the concrete shipped limit: X11 handles and XWayland compatibility are supported, compositor-native Wayland embedding is not.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Extended the shared backend contract so GPU and software consume the same dirty truth**
- **Found during:** Task 2 (实现 resident render-state / change-set，并让 renderer 按 dirty boundary 增量更新)
- **Issue:** The plan's file list focused on host/software state work, but leaving `ISurfaceChartRenderBackend` and `SurfaceChartGpuRenderBackend` unchanged would have created a second backend-specific dirty model.
- **Fix:** Updated the backend interface to accept `SurfaceChartRenderState` plus `SurfaceChartRenderChangeSet`, then refactored the GPU backend to apply full-reset, residency, color, and projection changes through that shared contract.
- **Files modified:** `src/Videra.SurfaceCharts.Rendering/ISurfaceChartRenderBackend.cs`, `src/Videra.SurfaceCharts.Rendering/SurfaceChartGpuRenderBackend.cs`, `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderHost.cs`
- **Verification:** `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartRenderStateTests|FullyQualifiedName~SurfaceChartGpuFallbackTests|FullyQualifiedName~SurfaceRendererInputTests"` and `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartIncrementalRenderingTests|FullyQualifiedName~SurfaceChartViewGpuFallbackTests|FullyQualifiedName~SurfaceChartRenderHostIntegrationTests"`
- **Committed in:** `fff5373`

---

**Total deviations:** 1 auto-fixed (1 missing critical)
**Impact on plan:** The extra interface/GPU changes were required to satisfy the plan's "same dirty-truth model" requirement and did not expand into Phase 17 scheduler scope.

## Issues Encountered
- Running the core and Avalonia verification suites in parallel caused a transient `Videra.SurfaceCharts.Rendering.dll` build lock; rerunning the three verification commands sequentially resolved it cleanly.
- Parallel `git add` calls intermittently left a stale `.git/index.lock`; switching the staging step back to a single git process resolved it without affecting repository content.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Phase 17 can build scheduler/residency evolution on top of an explicit renderer-state boundary instead of introducing a second dirty model.
- The shipped renderer truth is now locked in code, docs, and repository tests, so future demo/doc work has a stable baseline for backend/fallback wording.
- `REND-02` is now satisfied locally for this checkout, while `REND-03` remains correctly deferred to Phase 17.

## Self-Check

PASSED

- Found `.planning/phases/16-rendering-host-seam-and-gpu-main-path/16-03-SUMMARY.md`
- Found commit `d1843f9`
- Found commit `fff5373`
- Found commit `ff60d13`

---
*Phase: 16-rendering-host-seam-and-gpu-main-path*
*Completed: 2026-04-14*
