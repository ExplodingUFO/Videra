# Requirements: v2.56 ScottPlot 5 Interaction and Cookbook Experience

## Scope

v2.56 continues the v2.55 ScottPlot-like Plot API work. The focus shifts from first-chart data loading to the surrounding user experience: interaction configuration, concise code operations, live-view behavior, and a demo that reads like a cookbook.

Reference basis:

- ScottPlot 5 keeps first-use code short through `Plot.Add.*`, typed plottable handles, `Plot.Axes`, and `SavePng(...)` patterns.
- ScottPlot 5's demo emphasizes mouse-interactive recipes such as draggable axis lines/spans, selectable data points, custom mouse actions, custom context menu, shared axes, axis rules, and live data logger/streamer examples.
- Videra remains a 3D SurfaceCharts library, so the goal is inspired ergonomics, not full ScottPlot API compatibility.

## v1 Requirements

### Inventory, Reference, and Coordination

- [x] **INV-01**: Inventory current `Plot3D`, plottable handle, interaction overlay, axis, live scatter, demo, support evidence, and guardrail surfaces before implementation.
- [x] **INV-02**: Record exact ScottPlot 5 reference patterns used for this milestone: interactive demo recipes, custom mouse actions, custom context menu, shared axes, axis rules, data logger/streamer view management, and cookbook organization.
- [x] **INV-03**: Create Beads issues for every phase with parent-child links, blocking dependencies, ownership boundaries, validation expectations, and safe worktree/branch handoff notes.

### Plot Code Experience

- [x] **CODE-01**: Users can clear, remove, and reorder Plot-owned plottables with concise `Plot` methods while preserving deterministic revision and evidence truth.
- [x] **CODE-02**: Users can inspect Plot-owned plottables through a typed read-only list or query API without exposing mutable runtime internals.
- [x] **CODE-03**: Common plottable tweaks already supported by the series model remain reachable from returned handles without compatibility wrapper types.
- [x] **CODE-04**: Code experience changes stay chart-local and do not widen backend/runtime ownership.

### Interaction Configuration and Commands

- [x] **INT-01**: Users can configure the built-in chart interaction profile from a small public API: pan, zoom, reset/autoscale, keyboard shortcuts, and focus behavior.
- [x] **INT-02**: Users can disable specific built-in interactions explicitly, with no hidden fallback/downshift behavior.
- [x] **INT-03**: Users can attach bounded custom commands for common chart actions without introducing a generic plugin/workbench command system.
- [x] **INT-04**: Context-menu or toolbar command recipes reuse the same command surface and remain optional.

### Selection, Probe, and Draggable Overlay Recipes

- [ ] **PICK-01**: Users can convert pointer positions into chart-local probe/coordinate results through a concise, testable API.
- [ ] **PICK-02**: Users can select points, series, or bounded ranges through click or rectangle gestures while keeping selected state host-owned.
- [ ] **PICK-03**: Users can create bounded draggable marker/range overlay recipes that update chart-local axis or marker state explicitly.
- [ ] **PICK-04**: Selection and draggable overlays produce deterministic support evidence and do not mutate source data silently.

### Axis Rules, Linked Views, and Live View Management

- [x] **AXIS-01**: Users can define axis limit rules for lock, min/max bounds, and autoscale constraints through `Plot.Axes` without moving axis truth into dataset metadata.
- [x] **AXIS-02**: Users can link axis/view limits across two `VideraChartView` instances with explicit lifetime and no global registry.
- [x] **LIVE-01**: `DataLogger3D` or related live helpers can manage latest-window or full-data view behavior explicitly.
- [x] **LIVE-02**: Live view management exposes deterministic counters/evidence for appended points, dropped points, visible window, and autoscale decisions.

### Cookbook Demo and Documentation

- [ ] **COOK-01**: The SurfaceCharts demo provides a cookbook/gallery navigation model with recipe groups for first chart, styling, interactions, live data, linked axes, and export.
- [ ] **COOK-02**: Each cookbook recipe has a small isolated setup path, visible result, and copyable code snippet or matching README snippet.
- [ ] **COOK-03**: Cookbook recipes avoid shared hidden mutable state and do not turn the demo into a god-code chart editor.
- [ ] **DOC-01**: Root README and SurfaceCharts demo README link to the cookbook recipes and explain the Videra-specific 3D boundaries.
- [ ] **DOC-02**: Documentation names ScottPlot 5 as an ergonomics inspiration while explicitly rejecting full compatibility/parity.

### Verification and Closure

- [ ] **VER-01**: Focused tests cover Plot lifecycle/code experience, interaction profile behavior, selection/probe/draggable recipes, linked axes, live view management, and cookbook demo wiring.
- [ ] **VER-02**: Repository guardrails continue to reject old chart controls, direct public `Source`, compatibility wrappers, PDF/vector export, backend expansion, hidden fallback/downshift, and god-code workbench scope.
- [ ] **VER-03**: Beads state, generated public roadmap, phase evidence, branch/worktree state, and handoff notes are synchronized at milestone close.

## Future Requirements

- Full ScottPlot 5 API compatibility.
- Arbitrary user-defined custom plottables.
- Heterogeneous multiplot layout or subplot system.
- Visual regression gating for cookbook screenshots.
- Touch/mobile gesture support.
- Package release or public version tag.

## Out of Scope

| Exclusion | Reason |
|-----------|--------|
| Full ScottPlot compatibility layer | v2.56 targets inspired ergonomics, not API parity or adapter shims. |
| `SurfaceChartView`, `WaterfallChartView`, `ScatterChartView` | `VideraChartView` remains the single shipped chart control. |
| Public `VideraChartView.Source` | `Plot.Add.*` remains the public data-loading path. |
| PDF/vector export | Existing scope remains chart-local PNG/bitmap snapshot export only. |
| Backend/runtime rewrite | Interaction and cookbook work must stay chart-local unless a tiny explicit bridge is required. |
| Hidden fallback/downshift | Unsupported interaction or output behavior must be explicit. |
| Generic plugin/workbench command system | Command recipes stay bounded to chart actions. |
| God-code demo editor | Demo becomes a cookbook/gallery, not a general chart builder. |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| INV-01 | Phase 383 | Complete |
| INV-02 | Phase 383 | Complete |
| INV-03 | Phase 383 | Complete |
| CODE-01 | Phase 384 | Complete |
| CODE-02 | Phase 384 | Complete |
| CODE-03 | Phase 384 | Complete |
| CODE-04 | Phase 384 | Complete |
| INT-01 | Phase 385 | Complete |
| INT-02 | Phase 385 | Complete |
| INT-03 | Phase 385 | Complete |
| INT-04 | Phase 385 | Complete |
| PICK-01 | Phase 386 | Pending |
| PICK-02 | Phase 386 | Pending |
| PICK-03 | Phase 386 | Pending |
| PICK-04 | Phase 386 | Pending |
| AXIS-01 | Phase 387 | Complete |
| AXIS-02 | Phase 387 | Complete |
| LIVE-01 | Phase 387 | Complete |
| LIVE-02 | Phase 387 | Complete |
| COOK-01 | Phase 388 | Pending |
| COOK-02 | Phase 388 | Pending |
| COOK-03 | Phase 388 | Pending |
| DOC-01 | Phase 388 | Pending |
| DOC-02 | Phase 388 | Pending |
| VER-01 | Phase 389 | Pending |
| VER-02 | Phase 389 | Pending |
| VER-03 | Phase 389 | Pending |
