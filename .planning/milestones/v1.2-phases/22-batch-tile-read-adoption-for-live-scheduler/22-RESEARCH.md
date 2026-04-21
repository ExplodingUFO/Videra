# Phase 22 Research

## Current implementation gap

`ISurfaceTileBatchSource` is already part of `Videra.SurfaceCharts.Core`, and `SurfaceCacheTileSource` implements it with ordered batch reads through a persistent payload session. The missing piece is the live scheduler:

- `SurfaceTileScheduler.RequestPlanAsync(...)` still fans out per-key workers
- `RequestTileAsync(...)` calls `ISurfaceTileSource.GetTileAsync(...)` for every detail key

That means cache-backed data sources expose a better seam than the live chart actually consumes.

## Existing truths to preserve

- Overview tile remains the first request
- Visible tiles are still prioritized ahead of outer-neighborhood tiles
- Request supersession is still generation-based
- Non-batch sources must still work exactly as before
- `SurfaceChartController` and `SurfaceChartRuntime` should not need API changes for this cleanup

## Low-risk adoption shape

The least invasive design is:

1. Keep overview on the existing single-tile path
2. If the source implements `ISurfaceTileBatchSource`, partition ordered detail keys into contiguous chunks
3. Mark each key requested through the existing `SurfaceTileCache`
4. Call `GetTilesAsync(...)` once per chunk, preserving input order
5. Store/release tiles per key using the existing cache-generation semantics
6. Fall back to the existing worker path for non-batch sources

Using chunk size equal to the current scheduler concurrency limit preserves the bounded-work budget without creating a new tuning surface.

## Verification targets

- new scheduler regression tests proving detail keys flow through `GetTilesAsync(...)`
- existing non-batch scheduling tests still pass
- processing README and repository guard say the live scheduler now consumes batch reads when available and falls back to per-tile reads otherwise
