# Phase 251: Documentation and Repository Truth Closure - Context

**Gathered:** 2026-04-27
**Status:** Ready for planning
**Mode:** Autonomous, scoped from roadmap and changed public contracts

<domain>
## Phase Boundary

Close documentation and repository truth around the v2.28 streaming/FIFO, scatter interaction-quality, and benchmark evidence surfaces.
</domain>

<decisions>
## Implementation Decisions

- Update existing public docs instead of adding a new broad architecture page.
- Keep SurfaceCharts and viewer ownership boundaries explicit.
- Guard key terms through existing repository documentation tests.
- Keep streaming benchmark docs explicit that the new measurements are evidence-only, not hard thresholds.
</decisions>

<code_context>
## Existing Code Insights

- Root, SurfaceCharts module READMEs, demo README, Chinese SurfaceCharts docs, and benchmark runbook already contain related SurfaceCharts truth.
- Repository tests already centralize SurfaceCharts documentation terms and benchmark contract checks.
- Public API contract tests pass after the scatter interaction-quality additions.
</code_context>

<specifics>
## Specific Ideas

- Document `ScatterColumnarSeries`, `ReplaceRange(...)`, `AppendRange(...)`, `fifoCapacity`, `Pickable=false`, and streaming counters.
- Document scatter `InteractionQuality` on `ScatterChartRenderingStatus`.
- Document streaming benchmarks as `Mean` / `Allocated` evidence-only.
- Add repository tests for those phrases and threshold exclusion.
</specifics>

<deferred>
## Deferred Ideas

- No release notes or public package publication in this milestone.
- No new hard benchmark thresholds for streaming metrics.
</deferred>
