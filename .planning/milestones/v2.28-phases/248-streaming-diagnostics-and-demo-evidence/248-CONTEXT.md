# Phase 248: Streaming Diagnostics and Demo Evidence - Context

**Gathered:** 2026-04-27
**Status:** Ready for execution
**Mode:** Autonomous

<domain>
## Phase Boundary

Expose SurfaceCharts columnar streaming/FIFO truth through chart-local diagnostics and one focused demo evidence path.
</domain>

<decisions>
## Implementation Decisions

- Aggregate streaming counters from `ScatterColumnarSeries` through `ScatterChartData`.
- Publish those counters through `ScatterChartRenderingStatus`.
- Update the SurfaceCharts demo scatter proof support text instead of adding a new app or chart family.
- Use repository tests as packaged-path evidence for the public API shape.
</decisions>

<deferred>
## Deferred Ideas

- Interaction-quality refine policy remains Phase 249.
- Benchmark evidence remains Phase 250.
</deferred>
