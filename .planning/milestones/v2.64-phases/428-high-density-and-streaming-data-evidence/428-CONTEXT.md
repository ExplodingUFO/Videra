# Phase 428: High-Density and Streaming Data Evidence - Context

**Gathered:** 2026-04-30
**Status:** Ready for planning
**Mode:** Autonomous (smart discuss)
**Bead:** `Videra-7tqx.4`

## Phase Boundary

Phase 428 strengthens streaming and high-density workflows with real runtime
evidence. It proves a high-density or streaming SurfaceCharts scenario runs
with explicit ingestion/window/cache evidence, keeps update/retention/visible-
window behavior bounded, and reports real scenario scope without benchmark
overclaims.

## User Constraints (from Phase 425)

- Beads are the task, state, and handoff spine.
- Split tasks small and identify dependencies before implementation.
- Use isolated worktrees and branches for parallel beads when write scopes do
  not block each other.
- Every worker must have a responsibility boundary, write scope, validation
  command, and handoff notes.
- Avoid god code.
- Do not add compatibility layers, downshift behavior, fallback behavior, old
  chart controls, or fake validation evidence.
- Keep implementation simple and direct.

## Decisions

### D-01: Streaming Evidence Scope

Phase 428 focuses on proving existing streaming infrastructure works with real
evidence, not adding new streaming features. The key surfaces are:

- `DataLogger3D` — mutable columnar live-data facade with append/replace/FIFO
- `ScatterColumnarSeries` — high-density columnar storage
- `SurfaceCacheReader`/`SurfaceCacheTileSource` — cache-backed surface ingestion
- `SurfaceTileScheduler`/`SurfaceTileCache` — view-window tile management

### D-02: Evidence Format

Evidence is bounded text records (following existing `SurfaceDemoSupportSummary`
pattern) that describe:
- Scenario scope and dataset size
- Ingestion path (live append, cache-backed, offline)
- Window/cache behavior (visible window, tile counts, FIFO drops)
- Rendering status and resident data metrics
- Explicit non-goals and evidence boundaries

### D-03: Workspace Streaming Integration

The workspace from Phase 426 can track streaming status per chart:
- Per-chart dataset scale (series count, point count)
- Per-chart update mode (append, replace, FIFO)
- Per-chart cache/window state if applicable

### D-04: Demo Integration

The demo strengthens existing streaming scenarios with evidence copy:
- Existing `scatter-replace-100k`, `scatter-append-100k`, `scatter-fifo-trim-100k`
  scenarios already have deterministic evidence
- Phase 428 adds a workspace-aware streaming scenario that shows multiple
  charts with different streaming modes and aggregate evidence

### D-05: Performance Truth Boundary

Phase 428 does NOT:
- Promote evidence-only benchmarks into release blockers
- Claim renderer-side window crop without implementation
- Add fake benchmark thresholds
- Expand backend scope

## Canonical References

- `.planning/phases/425-analysis-workspace-and-streaming-inventory/425C-STREAMING-PERFORMANCE-INVENTORY.md` — full streaming inventory
- `src/Videra.SurfaceCharts.Core/DataLogger3D.cs` — live-data facade
- `src/Videra.SurfaceCharts.Core/ScatterColumnarSeries.cs` — high-density columnar
- `src/Videra.SurfaceCharts.Processing/SurfaceCacheReader.cs` — cache ingestion
- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileScheduler.cs` — tile scheduling
- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileCache.cs` — tile cache
- `samples/Videra.SurfaceCharts.Demo/Services/ScatterStreamingScenario.cs` — existing streaming scenarios
- `samples/Videra.SurfaceCharts.Demo/Services/ScatterStreamingEvidence.cs` — existing evidence
- `samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoSupportSummary.cs` — support summary pattern
- `benchmarks/Videra.SurfaceCharts.Benchmarks/SurfaceChartsStreamingBenchmarks.cs` — streaming benchmarks

## Existing Code Insights

### Reusable Assets
- `DataLogger3D` — live-data contract with append/replace/FIFO, retained count, `CreateLiveViewEvidence()`
- `ScatterColumnarSeries` — high-density append/replace, FIFO capacity, pickable flag
- `ScatterChartData` — dataset-level diagnostics (series count, point count, FIFO drops)
- `SurfaceCacheReader` + `SurfaceCacheTileSource` — cache-backed surface loading
- `SurfaceTileScheduler` + `SurfaceTileCache` — tile priority and caching
- `SurfaceDemoSupportSummary` — text-based support evidence
- Existing 100k scatter scenarios with deterministic evidence

### Established Patterns
- Evidence-only assertions (not benchmark claims)
- Bounded text evidence records
- Host-owned immutable results
- Explicit non-goals and evidence boundaries

### Integration Points
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` — workspace streaming scenario
- `samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoSupportSummary.cs` — extended evidence
- `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/SurfaceChartWorkspace.cs` — streaming status tracking

## Deferred Ideas

- Benchmark threshold promotion — needs CI history review
- Renderer-side window crop — separate feature work
- Multi-million point evidence — needs real dataset
- Streaming CI/release-readiness gates — Phase 430

---

*Phase: 428-high-density-and-streaming-data-evidence*
*Context gathered: 2026-04-30*
