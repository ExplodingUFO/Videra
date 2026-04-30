# Phase 244: Benchmark Evidence and Threshold Promotion - Context

**Gathered:** 2026-04-27
**Status:** Ready for planning
**Mode:** Autonomous continuation from v2.27 roadmap

<domain>
## Phase Boundary

Add benchmark evidence for the new viewer instance-batch runtime path. This phase should make normal-object versus instance-batch costs visible through the existing benchmark contract, while avoiding hard thresholds until enough data exists.

</domain>

<decisions>
## Implementation Decisions

- Keep this phase evidence-only for the new instance-batch benchmarks.
- Do not add hard thresholds to `benchmark-thresholds.json` without historical stability data.
- Reuse the existing Viewer benchmark project and contract tests.
- Include meaningful counts through BenchmarkDotNet params instead of adding multiple benchmark classes.

</decisions>

<code_context>
## Existing Code Insights

- `benchmarks/benchmark-contract.json` is the canonical benchmark evidence contract.
- `benchmark-thresholds.json` is the hard-gate subset.
- `BenchmarkContractRepositoryTests` locks benchmark names.
- `docs/benchmark-gates.md` documents evidence-only versus thresholded benchmarks.

</code_context>

<specifics>
## Specific Ideas

- Add `InstanceBatchBenchmarks` under `benchmarks/Videra.Viewer.Benchmarks`.
- Compare `SceneDocument` population for pre-existing normal objects versus retained instance batch entry creation.
- Compare normal object hit-test and instance-batch hit-test latency.
- Add a diagnostics snapshot benchmark that exercises retained instance count/pickable count fields.

</specifics>

<deferred>
## Deferred Ideas

- Promote instance-batch metrics into hard thresholds.
- Run full benchmark jobs in this local phase.
- Add GPU draw-call thresholds before real backend draw-call reporting exists.

</deferred>
