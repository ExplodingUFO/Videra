# Phase 433: Promote Bar+Contour to Production - Context

**Gathered:** 2026-04-30
**Status:** Ready for planning
**Mode:** Auto-generated (infrastructure phase — smart discuss skipped)

<domain>
## Phase Boundary

Move Bar and Contour chart families from proof-path to the public package API
contract. Both chart types already work in the demo and have tests — this phase
makes them official consumers of the package surface.

</domain>

<decisions>
## Implementation Decisions

### Claude's Discretion
All implementation choices are at Claude's discretion — pure infrastructure phase.
Bar and Contour already exist as proof-path implementations. The task is to promote
them to the public API contract by ensuring they appear in the public surface,
package validation scripts, and NuGet package metadata.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- Bar chart: `Plot3DBarSeries`, `BarRenderer`, `BarRenderScene`, `BarProbeStrategy` — all exist
- Contour chart: `Plot3DContourSeries`, `ContourRenderer`, `ContourRenderScene`, `ContourProbeStrategy` — all exist
- Both have `Plot.Add.Bar(...)` and `Plot.Add.Contour(...)` methods in `Plot3DAddApi`
- Both have tests, demo scenarios, and cookbook recipes

### Established Patterns
- Chart types follow a 7-step integration pattern (from Phase 432 inventory)
- Public API surface is controlled by package metadata and validation scripts
- Proof-path implementations may have `internal` access modifiers that need `public` promotion

### Integration Points
- `Plot3DAddApi` — the single authoring entry point for all chart types
- NuGet package surface — controls what consumers can access
- Package validation scripts — verify public surface includes expected types

</code_context>

<specifics>
## Specific Ideas

No specific requirements — infrastructure phase. Refer to ROADMAP phase description
and success criteria. Key requirement: existing Bar and Contour tests, demo scenarios,
and cookbook recipes must continue to pass without modification after promotion.

</specifics>

<deferred>
## Deferred Ideas

None — infrastructure phase, scope is clear.

</deferred>
