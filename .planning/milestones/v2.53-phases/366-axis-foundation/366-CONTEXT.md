# Phase 366: Axis Foundation - Context

**Gathered:** 2026-04-29
**Status:** Ready for planning
**Bead:** v2.53-axis-foundation

<domain>
## Phase Boundary

Chart axes support log scale, DateTime, and custom tick formatting — unblocking non-linear axis use across all chart types. This is the foundation phase: everything else depends on axis semantics being correct.

Key research finding: Most infrastructure already exists in skeleton form. `SurfaceAxisScaleKind` enum defines `Log`, `DateTime`, `ExplicitCoordinates`. `SurfaceGeometryGrid.MapNormalizedCoordinate` already has log math. `SurfaceChartOverlayOptions.LabelFormatter` already exists as `Func<string, double, string>?`.

</domain>

<decisions>
## Implementation Decisions

### Axis Foundation
- Log scale: Unblock `SurfaceAxisScaleKind.Log` by removing the throw in `SurfaceAxisDescriptor` constructor, implement log tick generation (powers of 10)
- DateTime axis: Implement `SurfaceAxisScaleKind.DateTime` with UTC-seconds storage (long), auto-formatted tick labels
- Custom tick formatters: Extend `LabelFormatter` to per-axis `Func<double, string>` delegate
- Log validation: Reject minimum <= 0 with explicit diagnostic
- DateTime precision: Store as UTC seconds (long) to avoid double precision loss

### the agent's Discretion
- Follow existing `SurfaceAxisDescriptor` patterns
- Tick generation follows `ComputeNiceStep()` with factors [1,2,2.5,5,10]
- Log ticks use powers-of-10 with minor ticks at 2x, 5x
- DateTime ticks use nice time intervals (seconds, minutes, hours, days)

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `SurfaceAxisScaleKind` enum — already defines Log, DateTime, ExplicitCoordinates
- `SurfaceAxisDescriptor` — axis model with Minimum/Maximum/ScaleKind/LabelFormatter
- `SurfaceGeometryGrid.MapNormalizedCoordinate` — already has log math for Log and DateTime cases
- `SurfaceChartOverlayOptions.LabelFunc` — existing `Func<string, double, string>?` delegate
- `ComputeNiceStep()` — existing tick spacing algorithm with factors [1,2,2.5,5,10]

### Established Patterns
- Axis types in `src/Videra.SurfaceCharts.Core/`
- Overlay presenters in `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/`
- Series model: `Plot3DSeries` with Kind enum
- Validation: `ArgumentOutOfRangeException`, `ArgumentException.ThrowIfNullOrWhiteSpace`

### Integration Points
- `SurfaceAxisDescriptor` — needs Log unblock and DateTime implementation
- `SurfaceGeometryGrid.MapNormalizedCoordinate` — already handles Log/DateTime cases
- Overlay tick presenters — need log/DateTime tick generation
- `Plot3D` — axis configuration API

</code_context>

<specifics>
## Specific Ideas

- The single blocker for Log scale is a `throw` in `SurfaceAxisDescriptor` constructor (line 46-51)
- DateTime needs tick generator that uses nice time intervals
- Custom formatters need try-catch wrapping to prevent bad formatters from crashing overlays
- Log axis needs `minimum > 0` validation

</specifics>

<deferred>
## Deferred Ideas

- Explicit coordinates (future)
- Multiple Y axes (future)
- Inverted axes (future)

</deferred>
