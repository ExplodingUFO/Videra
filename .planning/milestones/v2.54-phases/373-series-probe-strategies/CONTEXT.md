# Phase 373: Series Probe Strategies — Context

## Problem Statement

The existing `SurfaceProbeService` resolves probes for surface and waterfall series via heightfield ray-picking. Scatter, bar, and contour series have **no probe resolution** — hovering over them produces no readout. This phase fills that gap with an extensible strategy interface.

## Key Decisions

### 1. Scatter Probe: Nearest-Point Lookup
- **Approach:** Brute-force nearest-point search (defer KD-tree to profiling phase)
- **Reasoning:** Scatter datasets in v2.54 are expected to be <10k points. O(n) is acceptable.
- **Input:** Screen position → unproject to chart space → find nearest point by Euclidean distance in XZ plane
- **Output:** `SurfaceProbeInfo` with the nearest point's coordinates and value

### 2. Bar Probe: Bar Value and Category
- **Approach:** Hit-test against bar bounding boxes in chart space
- **Reasoning:** Bars are axis-aligned boxes; simple AABB containment check
- **Input:** Screen position → unproject to chart space → check which bar contains the XZ position
- **Output:** `SurfaceProbeInfo` with bar center coordinates, value, and category index

### 3. Contour Probe: Iso-Line Value at Cursor
- **Approach:** Find nearest contour segment to cursor position, return its iso-value
- **Reasoning:** Contour lines are 2D segments on the XZ plane; distance-to-segment check
- **Input:** Screen position → unproject to chart space → find nearest contour segment
- **Output:** `SurfaceProbeInfo` with cursor coordinates and the contour line's iso-value

### 4. ISeriesProbeStrategy Interface
- **Location:** `Videra.SurfaceCharts.Core` (in the `Picking` namespace)
- **Design:** Single method interface for extensibility per series kind
- **Dispatch:** `SurfaceProbeOverlayPresenter` dispatches to the correct strategy based on `Plot3DSeriesKind`

## Existing Infrastructure

- `SurfaceProbeService` — resolves probes for surface/waterfall via heightfield ray-pick
- `SurfaceProbeInfo` — immutable probe result record (sample space + axis space + value)
- `SurfaceProbeRequest` — probe request in sample space
- `SurfacePickHit` — 3D pick hit result
- `SurfaceProbeOverlayPresenter` — renders probe readout bubbles
- `Plot3DSeries` — series model with kind, scatter data, bar data, contour data
- `Plot3DSeriesKind` — enum: Surface, Waterfall, Scatter, Bar, Contour

## Data Structures Available

- `ScatterPoint` — (Horizontal, Value, Depth) in axis space
- `ScatterSeries` — collection of ScatterPoints with color/label
- `ScatterChartData` — metadata + series collection
- `BarSeries` — values per category with color/label
- `BarChartData` — series collection + layout mode (Grouped/Stacked)
- `BarRenderBar` — (Position, Size, Color) in chart space
- `BarRenderScene` — render-ready bars
- `ContourLine` — iso-value + segments
- `ContourSegment` — (Start, End) as Vector3 in normalized coordinates
- `ContourRenderScene` — metadata + extracted contour lines

## Files to Create

1. `ISeriesProbeStrategy.cs` — interface in Core/Picking
2. `ScatterProbeStrategy.cs` — nearest-point implementation
3. `BarProbeStrategy.cs` — bar hit-test implementation
4. `ContourProbeStrategy.cs` — contour line hit-test implementation
5. `SeriesProbeStrategyDispatcher.cs` — dispatches to correct strategy by series kind
6. Tests for each strategy

## Files to Modify

- `SurfaceProbeOverlayPresenter.cs` — integrate strategy dispatch for non-surface series
- `SurfaceProbeService.cs` — add strategy-based resolution path

## Constraints

- Follow existing overlay presenter pattern (immutable state → static methods)
- All probe coordinates must be finite (enforced by `SurfaceProbeInfo` constructor)
- Brute-force is acceptable for v2.54; optimize later if profiling shows issues
- No new NuGet packages; use existing System.Numerics math
