# Phase 365: Snapshot Export Guardrails and Docs - Context

**Gathered:** 2026-04-29
**Status:** Ready for planning
**Mode:** Auto-generated (docs/guardrails phase)

<domain>
## Phase Boundary

Align docs, public roadmap, repository guardrails, and Beads state around the chart-local bitmap snapshot export contract. Guardrails block old chart controls and direct public Source APIs from returning. Guardrails keep snapshot export work out of PDF/vector export, backend expansion, generic plotting engine scope, compatibility wrappers, hidden fallback/downshift behavior, and god-code. Package/sample docs show concise Plot-owned snapshot export and manifest usage.

</domain>

<decisions>
## Implementation Decisions

### the agent's Discretion
All implementation choices are at the agent's discretion — docs/guardrails phase. Use ROADMAP phase goal, success criteria, and codebase conventions. Key deliverables:
- Update AGENTS.md or project docs with snapshot export scope boundaries
- Add/update guardrail scripts that verify no old chart APIs or direct Source reappear
- Update package/sample docs with Plot-owned snapshot API usage examples
- Update Beads state to reflect milestone completion
- Clean up local planning state

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- Phase 361 SUMMARY.md — inventory and gap classification
- Phase 362-01 SUMMARY.md — contract types documentation
- Phase 363-01 SUMMARY.md — capture implementation documentation
- Phase 364-01 SUMMARY.md — evidence integration documentation
- AGENTS.md — project instructions file
- ROADMAP.md — milestone roadmap

### Established Patterns
- Guardrail scripts in `scripts/` directory
- Docs in project root and `docs/` directory
- Beads coordination via `bd` CLI

### Integration Points
- `AGENTS.md` — add snapshot export scope rules
- `ROADMAP.md` — mark milestone phases as complete
- Beads — close milestone beads

</code_context>

<specifics>
## Specific Ideas

- Guardrail should verify: no `SurfaceChartView`/`WaterfallChartView`/`ScatterChartView` public types, no direct `Source` API, no PDF/vector export code
- Docs should show: `PlotSnapshotRequest` → `CaptureSnapshotAsync` → `PlotSnapshotResult` with manifest
- Beads: close Videra-lu9.1 through Videra-lu9.5, then Videra-lu9 epic

</specifics>

<deferred>
## Deferred Ideas

None — this is the final milestone phase.

</deferred>
