# Phase 17 Research: Large-Dataset Residency, Cache Evolution, and Optional Rust Spike

**Phase:** 17  
**Name:** Large-Dataset Residency, Cache Evolution, and Optional Rust Spike  
**Date:** 2026-04-14  
**Status:** Ready for planning

## Objective

Answer the planning question for this phase:

> How do we make the surface-chart data path scale for large datasets on top of the shipped Phase 16 render-host seam, without collapsing the chart boundary or turning optional Rust work into an architecture rewrite?

## Current Code Reality

### 1. Scheduler supersession exists, but detail work is still sequential and restart-heavy

`SurfaceChartController` already supersedes older pipelines with a generation counter and a `CancellationTokenSource`, but the actual work model is still:

1. request overview tile
2. ask `SurfaceLodPolicy` for the current viewport selection
3. iterate `selection.EnumerateTileKeys()` in row-major order
4. await each `GetTileAsync` one at a time

That means the control can cancel stale pipelines, but it cannot keep interaction responsive under heavy viewport churn because detail requests are still serialized and re-issued from scratch.

### 2. Residency bookkeeping is too coarse for responsive interaction

`SurfaceTileCache` tracks requested keys and loaded tiles by generation, which is a good base, but its only pruning policy is effectively "keep overview, drop everything else." `SurfaceChartController.UpdateViewport` and `UpdateViewSize` call `PruneDetailTiles()` before launching a new pipeline, so nearby detail tiles are discarded even when the next viewport still overlaps them.

This is the main residency mismatch with the Phase 16 renderer seam: `SurfaceChartRenderState` can hold stable resident render tiles, but the data path keeps throwing detail tiles away.

### 3. Cache-backed reads are lazy, but each tile still reopens and recopies payload data

`SurfaceCacheReader.ReadAsync` eagerly validates and indexes the manifest, which is good. But `LoadTileAsync` still:

- opens the payload file for each tile read
- allocates a byte buffer per tile
- copies bytes into a float array per tile

The manifest plus payload-sidecar format is stable and explicit. The bottleneck is not the basic format shape; it is the per-request reopen and copy behavior.

### 4. Pyramid reduction is average-only, while truth consumers already need more than averages

`SurfacePyramidBuilder` currently reduces source blocks to a single averaged sample per output cell. `InMemorySurfaceTileSource` later computes tile `ValueRange` from the reduced matrix values it slices for the requested tile.

That is enough for rendering a coarse mesh, but it is not enough for truthful large-dataset behavior:

- peaks can disappear into averages
- legend truth can drift toward reduced values
- probe fallbacks need a clearer distinction between exact and aggregated data

The current code already has two consumers that care about this:

- `SurfaceProbeService`
- `SurfaceLegendOverlayPresenter`

### 5. Phase 16 already solved the renderer ownership problem

The important constraint from Phase 16 is that the renderer seam is now real:

- `SurfaceChartRenderHost`
- `SurfaceChartRenderState`
- `SurfaceChartRenderChangeSet`
- `SurfaceChartResidentTile`

Phase 17 should feed better residency and data into this seam. It should not redesign renderer ownership, move chart semantics into `VideraView`, or relitigate GPU versus software truth.

### 6. The repo has native-loading patterns, but no chart-native hotspot boundary yet

There is already a repo-native pattern for dynamic library loading in `Videra.Core.NativeLibrary.NativeLibraryHelper`. That is enough for a future narrow chart-native helper if profiling ever proves it is necessary.

What does *not* exist yet is a chart-local, coarse-grained hotspot boundary around reduction or batch cache reads. Right now, even the lower-level hot loops live as ad hoc managed loops in the production code.

### 7. Existing research already points at the right tools

The milestone-level research in `.planning/research/STACK.md` already recommends:

- `BenchmarkDotNet` for repeatable hotspot measurements
- `dotnet-trace` / `dotnet-counters` for allocation and churn visibility
- keeping Rust narrow, optional, and late

So Phase 17 does not need a fresh tool direction. It needs the chart-specific application of that direction.

## Design Options

### Scheduler and residency

#### Option A: Keep sequential detail loading and only clean up cancellation edges

This would preserve the current overview-first flow and only tighten generation checks. It is the lowest-risk change, but it does not satisfy `REND-03` or `DATA-01` because view-aware prioritization and bounded parallelism would still be missing.

**Conclusion:** Reject

#### Option B: Add a view-aware request planner plus bounded concurrent execution

Keep `SurfaceLodPolicy` as the source of which tiles matter, but introduce a scheduler-local planning step that:

- separates overview, visible tiles, and outer-neighborhood tiles
- orders detail work by viewport relevance instead of row-major enumeration
- caps in-flight requests
- preserves a bounded active neighborhood instead of dropping every detail tile

This aligns with the shipped render-state seam and can be verified entirely through existing test infrastructure.

**Conclusion:** Recommended

#### Option C: Build a long-lived background residency manager with broad speculative prefetch

This could eventually be useful, but it introduces too much new policy for one phase. The current acceptance target is responsive interaction on large datasets, not a full predictive streaming system.

**Conclusion:** Defer

### Cache I/O evolution

#### Option A: Eagerly preload the entire payload sidecar into memory

This removes reopen overhead, but it turns file-backed caches into memory-hungry caches and weakens the story for very large datasets.

**Conclusion:** Reject

#### Option B: Introduce a persistent payload session and additive batch reads

Keep the current manifest-plus-sidecar layout, but open the payload once per source/session and add a batched read path for callers that can benefit from grouping.

This is compatible with the current `ISurfaceTileSource` surface and with a future benchmark-driven native helper.

**Conclusion:** Recommended

#### Option C: Rewrite the cache format before optimizing reads

This is the highest-risk path. Nothing in the current evidence shows that raw float payloads are the first thing that must change.

**Conclusion:** Defer unless benchmarks prove a format limitation

### Reducers and truthful statistics

#### Option A: Keep average-only reduction and treat `ValueRange` as sufficient

This leaves the current blind spots intact. It is not enough for truthful peaks or for future diagnostics.

**Conclusion:** Reject

#### Option B: Keep average as the default output value, but add richer tile statistics and a reducer seam

This preserves current rendering behavior while making the data path honest about min, max, average, and aggregated-versus-exact semantics.

It also gives Phase 17 a lower-level hotspot seam without dragging UI or runtime orchestration into native territory.

**Conclusion:** Recommended

#### Option C: Push truth corrections into overlays only

This would hide the problem in presentation code and would not help cache-backed or future non-Avalonia consumers.

**Conclusion:** Reject

### Profiling and optional Rust work

#### Option A: Add Rust first, then benchmark later

This violates the milestone rules already written into project context.

**Conclusion:** Reject

#### Option B: Add budgets and a coarse managed hotspot seam first, then keep native work optional

This creates a measurable decision point and preserves C# ownership at the runtime and control layers.

**Conclusion:** Recommended

#### Option C: Refuse any hotspot seam and keep everything ad hoc managed code forever

This avoids native complexity, but it also fails `DATA-04`, which explicitly asks for coarse seams suitable for optional Rust acceleration.

**Conclusion:** Reject

## Recommended Architecture

### 1. Keep LOD selection, add scheduler-local planning

Do not replace `SurfaceLodPolicy`. Use it to define the current viewport neighborhood, then add a scheduler-local plan object that turns those keys into:

- retained residency keys
- ordered request keys
- bounded execution groups

That keeps the "what is relevant?" logic in `Core` and the "how do we fetch it?" logic in Avalonia orchestration.

### 2. Preserve the existing source contract, add batch capability alongside it

`ISurfaceTileSource` should remain the compatibility contract. Add a second, additive batch contract for sources that can exploit grouped reads:

- cache-backed sources implement both
- in-memory sources may implement only the single-tile contract at first
- scheduler code can stay compatible and opportunistically use batch capability later

This keeps the runtime surface stable while opening a lower-level hotspot seam.

### 3. Introduce a persistent cache payload session

The cleanest place to fix repeated reopen and recopy work is a dedicated session type under `Videra.SurfaceCharts.Processing`:

- owns one payload stream or handle
- supports one-tile and many-tile reads
- keeps manifest validation in `SurfaceCacheReader`
- keeps runtime adaptation in `SurfaceCacheTileSource`

That separates format knowledge from payload-lifetime management.

### 4. Make tile statistics first-class, but keep rendering values compatible

Phase 17 does not need to replace averaged render samples. It does need first-class tile statistics that make aggregated truth explicit.

Recommended shape:

- `SurfaceTileStatistics`
- tile carries `Statistics` in addition to `ValueRange`
- reduced tiles still render from average-like values by default
- probes and overlays can distinguish exact from aggregated truth using the statistics surface

### 5. Expose a coarse reduction hotspot seam inside the data path

Add a lower-level reduction kernel abstraction around the hot loops that summarize source regions for pyramid output or tile statistics.

The key is *coarse*:

- batch or region oriented
- pure data in, pure data out
- no callbacks into control code
- no renderer or UI ownership

That is the correct seam for future native work if benchmarks ever justify it.

### 6. Profile the combined path after scheduler and cache work land

The scheduler and cache plans should ship first because they change the shape of the workload. Only then do the benchmark numbers become meaningful.

Planning implication:

- `17-01` and `17-02` can run in parallel if their write sets stay separate
- `17-03` should depend on both because it measures and freezes the combined data path

## Testing and Verification Implications

### Scheduler

Existing helper sources in `SurfaceChartViewLifecycleTests.cs` already give the right test hooks:

- `RecordingSurfaceTileSource`
- `ScriptedSurfaceTileSource`
- request-count wait helpers

These should be extended rather than replaced.

### Cache I/O

Use the existing file-system indirection in `SurfaceCacheFileSystem` to count opens and enforce session reuse in tests.

### Statistics

Core tests should prove:

- default reduction remains compatible
- tile statistics remain truthful for reduced tiles
- exact versus aggregated semantics stay explicit

### Benchmarking

Add a benchmark project rather than trying to force performance decisions into unit tests. Unit tests can guard correctness and light regression signals; benchmarks should establish the actual hotspot data.

## Risks

### 1. Overloading the scheduler with too many new responsibilities

Mitigation: keep selection in `Core`, execution policy in Avalonia, and persistence in `Processing`.

### 2. Breaking the public tile contract too aggressively

Mitigation: keep `ISurfaceTileSource` intact and make new capability additive.

### 3. Letting benchmark work sprawl into a premature native subsystem

Mitigation: benchmark project first, coarse kernel seam second, native implementation only if the measurements demand it.

### 4. Reintroducing renderer-coupled logic into the control layer

Mitigation: treat Phase 16 render-host files as fixed seams and make Phase 17 feed them better data instead of changing ownership.

## Recommendation

Phase 17 should be planned as two implementation slices plus one measurement-and-boundary slice:

1. view-aware scheduler and bounded residency behavior
2. persistent cache reads plus truthful tile statistics
3. profiling budgets plus a coarse managed hotspot seam that keeps native work optional

That is the smallest path that satisfies the roadmap, reuses the shipped Phase 16 seam, and leaves the project with better data truth even if no Rust code is ever added.
