# Phase 91: Benchmark Compile Closure - Context

**Gathered:** 2026-04-20
**Status:** Completed and verified
**Mode:** Auto-generated (discuss skipped via autonomous infrastructure detection)

<domain>
## Phase Boundary

Repair the viewer benchmark sources so repo-wide verify and matching-host native validation no longer fail during the shared benchmark compile prelude.

</domain>

<decisions>
## Implementation Decisions

### the agent's Discretion
- This is a pure infrastructure repair phase. Implementation choices stay narrow and should optimize for compile correctness, stable regression evidence, and minimal surface-area change.
- Prefer compile-backed validation (`Videra.slnx`, `scripts/verify.ps1`, and targeted benchmark-project builds) over brittle source-string repository guards.
- Do not widen public API or change benchmark intent just to work around the compile break.

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `tests/Videra.Core.Tests/Repository/RepositoryArchitectureTests.cs` already hosts repository/source-contract guards for benchmark presence and architecture constraints.
- `benchmarks/Videra.Viewer.Benchmarks/InspectionBenchmarks.cs` and `ScenePipelineBenchmarks.cs` are the only viewer benchmark sources currently instantiating static runtime services.

### Established Patterns
- Runtime services `VideraSnapshotExportService` and `SceneDeltaPlanner` are internal static helpers and are normally consumed through static method calls.
- The repo green-line path already compiles `benchmarks/Videra.Viewer.Benchmarks` because `Videra.slnx` includes that project and `scripts/verify.ps1` builds the solution up front.

### Integration Points
- `scripts/verify.ps1` and matching-host native validation hit these benchmark sources during the shared solution build/test prelude.
- Fixing the benchmark-source contract should unblock later validation phases without touching the public viewer/runtime boundary.

</code_context>

<specifics>
## Specific Ideas

- The red line is specifically `CS0723` from instantiating `VideraSnapshotExportService` and `SceneDeltaPlanner`.
- Regression evidence should stay anchored in benchmark/solution compilation so CI catches the misuse through the same path it actually exercises.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>
