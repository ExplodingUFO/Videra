---
phase: 368
plan: 368-01
type: auto
autonomous: true
wave: 1
depends_on: Phase 366
---

# Phase 368 Plan: Bar Chart Series

## Objective
Implement 3D bar chart rendering via `Plot.Add.Bar()` following the established `Plot3DSeriesKind` → `Plot3DAddApi` → renderer → overlay pattern. Bars render as axis-aligned quads rising from the base plane, supporting grouped and stacked layouts with per-series color configuration.

## Context
- Follows established pattern: Scatter was added in v2.1, Waterfall in v1.27 — same pattern for Bar
- Phase 366 (Axis Foundation) is a prerequisite — provides log scale and DateTime axis support
- Bar chart needs categorical axis support initially via numeric indices with `LabelFormatter`
- Research recommends axis-aligned quads first (not true 3D boxes) for simplicity
- z-fighting mitigation via small gaps between grouped bars

## Tasks

### Task 1: Add Bar to Plot3DSeriesKind enum
**Type:** auto
**Description:** Add `Bar` enum value to `Plot3DSeriesKind` with XML documentation.

**Files:**
- Modify: `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeriesKind.cs`

**Verification:**
- Enum compiles with new value
- XML doc matches existing style

**Done Criteria:**
- [x] `Plot3DSeriesKind.Bar` exists with `/// A vertical bar chart series.` doc

---

### Task 2: Create BarSeries data model
**Type:** auto
**Description:** Create `BarSeries` class representing one immutable bar series with values, color, and label. Follows `ScatterSeries` pattern.

**Files:**
- Create: `src/Videra.SurfaceCharts.Core/BarSeries.cs`

**Verification:**
- Class is immutable (ReadOnlyCollection pattern)
- Constructor validates: non-null values, no NaN values, no empty arrays
- Properties: Values, Color, Label

**Done Criteria:**
- [x] `BarSeries` compiles with validation
- [x] Immutability enforced via ReadOnlyCollection

---

### Task 3: Create BarChartData container
**Type:** auto
**Description:** Create `BarChartData` class that holds one or more `BarSeries` for bar chart rendering. Follows `ScatterChartData` pattern.

**Files:**
- Create: `src/Videra.SurfaceCharts.Core/BarChartData.cs`

**Verification:**
- Validates all series have same length (same number of categories)
- Rejects empty series list
- Exposes SeriesCount, CategoryCount, bar layout mode

**Done Criteria:**
- [x] `BarChartData` compiles with grouped/stacked layout enum
- [x] Validation rejects mismatched series lengths

---

### Task 4: Create BarRenderScene and BarRenderBar types
**Type:** auto
**Description:** Create render-ready types for bar chart rendering.

**Files:**
- Create: `src/Videra.SurfaceCharts.Core/Rendering/BarRenderBar.cs`
- Create: `src/Videra.SurfaceCharts.Core/Rendering/BarRenderScene.cs`

**Verification:**
- `BarRenderBar`: position (Vector3), size (Vector3), color (uint)
- `BarRenderScene`: metadata + bars list (immutable)

**Done Criteria:**
- [x] Types compile and follow scatter render pattern
- [x] BarRenderBar stores 3D position and dimensions

---

### Task 5: Create BarRenderer
**Type:** auto
**Description:** Create `BarRenderer` that builds render-ready bar geometry from `BarChartData`. Handles grouped and stacked layout calculations.

**Files:**
- Create: `src/Videra.SurfaceCharts.Core/Rendering/BarRenderer.cs`

**Verification:**
- `BuildScene(BarChartData)` returns `BarRenderScene`
- Grouped layout: bars offset horizontally per series
- Stacked layout: bars stacked vertically per category
- Bar width calculated from category count

**Done Criteria:**
- [x] `BarRenderer.BuildScene` produces correct geometry
- [x] Grouped and stacked layouts verified

---

### Task 6: Wire Plot3DAddApi.Bar() and Plot3DSeries.BarData
**Type:** auto
**Description:** Add `Bar()` method to `Plot3DAddApi` and `BarData` property to `Plot3DSeries`. Follows Scatter pattern exactly.

**Files:**
- Modify: `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs`
- Modify: `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs`

**Verification:**
- `Plot.Add.Bar(double[] values)` creates series with `Plot3DSeriesKind.Bar`
- `Plot.Add.Bar(BarChartData data)` creates series from full data model
- `Plot3DSeries.BarData` property exposes bar data

**Done Criteria:**
- [x] `Plot3DAddApi.Bar()` methods compile
- [x] `Plot3DSeries.BarData` property exists

---

### Task 7: Update Plot3D and VideraChartView for bar support
**Type:** auto
**Description:** Add `ActiveBarSeries` helper to `Plot3D` and wire bar rendering into `VideraChartView`. Add `BarChartRenderingStatus`.

**Files:**
- Modify: `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs`
- Modify: `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs`
- Create: `src/Videra.SurfaceCharts.Avalonia/Controls/BarChartRenderingStatus.cs`

**Verification:**
- `Plot3D.ActiveBarSeries` returns bar series when last series is Bar kind
- `VideraChartView` creates `BarChartRenderingStatus`
- Bar rendering status exposed as property

**Done Criteria:**
- [x] `Plot3D.ActiveBarSeries` compiles
- [x] `BarChartRenderingStatus` created
- [x] `VideraChartView` handles bar series

---

### Task 8: Update evidence types for Bar series
**Type:** auto
**Description:** Update `Plot3DDatasetEvidence` and `Plot3DOutputEvidence` to handle `Plot3DSeriesKind.Bar`.

**Files:**
- Modify: `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DDatasetEvidence.cs`
- Modify: `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs`

**Verification:**
- `Plot3DSeriesDatasetEvidence.Create` handles Bar kind without throwing
- `Plot3D.CreateOutputEvidence` handles bar series
- Evidence reports bar series count and category count

**Done Criteria:**
- [x] Evidence types handle Bar kind
- [x] No `ArgumentOutOfRangeException` for bar series

---

### Task 9: Write bar chart tests
**Type:** auto
**Description:** Write comprehensive tests for BarSeries, BarChartData, BarRenderer, and Plot3DAddApi.Bar().

**Files:**
- Create: `tests/Videra.SurfaceCharts.Core.Tests/Rendering/BarRendererTests.cs`

**Verification:**
- Tests cover: validation, grouped layout, stacked layout, NaN rejection
- All tests pass

**Done Criteria:**
- [x] Test file compiles and all tests pass
- [x] Coverage for BAR-01 through BAR-05

---

## Verification / Success Criteria

1. `Plot.Add.Bar(double[] values)` adds bar series and returns `Plot3DSeries` with `Kind == Bar`
2. `Plot.Add.Bar(BarChartData data)` adds bar series from full data model
3. Bar renderer produces correct geometry for grouped and stacked layouts
4. Empty arrays and NaN values throw `ArgumentException`
5. Evidence types handle Bar kind without errors
6. All tests pass

## Output

- Bar chart series fully integrated into the Plot3D pipeline
- BAR-01 through BAR-05 requirements satisfied
- Tests provide regression coverage
