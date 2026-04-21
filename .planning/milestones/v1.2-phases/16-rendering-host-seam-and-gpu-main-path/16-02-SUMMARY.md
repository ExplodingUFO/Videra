---
phase: 16-rendering-host-seam-and-gpu-main-path
plan: 02
subsystem: rendering
tags: [surface-charts, gpu, rendering, avalonia, native-host, fallback]
requires:
  - phase: 16-rendering-host-seam-and-gpu-main-path
    provides: render-host seam, typed render inputs/snapshot vocabulary, and a software backend adapter from plan 16-01
provides:
  - chart-local GPU backend over Videra graphics abstractions with software fallback selection
  - chart-local native host shell and transparent overlay container on SurfaceChartView
  - explicit rendering status truth for active backend, fallback state, reason, and native-surface usage
affects: [16-03-incremental-render-state-residency-path-and-host-limit-documentation, 17-large-dataset-residency-cache-evolution-and-optional-rust-spike, 18-demo-docs-and-repository-truth-for-professional-charts]
tech-stack:
  added: []
  patterns: [chart-local gpu backend, explicit fallback truth, chart-local native-host shell, control-visible rendering status]
key-files:
  created:
    - src/Videra.SurfaceCharts.Rendering/SurfaceChartGpuRenderBackend.cs
    - src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderingStatus.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/ISurfaceChartNativeHost.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/ISurfaceChartNativeHostFactory.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/DefaultSurfaceChartNativeHostFactory.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartNativeHost.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartLinuxNativeHost.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartMacOSNativeHost.cs
  modified:
    - src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderHost.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs
    - tests/Videra.SurfaceCharts.Core.Tests/Rendering/SurfaceChartGpuFallbackTests.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartViewGpuFallbackTests.cs
key-decisions:
  - "Keep the GPU path chart-local by feeding SurfaceRenderer-derived tile geometry directly into Videra.Core graphics buffers instead of involving VideraView or RenderSession."
  - "Project fallback truth as a public RenderingStatus on SurfaceChartView while keeping the native host contract internal and chart-specific."
patterns-established:
  - "SurfaceChartRenderHost now prefers GPU when a native handle and GPU backend are available, then falls back to software with an explicit reason."
  - "SurfaceChartView owns a chart-local native host plus overlay shell and exposes RenderStatusChanged for backend/fallback transitions."
requirements-completed: [REND-01]
duration: 16 min
completed: 2026-04-14
---

# Phase 16 Plan 02: GPU surface renderer with software fallback selection Summary

**Chart-local GPU rendering through Videra graphics abstractions, paired with a native-host shell and explicit software-fallback status on SurfaceChartView**

## Performance

- **Duration:** 16 min
- **Started:** 2026-04-14T10:41:53.6933988Z
- **Completed:** 2026-04-14T10:58:03.6020258Z
- **Tasks:** 2
- **Files modified:** 14

## Accomplishments
- Added `SurfaceChartGpuRenderBackend` and extended `SurfaceChartRenderHost` so charts prefer GPU on a bound native handle, then fall back to software with an explicit reason.
- Added chart-local native-host contracts and platform hosts so `SurfaceChartView` can present through a native surface without borrowing `VideraView` or `IVideraNativeHost`.
- Exposed readonly `RenderingStatus` plus `RenderStatusChanged`, and kept overlays chart-local in software fallback scenarios.

## Task Commits

Each task was committed atomically:

1. **Task 1: 先补 GPU backend selection / fallback truth 的 failing tests** - `eb31164` (`test`)
2. **Task 2: 实现 chart-local GPU backend、native host shell 与 rendering status** - `5849395` (`feat`)

**Plan metadata:** not committed (`commit_docs: false`; `.planning/` stays local in this checkout).

## Files Created/Modified
- `src/Videra.SurfaceCharts.Rendering/SurfaceChartGpuRenderBackend.cs` - GPU backend that builds chart-local buffers from surface render tiles and drives the graphics backend directly.
- `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderHost.cs` - GPU-first backend selection with explicit software fallback truth and status projection.
- `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderingStatus.cs` - Public readonly status record for active backend, fallback state, reason, native-surface use, and readiness.
- `src/Videra.SurfaceCharts.Avalonia/Controls/ISurfaceChartNativeHost.cs` - Chart-local native-host contract kept separate from `IVideraNativeHost`.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.cs` - Control shell now owns render-host injection, public status projection, and a chart-local host container.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs` - Native-host acquisition/release, render-host synchronization, and overlay-layer rendering for GPU/software modes.
- `tests/Videra.SurfaceCharts.Core.Tests/Rendering/SurfaceChartGpuFallbackTests.cs` - Core coverage for GPU selection, software fallback, and fallback-reason truth.
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartViewGpuFallbackTests.cs` - Integration coverage for native-host activation, control-visible status, and overlay continuity under software fallback.

## Decisions Made
- Reused `SurfaceRenderer` as the truth source for tile geometry while uploading chart-local GPU buffers, so sample/value mapping stays aligned with the software path.
- Kept the new native-host contract internal to `Videra.SurfaceCharts.Avalonia`; the shipped public truth is `RenderingStatus`, not the host-shell plumbing.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Running the core and Avalonia verification commands in parallel produced a transient `Videra.Core.dll` build lock; rerunning the integration suite sequentially resolved it cleanly and did not require code changes.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Phase `16-03` can now hang incremental render-state/residency work off a real GPU/software backend split instead of a software-only seam.
- README/demo/repository truth updates can target the new `RenderingStatus` and chart-local host-shell vocabulary directly.

## Self-Check

PASSED

- Found `.planning/phases/16-rendering-host-seam-and-gpu-main-path/16-02-SUMMARY.md`
- Found `src/Videra.SurfaceCharts.Rendering/SurfaceChartGpuRenderBackend.cs`
- Found `src/Videra.SurfaceCharts.Avalonia/Controls/ISurfaceChartNativeHost.cs`
- Found `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderingStatus.cs`
- Found commit `eb31164`
- Found commit `5849395`

---
*Phase: 16-rendering-host-seam-and-gpu-main-path*
*Completed: 2026-04-14*
