# Phase 361: Chart Snapshot Export Inventory - Context

**Gathered:** 2026-04-29
**Status:** Ready for planning
**Mode:** Auto-generated (infrastructure/inventory phase)

<domain>
## Phase Boundary

Map the current chart screenshot/export, output evidence, dataset evidence, demo/support, consumer smoke, Doctor, docs, tests, and guardrail surfaces before changing code. The inventory classifies bitmap snapshot/export gaps as implement, document, defer, or reject; target examples show concise Plot-owned snapshot export and manifest usage; non-goals explicitly reject old chart views, direct `Source`, PDF/vector export, generic plotting engine scope, backend expansion, hidden fallback/downshift, and god-code; handoff identifies implementation owners and write boundaries.

</domain>

<decisions>
## Implementation Decisions

### the agent's Discretion
All implementation choices are at the agent's discretion — pure inventory/investigation phase. Use ROADMAP phase goal, success criteria, and codebase conventions to guide decisions. The inventory should cover:
- Current chart screenshot/export surfaces in `VideraChartView`, `Plot3D`, and evidence contracts
- Output evidence (`Plot3DOutputEvidence`) and dataset evidence (`Plot3DDatasetEvidence`)
- Demo/support/consumer smoke samples and their current snapshot capabilities
- Doctor parsing surfaces
- Tests and guardrails related to chart output/export
- Existing gaps that the snapshot export vertical must fill

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `VideraChartView` — the single shipped chart control
- `Plot3D` — core plot API with `Add.Surface()`, `.Waterfall()`, `.Scatter()`
- `Plot3DOutputEvidence` — chart-local output evidence contract
- `Plot3DDatasetEvidence` — chart-local dataset evidence contract
- `SurfaceChartProbeEvidence` — probe evidence formatter
- `WorkbenchSupportCapture` — support capture formatter in workbench sample

### Established Patterns
- Chart-local evidence contracts live in `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/`
- Output evidence tests in `tests/Videra.SurfaceCharts.Core.Tests/SurfaceChartOutputEvidenceTests.cs`
- Integration tests cover waterfall, state, plot API, lifecycle, GPU fallback, probe overlay
- Demo/sample patterns in `samples/Videra.SurfaceCharts.Demo/`, `samples/Videra.AvaloniaWorkbenchSample/`
- Consumer smoke in `smoke/Videra.SurfaceCharts.ConsumerSmoke/`

### Integration Points
- `VideraChartView.Plot` is the public API entry point
- Evidence contracts are chart-local (not in Core or base Avalonia)
- Workbench sample aggregates evidence for support capture

</code_context>

<specifics>
## Specific Ideas

No specific requirements — inventory phase. Refer to ROADMAP phase description and success criteria.

</specifics>

<deferred>
## Deferred Ideas

None — inventory phase scope.

</deferred>
