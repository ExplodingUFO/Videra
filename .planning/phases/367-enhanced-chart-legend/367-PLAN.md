---
phase: 367
plan: 367-PLAN
type: auto
autonomous: true
wave: 1
depends_on: 366
requirements: [LEG-01, LEG-02, LEG-03]
---

# Phase 367 Plan: Enhanced Chart Legend

## Objective
Redesign the legend overlay from a single-series color swatch to a multi-series legend with per-kind visual indicators and configurable position.

## Context
- Current legend (`SurfaceLegendOverlayState`/`SurfaceLegendOverlayPresenter`) renders a single continuous color gradient for one series
- `Plot3D.Series` already provides multi-series access
- New chart types (Bar, Contour) will need different visual indicators
- Legend must be 2D overlay, not 3D geometry

## Tasks

### Task 1: Add LegendPosition enum and property
**Type:** auto
**Files:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs`

**Actions:**
1. Add `SurfaceChartLegendPosition` enum with values: `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight`
2. Add `LegendPosition` property to `SurfaceChartOverlayOptions` with default `TopRight`
3. Update `Default` static property to include `LegendPosition`

**Verification:**
- Enum compiles
- Property has correct default value

---

### Task 2: Create legend entry model
**Type:** auto
**Files:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayState.cs`

**Actions:**
1. Add `LegendIndicatorKind` enum: `Swatch`, `Dot`, `Line`
2. Add `SurfaceLegendEntry` record: `SeriesName` (string), `SeriesKind` (Plot3DSeriesKind), `IsVisible` (bool), `Color` (uint ARGB), `IndicatorKind` (LegendIndicatorKind)
3. Redesign `SurfaceLegendOverlayState` to hold: `IReadOnlyList<SurfaceLegendEntry> Entries`, `SurfaceChartLegendPosition Position`, `Rect Bounds`, `bool IsTruncated`
4. Update `Empty` static property

**Verification:**
- New types compile
- `Empty` property works

---

### Task 3: Update legend presenter for multi-series rendering
**Type:** auto
**Files:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayPresenter.cs`

**Actions:**
1. Update `CreateState` signature to accept `IReadOnlyList<Plot3DSeries>`, `SurfaceChartLegendPosition`, `SurfaceChartOverlayOptions`
2. Filter series to visible only (exclude hidden)
3. Map each series to `SurfaceLegendEntry` with appropriate `IndicatorKind` based on `SeriesKind`
4. Calculate layout bounds based on `LegendPosition`
5. Implement truncation if entries exceed available space
6. Update `Render` to draw entries with kind-specific indicators:
   - `Swatch`: Small colored rectangle (12x12)
   - `Dot`: Small colored circle (diameter 10)
   - `Line`: Small colored line segment (16x3)

**Verification:**
- State creation works with multiple series
- Entries have correct indicator kinds
- Layout positions correctly for each corner
- Rendering draws correct indicators

---

### Task 4: Wire series into overlay coordinator
**Type:** auto
**Files:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartOverlayCoordinator.cs`

**Actions:**
1. Update `Refresh` signature to accept `IReadOnlyList<Plot3DSeries>`
2. Pass series to `SurfaceLegendOverlayPresenter.CreateState`
3. Update `VideraChartView.Overlay.cs` to pass `Plot.Series` to coordinator

**Verification:**
- Legend updates when series change
- Multi-series legend appears

---

### Task 5: Add unit tests
**Type:** auto
**Files:**
- `tests/Videra.SurfaceCharts.Avalonia.UnitTests/Overlay/SurfaceLegendOverlayStateTests.cs` (NEW)
- `tests/Videra.SurfaceCharts.Avalonia.UnitTests/Overlay/SurfaceLegendOverlayPresenterTests.cs` (NEW)

**Actions:**
1. Test `LegendIndicatorKind` enum values
2. Test `SurfaceLegendEntry` creation and properties
3. Test `SurfaceLegendOverlayState.Empty` is valid
4. Test `CreateState` with single series
5. Test `CreateState` with multiple series
6. Test `CreateState` filters hidden series
7. Test `CreateState` assigns correct `IndicatorKind` per series kind
8. Test `CreateState` positions legend correctly for each `LegendPosition`
9. Test `CreateState` truncates when entries exceed space

**Verification:**
- All tests pass
- Coverage for all series kinds
- Coverage for all legend positions

---

## Verification Criteria

### Success Criteria (must ALL be true)
1. User sees a legend overlay listing all visible series with kind-specific indicators
2. User can configure legend position to any corner (top-left, top-right, bottom-left, bottom-right)
3. Hidden series are automatically excluded from legend display

### Test Commands
```bash
dotnet test tests/Videra.SurfaceCharts.Avalonia.UnitTests --filter "FullyQualifiedName~Legend"
```

## Output
- Modified files: 4
- New files: 2 test files
- Total commits: 5
