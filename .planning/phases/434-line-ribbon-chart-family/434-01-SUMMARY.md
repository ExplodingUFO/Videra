---
phase: 434-line-ribbon-chart-family
plan: 01
subsystem: surfacecharts-core
tags: [line-chart, ribbon-chart, 3d-rendering, probe-strategy, polyline, tube-geometry]

# Dependency graph
requires:
  - phase: 432-chart-type-inventory-and-api-design
    provides: API design contracts and pattern map for Line/Ribbon chart family
  - phase: 433-bar-contour-promotion
    provides: Production-ready Bar/Contour pattern baseline for new chart types
provides:
  - LineChartData/LineSeries sealed immutable data model with ScatterPoint arrays
  - RibbonChartData/RibbonSeries sealed immutable data model with tube radius
  - LineRenderer/LineRenderScene with LineRenderSegment per adjacent point pair
  - RibbonRenderer/RibbonRenderScene with RibbonRenderSegment with 8-sided tube cross-section
  - LineProbeStrategy with point-to-segment distance probing and Y interpolation
  - RibbonProbeStrategy with radius-inclusive snap distance for tube probing
affects:
  - 434-02 (Avalonia integration layer wiring Plot3DSeries, Plot3DAddApi, overlays)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Polyline-to-segment renderer pattern: adjacent point pair iteration producing record structs"
    - "Point-to-segment distance probing with Y interpolation for line/ribbon chart types"
    - "Radius-inclusive snap distance for tube geometry probe strategies"

key-files:
  created:
    - src/Videra.SurfaceCharts.Core/LineChartData.cs
    - src/Videra.SurfaceCharts.Core/RibbonChartData.cs
    - src/Videra.SurfaceCharts.Core/Rendering/LineRenderer.cs
    - src/Videra.SurfaceCharts.Core/Rendering/LineRenderScene.cs
    - src/Videra.SurfaceCharts.Core/Rendering/RibbonRenderer.cs
    - src/Videra.SurfaceCharts.Core/Rendering/RibbonRenderScene.cs
    - src/Videra.SurfaceCharts.Core/Picking/LineProbeStrategy.cs
    - src/Videra.SurfaceCharts.Core/Picking/RibbonProbeStrategy.cs
  modified: []

key-decisions:
  - "LineRenderSegment includes Width property for per-series line width rendering"
  - "RibbonRenderSegment includes Sides property (default 8) for configurable tube cross-section geometry"
  - "RibbonProbeStrategy effective snap radius = base snap radius + segment Radius for tube-aware probing"

patterns-established:
  - "Polyline data model pattern: sealed class with ReadOnlyCollection<ScatterPoint> and ScatterChartMetadata"
  - "Segment renderer pattern: static class with BuildScene producing segment-per-adjacent-pair topology"
  - "Line-based probe strategy pattern: point-to-segment distance with ProjectOntoSegment Y interpolation"

requirements-completed: [LINE-01, LINE-02, LINE-03]

# Metrics
duration: 3min
completed: 2026-04-30
---

# Phase 434 Plan 01: Line/Ribbon Core Infrastructure Summary

**Polyline segment renderers, tube geometry, and point-to-segment probe strategies for Line and Ribbon 3D chart types**

## Performance

- **Duration:** 3 min
- **Started:** 2026-04-30T17:08:49Z
- **Completed:** 2026-04-30T17:11:59Z
- **Tasks:** 2
- **Files created:** 8

## Accomplishments

- Created sealed immutable LineChartData/LineSeries and RibbonChartData/RibbonSeries data models following the Bar/Contour pattern with ReadOnlyCollection backing and constructor validation
- Built stateless LineRenderer and RibbonRenderer that produce render scenes with one segment per adjacent point pair topology
- Implemented LineProbeStrategy and RibbonProbeStrategy with point-to-segment distance probing, cursor projection, and Y value interpolation; ribbon probe accounts for tube radius in snap distance

## Task Commits

Each task was committed atomically:

1. **Task 1: Create Line and Ribbon data models** - `73d78f6` (feat)
2. **Task 2: Create Line and Ribbon renderers, render scenes, and probe strategies** - `db7e6a1` (feat)

## Files Created/Modified

- `src/Videra.SurfaceCharts.Core/LineChartData.cs` - Sealed LineChartData and LineSeries with ScatterPoint arrays, ReadOnlyCollection backing, ScatterChartMetadata
- `src/Videra.SurfaceCharts.Core/RibbonChartData.cs` - Sealed RibbonChartData and RibbonSeries with ScatterPoint arrays, positive radius validation
- `src/Videra.SurfaceCharts.Core/Rendering/LineRenderScene.cs` - LineRenderSegment record struct (Start/End/Width/Color) and LineRenderScene sealed class
- `src/Videra.SurfaceCharts.Core/Rendering/LineRenderer.cs` - Static LineRenderer with BuildScene producing segments per adjacent point pair
- `src/Videra.SurfaceCharts.Core/Rendering/RibbonRenderScene.cs` - RibbonRenderSegment record struct (Start/End/Radius/Sides/Color) and RibbonRenderScene sealed class
- `src/Videra.SurfaceCharts.Core/Rendering/RibbonRenderer.cs` - Static RibbonRenderer with BuildScene producing tube segments with 8-sided cross-section
- `src/Videra.SurfaceCharts.Core/Picking/LineProbeStrategy.cs` - ISeriesProbeStrategy with point-to-segment distance, projection, and Y interpolation
- `src/Videra.SurfaceCharts.Core/Picking/RibbonProbeStrategy.cs` - ISeriesProbeStrategy with radius-inclusive snap distance for tube probing

## Decisions Made

- LineRenderSegment includes Width property to support per-series line width rendering (plan specifies `float Width`)
- RibbonRenderSegment includes Sides property (int, default 8) for configurable tube cross-section geometry
- RibbonProbeStrategy effective snap distance = `_snapRadius + segment.Radius` so tube-aware probing accounts for the physical ribbon extent

## Deviations from Plan

None - plan executed exactly as written.

## Known Stubs

None - all implementations are complete with no placeholders.

## Threat Flags

None - all new files are data models, stateless renderers, and probe strategies with no new network endpoints, auth paths, or trust boundary changes.

## Issues Encountered

- dotnet SDK not available in worktree environment; build verification performed through code inspection and grep-based acceptance criteria instead

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- All 8 Core-layer files for Line and Ribbon chart types are ready
- Plan 02 (434-02) can wire these into Plot3DSeries, Plot3DAddApi, Plot3DSeriesKind enum, overlay infrastructure, and rendering status records
- The segment renderer and probe strategy patterns established here apply directly to future chart families

---
*Phase: 434-line-ribbon-chart-family*
*Completed: 2026-04-30*
