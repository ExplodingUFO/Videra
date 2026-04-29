# Phase 367: Enhanced Chart Legend - Context

## Goal
Chart displays a configurable, multi-series legend overlay with kind-specific visual indicators.

## Requirements
- **LEG-01**: Chart displays a legend overlay showing all series with labels and kind-specific indicators
- **LEG-02**: Legend position is configurable (top-left, top-right, bottom-left, bottom-right)
- **LEG-03**: Legend respects series visibility — hidden series are excluded from legend

## Key Decisions

### Decision 1: Multi-Series Legend Model
**Decision**: Redesign `SurfaceLegendOverlayState` to support multiple series entries with per-kind visual indicators.
**Rationale**: Current implementation is a single color swatch for one series. New chart types (Bar, Contour) need different visual indicators. Multi-series support is essential for plots with multiple series.
**Alternatives**: 
- Keep single-series and stack multiple legends (rejected: cluttered, no unified view)
- Separate legend per series (rejected: poor UX, no consolidated view)

### Decision 2: Kind-Specific Visual Indicators
**Decision**: Each series kind gets a distinct visual indicator:
- **Surface/Waterfall**: Small colored rectangle (swatch)
- **Scatter**: Small colored dot (circle)
- **Bar**: Small colored rectangle with bar-like proportions (future)
- **Contour**: Small colored line segment (future)
**Rationale**: Visual indicators should be immediately recognizable and match the series rendering style. Using geometric shapes (rect, circle, line) is standard in charting libraries.
**Alternatives**:
- Use same indicator for all (rejected: loses kind distinction)
- Use text-only indicators (rejected: less visual, harder to scan)

### Decision 3: Legend Position Configuration
**Decision**: Add `LegendPosition` enum to `SurfaceChartOverlayOptions` with four corner options: `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight`.
**Rationale**: Users need flexibility to position legend where it doesn't obscure data. Four corners cover most use cases.
**Alternatives**:
- Free-form positioning (rejected: complex, not needed for v2.53)
- Edge positions (top, bottom, left, right) (rejected: corners sufficient, edges obscure more data)

### Decision 4: Legend Overflow Strategy
**Decision**: For v2.53, truncate legend if entries exceed available space. Show "+N more" indicator.
**Rationale**: Scrolling is over-engineering for v2.53. Truncation is simple, predictable, and sufficient.
**Alternatives**:
- Scrollable legend (deferred: complex interaction model)
- Reduce font size (rejected: poor readability)

### Decision 5: Legend Entry Model
**Decision**: Create `SurfaceLegendEntry` record with: `SeriesName`, `SeriesKind`, `IsVisible`, `Color` (uint ARGB), `IndicatorKind` (enum: Swatch, Dot, Line).
**Rationale**: Clean separation of data (entry) from presentation (indicator). `IndicatorKind` derived from `SeriesKind` but can be overridden if needed.
**Alternatives**:
- Store indicator shape directly (rejected: couples data to presentation)
- Use `Plot3DSeries` directly (rejected: legend needs different shape, not tied to runtime)

## Architecture

### Current State
- `SurfaceLegendOverlayState`: Single-series color swatch model
- `SurfaceLegendOverlayPresenter`: Renders continuous color gradient
- `SurfaceChartOverlayCoordinator`: Orchestrates overlays, passes metadata/colorMap
- `Plot3D.Series`: List of `Plot3DSeries` (already multi-series capable)

### Target State
- `SurfaceLegendOverlayState`: Multi-series entry list + position + layout bounds
- `SurfaceLegendOverlayPresenter`: Renders entries with kind-specific indicators
- `SurfaceChartOverlayCoordinator`: Passes `Plot3D.Series` to legend
- `SurfaceChartOverlayOptions`: New `LegendPosition` property

### Changes Required

#### New Files
- None (all changes in existing files)

#### Modified Files
1. **`SurfaceLegendOverlayState.cs`** — Redesign from single swatch to multi-entry model
2. **`SurfaceLegendOverlayPresenter.cs`** — Render entries with kind-specific indicators
3. **`SurfaceChartOverlayOptions.cs`** — Add `LegendPosition` enum and property
4. **`SurfaceChartOverlayCoordinator.cs`** — Pass `Plot3D.Series` to legend creation

## Research Findings

From `.planning/research/SUMMARY.md`:
- `SurfaceLegendOverlayPresenter` already renders a color-mapped swatch
- Overlay system uses coordinator → presenter → state pattern
- `Plot3D.Series` already provides multi-series access
- Legend should be 2D overlay, not 3D geometry (Pitfall 11)

## Success Criteria

1. **LEG-01**: User sees a legend overlay listing all visible series with kind-specific indicators
2. **LEG-02**: User can configure legend position to any corner
3. **LEG-03**: Hidden series are automatically excluded from legend display

## Dependencies

- Phase 366 (Axis Foundation) — complete
- No external dependencies

## Out of Scope

- Legend interactivity (click-to-toggle) — deferred to future milestone
- Legend scrolling — truncation for v2.53
- Categorical axis labels — numeric indices with LabelFormatter
- Bar/Contour specific indicators — only Surface/Waterfall/Scatter for now
