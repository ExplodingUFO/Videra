---
phase: 369
plan: 369-01
type: auto
autonomous: true
wave: 1
depends_on: [366]
requirements: [CTR-01, CTR-02, CTR-03, CTR-04, CTR-05]
---

# Phase 369 Plan: Contour Plot Series

## Objective

Implement contour plot series with marching squares iso-line extraction, 3D line rendering, configurable iso-levels, and result caching.

## Success Criteria

1. User can call `Plot.Add.Contour(double[,] values)` and see iso-lines rendered as 3D line geometry
2. Contour lines are extracted via marching squares algorithm at evenly-spaced value levels
3. User can configure the number of contour levels (iso-line count)
4. Contour extraction results are cached and only re-extracted when data changes

## Context

- Follows existing `Plot3DSeriesKind` → `Plot3DAddApi` → renderer pattern established by Scatter
- Marching squares is ~100 lines of C# — no external library needed
- Contour lines must respect `SurfaceMask` (masked regions produce no segments)
- Iso-lines only for v2.53 (not filled regions)

## Tasks

### Task 1: Add Contour enum value and data model

**Type:** auto

Add `Contour` to `Plot3DSeriesKind` enum and create `ContourChartData` data model.

**Implementation:**
1. Add `Contour` value to `Plot3DSeriesKind.cs`
2. Create `ContourChartData.cs` in `SurfaceCharts.Core`:
   - Constructor takes `SurfaceScalarField field`, optional `SurfaceMask mask`, optional `int levelCount`
   - Validates field dimensions, level count > 0
   - Exposes `Field`, `Mask`, `LevelCount` properties
   - Level count defaults to 10
3. Update `Plot3DSeries.cs` to add `ContourData` property
4. Update `Plot3D.cs` to add `ActiveContourSeries` property

**Files to modify:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeriesKind.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs`

**Files to create:**
- `src/Videra.SurfaceCharts.Core/ContourChartData.cs`

**Verification:**
- Project builds without errors
- `Plot3DSeriesKind.Contour` exists
- `ContourChartData` validates inputs correctly

**Done criteria:**
- [x] Contour enum value added
- [x] ContourChartData data model created with validation
- [x] Plot3DSeries updated with ContourData property
- [x] Plot3D updated with ActiveContourSeries

---

### Task 2: Implement Marching Squares algorithm

**Type:** auto**

Implement the marching squares algorithm for iso-line extraction from 2D scalar fields.

**Implementation:**
1. Create `MarchingSquaresExtractor.cs` in `SurfaceCharts.Core`:
   - Static method `Extract(SurfaceScalarField field, float isoValue, SurfaceMask? mask)` → `IReadOnlyList<ContourSegment>`
   - `ContourSegment` has `Vector3 Start` and `Vector3 End` (normalized coordinates)
   - 16-case lookup table for marching squares
   - Handles saddle points with asymptotic decider
   - Skips masked cells (any corner masked → skip)
   - Skips cells with NaN/Infinity values
2. Create `ContourLine.cs` in `SurfaceCharts.Core`:
   - Represents a single contour level with its segments
   - Has `float IsoValue` and `IReadOnlyList<ContourSegment> Segments`
3. Create `ContourExtractor.cs` in `SurfaceCharts.Core`:
   - Orchestrates extraction of all contour levels
   - Method `ExtractAll(ContourChartData data)` → `IReadOnlyList<ContourLine>`
   - Computes evenly-spaced iso-values from field range
   - Calls `MarchingSquaresExtractor` for each level

**Files to create:**
- `src/Videra.SurfaceCharts.Core/MarchingSquaresExtractor.cs`
- `src/Videra.SurfaceCharts.Core/ContourLine.cs`
- `src/Videra.SurfaceCharts.Core/ContourExtractor.cs`

**Verification:**
- Unit tests pass for marching squares on known scalar fields
- Edge cases handled: flat field, single-value field, NaN regions, masked regions

**Done criteria:**
- [x] MarchingSquaresExtractor implemented with 16-case lookup
- [x] ContourSegment and ContourLine types created
- [x] ContourExtractor orchestrates multi-level extraction
- [x] Mask and NaN handling correct

---

### Task 3: Add Plot3DAddApi.Contour method

**Type:** auto

Add `Contour()` method to `Plot3DAddApi` following the existing pattern.

**Implementation:**
1. Add `Contour(double[,] values, string? name = null)` overload:
   - Converts `double[,]` to `SurfaceScalarField`
   - Creates `ContourChartData` with default level count (10)
   - Returns `Plot3DSeries` with `Kind = Contour`
2. Add `Contour(SurfaceScalarField field, string? name = null)` overload:
   - Creates `ContourChartData` from field
3. Add `Contour(ContourChartData data, string? name = null)` overload:
   - Full control overload
4. Update `Plot3DSeries` constructor to accept `ContourChartData`
5. Wire `ActiveContourSeries` in `Plot3D`

**Files to modify:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs`

**Verification:**
- `Plot.Add.Contour(values)` creates series with correct kind
- Series has ContourData populated
- Revision increments on add

**Done criteria:**
- [x] Contour method overloads added
- [x] double[,] conversion to SurfaceScalarField works
- [x] Series creation follows existing pattern

---

### Task 4: Implement contour render scene and caching

**Type:** auto

Create the contour render scene with caching to avoid redundant extraction.

**Implementation:**
1. Create `ContourRenderScene.cs` in `SurfaceCharts.Core`:
   - Holds `IReadOnlyList<ContourLine> Lines` and `SurfaceMetadata Metadata`
   - Created from `ContourChartData` via `ContourExtractor`
2. Create `ContourSceneCache.cs` in `SurfaceCharts.Core`:
   - Caches `ContourRenderScene` keyed by data revision
   - `GetOrCompute(ContourChartData data, int revision)` → `ContourRenderScene`
   - Invalidates when revision changes
3. Wire caching into rendering pipeline

**Files to create:**
- `src/Videra.SurfaceCharts.Core/ContourRenderScene.cs`
- `src/Videra.SurfaceCharts.Core/ContourSceneCache.cs`

**Verification:**
- Cache returns same scene for same revision
- Cache invalidates on data change
- No redundant extraction calls

**Done criteria:**
- [x] ContourRenderScene created
- [x] ContourSceneCache implements revision-based caching
- [x] Cache invalidation works correctly

---

### Task 5: Implement contour line rendering in SurfaceScenePainter

**Type:** auto

Add contour line rendering to the existing scene painter.

**Implementation:**
1. Add `DrawContourLines` method to `SurfaceScenePainter.cs`:
   - Takes `DrawingContext`, `ContourRenderScene`, `SurfaceChartProjection`
   - Projects 3D contour segments to 2D screen coordinates
   - Draws lines using `StreamGeometry` with `context.DrawGeometry(null, pen, geometry)`
   - Uses configurable pen color (default: dark gray)
2. Wire contour rendering into `VideraChartView.Rendering.cs`:
   - Check for active contour series
   - Build/cache contour render scene
   - Call `DrawContourLines` after surface rendering
3. Add `ContourChartRenderingStatus` similar to `ScatterChartRenderingStatus`

**Files to modify:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceScenePainter.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Rendering.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs`

**Files to create:**
- `src/Videra.SurfaceCharts.Avalonia/Controls/ContourChartRenderingStatus.cs`

**Verification:**
- Contour lines render as visible line geometry
- Lines respect 3D projection (correct perspective)
- Lines are drawn on top of surface

**Done criteria:**
- [x] DrawContourLines method implemented
- [x] Contour rendering wired into VideraChartView
- [x] ContourChartRenderingStatus created

---

### Task 6: Add unit tests for contour functionality

**Type:** auto

Create comprehensive tests for contour plot functionality.

**Implementation:**
1. Create `ContourChartDataTests.cs` in `SurfaceCharts.Core.Tests`:
   - Test validation: null field, zero dimensions, invalid level count
   - Test default level count
2. Create `MarchingSquaresExtractorTests.cs` in `SurfaceCharts.Core.Tests`:
   - Test on known scalar field (e.g., radial function)
   - Test flat field (no contours)
   - Test single-value field
   - Test with mask (masked regions produce no segments)
   - Test with NaN values
3. Create `ContourExtractorTests.cs` in `SurfaceCharts.Core.Tests`:
   - Test multi-level extraction
   - Test evenly-spaced iso-values
4. Create `ContourSceneCacheTests.cs` in `SurfaceCharts.Core.Tests`:
   - Test cache hit/miss
   - Test invalidation on revision change

**Files to create:**
- `tests/Videra.SurfaceCharts.Core.Tests/ContourChartDataTests.cs`
- `tests/Videra.SurfaceCharts.Core.Tests/MarchingSquaresExtractorTests.cs`
- `tests/Videra.SurfaceCharts.Core.Tests/ContourExtractorTests.cs`
- `tests/Videra.SurfaceCharts.Core.Tests/ContourSceneCacheTests.cs`

**Verification:**
- All tests pass
- Edge cases covered

**Done criteria:**
- [x] ContourChartData tests pass
- [x] MarchingSquaresExtractor tests pass
- [x] ContourExtractor tests pass
- [x] ContourSceneCache tests pass

---

### Task 7: Add integration tests for Plot.Add.Contour

**Type:** auto

Create integration tests verifying the full contour plot pipeline.

**Implementation:**
1. Add contour tests to `VideraChartViewPlotApiTests.cs`:
   - Test `Plot.Add.Contour(values)` creates series with correct kind
   - Test contour series has ContourData populated
   - Test revision increments on add
   - Test remove contour series
   - Test contour rendering status
2. Add contour-specific test file `VideraChartViewContourIntegrationTests.cs`:
   - Test contour with surface series coexistence
   - Test contour with different level counts
   - Test contour evidence reporting

**Files to modify:**
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewPlotApiTests.cs`

**Files to create:**
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewContourIntegrationTests.cs`

**Verification:**
- All integration tests pass
- Contour series integrates correctly with existing chart infrastructure

**Done criteria:**
- [x] Plot.Add.Contour integration tests pass
- [x] Contour lifecycle tests pass
- [x] Contour evidence tests pass

---

## Overall Verification

Run all tests:
```bash
dotnet test tests/Videra.SurfaceCharts.Core.Tests/
dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/
```

Build verification:
```bash
dotnet build src/Videra.SurfaceCharts.Core/
dotnet build src/Videra.SurfaceCharts.Avalonia/
```

## Output Spec

- All new types in `Videra.SurfaceCharts.Core` namespace
- All new Avalonia types in `Videra.SurfaceCharts.Avalonia.Controls` namespace
- Tests in corresponding test projects
- No external dependencies added
