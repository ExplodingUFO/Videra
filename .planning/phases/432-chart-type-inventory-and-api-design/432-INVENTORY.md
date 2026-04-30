# Phase 432: Current Chart Type Inventory

**Mapped:** 2026-04-30
**Source:** Codebase inspection of `src/Videra.SurfaceCharts.Core/` and `src/Videra.SurfaceCharts.Avalonia/Controls/`

## Current Chart Type API Surface

### Plot3DSeriesKind Enum

**File:** `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeriesKind.cs`

```csharp
public enum Plot3DSeriesKind
{
    Surface,
    Waterfall,
    Scatter,
    Bar,
    Contour,
}
```

### Chart Kinds

#### 1. Surface

- **Plot3DAddApi overloads** (`src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs`):
  - `SurfacePlot3DSeries Surface(ISurfaceTileSource source, string? name = null)`
  - `SurfacePlot3DSeries Surface(SurfaceMatrix matrix, string? name = null)`
  - `SurfacePlot3DSeries Surface(double[,] values, string? name = null)`
- **Plot3DSeries subclass:** `SurfacePlot3DSeries` -- passes `ISurfaceTileSource` via `surfaceSource` slot
- **Data model:** `ISurfaceTileSource` (interface), `SurfaceMatrix` (concrete), `SurfaceMetadata`
- **IPlottable3D contract:** `Label { get; set; }`, `IsVisible { get; set; }`

#### 2. Waterfall

- **Plot3DAddApi overloads** (`src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs`):
  - `WaterfallPlot3DSeries Waterfall(ISurfaceTileSource source, string? name = null)`
  - `WaterfallPlot3DSeries Waterfall(SurfaceMatrix matrix, string? name = null)`
  - `WaterfallPlot3DSeries Waterfall(double[,] values, string? name = null)`
- **Plot3DSeries subclass:** `WaterfallPlot3DSeries` -- passes `ISurfaceTileSource` via `surfaceSource` slot
- **Data model:** `ISurfaceTileSource` (interface), `SurfaceMatrix` (concrete), `SurfaceMetadata`

#### 3. Scatter

- **Plot3DAddApi overloads** (`src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs`):
  - `ScatterPlot3DSeries Scatter(ScatterChartData data, string? name = null)`
  - `ScatterPlot3DSeries Scatter(double[] x, double[] y, double[] z, string? name = null, uint color = 0xFF2F80EDu)`
- **Plot3DSeries subclass:** `ScatterPlot3DSeries` -- passes `ScatterChartData` via `scatterData` slot
- **Data model:** `ScatterChartData` (sealed class) with `ScatterChartMetadata Metadata`, `IReadOnlyList<ScatterSeries> Series`, `IReadOnlyList<ScatterColumnarSeries> ColumnarSeries`

#### 4. Bar

- **Plot3DAddApi overloads** (`src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs`):
  - `BarPlot3DSeries Bar(double[] values, string? name = null)`
  - `BarPlot3DSeries Bar(double[] values, IReadOnlyList<string> categoryLabels, string? name = null)`
  - `BarPlot3DSeries Bar(BarChartData data, string? name = null)`
- **Plot3DSeries subclass:** `BarPlot3DSeries` (sealed class, internal constructor) -- passes `BarChartData` via `barData` slot
  - `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/BarPlot3DSeries.cs`
  - Methods: `SetSeriesColor(int seriesIndex, uint color)`
- **Data model:** `BarChartData` (sealed class) with `BarChartLayout Layout`, `IReadOnlyList<BarSeries> Series`, `IReadOnlyList<string> CategoryLabels`
  - `src/Videra.SurfaceCharts.Core/BarChartData.cs`
  - Constructor validation: `ArgumentNullException.ThrowIfNull(series)`, at least one series required
  - `ReadOnlyCollection<BarSeries>` backing field

#### 5. Contour

- **Plot3DAddApi overloads** (`src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs`):
  - `ContourPlot3DSeries Contour(double[,] values, string? name = null)`
  - `ContourPlot3DSeries Contour(double[,] values, IReadOnlyList<float> explicitLevels, string? name = null)`
  - `ContourPlot3DSeries Contour(SurfaceScalarField field, string? name = null)`
  - `ContourPlot3DSeries Contour(SurfaceScalarField field, IReadOnlyList<float> explicitLevels, string? name = null)`
  - `ContourPlot3DSeries Contour(ContourChartData data, string? name = null)`
- **Plot3DSeries subclass:** `ContourPlot3DSeries` (sealed class, internal constructor) -- passes `ContourChartData` via `contourData` slot
  - `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/ContourPlot3DSeries.cs`
- **Data model:** `ContourChartData` (sealed class) with `SurfaceScalarField Field`, `SurfaceMask? Mask`, `int LevelCount`, `IReadOnlyList<float> ExplicitLevels`
  - `src/Videra.SurfaceCharts.Core/ContourChartData.cs`

### IPlottable3D Interface Contract

**File:** `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/IPlottable3D.cs`

```csharp
public interface IPlottable3D
{
    string? Label { get; set; }
    bool IsVisible { get; set; }
}
```

### Plot3DSeries Constructor Pattern

**File:** `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs`

The base class uses a discriminated union constructor where each chart kind passes its data to exactly one slot and null to the rest:

```csharp
internal Plot3DSeries(
    Plot3DSeriesKind kind,
    string? name,
    ISurfaceTileSource? surfaceSource,   // Surface/Waterfall only
    ScatterChartData? scatterData,       // Scatter only
    BarChartData? barData,               // Bar only
    ContourChartData? contourData)       // Contour only
```

**Properties exposed:**
- `Plot3DSeriesKind Kind { get; }`
- `string? Name => Label`
- `string? Label { get; set; }` -- normalizes whitespace
- `bool IsVisible { get; set; }`
- `ISurfaceTileSource? SurfaceSource { get; }`
- `ScatterChartData? ScatterData { get; }`
- `BarChartData? BarData { get; private set; }` -- mutable via `ReplaceBarData()`
- `ContourChartData? ContourData { get; }`

**Mutable update pattern:** `private protected void ReplaceBarData(BarChartData data)` validates kind, avoids no-op reference equality, sets data, calls `NotifyChanged()`.

## Rendering Seams

### Surface

- **Renderer:** `SurfaceRenderer` (instance class, not static) in `src/Videra.SurfaceCharts.Core/Rendering/SurfaceRenderer.cs`
  - `SurfaceRenderTile BuildTile(SurfaceMetadata metadata, SurfaceTile tile, SurfaceColorMap colorMap)`
  - `SurfaceRenderScene BuildScene(SurfaceMetadata metadata, IEnumerable<SurfaceTile> tiles, SurfaceColorMap colorMap)`
- **Render scene:** `SurfaceRenderScene` with `SurfaceMetadata`, `IReadOnlyList<SurfaceRenderTile>`
- **Render element:** `SurfaceRenderVertex` with `Vector3 Position`, `uint Color`
- **Painter integration:** `SurfaceScenePainter` in Avalonia layer

### Waterfall

- **Renderer:** Reuses `SurfaceRenderer` with waterfall-specific presentation
- **Render scene:** Reuses `SurfaceRenderScene`
- **Painter integration:** `SurfaceScenePainter` with waterfall layout mode

### Scatter

- **Renderer:** `ScatterRenderer` (static class) in `src/Videra.SurfaceCharts.Core/Rendering/ScatterRenderer.cs`
  - `static ScatterRenderScene BuildScene(ScatterChartData data)`
- **Render scene:** `ScatterRenderScene` with `ScatterChartMetadata`, `IReadOnlyList<ScatterRenderSeries>`
- **Render element:** `ScatterRenderPoint` with `Vector3 Position`, `uint Color`, `float Size`
- **Painter integration:** `ScatterScenePainter` in Avalonia layer

### Bar

- **Renderer:** `BarRenderer` (static class) in `src/Videra.SurfaceCharts.Core/Rendering/BarRenderer.cs`
  - `static BarRenderScene BuildScene(BarChartData data)`
  - Supports `BarChartLayout.Grouped` and `BarChartLayout.Stacked`
- **Render scene:** `BarRenderScene` with `int CategoryCount`, `int SeriesCount`, `BarChartLayout Layout`, `IReadOnlyList<BarRenderBar> Bars`
  - `src/Videra.SurfaceCharts.Core/Rendering/BarRenderScene.cs`
- **Render element:** `BarRenderBar` -- `readonly record struct(Vector3 Position, Vector3 Size, uint Color)`
- **Painter integration:** `BarScenePainter` in Avalonia layer

### Contour

- **Renderer:** `ContourRenderer` (static class) in `src/Videra.SurfaceCharts.Core/Rendering/`
  - `static ContourRenderScene BuildScene(ContourChartData data)`
- **Render scene:** `ContourRenderScene` with `IReadOnlyList<ContourLine> Lines`
- **Render element:** `ContourSegment` with `Vector3 Start`, `Vector3 End`; `ContourLine` with `float IsoValue`, `IReadOnlyList<ContourSegment> Segments`
- **Scene caching:** `ContourSceneCache` (revision-keyed) in `src/Videra.SurfaceCharts.Avalonia/Controls/`
- **Painter integration:** Contour painter in Avalonia layer using `StreamGeometry`

## Probe/Overlay Infrastructure

### ISeriesProbeStrategy Interface

**File:** `src/Videra.SurfaceCharts.Core/Picking/ISeriesProbeStrategy.cs`

```csharp
public interface ISeriesProbeStrategy
{
    SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata);
}
```

### Current Probe Strategies

| Kind | Strategy Class | File | Approach |
|------|---------------|------|----------|
| Surface | (built into SurfaceProbeService) | `src/Videra.SurfaceCharts.Core/Picking/` | Tile-based interpolation |
| Waterfall | (shared with Surface) | same | Tile-based interpolation |
| Scatter | `ScatterProbeStrategy` | `src/Videra.SurfaceCharts.Core/Picking/ScatterProbeStrategy.cs` | Nearest-point brute-force XZ distance |
| Bar | `BarProbeStrategy` | `src/Videra.SurfaceCharts.Core/Picking/BarProbeStrategy.cs` | Bounding-box hit-test, nearest-center |
| Contour | `ContourProbeStrategy` | `src/Videra.SurfaceCharts.Core/Picking/ContourProbeStrategy.cs` | Nearest line segment with snap radius |

### ScatterProbeStrategy

- Constructor: `ScatterProbeStrategy(ScatterChartData data)`
- Searches both `ScatterSeries.Points` and `ScatterColumnarSeries` (handles NaN skipping)
- Returns `SurfaceProbeInfo` with `sampleX`, `sampleY`, `axisX`, `axisY`, `value`, `isApproximate: false`

### BarProbeStrategy

- Constructor: `BarProbeStrategy(BarRenderScene scene)`
- Iterates bars, checks if chartX/chartZ is within bar XZ footprint (half-width/half-depth)
- Returns `SurfaceProbeInfo` with bar top value (`Position.Y + Size.Y`)

### ContourProbeStrategy

- Constructor: `ContourProbeStrategy(ContourRenderScene scene, double snapRadius = 0.05)`
- Finds nearest contour line segment within snap radius
- Projects cursor onto segment for sample position
- Returns `SurfaceProbeInfo` with `nearestLine.IsoValue`
- Reusable helpers: `PointToSegmentDistance()`, `ProjectOntoSegment()`

### SurfaceProbeInfo Structure

**File:** `src/Videra.SurfaceCharts.Core/Picking/SurfaceProbeInfo.cs`

```csharp
public sealed class SurfaceProbeInfo
{
    public double SampleX { get; }
    public double SampleY { get; }
    public double AxisX { get; }
    public double AxisY { get; }
    public double Value { get; }
    public bool IsApproximate { get; }
}
```

### SeriesProbeStrategyDispatcher

**File:** `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SeriesProbeStrategyDispatcher.cs`

```csharp
internal sealed class SeriesProbeStrategyDispatcher
{
    private readonly Dictionary<Plot3DSeriesKind, ISeriesProbeStrategy> _strategies;

    public SeriesProbeStrategyDispatcher(IReadOnlyDictionary<Plot3DSeriesKind, ISeriesProbeStrategy> strategies)
    public SurfaceProbeInfo? TryResolve(Plot3DSeriesKind kind, double chartX, double chartZ, SurfaceMetadata metadata)
    public bool HasStrategy(Plot3DSeriesKind kind)
}
```

**Registration pattern:** Construction site builds a `Dictionary<Plot3DSeriesKind, ISeriesProbeStrategy>` with entries for each supported kind, passes it to the constructor.

### SurfaceLegendOverlayPresenter

**File:** `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayPresenter.cs`

`CreateLegendEntry(Plot3DSeries series)` uses two switch expressions:

**Indicator kind mapping:**
```csharp
var indicatorKind = series.Kind switch
{
    Plot3DSeriesKind.Surface => LegendIndicatorKind.Swatch,
    Plot3DSeriesKind.Waterfall => LegendIndicatorKind.Swatch,
    Plot3DSeriesKind.Scatter => LegendIndicatorKind.Dot,
    _ => LegendIndicatorKind.Swatch,  // default fallback
};
```

**Default color mapping:**
```csharp
var color = series.Kind switch
{
    Plot3DSeriesKind.Surface => 0xFF4DA3FF,   // Blue
    Plot3DSeriesKind.Waterfall => 0xFF4DA3FF, // Blue
    Plot3DSeriesKind.Scatter => 0xFFFF6B6B,   // Red
    _ => 0xFFCCCCCC,                           // Gray (default)
};
```

### LegendIndicatorKind Enum

**File:** `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayState.cs`

```csharp
internal enum LegendIndicatorKind
{
    Swatch,  // Small colored rectangle (surface, waterfall)
    Dot,     // Small colored circle (scatter)
    Line,    // Small colored line segment (contour)
}
```

### Rendering Status Pattern

**File (example):** `src/Videra.SurfaceCharts.Avalonia/Controls/BarChartRenderingStatus.cs`

```csharp
public sealed record BarChartRenderingStatus
{
    public bool HasSource { get; init; }
    public bool IsReady { get; init; }
    public SurfaceChartRenderBackendKind BackendKind { get; init; } = SurfaceChartRenderBackendKind.Software;
    public bool IsInteracting { get; init; }
    public int SeriesCount { get; init; }
    public int CategoryCount { get; init; }
    public int BarCount { get; init; }
    public BarChartLayout Layout { get; init; }
    public Size ViewSize { get; init; }
}
```

Current rendering status types:
- `SurfaceChartRenderingStatus` (Surface/Waterfall)
- `ScatterChartRenderingStatus` (Scatter)
- `BarChartRenderingStatus` (Bar)
- `ContourChartRenderingStatus` (Contour)

## Integration Points for New Chart Types

Every chart kind follows a 7-step integration pattern (verified through Bar and Contour):

### Step 1: Plot3DSeriesKind Enum Value

Add a new value to `Plot3DSeriesKind` enum in `Plot3DSeriesKind.cs`.

### Step 2: Data Model in Core

Create `{Kind}ChartData.cs` in `src/Videra.SurfaceCharts.Core/`:
- `sealed class` with `ReadOnlyCollection<T>` backing fields
- Constructor validation with `ArgumentNullException.ThrowIfNull`
- XML doc comments on all public members
- Namespace: `Videra.SurfaceCharts.Core`

### Step 3: Renderer in Core/Rendering

Create `{Kind}Renderer.cs` in `src/Videra.SurfaceCharts.Core/Rendering/`:
- `public static class` (stateless) with single `BuildScene({Kind}ChartData data)` entry point
- Returns immutable render scene
- Uses `System.Numerics.Vector3` for all 3D geometry

### Step 4: Render Scene in Core/Rendering

Create `{Kind}RenderScene.cs` in `src/Videra.SurfaceCharts.Core/Rendering/`:
- `sealed class` with `ReadOnlyCollection<{Kind}RenderElement>`
- Elements are `readonly record struct` with `Vector3 Position`, `uint Color`
- Constructor validation

### Step 5: Plot3DSeries Subclass

Create `{Kind}Plot3DSeries.cs` in `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/`:
- `sealed class` extending `Plot3DSeries`
- `internal` constructor (only `Plot3DAddApi` creates instances)
- Pass `Plot3DSeriesKind.{Kind}` as first argument, data to appropriate slot, null to all other slots
- Optional mutable update methods using `Replace*Data()` pattern

### Step 6: Plot3DAddApi Method

Add overloaded methods to `Plot3DAddApi.cs`:
- Convenience overload(s) that construct the data model from raw arrays
- Data model overload that takes `{Kind}ChartData`
- Both return `{Kind}Plot3DSeries`
- Both use `ArgumentNullException.ThrowIfNull`

### Step 7: Probe Strategy

Create `{Kind}ProbeStrategy.cs` in `src/Videra.SurfaceCharts.Core/Picking/`:
- `sealed class` implementing `ISeriesProbeStrategy`
- Constructor takes render scene (or data model for data-driven probing)
- Single `TryResolve` method returning `SurfaceProbeInfo?`
- Returns `null` when no hit detected

### Additional Integration Points (beyond the 7 steps)

8. **Plot3DSeries base class:** Add `{Kind}ChartData? {Kind}Data` property and constructor parameter slot
9. **Plot3D.cs:** Add `Active{Kind}Series`, `Active{Kind}Data`, `GetVisibleSeries` support
10. **Plot3DSeriesComposition.cs:** Add `Create{Kind}Data()` for same-kind series merging
11. **SeriesProbeStrategyDispatcher.cs:** Register new strategy in dispatcher construction site
12. **SurfaceLegendOverlayPresenter.cs:** Add case in `CreateLegendEntry()` switch for indicator kind and default color
13. **VideraChartView.Core.cs:** Add `Create{Kind}RenderingStatus()`, call from `Refresh()` and `ArrangeOverride()`
14. **Plot3DOutputEvidence:** Handle new kind in `CreateRenderingEvidence()` and series identity creation
15. **Plot3DDatasetEvidence:** Handle new kind in series identity creation

## Files to Modify

When adding a new chart kind, the following files require changes:

### Core Layer (`src/Videra.SurfaceCharts.Core/`)

| File | Integration Point |
|------|------------------|
| `{Kind}ChartData.cs` | **NEW** -- Data model class |
| `Rendering/{Kind}Renderer.cs` | **NEW** -- Static renderer |
| `Rendering/{Kind}RenderScene.cs` | **NEW** -- Render scene + element types |
| `Picking/{Kind}ProbeStrategy.cs` | **NEW** -- Probe strategy |

### Avalonia Layer (`src/Videra.SurfaceCharts.Avalonia/Controls/`)

| File | Integration Point |
|------|------------------|
| `Plot/Plot3DSeriesKind.cs` | Add enum value |
| `Plot/Plot3DSeries.cs` | Add `{Kind}Data` property and constructor parameter |
| `Plot/Plot3DAddApi.cs` | Add `{Kind}(...)` overloaded methods |
| `Plot/Plot3D.cs` | Add `Active{Kind}Series`, `Active{Kind}Data` properties |
| `Plot/Plot3DSeriesComposition.cs` | Add `Create{Kind}Data()` merge method |
| `Plot/{Kind}Plot3DSeries.cs` | **NEW** -- Series subclass |
| `Overlay/SeriesProbeStrategyDispatcher.cs` | Register new strategy in construction |
| `Overlay/SurfaceLegendOverlayPresenter.cs` | Add case in `CreateLegendEntry()` switch |
| `Overlay/SurfaceLegendOverlayState.cs` | Add `LegendIndicatorKind` value if needed |
| `VideraChartView.Core.cs` | Add `Create{Kind}RenderingStatus()`, call from `Refresh()` |
| `{Kind}ChartRenderingStatus.cs` | **NEW** -- Diagnostics record |

### Total per chart kind: 4 new Core files, 2 new Avalonia files, 7-8 modified files

## Composition and Multi-Series Support

**Plot3DSeriesComposition** (`src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeriesComposition.cs`):

Merges multiple visible series of the same kind into a single composed dataset for rendering:
- `CreateSurfaceSource()` -- Composes `ISurfaceTileSource` via `ComposedSurfaceTileSource`
- `CreateScatterData()` -- Merges metadata and concatenates series
- `CreateBarData()` -- Merges if layout/category-count compatible, otherwise returns last
- Each new kind needs a `Create{Kind}Data()` method

**Plot3D.ActiveSeries** returns the last visible series in draw order. `ActiveComposedSeries` returns all visible series of the active kind.

## Legend Overlay State

**SurfaceLegendOverlayState** (`src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayState.cs`):

Contains `IReadOnlyList<SurfaceLegendEntry> Entries`, each with:
- `string SeriesName`
- `Plot3DSeriesKind SeriesKind`
- `bool IsVisible`
- `uint Color`
- `LegendIndicatorKind IndicatorKind`
