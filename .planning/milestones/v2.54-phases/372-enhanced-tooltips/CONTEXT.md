# Phase 372: Enhanced Tooltips — Context

## Goal
Users see detailed, multi-series-aware data tooltips when hovering over chart elements, positioned to avoid chart-edge clipping.

## Requirements
- TOOL-01: User sees a tooltip with series name, world coordinates, and data value when hovering over chart elements
- TOOL-02: Tooltip shows values from all series at the same X/Z position (multi-series awareness)
- TOOL-03: Tooltip repositions itself automatically to stay within chart bounds (no edge clipping)
- TOOL-04: Tooltip follows the pointer with a configurable offset from the cursor

## Key Decisions

### Decision 1: Extend existing SurfaceProbeOverlayPresenter
The current `SurfaceProbeOverlayPresenter` already renders readout bubbles for hovered probes. Rather than creating a new presenter, we enhance the existing one to support multi-series tooltips.

### Decision 2: Multi-series probe resolution
The current `SurfaceProbeService` resolves probes against a single tile source. For multi-series awareness, we need to resolve probes against ALL loaded series at the same X/Z position. This means iterating through `Plot3DSeries` list and resolving probe values for each series type.

### Decision 3: Tooltip content model
Enhanced tooltip shows:
- Series name (when multiple series present)
- World coordinates (X, Z)
- Data value (Y)
- Approximate indicator for coarse tiles
- Multi-series values listed vertically

### Decision 4: Edge avoidance positioning
The existing `ClampBubbleOrigin` already handles edge clamping. We enhance it to:
- Prefer positioning above/right of cursor by default
- Flip to below/left when near right/bottom edge
- Add configurable offset from cursor position

### Decision 5: Configurable offset
Add `TooltipOffset` property to `SurfaceChartOverlayOptions` (default: 12px horizontal, -12px vertical) so consumers can tune tooltip positioning.

## Architecture Approach

### Files to modify:
1. `SurfaceChartOverlayOptions.cs` — Add `TooltipOffset` property
2. `SurfaceProbeOverlayPresenter.cs` — Enhance `CreateState` to resolve multi-series probes, enhance `Render` to draw multi-series tooltip
3. `SurfaceProbeOverlayState.cs` — Add `MultiSeriesProbes` property
4. `SurfaceChartOverlayCoordinator.cs` — Pass series list to `SurfaceProbeOverlayPresenter.CreateState`

### New files:
1. `SurfaceTooltipSeriesEntry.cs` — Immutable record for per-series tooltip entry (series name, probe info)
2. `SurfaceTooltipContent.cs` — Aggregated tooltip content model (list of series entries)

### Test approach:
- Unit tests for multi-series probe resolution
- Unit tests for tooltip edge-avoidance positioning
- Integration tests for tooltip content formatting

## Existing Patterns to Follow
- Immutable state records (SurfaceProbeOverlayState pattern)
- Static presenter methods (CreateState + Render)
- Coordinator orchestration (SurfaceChartOverlayCoordinator.Refresh)
- Chart-local numeric formatting (SurfaceChartOverlayOptions.FormatLabel)

## Risks
- Performance: Resolving probes for many series on every pointer move. Mitigation: Debounce + cache.
- Series type dispatch: Different series types need different probe resolution. Phase 373 adds `ISeriesProbeStrategy`; for now we handle surface/waterfall series which already have probe support.
