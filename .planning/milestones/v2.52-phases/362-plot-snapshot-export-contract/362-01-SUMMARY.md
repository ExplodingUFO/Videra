---
phase: 362-plot-snapshot-export-contract
plan: 01
subsystem: ui
tags: [avalonia, chart, snapshot, export, contract, png, evidence]

# Dependency graph
requires:
  - phase: 361-chart-snapshot-export-inventory
    provides: "Gap analysis, target examples, and non-goals for chart-local snapshot export"
provides:
  - "PlotSnapshotRequest contract type with public ctor and validation"
  - "PlotSnapshotResult with Success/Failed factories and invariant enforcement"
  - "PlotSnapshotManifest with 8 deterministic metadata properties"
  - "PlotSnapshotDiagnostic for explicit error reporting"
  - "PlotSnapshotFormat enum (Png)"
  - "PlotSnapshotBackground enum (Transparent, Opaque)"
  - "20 unit tests covering construction, validation, and evidence linkage"
affects: [363-chart-snapshot-capture-implementation, 364-demo-smoke-doctor-snapshot-evidence]

# Tech tracking
tech-stack:
  added: []
  patterns: [internal-ctor-validation, sealed-class-contract, static-factory-result, evidence-kind-linkage]

key-files:
  created:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotFormat.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotBackground.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotDiagnostic.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotRequest.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotResult.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotManifest.cs
    - tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotContractTests.cs
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj
    - tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj

key-decisions:
  - "PlotSnapshotRequest uses public ctor (user-facing API) while other types use internal ctors (factory pattern)"
  - "PlotSnapshotResult enforces success/failure invariants at construction time — success requires path+manifest, failure requires diagnostic"
  - "Added InternalsVisibleTo for Videra.SurfaceCharts.Core.Tests to access internal constructors"

patterns-established:
  - "Snapshot contract pattern: public request → internal result/manifest/diagnostic with validation"
  - "Evidence kind linkage: manifest references Plot3DOutputEvidence and Plot3DDatasetEvidence by kind string"

requirements-completed: [SNAP-01, SNAP-02, SNAP-03, SNAP-04, VER-01, VER-02, VER-03]

# Metrics
duration: 5min
completed: 2026-04-29
---

# Phase 362: Plot Snapshot Export Contract Summary

**Six Plot-owned snapshot contract types (request/result/manifest/diagnostic/format/background) with 20 unit tests validating construction, validation, and evidence kind linkage**

## Performance

- **Duration:** 5 min
- **Started:** 2026-04-29T16:02:40Z
- **Completed:** 2026-04-29T16:08:00Z
- **Tasks:** 2
- **Files modified:** 9

## Accomplishments
- Created PlotSnapshotRequest with public ctor and validation (width, height, scale, background, format)
- Created PlotSnapshotResult with static Success/Failed factories enforcing invariant constraints
- Created PlotSnapshotManifest with 8 deterministic metadata properties linked to Plot3D evidence conventions
- Created PlotSnapshotDiagnostic for explicit error reporting with diagnostic code
- Added 20 unit tests covering request validation, result factory behavior, manifest construction, and evidence kind linkage

## Task Commits

Each task was committed atomically:

1. **Task 1: Create snapshot contract types** - `925ed74` (feat)
2. **Task 2: Write contract unit tests** - `791beba` (test)

## Files Created/Modified
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotFormat.cs` - Format enum with Png value
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotBackground.cs` - Background enum with Transparent and Opaque
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotDiagnostic.cs` - Diagnostic type with code and message
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotRequest.cs` - Request type with public ctor and validation
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotResult.cs` - Result type with Success/Failed factories
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshotManifest.cs` - Manifest with 8 deterministic metadata properties
- `tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshotContractTests.cs` - 20 unit tests
- `src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj` - Added InternalsVisibleTo for test project
- `tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj` - Added ProjectReference to Avalonia

## Decisions Made
- PlotSnapshotRequest uses public ctor (user-facing API per SNAP-01) while PlotSnapshotResult, PlotSnapshotManifest, and PlotSnapshotDiagnostic use internal ctors following the Plot3DOutputEvidence pattern
- PlotSnapshotResult enforces success/failure invariants at construction time: success requires non-null path and manifest, failure requires non-null diagnostic
- Added InternalsVisibleTo for Videra.SurfaceCharts.Core.Tests to enable testing of internal constructors

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added InternalsVisibleTo for test project access**
- **Found during:** Task 2 (Write contract unit tests)
- **Issue:** PlotSnapshotManifest, PlotSnapshotResult, and PlotSnapshotDiagnostic have internal constructors inaccessible from the test project
- **Fix:** Added `InternalsVisibleTo` attribute for `Videra.SurfaceCharts.Core.Tests` in the Avalonia csproj
- **Files modified:** `src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj`
- **Verification:** Tests compile and pass with internal constructor access
- **Committed in:** `925ed74` (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Necessary for test project to access internal constructors. No scope creep.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Known Stubs
None - all contract types are fully implemented with validation and properties.

## Next Phase Readiness
- Contract types are ready for Phase 363 to implement `CaptureSnapshotAsync` on Plot3D
- PlotSnapshotRequest/Result/Manifest are the API surface Phase 363 will consume
- Phase 364 can reference PlotSnapshotManifest for Doctor parsing and demo support evidence

---
*Phase: 362-plot-snapshot-export-contract*
*Completed: 2026-04-29*

## Self-Check: PASSED

All files verified to exist on disk. All commits verified in git history.
- 9 files found: 6 contract types, 1 test file, 1 verification doc, 1 summary
- 2 commits found: `925ed74` (feat), `791beba` (test)
