# Requirements: v2.55 ScottPlot-like Plot API

## Scope

v2.55 starts from the v2.54 SurfaceCharts baseline: `VideraChartView` is the single shipped chart control; `Plot.Add.*` is the public data-loading path; Surface, Waterfall, Scatter, Bar, and Contour series are available; chart-local PNG snapshot export and interactivity are shipped.

The milestone improves first-chart ergonomics so a new user can write short, ScottPlot-like code without constructing internal data models. The work stays in the facade/presentation layer unless a small core helper is required for live scatter data.

## v1 Requirements

### Inventory and Boundaries

- [x] **INV-01**: Inventory current `Plot3D`, `Plot3DAddApi`, series handle, axis, snapshot, demo, smoke, and guardrail surfaces before implementation.
- [x] **INV-02**: Record exact non-goals: no old chart controls, no public `Source`, no compatibility wrappers, no PDF/vector export, no backend/runtime rewrite, no hidden fallback/downshift, no generic plotting engine, no god-code workbench.
- [x] **INV-03**: Define disjoint implementation beads and worktree boundaries before parallel work begins.

### Plot.Add Ergonomics and Plottables

- [x] **ADD-01**: `Plot.Add.Surface(...)` accepts raw 2D numeric arrays without requiring callers to construct `SurfaceMetadata` or `SurfaceMatrix`.
- [x] **ADD-02**: `Plot.Add.Waterfall(...)` accepts raw 2D numeric arrays without requiring callers to construct `SurfaceMetadata` or `SurfaceMatrix`.
- [x] **ADD-03**: `Plot.Add.Scatter(...)` accepts raw X/Y/Z arrays or point-like numeric inputs without requiring callers to construct `ScatterChartData`.
- [x] **ADD-04**: Existing advanced overloads remain available without adding compatibility wrapper types.
- [x] **PLOT-01**: `Plot.Add.*` returns typed plottable handles through a small public `IPlottable3D` contract.
- [x] **PLOT-02**: Plottable handles expose `Label` and `IsVisible` consistently.
- [x] **PLOT-03**: Plottable handles expose existing color or color-map settings where the series kind already supports them.

### Axes and Snapshot Convenience

- [x] **AXIS-01**: `Plot.Axes` provides discoverable X/Y/Z axis facade objects.
- [x] **AXIS-02**: Axis facade supports labels without moving dataset metadata ownership into `Plot3D`.
- [x] **AXIS-03**: Axis facade supports `SetLimits` and `AutoScale` through the existing `VideraChartView` viewport/runtime ownership.
- [x] **PNG-01**: `Plot.SavePngAsync(...)` is a convenience wrapper over existing chart-local snapshot export.
- [x] **PNG-02**: PNG convenience keeps unsupported output explicit and does not introduce PDF/vector export.

### Live Scatter Data

- [x] **LIVE-01**: Add a first-class `DataLogger3D` or `ScatterStream` API that reuses existing scatter columnar append/replace/FIFO semantics.
- [x] **LIVE-02**: Live data API exposes deterministic counters/evidence already supported by scatter columnar series.
- [x] **LIVE-03**: Live data API avoids a new render loop, scheduler, or background worker abstraction.

### Multi-Series Composition

- [x] **MULTI-01**: Same-type series compose instead of relying only on last-series-wins active rendering.
- [x] **MULTI-02**: Legend, probe, snapshot, and evidence paths identify visible composed series deterministically.
- [x] **MULTI-03**: Multi-series support is scoped to existing series kinds and does not introduce new chart types.

### Cookbook, Demo, and Verification

- [x] **DOC-01**: Root README and SurfaceCharts demo README show cookbook-style recipes for first surface, scatter, axes, snapshot PNG, and live scatter.
- [x] **DEMO-01**: Demo exercises concise `Plot.Add.*`, `Plot.Axes`, `SavePngAsync`, and live scatter paths without turning into a general workbench.
- [ ] **VER-01**: Focused tests cover new facade APIs and regression guardrails.
- [ ] **VER-02**: Beads state, generated roadmap, and handoff notes stay synchronized with phase progress.
- [ ] **VER-03**: Existing snapshot-export scope guardrail remains passing.

## Future Requirements

- Heterogeneous multi-series composition across unrelated chart kinds.
- Touch/mobile chart gestures.
- Vector/PDF export.
- Broader ScottPlot API parity beyond the first-chart facade.
- Renderer/backend-level chart architecture changes.

## Out of Scope

| Exclusion | Reason |
|-----------|--------|
| `SurfaceChartView`, `WaterfallChartView`, `ScatterChartView` | `VideraChartView` is the single shipped chart control. |
| Public `VideraChartView.Source` | `Plot.Add.*` remains the public data-loading path. |
| PDF/vector export | v2.55 is API ergonomics; PNG already exists through chart-local bitmap snapshot export. |
| Backend/runtime rewrite | Facade layer only unless a tiny core helper is required. |
| Compatibility wrappers | Removed alpha APIs stay removed. |
| Hidden fallback/downshift | Unsupported behavior stays explicit. |
| Generic plotting engine | This milestone is a bounded SurfaceCharts facade slice. |
| God-code workbench | Demo changes stay cookbook/reference-app sized. |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| INV-01 | Phase 376 | Complete |
| INV-02 | Phase 376 | Complete |
| INV-03 | Phase 376 | Complete |
| ADD-01 | Phase 377 | Complete |
| ADD-02 | Phase 377 | Complete |
| ADD-03 | Phase 377 | Complete |
| ADD-04 | Phase 377 | Complete |
| PLOT-01 | Phase 377 | Complete |
| PLOT-02 | Phase 377 | Complete |
| PLOT-03 | Phase 377 | Complete |
| AXIS-01 | Phase 378 | Complete |
| AXIS-02 | Phase 378 | Complete |
| AXIS-03 | Phase 378 | Complete |
| PNG-01 | Phase 378 | Complete |
| PNG-02 | Phase 378 | Complete |
| LIVE-01 | Phase 379 | Complete |
| LIVE-02 | Phase 379 | Complete |
| LIVE-03 | Phase 379 | Complete |
| MULTI-01 | Phase 380 | Complete |
| MULTI-02 | Phase 380 | Complete |
| MULTI-03 | Phase 380 | Complete |
| DOC-01 | Phase 381 | Complete |
| DEMO-01 | Phase 381 | Complete |
| VER-01 | Phase 382 | Pending |
| VER-02 | Phase 382 | Pending |
| VER-03 | Phase 382 | Pending |
