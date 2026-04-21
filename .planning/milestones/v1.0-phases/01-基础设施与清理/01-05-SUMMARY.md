---
phase: 01-基础设施与清理
plan: 05
subsystem: testing, integration
tags: [xunit, fluentassertions, software-renderer, object3d, renderstyleservice]

# Dependency graph
requires:
  - phase: 01-01
    provides: "Videra.Core.IntegrationTests project and test infrastructure"
  - phase: 01-04
    provides: "Clean core/platform logging baseline for test execution"
provides:
  - "SoftwareBackend integration tests covering initialization, resource usage, frame cycle, and resize"
  - "Object3D integration tests using real SoftwareResourceFactory and wireframe setup"
  - "RenderStyleService integration tests covering preset application and StyleChanged event flow"
affects: [testing, rendering, software-backend, styles]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Integration tests access internal software renderer types through InternalsVisibleTo on Videra.Core"
    - "Software renderer tests exercise real buffers, pipelines, and command execution without GPU dependencies"

key-files:
  created:
    - tests/Videra.Core.IntegrationTests/Rendering/SoftwareBackendIntegrationTests.cs
    - tests/Videra.Core.IntegrationTests/Rendering/Object3DIntegrationTests.cs
    - tests/Videra.Core.IntegrationTests/Styles/StyleEventIntegrationTests.cs
  modified:
    - src/Videra.Core/Videra.Core.csproj
    - tests/Videra.Core.IntegrationTests/PlaceholderIntegrationTest.cs

key-decisions:
  - "Used the CPU SoftwareBackend directly instead of engine-level mocks so integration tests validate real buffer/pipeline interactions"
  - "Granted InternalsVisibleTo to Videra.Core.IntegrationTests so tests can verify internal software renderer components without changing production visibility"
  - "Style event test switches from the default Realistic preset to Tech first, because applying the default preset does not emit a StyleChanged event"

patterns-established:
  - "Rendering integration tests should prefer SoftwareBackend and SoftwareResourceFactory for deterministic no-GPU coverage"
  - "When internal renderer components need verification, expose them to test assemblies with InternalsVisibleTo rather than widening runtime API surface"

requirements-completed: [TEST-03]

# Metrics
duration: 10min
completed: 2026-03-28
---

# Phase 1 Plan 05: Rendering Pipeline Integration Tests Summary

Software backend integration coverage for render lifecycle, Object3D resource wiring, and RenderStyleService event propagation without GPU dependencies.

## Performance

- **Duration:** 10 min
- **Started:** 2026-03-28T09:27:35Z
- **Completed:** 2026-03-28T09:37:35Z
- **Tasks:** 1
- **Files modified:** 5

## Accomplishments

- Added end-to-end `SoftwareBackend` tests for initialization, real resource creation, draw execution, frame lifecycle, and resize.
- Added `Object3D` integration tests using a real `SoftwareResourceFactory`, real buffers, uniform updates, and wireframe initialization.
- Added `RenderStyleService` integration tests for preset transitions, `StyleChanged` events, and custom parameter updates, while removing the placeholder integration test.

## Task Commits

Each task was committed atomically:

1. **Task 1: Create integration tests for rendering pipeline** - `20a4c34` (feat)

## Files Created/Modified

- `tests/Videra.Core.IntegrationTests/Rendering/SoftwareBackendIntegrationTests.cs` - Verifies software backend lifecycle and real command execution.
- `tests/Videra.Core.IntegrationTests/Rendering/Object3DIntegrationTests.cs` - Verifies `Object3D` initializes real software buffers and wireframe resources.
- `tests/Videra.Core.IntegrationTests/Styles/StyleEventIntegrationTests.cs` - Verifies style preset changes and event propagation.
- `src/Videra.Core/Videra.Core.csproj` - Adds `InternalsVisibleTo` entries for test assemblies so internal software renderer types can be exercised directly.
- `tests/Videra.Core.IntegrationTests/PlaceholderIntegrationTest.cs` - Removed obsolete placeholder coverage.

## Decisions Made

- Used direct `SoftwareBackend`, `SoftwareCommandExecutor`, and `SoftwareResourceFactory` instances to keep the tests integration-focused but GPU-free.
- Kept verification at the integration test project and solution-filter levels; the solution command passes even though non-target test assemblies report no matches.
- Validated style events using a non-default preset transition because the service starts in `RenderStylePreset.Realistic`.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking Issue] Exposed internal software renderer types to the integration test assembly**
- **Found during:** Task 1 (integration test implementation)
- **Issue:** `SoftwareBackend`, `SoftwareCommandExecutor`, `SoftwareResourceFactory`, `SoftwareBuffer`, and `SoftwareFrameBuffer` are internal, so the planned direct integration tests could not compile from `Videra.Core.IntegrationTests`.
- **Fix:** Added `InternalsVisibleTo` entries for `Videra.Core.Tests` and `Videra.Core.IntegrationTests` in `src/Videra.Core/Videra.Core.csproj`.
- **Files modified:** `src/Videra.Core/Videra.Core.csproj`
- **Verification:** `dotnet test F:/CodeProjects/DotnetCore/Videra/tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj --no-restore`
- **Committed in:** `20a4c34` (part of task commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Required to execute the planned real-component integration tests without widening production runtime APIs.

## Issues Encountered

- The initial style-event assertion used the default preset and observed no event; switching to a non-default preset matched the service's actual behavior and preserved the integration intent.
- `dotnet test Videra.slnx --filter "FullyQualifiedName~Videra.Core.IntegrationTests" --no-restore` succeeds, but emits expected "no test matches" messages for other test assemblies included in the solution.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Phase 1 now has both unit and integration coverage for the core software rendering path.
- Coverage-report work in `01-03` can build on these integration tests if that plan is revisited for final phase completeness.

## Self-Check: PASSED

- Summary file and all three integration test files verified present on disk.
- Task commit `20a4c34` verified in git log.

---
*Phase: 01-基础设施与清理*
*Completed: 2026-03-28*
