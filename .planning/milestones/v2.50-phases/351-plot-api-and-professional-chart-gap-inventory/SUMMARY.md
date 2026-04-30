# Phase 351: Plot API and Professional Chart Gap Inventory

## Bead

`Videra-z44.1`

## Outcome

Completed the v2.50 Plot API and professional chart gap inventory. The next implementation should remain a small Plot-owned usability polish, not a new chart architecture.

## Current State

- `VideraChartView` is the single shipped chart control.
- `VideraChartView.Plot.Add.Surface(...)`, `.Waterfall(...)`, and `.Scatter(...)` are the public chart authoring and runtime data-loading entrypoints.
- `Plot3D` currently exposes `Series`, `Add`, `ColorMap`, `OverlayOptions`, `Revision`, and `Clear()`.
- `Plot3DSeries` exposes `Kind`, `Name`, `SurfaceSource`, and `ScatterData`.
- Active-series selection is internal and last-series-wins.
- There is no targeted series removal, public active-series inspection, stable id/index, or Plot-owned support-summary model.
- Existing tests cover the single Plot.Add API, basic names/kinds/payloads, revision refresh, `ColorMap`, `OverlayOptions`, clear behavior, surface/waterfall runtime activation, scatter status activation, and absence of old `Source` / old chart view APIs.
- Professional palette, overlay, and numeric precision primitives already exist, but output evidence is not yet directly tied to Plot presentation state.

## Gap Classification

### Implement

- Add explicit Plot series lifecycle/inspection affordances: stable series identity, active series, index/draw-order, remove, and deterministic revision behavior.
- Add focused tests for `Surface(SurfaceMatrix)`, `Waterfall(SurfaceMatrix)`, null argument guards, name normalization, collection immutability, empty `Clear()` no-op revision behavior, and mixed surface/waterfall/scatter lifecycle transitions.
- Keep implementation in `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/*` plus focused integration tests.
- Add Plot-owned professional presentation/evidence helpers that reuse existing `SurfaceChartOverlayOptions`, `SurfaceChartNumericLabelPresets`, `SurfaceChartOverlayPresets`, and `SurfaceColorMapPresets`.
- Refresh demo/support evidence to report Plot series count, active series identity, chart kind, color map, precision profile, and rendering status.

### Document

- Show concise `VideraChartView.Plot.Add.*` examples for surface, waterfall, and scatter.
- Explain that `VideraChartView.Plot` is chart-local and independent from `VideraView`.
- Explain that `Plot.ColorMap` and `Plot.OverlayOptions` are the supported presentation seams.

### Defer

- Additional chart families.
- Generic plotting engine semantics.
- Publication/export/layout tooling.
- GPU-driven chart runtime or renderer/backend expansion.

### Reject

- Restoring `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView`.
- Restoring public `VideraChartView.Source` / `SourceProperty`.
- Compatibility wrappers for removed alpha APIs.
- Hidden fallback/downshift behavior.
- A broad demo/workbench god component.

## Phase Handoff

### Phase 352: Plot Series Lifecycle and Naming Polish

Owned files:

- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs`
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewPlotApiTests.cs`

Expected shape:

- Add public active-series inspection without exposing internal runtime state.
- Add targeted `Remove(...)` or equivalent explicit lifecycle method.
- Preserve `Plot.Add.*` as the only data-loading path.
- Keep old chart views and direct `Source` absent.

### Phase 353: Professional Plot Presentation and Precision Wiring

Owned files:

- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/*`
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartProbeEvidence.cs`
- `src/Videra.SurfaceCharts.Core/SurfaceChartOutputEvidence.cs`
- relevant SurfaceCharts core/Avalonia tests

Expected shape:

- Reuse existing overlay/color-map presets.
- Make style/precision evidence reachable through Plot-owned chart-local APIs.
- Do not create a separate styling engine.

### Phase 354: Plot Demo and Support Evidence Refresh

Owned files:

- `samples/Videra.SurfaceCharts.Demo/*`
- `smoke/Videra.SurfaceCharts.ConsumerSmoke/*`
- focused sample/smoke tests under `tests/Videra.Core.Tests/Samples`

Expected shape:

- Report Plot series identity/count/kind and active presentation profile.
- Keep demo bounded and support-oriented.
- Continue to use only `Plot.Add.*`.

### Phase 355: Single Chart View Guardrails and Documentation

Owned files:

- `README.md`
- `src/Videra.SurfaceCharts.Avalonia/README.md`
- `samples/Videra.SurfaceCharts.Demo/README.md`
- `docs/*` SurfaceCharts package/support pages
- repository guardrail tests
- Beads export and generated roadmap

Expected shape:

- Lock public docs to `VideraChartView` plus `Plot.Add.*`.
- Strengthen guardrails only around concrete drift risks found in v2.50.
- Keep Beads and roadmap clean.

## Verification

Inventory verification was read-only:

- inspected Plot API files
- inspected integration and repository guardrail tests
- inspected demo and consumer-smoke Plot usage
- inspected SurfaceCharts docs and support evidence vocabulary
- confirmed no product code was changed in this phase

No product tests were run because Phase 351 only produced planning artifacts and Beads state.
