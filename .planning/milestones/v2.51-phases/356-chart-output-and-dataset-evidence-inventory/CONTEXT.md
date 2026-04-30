# Phase 356: Chart Output and Dataset Evidence Inventory - Context

**Gathered:** 2026-04-29
**Status:** Ready for planning
**Bead:** Videra-779.1

<domain>
## Phase Boundary

Inventory the current chart output, report, dataset metadata, demo/support, consumer smoke, Doctor, docs, tests, and guardrail surfaces before changing code.

This is a read-only phase. It must hand off bounded write sets for Phase 357 and Phase 358, and keep the v2.51 scope chart-local to `VideraChartView.Plot`.
</domain>

<decisions>
## Implementation Decisions

- Keep `VideraChartView` as the only shipped chart control.
- Keep `VideraChartView.Plot.Add.Surface(...)`, `.Waterfall(...)`, and `.Scatter(...)` as the runtime data-loading path.
- Do not restore old chart views, public direct `Source`, compatibility wrappers, hidden fallback/downshift behavior, backend expansion, generic plotting engine scope, or a broad workbench/editor.
- Treat output and dataset evidence as deterministic support/report contracts, not image export, benchmark truth, replay files, or visual regression baselines.
</decisions>

<code_context>
## Existing Code Insights

- Plot model owner: `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs`.
- Plot series owner: `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs`.
- Plot add API owner: `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs`.
- View owner: `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs`.
- Existing output evidence owner: `src/Videra.SurfaceCharts.Core/SurfaceChartOutputEvidence.cs`.
- Dataset truth:
  - `SurfaceMetadata` exposes width, height, sample count, geometry, axis descriptors, and value range.
  - `ScatterChartData` exposes metadata, series count, point count, columnar counts, FIFO/streaming counters, and pickable point count.
  - `ScatterColumnarSeries` exposes sorted/NaN/pickable/FIFO flags and streaming counters.
- Demo support summary owner: `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs`.
- Packaged smoke support contract owner: `scripts/Invoke-ConsumerSmoke.ps1`.
- Doctor parser owner: `scripts/Invoke-VideraDoctor.ps1`.
- Repository guardrails owner: `tests/Videra.Core.Tests/Repository/SurfaceChartsRepositoryArchitectureTests.cs`.
- Plot integration tests owner: `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewPlotApiTests.cs`.
- Core output evidence tests owner: `tests/Videra.SurfaceCharts.Core.Tests/SurfaceChartOutputEvidenceTests.cs`.
</code_context>

<specifics>
## Specific Ideas

### Implement in Phase 357

- Add a Plot-owned report surface, for example `VideraChartView.Plot.CreateOutputReport(...)` or `CreateOutputEvidence(...)`.
- Output evidence should include chart control/model identifiers, active chart kind, series count, active series name/kind/index, color map, precision profile, rendering status summary, and unsupported capability diagnostics.
- Keep the public contract deterministic and data-only. Do not introduce image/PDF/vector export, render capture, backend-specific details, or automatic fallback.

### Implement in Phase 358

- Add deterministic dataset evidence derived from `Plot3DSeries`.
- Surface/waterfall evidence should use `ISurfaceTileSource.Metadata`.
- Scatter evidence should use `ScatterChartData.Metadata`, point counts, columnar/streaming counters, pickable count, and FIFO settings.
- Evidence should update after add/remove/clear/replace because it reads current Plot truth instead of cached runtime internals.

### Implement in Phase 359

- Add the new report/dataset fields to demo support summaries and packaged consumer smoke validation.
- Extend Doctor parsing for the new fields.
- Keep output evidence usable without requiring a running UI when parsing saved artifacts.

### Implement in Phase 360

- Update docs and guardrail tests.
- Regenerate public Beads roadmap.
- Close Beads and planning state.
</specifics>

<deferred>
## Deferred Ideas

- Actual bitmap, PDF, SVG, or vector export pipeline.
- Publication-layout engine.
- Visual regression screenshot gate.
- New chart families.
- Backend/render-host expansion.
- Generic plotting engine.
</deferred>
