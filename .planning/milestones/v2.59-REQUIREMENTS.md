# Requirements: v2.59 ScottPlot5 Interaction and Cookbook Experience

## Goal

Continue SurfaceCharts toward ScottPlot5-inspired interaction and code-experience optimization while turning demos into cookbook-style, copyable recipes. This milestone improves ergonomics and discoverability without claiming ScottPlot compatibility or adding compatibility layers.

## Active Requirements

### Interaction Experience

- **INT-01**: Inventory current SurfaceCharts interaction APIs, overlays, commands, demo flows, docs, and support evidence before selecting implementation slices.
- **INT-02**: Users can exercise the selected interaction improvements through simple, direct public APIs or demo-owned recipes without hidden fallback/downshift behavior.
- **INT-03**: Users can understand the selected interaction behavior through deterministic evidence, focused tests, and docs that preserve Videra's 3D chart boundaries.

### Code Experience

- **CODE-01**: Inventory the current `VideraChartView`, `Plot.Add.*`, plottable, axis, snapshot, and helper surfaces for code-experience friction.
- **CODE-02**: Users can write the selected first-chart or interaction code with less ceremony while staying on current public APIs.
- **CODE-03**: Repository tests and docs checks prevent obsolete chart controls, direct public `Source`, compatibility wrappers, or ScottPlot parity claims from re-entering the public surface.

### Cookbook Demo Experience

- **COOK-01**: Inventory current SurfaceCharts demo, README, docs index, cookbook, gallery, and support entry points.
- **COOK-02**: Users can discover cookbook-style SurfaceCharts recipes from public docs and demo README files.
- **COOK-03**: Users can copy current-public-API recipes for representative chart/interaction workflows without depending on internal demo-only infrastructure.

### Final Verification and Handoff

- **VERIFY-01**: Run focused tests and docs checks covering interaction/code-experience changes and cookbook/demo entry points.
- **VERIFY-02**: Synchronize Beads state, generated public roadmap, phase evidence, milestone archive, branch/worktree cleanup, Git push, and Dolt Beads push.
- **VERIFY-03**: Record any deferred ScottPlot-inspired ideas as future work without adding compatibility, parity, fallback, renderer/backend, or god-workbench scope.

## Future Requirements

- Broader chart-family expansion beyond current SurfaceCharts families.
- Full ScottPlot API parity or adapter behavior.
- Additional release/publication automation beyond the manual-gated v2.58 release boundary.

## Out of Scope

- ScottPlot compatibility adapters or parity claims.
- Restoring `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView`.
- Restoring a public direct `Source` API.
- Compatibility wrappers for removed alpha APIs.
- PDF/vector export or broad export-format expansion.
- Renderer/backend/platform expansion.
- Hidden fallback/downshift behavior.
- Generic plotting-engine or god-code demo/workbench expansion.
- Public NuGet publish, public tag creation, or GitHub release publication unless separately approved.

## Traceability

- **Phase 401**: INT-01, CODE-01, COOK-01
- **Phase 402**: INT-02, INT-03, CODE-02, CODE-03
- **Phase 403**: COOK-02, COOK-03, CODE-03
- **Phase 404**: VERIFY-01, VERIFY-02, VERIFY-03
