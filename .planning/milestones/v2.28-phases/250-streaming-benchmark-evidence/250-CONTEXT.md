# Phase 250: Streaming Benchmark Evidence - Context

**Gathered:** 2026-04-27
**Status:** Ready for planning
**Mode:** Autonomous, scoped from roadmap and existing benchmark contracts

<domain>
## Phase Boundary

Add evidence for SurfaceCharts columnar scatter append/FIFO cost and allocation behavior.

Measurements should remain evidence-only because the milestone explicitly says not to promote noisy metrics into hard gates without CI history.
</domain>

<decisions>
## Implementation Decisions

- Extend the existing `Videra.SurfaceCharts.Benchmarks` project.
- Use BenchmarkDotNet `MemoryDiagnoser` so allocation evidence is generated with the benchmark output.
- Add contract entries in `benchmark-contract.json`.
- Leave `benchmark-thresholds.json` unchanged.
</decisions>

<code_context>
## Existing Code Insights

- SurfaceCharts benchmarks already group evidence by families such as Selection, RenderState, Cache, Probe, RenderHostContract, and Diagnostics.
- Repository tests assert the benchmark contract and hard-threshold slices separately.
- `ScatterColumnarSeries` exposes append, replace, FIFO capacity, retained count, dropped count, and streaming counters needed for focused benchmark surfaces.
</code_context>

<specifics>
## Specific Ideas

- Add a `Streaming` benchmark family.
- Benchmark one unbounded append, one FIFO-trim append, and one diagnostics aggregation path.
- Build the benchmark project and run benchmark contract/threshold tests.
</specifics>

<deferred>
## Deferred Ideas

- No hard thresholds for streaming benchmarks in this milestone.
- No benchmark execution gate or baseline values until CI history proves stability.
</deferred>
