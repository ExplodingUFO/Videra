---
phase: 372
plan: 372-PLAN
type: auto
autonomous: true
wave: 2
depends_on: []
requirements: [TOOL-01, TOOL-02, TOOL-03, TOOL-04]
---

# Phase 372 Plan: Enhanced Tooltips

## Objective
Enhance the existing probe overlay system to show rich, multi-series-aware tooltips that display series name, world coordinates, and data values — with edge-avoidance positioning and configurable cursor offset.

## Context
- Existing `SurfaceProbeOverlayPresenter` already renders single-series readout bubbles
- `SurfaceProbeService` resolves probes for individual tiles
- `SurfaceChartOverlayCoordinator` orchestrates all overlay presenters
- Gap: No multi-series awareness, no configurable tooltip offset, no series name display

## Tasks

### Task 1: Add TooltipOffset to SurfaceChartOverlayOptions
**Type:** auto
**Description:** Add `TooltipOffset` property to `SurfaceChartOverlayOptions` for configurable tooltip positioning relative to cursor.

**Implementation:**
- Add `Vector2 TooltipOffset { get; init; }` property with default `(12, -12)`
- Add XML documentation

**Files:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs`

**Verification:**
- Property compiles with correct default value
- Existing tests pass (no breaking change)

**Done Criteria:**
- [x] `TooltipOffset` property exists with default value
- [x] XML documentation present

---

### Task 2: Create SurfaceTooltipSeriesEntry record
**Type:** auto
**Description:** Create immutable record for per-series tooltip content entry.

**Implementation:**
- New file `SurfaceTooltipSeriesEntry.cs` in Overlay directory
- Readonly record struct with: `SeriesName`, `ProbeInfo`, `SeriesKind`
- Follow existing `SurfaceProbeInfo` pattern

**Files:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceTooltipSeriesEntry.cs`

**Verification:**
- Record compiles and is immutable
- Can be constructed with all required fields

**Done Criteria:**
- [x] `SurfaceTooltipSeriesEntry` record exists
- [x] Properties: SeriesName, ProbeInfo, SeriesKind

---

### Task 3: Create SurfaceTooltipContent model
**Type:** auto
**Description:** Create aggregated tooltip content model that holds multiple series entries for a single tooltip.

**Implementation:**
- New file `SurfaceTooltipContent.cs` in Overlay directory
- Readonly record with: `WorldX`, `WorldZ`, `IsApproximate`, `IReadOnlyList<SurfaceTooltipSeriesEntry> Entries`
- Factory method `FromSeriesProbes` that takes resolved probes from multiple series

**Files:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceTooltipContent.cs`

**Verification:**
- Model compiles and is immutable
- Factory method correctly aggregates series entries

**Done Criteria:**
- [x] `SurfaceTooltipContent` record exists
- [x] `FromSeriesProbes` factory method works correctly

---

### Task 4: Enhance SurfaceProbeOverlayState for multi-series
**Type:** auto
**Description:** Extend `SurfaceProbeOverlayState` to carry multi-series tooltip content alongside existing single-series state.

**Implementation:**
- Add `SurfaceTooltipContent? TooltipContent` property to `SurfaceProbeOverlayState`
- Update constructor to accept optional `tooltipContent` parameter
- Update `Empty` static instance

**Files:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayState.cs`

**Verification:**
- Existing constructor overloads still work
- New property defaults to null for backward compatibility
- Existing tests pass

**Done Criteria:**
- [x] `TooltipContent` property added
- [x] Backward compatible (null default)
- [x] Existing tests pass

---

### Task 5: Enhance SurfaceProbeOverlayPresenter.CreateState for multi-series
**Type:** auto
**Description:** Add new `CreateState` overload that accepts `IReadOnlyList<Plot3DSeries>` and resolves probes across all series at the hovered position.

**Implementation:**
- Add new `CreateState` overload accepting `series` parameter
- For each series with a `SurfaceSource`, resolve probe at hovered position
- Build `SurfaceTooltipContent` from resolved probes
- Use series `Name` (or generate default like "Series 1") for display
- Handle null/empty series gracefully

**Files:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs`

**Verification:**
- Single-series behavior unchanged (existing overload)
- Multi-series overload correctly resolves probes for all surface series
- Series names appear in tooltip content

**Done Criteria:**
- [x] New `CreateState` overload with series parameter
- [x] Multi-series probe resolution works
- [x] Series names populated correctly

---

### Task 6: Enhance SurfaceProbeOverlayPresenter.Render for multi-series tooltip
**Type:** auto
**Description:** Update `Render` method to draw enhanced multi-series tooltip when `TooltipContent` is present.

**Implementation:**
- Check for `TooltipContent` in overlay state
- If present and has entries, render multi-series tooltip instead of single readout
- Format: Series name line, then X/Z/Value lines per series
- Apply configurable `TooltipOffset` for positioning
- Use existing edge-avoidance clamping

**Files:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs`

**Verification:**
- Single-series tooltip still renders correctly
- Multi-series tooltip shows all series values
- Edge avoidance works with new offset

**Done Criteria:**
- [x] Multi-series tooltip renders correctly
- [x] Uses configurable offset
- [x] Edge avoidance works

---

### Task 7: Update SurfaceChartOverlayCoordinator to pass series
**Type:** auto
**Description:** Update `SurfaceChartOverlayCoordinator.Refresh` to pass series list to the new `CreateState` overload.

**Implementation:**
- Update `Refresh` method to use new `CreateState` overload when series are available
- Pass series list from existing `series` parameter
- Maintain backward compatibility when series is null

**Files:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartOverlayCoordinator.cs`

**Verification:**
- Coordinator correctly passes series to presenter
- Existing behavior preserved when series is null

**Done Criteria:**
- [x] Series list passed to new CreateState overload
- [x] Backward compatible with null series

---

### Task 8: Enhance edge-avoidance positioning with configurable offset
**Type:** auto
**Description:** Update `CreateHoveredBubbleOrigin` to use configurable offset and improve edge-avoidance logic.

**Implementation:**
- Accept `TooltipOffset` parameter in positioning methods
- Default behavior: position above-right of cursor
- Flip to below-left when near right/bottom edge
- Apply offset to final position

**Files:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs`

**Verification:**
- Default offset (12, -12) positions tooltip correctly
- Near right edge: tooltip flips to left of cursor
- Near bottom edge: tooltip flips above cursor
- Custom offset values are respected

**Done Criteria:**
- [x] Configurable offset applied
- [x] Edge flipping works correctly
- [x] Default offset is (12, -12)

---

### Task 9: Add unit tests for multi-series tooltip
**Type:** auto
**Description:** Add comprehensive tests for multi-series tooltip functionality.

**Implementation:**
- Test multi-series probe resolution with 2+ series
- Test tooltip content aggregation
- Test edge-avoidance positioning with various offsets
- Test single-series backward compatibility
- Test series name display (named vs unnamed)

**Files:**
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceTooltipOverlayTests.cs`

**Verification:**
- All new tests pass
- Existing probe overlay tests still pass

**Done Criteria:**
- [x] Multi-series probe resolution tests pass
- [x] Edge avoidance tests pass
- [x] Backward compatibility tests pass
- [x] Series name tests pass

---

## Verification Criteria

### Per-task:
Each task has specific done criteria listed above.

### Overall Success:
1. User sees tooltip with series name, world coordinates, and data value (TOOL-01)
2. Tooltip shows values from all series at same X/Z position (TOOL-02)
3. Tooltip repositions to stay within chart bounds (TOOL-03)
4. Tooltip follows pointer with configurable offset (TOOL-04)
5. All existing tests pass (no regression)
6. New tests provide coverage for enhanced functionality

## Output
- Modified: `SurfaceChartOverlayOptions.cs`
- Modified: `SurfaceProbeOverlayState.cs`
- Modified: `SurfaceProbeOverlayPresenter.cs`
- Modified: `SurfaceChartOverlayCoordinator.cs`
- Created: `SurfaceTooltipSeriesEntry.cs`
- Created: `SurfaceTooltipContent.cs`
- Created: `SurfaceTooltipOverlayTests.cs`
