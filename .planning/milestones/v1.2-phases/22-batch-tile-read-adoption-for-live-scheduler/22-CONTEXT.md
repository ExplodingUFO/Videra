# Phase 22: Batch Tile Read Adoption for Live Scheduler - Context

**Gathered:** 2026-04-16  
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 22 is a cleanup/data-path polish phase created after the v1.2 re-audit. The audit confirmed that `ISurfaceTileBatchSource` already exists and is tested on `SurfaceCacheTileSource`, but the live scheduler still pulls detail tiles one-by-one through `GetTileAsync(...)`.

This phase adopts the existing batch seam in the scheduler without changing the shipped visible-first ordering, cancellation, or fallback truth.

</domain>

<decisions>
## Implementation Decisions

### Reuse the current scheduler contract
`SurfaceTileScheduler` already owns request ordering, overview-first behavior, and request-generation handling. Batch adoption should extend that seam rather than introducing a second scheduler path.

### Preserve fallback for non-batch sources
Only sources that implement `ISurfaceTileBatchSource` should use the batch path. Existing non-batch integration tests must continue to pass unchanged.

### Keep docs and tests aligned
The processing README already documents additive batch reads on the tile-source seam. After scheduler adoption, docs and tests should also say the live scheduler consumes that seam when available and falls back gracefully otherwise.

</decisions>

<code_context>
## Existing Code Insights

- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileScheduler.cs` currently requests all detail tiles through per-key `GetTileAsync(...)`
- `src/Videra.SurfaceCharts.Processing/SurfaceCacheTileSource.cs` already implements `ISurfaceTileBatchSource.GetTilesAsync(...)`
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartTileSchedulingTests.cs` already covers prioritization, concurrency, stale work suppression, and interactive/refine request behavior
- `tests/Videra.SurfaceCharts.Processing.Tests/SurfaceCacheTileSourceTests.cs` already proves batch reads reuse a persistent payload session

</code_context>

<specifics>
## Specific Ideas

- Add scheduler regression tests with a fake batch-capable tile source
- Flatten batch-request logs and compare them to the current prioritized key order
- Keep overview requests singular, but dispatch detail keys through ordered batches sized by the current scheduler concurrency budget
- Update `src/Videra.SurfaceCharts.Processing/README.md` and its repository guard to mention live-scheduler batch adoption plus per-tile fallback

</specifics>

<deferred>
## Deferred Ideas

- Do not redesign `SurfaceLodPolicy`
- Do not add a new benchmark project just for this cleanup phase
- Do not remove the non-batch `ISurfaceTileSource` fallback path

</deferred>
