---
phase: 16-rendering-host-seam-and-gpu-main-path
plan: 01
subsystem: rendering
tags: [surface-charts, rendering, software-fallback, avalonia, gpu-seam]
requires:
  - phase: 13-surfacechart-runtime-and-view-state-contract
    provides: chart-local shell/runtime boundaries that keep surface charts out of the VideraView ownership path
  - phase: 15-adaptive-axes-legend-and-probe-readout
    provides: shared chart projection and overlay behavior that the renderer seam must preserve
provides:
  - chart-local rendering package with typed host inputs, backend kind, and snapshot vocabulary
  - software backend adapter over SurfaceRenderer and SurfaceRenderScene truth
  - SurfaceChartView shell synchronization through SurfaceChartRenderHost instead of direct renderer-owned scene rebuilds
affects: [16-02-gpu-surface-renderer-with-software-fallback-selection, 16-03-incremental-render-state-residency-path-and-host-limit-documentation, 17-large-dataset-residency-cache-evolution-and-optional-rust-spike, 18-demo-docs-and-repository-truth-for-professional-charts]
tech-stack:
  added: []
  patterns: [chart-local render-host seam, typed render snapshot, shared projection settings contract]
key-files:
  created:
    - src/Videra.SurfaceCharts.Rendering/Videra.SurfaceCharts.Rendering.csproj
    - src/Videra.SurfaceCharts.Rendering/SurfaceChartProjectionSettings.cs
    - src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderHost.cs
    - src/Videra.SurfaceCharts.Rendering/Software/SurfaceChartSoftwareRenderBackend.cs
    - tests/Videra.SurfaceCharts.Core.Tests/Rendering/SurfaceChartRenderHostTests.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartRenderHostIntegrationTests.cs
  modified:
    - Videra.slnx
    - src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceCameraController.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartProjection.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs
    - tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceAxisOverlayTests.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartPinnedProbeTests.cs
key-decisions:
  - "Move SurfaceChartProjectionSettings into the rendering package so the render-host seam stays free of Avalonia-only types."
  - "Keep the existing SurfaceRenderScene truth behind a software backend adapter while SurfaceChartView becomes a host-synchronizing shell."
patterns-established:
  - "SurfaceChartView now syncs typed render inputs into SurfaceChartRenderHost and reads a typed RenderSnapshot for backend state."
  - "Projection settings are shared through Videra.SurfaceCharts.Rendering so controls, overlays, and render backends speak one seam."
requirements-completed: []
duration: 9 min
completed: 2026-04-14
---

# Phase 16 Plan 01: Rendering package and render-host contract spike Summary

**Chart-local render-host contracts and a software backend adapter that move SurfaceChartView onto typed renderer orchestration without changing the existing software scene truth**

## Performance

- **Duration:** 9 min
- **Started:** 2026-04-14T18:28:36.2358255+08:00
- **Completed:** 2026-04-14T18:37:33.1060258+08:00
- **Tasks:** 2
- **Files modified:** 20

## Accomplishments
- Added a new `Videra.SurfaceCharts.Rendering` project with typed backend-kind, inputs, projection-settings, snapshot, backend, and host contracts.
- Wrapped the existing `SurfaceRenderer` software path in `SurfaceChartSoftwareRenderBackend` and exposed render-ready state through `SurfaceChartRenderHost`.
- Reworked `SurfaceChartView` into a shell that syncs source, tiles, color map, viewport, projection, and view size into `_renderHost` while keeping existing scene-painter and overlay behavior green.

## Task Commits

Each task was committed atomically:

1. **Task 1: 先补 render-host contract 的 failing tests 与 project wiring** - `cfc0912` (`test`)
2. **Task 2: 实现 `SurfaceChartRenderHost` 与 software backend adapter，并让 `SurfaceChartView` 改为 shell 同步模式** - `88816a5` (`feat`)

**Plan metadata:** not committed (`commit_docs: false`; `.planning/` stays local in this checkout).

## Files Created/Modified
- `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderHost.cs` - Chart-local render host that stores typed inputs, snapshot, and software-scene access.
- `src/Videra.SurfaceCharts.Rendering/Software/SurfaceChartSoftwareRenderBackend.cs` - Software adapter that keeps `SurfaceRenderer.BuildScene(...)` as the rendering truth.
- `src/Videra.SurfaceCharts.Rendering/SurfaceChartProjectionSettings.cs` - Shared projection-settings seam moved out of Avalonia-only code.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs` - Control-side rendering now synchronizes inputs into `_renderHost` and draws only host-owned software scenes.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartProjection.cs` - Overlay projection now consumes the shared rendering-package projection settings type.
- `tests/Videra.SurfaceCharts.Core.Tests/Rendering/SurfaceChartRenderHostTests.cs` - Core coverage for host snapshot readiness and software-scene exposure without Avalonia.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartRenderHostIntegrationTests.cs` - Integration coverage for `_renderHost` ownership and control-visible render snapshot behavior.

## Decisions Made
- Promoted `SurfaceChartProjectionSettings` into the rendering package because the render-host seam cannot depend on an internal Avalonia overlay type.
- Kept `_renderScene` only as a control-local mirror for existing painter-oriented integration helpers while moving actual scene construction ownership into `SurfaceChartRenderHost`.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Projection settings had to move out of the Avalonia overlay namespace**
- **Found during:** Task 2 (implement `SurfaceChartRenderHost` and the software backend adapter)
- **Issue:** The planned `SurfaceChartRenderInputs.ProjectionSettings` contract could not live in the new rendering project while `SurfaceChartProjectionSettings` remained an internal Avalonia-only type.
- **Fix:** Added `src/Videra.SurfaceCharts.Rendering/SurfaceChartProjectionSettings.cs` and rewired the Avalonia control, overlay projection, camera controller, and affected integration tests to use the shared rendering-package type.
- **Files modified:** `src/Videra.SurfaceCharts.Rendering/SurfaceChartProjectionSettings.cs`, `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceCameraController.cs`, `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartProjection.cs`, `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs`, `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceAxisOverlayTests.cs`, `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartPinnedProbeTests.cs`
- **Verification:** `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartRenderHostTests|FullyQualifiedName~SurfaceRendererInputTests"`; `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartRenderHostIntegrationTests|FullyQualifiedName~SurfaceScenePainterTests"`
- **Committed in:** `88816a5`

**2. [Rule 1 - Bug] The structural render-host integration test initially created the control off the Avalonia UI thread**
- **Found during:** Task 2 verification
- **Issue:** `SurfaceChartView_UsesRenderHostInsteadOfControlOwnedRendererState` threw `InvalidOperationException: Call from invalid thread`, so the verification failure came from the test harness rather than the renderer seam.
- **Fix:** Wrapped the structural assertions in `AvaloniaHeadlessTestSession.Run(...)`.
- **Files modified:** `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartRenderHostIntegrationTests.cs`
- **Verification:** `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartRenderHostIntegrationTests|FullyQualifiedName~SurfaceScenePainterTests"`
- **Committed in:** `88816a5`

---

**Total deviations:** 2 auto-fixed (1 blocking, 1 bug)
**Impact on plan:** Both changes were required to keep the new seam Avalonia-free and to make the verification evidence trustworthy; no extra feature scope was added.

## Issues Encountered
- The first green integration run surfaced a UI-thread bug in the new structural test; it was fixed immediately and folded into the task-2 verification loop.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- `SurfaceChartRenderHost` and `RenderSnapshot` now give Phase 16 plan 02 a stable place to hang GPU backend selection and explicit fallback truth.
- `SurfaceRendererInputTests` and `SurfaceScenePainterTests` still pass, so the software path truth is preserved under the new ownership seam.
- Phase-level requirements `REND-01` and `REND-02` remain pending until the GPU backend and later incremental renderer-state work land in plans `16-02` and `16-03`.

## Self-Check: PASSED
- Found `.planning/phases/16-rendering-host-seam-and-gpu-main-path/16-01-SUMMARY.md`
- Found `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderHost.cs`
- Found `src/Videra.SurfaceCharts.Rendering/Software/SurfaceChartSoftwareRenderBackend.cs`
- Found commit `cfc0912`
- Found commit `88816a5`

---
*Phase: 16-rendering-host-seam-and-gpu-main-path*
*Completed: 2026-04-14*
