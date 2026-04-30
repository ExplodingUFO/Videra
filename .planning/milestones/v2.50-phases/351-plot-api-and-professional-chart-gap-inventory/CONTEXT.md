# Phase 351: Plot API and Professional Chart Gap Inventory - Context

**Gathered:** 2026-04-29  
**Status:** Ready for implementation handoff  
**Mode:** Autonomous inventory  
**Bead:** Videra-z44.1

<domain>

## Phase Boundary

Inventory the current `VideraChartView.Plot` API, Plot-owned runtime path, chart demo, docs, tests, support evidence, and repository guardrails before changing code.

This phase is intentionally read-only except for planning artifacts and Beads status. It must not introduce new public API, compatibility layers, direct `Source` API, old chart views, renderer/backend changes, hidden fallback/downshift behavior, or a broad chart workbench.

</domain>

<decisions>

## Implementation Decisions

### Keep

- `VideraChartView` remains the single shipped chart control.
- `VideraChartView.Plot.Add.Surface(...)`, `.Waterfall(...)`, and `.Scatter(...)` remain the public runtime data-loading path.
- `Plot.ColorMap` and `Plot.OverlayOptions` remain chart-local presentation seams.
- `RenderingStatus`, `ScatterRenderingStatus`, and support-summary text remain evidence surfaces rather than hidden recovery logic.

### Implement Next

- Phase 352 should polish Plot series lifecycle and host-facing inspection: stable series identity, active series, remove/clear semantics, deterministic revision behavior, and focused tests.
- Phase 353 should polish professional presentation through the existing Plot seams: style/precision profile helpers and evidence creation without a duplicate style system.
- Phase 354 should refresh demo/support evidence to report Plot series identity, chart kind, style/precision profile, and rendering status.
- Phase 355 should close docs and repository guardrails around the single chart view plus Plot.Add contract.

### Reject

- Do not restore `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView`.
- Do not restore public `VideraChartView.Source` / `SourceProperty`.
- Do not add compatibility wrappers for removed alpha APIs.
- Do not create a generic plotting engine, renderer rewrite, backend expansion, shader/material graph, hidden fallback/downshift, or god-code workbench.

</decisions>

<code_context>

## Existing Code Insights

### Plot API

- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs` exposes `public Plot3D Plot { get; }`.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs` owns `Series`, `Add`, `ColorMap`, `OverlayOptions`, `Revision`, and `Clear()`.
- `Plot3D.ActiveSeries`, `ActiveSurfaceSeries`, and `ActiveScatterSeries` are internal.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs` exposes `Surface(ISurfaceTileSource)`, `Surface(SurfaceMatrix)`, `Waterfall(ISurfaceTileSource)`, `Waterfall(SurfaceMatrix)`, and `Scatter(ScatterChartData)`.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs` exposes immutable `Kind`, `Name`, `SurfaceSource`, and `ScatterData`; there is no public stable id, index, or remove method.

### Runtime Integration

- `VideraChartView.OnPlotChanged()` selects the active surface-family or scatter series from the last Plot series.
- Surface/waterfall runtime source comes from `Plot.ActiveSurfaceSeries?.SurfaceSource`.
- Scatter status comes from `Plot.ActiveScatterSeries?.ScatterData`.
- `Plot.Clear()` clears series and triggers refresh; there is no targeted removal API.

### Presentation And Precision

- `SurfaceChartOverlayOptions` contains tick/legend format and precision policy plus axis/legend title/unit overrides.
- `SurfaceChartNumericLabelPresets.Engineering/Scientific/Fixed(...)` create overlay options with matching tick and legend precision.
- `SurfaceChartOverlayPresets.Professional` and `.Compact` already exist.
- `SurfaceColorMapPresets.CreateDefault/CreateProfessional/CreateCoolWarm/CreateGrayscale()` already exist.
- `SurfaceChartEvidenceFormatter.Create(...)` currently reports palette stops and `InvariantCulture:G6` sample labels; it is not tied to `Plot.OverlayOptions`.
- `SurfaceChartProbeEvidenceFormatter.Create(...)` uses `SurfaceChartOverlayOptions`, so probe evidence already shares overlay formatting.

### Demo / Smoke / Docs

- `samples/Videra.SurfaceCharts.Demo` uses three `VideraChartView` controls, one each for surface, waterfall, and scatter views, but all use `Plot.Add.*`.
- Demo support summary reports plot path, details, rendering status, interaction quality, overlay summary, cache asset, and dataset.
- Scatter support summary reports Plot revision and scatter rendering counters, but series identity is derived from local headings rather than Plot series metadata.
- `smoke/Videra.SurfaceCharts.ConsumerSmoke` uses `Plot.Add.Surface(...)`, `Plot.ColorMap`, and `Plot.OverlayOptions`.
- `src/Videra.SurfaceCharts.Avalonia/README.md`, root README, package matrix, support matrix, and Chinese docs already describe `VideraChartView` + `Plot.Add.*`.

### Guardrails

- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewPlotApiTests.cs` covers `Plot.Add.*`, `Plot.Clear()`, presentation option ownership, scatter status, and absence of old public chart views/source properties.
- `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs` rejects deleted direct `Source` API terms in public entry files.
- Existing repository tests already enforce chart-local boundaries against `VideraView`.

</code_context>

<specifics>

## Target Examples

### Series Lifecycle

```csharp
var chart = new VideraChartView();

var surface = chart.Plot.Add.Surface(source, "baseline");
var scatter = chart.Plot.Add.Scatter(points, "markers");

var active = chart.Plot.ActiveSeries;
chart.Plot.Remove(surface);
chart.Plot.Clear();
```

### Professional Presentation

```csharp
var chart = new VideraChartView();

chart.Plot.Add.Surface(source, "spectrum");
chart.Plot.ColorMap = SurfaceColorMap.ForProfessional(source.Metadata.ValueRange);
chart.Plot.OverlayOptions = SurfaceChartOverlayPresets.Professional;
```

### Support Evidence

```text
PlotSeriesCount: 1
ActiveSeries: Surface "spectrum"
ColorMap: Professional
PrecisionProfile: Scientific(3)
RenderingStatus: ActiveBackend ...
```

</specifics>

<deferred>

## Deferred Ideas

- Additional chart families beyond surface, waterfall, and scatter.
- Generic 2D/3D plotting engine semantics.
- Publication layout engine, image export, visual regression gate, or screenshot comparison framework.
- GPU-driven chart runtime, indirect draw, renderer rewrite, or backend expansion.
- Compatibility adapters for removed alpha chart APIs.

</deferred>
