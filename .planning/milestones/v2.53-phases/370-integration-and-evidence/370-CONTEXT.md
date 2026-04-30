# Phase 370: Integration and Evidence - Context

**Gathered:** 2026-04-29
**Status:** Ready for planning
**Mode:** Auto-generated (integration/evidence phase)

<domain>
## Phase Boundary

All new chart types are wired into demo, diagnostics, evidence contracts, and repository guardrails. Bar and Contour produce output evidence and dataset evidence. Demo exposes sample data and UI for new types. Consumer smoke validates rendering. Existing surface/waterfall/scatter shows no regression.

</domain>

<decisions>
## Implementation Decisions

### the agent's Discretion
- Wire Bar and Contour into Plot3DOutputEvidence contract
- Wire Bar and Contour into Plot3DDatasetEvidence contract
- Add Bar and Contour sample data to demo
- Add Bar and Contour validation to consumer smoke
- Verify existing surface/waterfall/scatter no regression
- Follow existing evidence contract patterns from Plot3DOutputEvidence/Plot3DDatasetEvidence

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- Phase 366: Axis Foundation (Log, DateTime, custom formatters)
- Phase 367: Enhanced Chart Legend (multi-series, per-kind indicators)
- Phase 368: Bar Chart Series (Plot.Add.Bar, grouped/stacked)
- Phase 369: Contour Plot Series (Plot.Add.Contour, marching squares)
- Plot3DOutputEvidence — output evidence contract
- Plot3DDatasetEvidence — dataset evidence contract
- Consumer smoke MainWindow — existing smoke validation pattern

### Established Patterns
- Evidence contracts in `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/`
- Demo in `samples/Videra.SurfaceCharts.Demo/`
- Consumer smoke in `smoke/Videra.SurfaceCharts.ConsumerSmoke/`

</code_context>

<specifics>
## Specific Ideas

- Output evidence should report Bar/Contour series kind
- Dataset evidence should include Bar/Contour dimensions
- Demo should show Bar chart with sample data and Contour plot with sample scalar field
- Consumer smoke should add Bar and Contour series and validate rendering

</specifics>

<deferred>
## Deferred Ideas

None — this is the final milestone phase.

</deferred>
