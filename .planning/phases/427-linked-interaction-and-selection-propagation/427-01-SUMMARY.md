---
phase: 427-linked-interaction-and-selection-propagation
plan: 01
subsystem: ui
tags: [avalonia, surfacecharts, linked-views, pairwise-sync]

# Dependency graph
requires:
  - phase: 426
    provides: "SurfaceChartLinkGroup with FullViewState policy and NotSupportedException stubs for CameraOnly/AxisOnly"
provides:
  - "CameraOnly pairwise link class synchronizing only Camera field"
  - "AxisOnly pairwise link class synchronizing only DataWindow field"
  - "SurfaceChartLinkGroup accepting all three policies (FullViewState, CameraOnly, AxisOnly)"
  - "Policy property on SurfaceChartLinkGroup for evidence reporting"
affects: [427-02]

# Tech tracking
tech-stack:
  added: []
  patterns: [filtered-pairwise-viewstate-link, policy-switch-link-creation]

key-files:
  created:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/VideraChartViewCameraOnlyLink.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/VideraChartViewAxisOnlyLink.cs
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartLinkGroup.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Workspace/SurfaceChartLinkGroupTests.cs

key-decisions:
  - "Used same _isSynchronizing re-entrancy guard pattern as VideraChartViewLink for both filtered link classes"
  - "Filtered links also check equality of the copied field before setting _isSynchronizing, avoiding unnecessary PropertyChanged cycles"

patterns-established:
  - "Filtered pairwise link: internal sealed class implementing IDisposable, subscribes to ViewStateProperty on both charts, copies only the relevant field subset"

requirements-completed: [LINK-01]

# Metrics
duration: 4min
completed: 2026-04-30
---

# Phase 427 Plan 01: CameraOnly and AxisOnly Link Policies Summary

**Filtered pairwise ViewState synchronization: CameraOnly copies only Camera pose, AxisOnly copies only DataWindow, both using proven re-entrancy guard from VideraChartViewLink**

## Performance

- **Duration:** 4 min
- **Started:** 2026-04-30T14:11:00Z
- **Completed:** 2026-04-30T14:14:34Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- Created VideraChartViewCameraOnlyLink and VideraChartViewAxisOnlyLink filtered link classes
- Updated SurfaceChartLinkGroup to accept all three policies without throwing
- Added functional tests proving filtered synchronization (CameraOnly syncs Camera only, AxisOnly syncs DataWindow only)
- Added isolation tests proving no cross-field sync (CameraOnly does not sync DataWindow, AxisOnly does not sync Camera)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create filtered pairwise link classes** - `d5619fb` (feat)
2. **Task 2: Update SurfaceChartLinkGroup to support all three policies** - `09fa192` (feat)

## Files Created/Modified
- `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/VideraChartViewCameraOnlyLink.cs` - Internal sealed class synchronizing only Camera across two linked charts
- `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/VideraChartViewAxisOnlyLink.cs` - Internal sealed class synchronizing only DataWindow across two linked charts
- `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartLinkGroup.cs` - Removed NotSupportedException guard, added switch for policy-appropriate link creation, added Policy property
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Workspace/SurfaceChartLinkGroupTests.cs` - Replaced stub tests with functional CameraOnly/AxisOnly tests, added isolation and Policy property tests

## Decisions Made
- Used same _isSentrancy re-entrancy guard pattern as VideraChartViewLink for both filtered link classes (proven pattern, no new risk)
- Filtered links also check equality of the copied field before setting _isSynchronizing, avoiding unnecessary PropertyChanged cycles

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- CameraOnly and AxisOnly link policies are fully functional
- Plan 427-02 can now use the Policy property for evidence reporting
- All 11 link group tests pass (5 existing FullViewState + 6 new CameraOnly/AxisOnly tests)

## Self-Check: PASSED

- All created files verified on disk
- All commit hashes verified in git log
- No accidental file deletions
- No untracked generated files

---
*Phase: 427-linked-interaction-and-selection-propagation*
*Completed: 2026-04-30*
