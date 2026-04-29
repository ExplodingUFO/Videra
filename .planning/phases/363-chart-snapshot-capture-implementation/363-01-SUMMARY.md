---
phase: 363-chart-snapshot-capture-implementation
plan: 01
subsystem: ui
tags: [avalonia, render-target-bitmap, png-export, snapshot, chart]

# Dependency graph
requires:
  - phase: 362-plot-snapshot-export-contract
    provides: PlotSnapshotRequest, PlotSnapshotResult, PlotSnapshotManifest, PlotSnapshotDiagnostic contract types
provides:
  - Plot3D.CaptureSnapshotAsync method with validation and PNG export
  - VideraChartView offscreen render bridge via RenderTargetBitmap
  - ImageExport capability marked as supported in diagnostics
  - 11 capture integration tests covering validation, manifest, and diagnostics
affects: [364-demo-smoke-doctor-snapshot-evidence, 365-snapshot-export-guardrails-and-docs]

# Tech tracking
tech-stack:
  added: []
  patterns: [internal render bridge delegate, offscreen RenderTargetBitmap rendering, defense-in-depth validation]

key-files:
  created:
    - tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotCaptureTests.cs
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Rendering.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs
    - tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj

key-decisions:
  - "Used Avalonia Bitmap.Save() for PNG encoding instead of explicit SkiaSharp dependency — Avalonia 11.x bundles SkiaSharp internally and Save() produces PNG by default"
  - "Dimension validation in CaptureSnapshotAsync is defense-in-depth — PlotSnapshotRequest constructor already validates with ThrowIfNegativeOrZero"
  - "Added explicit Avalonia package reference to test project to resolve runtime assembly loading for CaptureSnapshotAsync tests"

patterns-established:
  - "Internal render bridge: Plot3D delegates offscreen rendering to VideraChartView via Func<int,int,double,Task<RenderTargetBitmap>>"
  - "Defense-in-depth validation: CaptureSnapshotAsync validates dimensions even though request constructor already validates"

requirements-completed: [CAP-01, CAP-02, CAP-03, CAP-04, VER-01, VER-02, VER-03]

# Metrics
duration: 15min
completed: 2026-04-29
---

# Phase 363: Chart Snapshot Capture Implementation Summary

**Plot3D.CaptureSnapshotAsync with Avalonia RenderTargetBitmap offscreen rendering, PNG export, validation diagnostics, and 11 integration tests**

## Performance

- **Duration:** 15 min
- **Started:** 2026-04-29T08:34:00Z
- **Completed:** 2026-04-29T08:49:23Z
- **Tasks:** 2
- **Files modified:** 6

## Accomplishments
- Implemented CaptureSnapshotAsync on Plot3D with validation for dimensions, chart readiness, format, and render host availability
- Added offscreen render bridge (RenderOffscreenAsync) to VideraChartView using Avalonia RenderTargetBitmap
- Updated Plot3DOutputCapabilityDiagnostic to report ImageExport as supported
- Created 11 integration tests covering validation paths, manifest determinism, and capability diagnostics

## Task Commits

Each task was committed atomically:

1. **Task 1: Add capture bridge, CaptureSnapshotAsync, and update diagnostics** - `05f3772` (feat)
2. **Task 2: Write capture integration tests** - `712c8bc` (test)

## Files Created/Modified
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs` - Added CaptureSnapshotAsync, SetRenderOffscreen, EncodeAndSavePng, and _renderOffscreen field
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs` - Updated CreateUnsupportedExportDiagnostics to mark ImageExport as supported
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Rendering.cs` - Added RenderOffscreenAsync and RenderToBitmap methods for offscreen rendering
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs` - Wired Plot3D to render bridge via SetRenderOffscreen in constructor
- `tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotCaptureTests.cs` - 11 capture integration tests
- `tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj` - Added Avalonia package reference for runtime assembly resolution

## Decisions Made
- Used Avalonia Bitmap.Save() for PNG encoding instead of explicit SkiaSharp dependency — Avalonia 11.x bundles SkiaSharp internally and Save() produces PNG by default
- Dimension validation in CaptureSnapshotAsync is defense-in-depth — PlotSnapshotRequest constructor already validates with ThrowIfNegativeOrZero
- Added explicit Avalonia package reference to test project to resolve runtime assembly loading for CaptureSnapshotAsync tests

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added Avalonia package reference to test project**
- **Found during:** Task 2 (Write capture integration tests)
- **Issue:** Tests calling CaptureSnapshotAsync failed with FileNotFoundException for Avalonia.Base assembly at runtime
- **Fix:** Added explicit `<PackageReference Include="Avalonia" Version="11.3.9" />` to test project csproj
- **Files modified:** tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj
- **Verification:** All 228 tests pass (11 new + 217 existing)
- **Committed in:** 712c8bc (Task 2 commit)

**2. [Rule 1 - Bug] Fixed code warnings in CaptureSnapshotAsync**
- **Found during:** Task 1 (Add capture bridge implementation)
- **Issue:** Path.GetTempFileName() security warning (S5445) and CA2007 ConfigureAwait warnings
- **Fix:** Changed to Path.GetRandomFileName() and added ConfigureAwait(false)
- **Files modified:** src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs
- **Verification:** Build succeeds with 0 warnings
- **Committed in:** 05f3772 (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (1 blocking, 1 bug)
**Impact on plan:** Both auto-fixes necessary for correctness and test execution. No scope creep.

## Issues Encountered
None — plan executed smoothly after resolving assembly loading issue.

## User Setup Required
None — no external service configuration required.

## Next Phase Readiness
- Capture path complete — Phase 364 can parse snapshot artifacts for demo/smoke evidence
- ImageExport capability now reported as supported — Phase 364 can update consumer smoke
- All contract types from Phase 362 are wired to actual PNG export

---
*Phase: 363-chart-snapshot-capture-implementation*
*Completed: 2026-04-29*
