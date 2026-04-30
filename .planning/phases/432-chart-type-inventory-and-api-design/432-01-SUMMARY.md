---
phase: 432-chart-type-inventory-and-api-design
plan: 01
subsystem: surface-charts
tags: [chart-types, api-design, inventory, 3d-rendering, probe-strategy, legend-overlay]

requires:
  - phase: v2.64
    provides: "Clean native VideraChartView + Plot.Add.* route with analysis workspace"
provides:
  - "Complete inventory of current chart type API surface (5 kinds: Surface, Waterfall, Scatter, Bar, Contour)"
  - "Rendering seams documented for each chart kind (renderer, render scene, render element, painter)"
  - "Probe/overlay infrastructure integration points documented (ISeriesProbeStrategy, dispatcher, legend presenter)"
  - "7-step integration pattern for adding new chart kinds"
  - "API contracts for Line, Ribbon, Vector Field, Heatmap Slice, and Box Plot with type signatures"
affects: [433, 434, 435, 436, 437, 438, 439]

tech-stack:
  added: []
  patterns:
    - "7-step chart kind integration pattern (enum, data model, renderer, render scene, series subclass, add-api, probe strategy)"
    - "Sealed class data model with ReadOnlyCollection<T> and constructor validation"
    - "Static renderer with BuildScene() entry point returning immutable render scene"
    - "readonly record struct render elements with Vector3 Position + uint Color"
    - "Sealed class probe strategy implementing ISeriesProbeStrategy"
    - "Sealed record rendering status with HasSource/IsReady/kind-specific fields"

key-files:
  created:
    - ".planning/phases/432-chart-type-inventory-and-api-design/432-INVENTORY.md"
    - ".planning/phases/432-chart-type-inventory-and-api-design/432-API-DESIGN.md"
  modified: []

key-decisions:
  - "Follow per-kind data property slots on Plot3DSeries for type safety (not generic object slot)"
  - "Reuse ScatterChartMetadata and ScatterPoint types for Line and Ribbon data models"
  - "Use SurfaceProbeInfo for all probe strategies (single Value field, not per-kind extensions)"
  - "Use LegendIndicatorKind.Line for Line charts, Swatch for all others"
  - "VectorFieldRenderer maps magnitude to color using range normalization"
  - "HeatmapSlice uses normalized 0..1 position parameter for slice location"

patterns-established:
  - "7-step chart kind integration pattern with specific file paths and integration points"
  - "Per-kind data property pattern on Plot3DSeries base class"
  - "Composition merge pattern via Plot3DSeriesComposition.Create*Data()"
  - "Rendering status update pattern via Create*RenderingStatus() called from Refresh()"
  - "Legend indicator mapping pattern via SurfaceLegendOverlayPresenter.CreateLegendEntry()"

requirements-completed: []

duration: 17min
completed: 2026-04-30
---

# Phase 432 Plan 01: Chart Type Inventory and API Design Summary

**Mapped current chart type API surface (5 kinds) and designed API contracts for Line, Ribbon, Vector Field, Heatmap Slice, and Box Plot chart families with complete type signatures**

## Performance

- **Duration:** 17 min
- **Started:** 2026-04-30T15:53:18Z
- **Completed:** 2026-04-30T16:10:48Z
- **Tasks:** 2
- **Files created:** 2

## Accomplishments
- Inventoried all 5 current chart kinds (Surface, Waterfall, Scatter, Bar, Contour) with exact API signatures, data models, renderer patterns, and probe strategies
- Documented the 7-step integration pattern for adding new chart kinds with specific file paths
- Designed complete API contracts for 5 new chart families: Line, Ribbon, Vector Field, Heatmap Slice, Box Plot
- All contracts follow established patterns: sealed class, ReadOnlyCollection<T>, constructor validation, static renderer, readonly record struct elements

## Task Commits

Each task was committed atomically:

1. **Task 1: Inventory Current Chart Type API Surface and Rendering Seams** - `71f6fb7` (docs)
2. **Task 2: Design API Contracts for New Chart Families** - `ba3eaad` (docs)

## Files Created/Modified
- `.planning/phases/432-chart-type-inventory-and-api-design/432-INVENTORY.md` - Current chart type API surface, rendering seams, probe/overlay infrastructure, 7-step integration pattern, files to modify
- `.planning/phases/432-chart-type-inventory-and-api-design/432-API-DESIGN.md` - API contracts for Line, Ribbon, Vector Field, Heatmap Slice, and Box Plot with complete type signatures

## Decisions Made
- Follow per-kind data property slots on Plot3DSeries for type safety (not generic object slot) -- matches existing pattern
- Reuse ScatterChartMetadata and ScatterPoint types for Line and Ribbon data models -- avoids type duplication
- Use SurfaceProbeInfo for all probe strategies (single Value field) -- consistent probe contract
- Use LegendIndicatorKind.Line for Line charts, Swatch for all others -- matches visual semantics
- VectorFieldRenderer maps magnitude to color using range normalization -- standard approach
- HeatmapSlice uses normalized 0..1 position parameter -- axis-independent slice location

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- 432-INVENTORY.md provides the integration point reference for implementation phases 434-437
- 432-API-DESIGN.md provides type-ready contracts that implementation phases can follow without architectural drift
- Phase 433 (Bar+Contour promotion) can proceed independently
- Phases 434-437 (new chart kind implementation) can proceed in parallel after 433

## Self-Check: PASSED

All created files verified present. All task commits verified in git log.

---
*Phase: 432-chart-type-inventory-and-api-design*
*Completed: 2026-04-30*
