---
phase: 17-large-dataset-residency-cache-evolution-and-optional-rust-spike
plan: 02
subsystem: surface-charts-data
tags: [surface-charts, cache, batch-read, statistics, preprocessing]
requires:
  - phase: 16-rendering-host-seam-and-gpu-main-path
    provides: render-host seam and chart-local residency path from phase 16
provides:
  - additive `ISurfaceTileBatchSource` contract for ordered batch tile reads
  - persistent cache payload sessions that reuse one payload stream across reads
  - first-class `SurfaceTileStatistics` carried through in-memory pyramid tiles and cache round-trips
affects: [17-01-adaptive-scheduler-and-residency-cancellation, 17-03-benchmark-and-native-boundary-guard]
tech-stack:
  added: []
  patterns: [persistent payload session, additive batch reads, source-region statistics, compatibility-first cache evolution]
key-files:
  created:
    - src/Videra.SurfaceCharts.Core/ISurfaceTileBatchSource.cs
    - src/Videra.SurfaceCharts.Core/SurfaceTileStatistics.cs
    - src/Videra.SurfaceCharts.Processing/SurfaceCachePayloadSession.cs
    - tests/Videra.SurfaceCharts.Processing.Tests/SurfaceCachePayloadSessionTests.cs
  modified:
    - src/Videra.SurfaceCharts.Core/SurfaceTile.cs
    - src/Videra.SurfaceCharts.Core/SurfacePyramidBuilder.cs
    - src/Videra.SurfaceCharts.Core/InMemorySurfaceTileSource.cs
    - src/Videra.SurfaceCharts.Processing/SurfaceCacheReader.cs
    - src/Videra.SurfaceCharts.Processing/SurfaceCacheTileSource.cs
    - src/Videra.SurfaceCharts.Processing/SurfaceCacheWriter.cs
    - src/Videra.SurfaceCharts.Processing/README.md
    - tests/Videra.SurfaceCharts.Core.Tests/SurfacePyramidBuilderTests.cs
    - tests/Videra.SurfaceCharts.Core.Tests/InMemorySurfaceTileSourceTests.cs
    - tests/Videra.SurfaceCharts.Processing.Tests/SurfaceCacheTileSourceTests.cs
    - tests/Videra.SurfaceCharts.Processing.Tests/SurfaceCacheRoundTripTests.cs
requirements-completed: [DATA-02, DATA-03]
completed: 2026-04-14
---

# Phase 17 Plan 02 Summary

## Accomplishments
- Added `ISurfaceTileBatchSource` without replacing `ISurfaceTileSource`, so callers can request ordered tile batches through the same source surface.
- Added `SurfaceTileStatistics` and threaded it through `SurfaceTile`, in-memory pyramid generation, cache serialization, cache deserialization, and cache-backed tile sources.
- Added `SurfaceCachePayloadSession` and updated `SurfaceCacheTileSource` to reuse one payload stream for repeated reads instead of reopening the `.bin` sidecar per tile.
- Kept the cache format compatibility-first by preserving the manifest-plus-sidecar layout and treating `statistics` as additive JSON fields.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfacePyramidBuilderTests|FullyQualifiedName~InMemorySurfaceTileSourceTests"`
- `dotnet test tests/Videra.SurfaceCharts.Processing.Tests/Videra.SurfaceCharts.Processing.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceCacheTileSourceTests|FullyQualifiedName~SurfaceCacheRoundTripTests|FullyQualifiedName~SurfaceCachePayloadSessionTests"`

## Notes
- Reduced tiles keep average-reduced values as the rendered sample grid while `SurfaceTileStatistics` records source-region range, average, sample count, and exact-vs-reduced truth.
- `SurfaceTile.ValueRange` remains tied to the rendered tile values; `SurfaceTile.Statistics.Range` carries the broader covered-region truth when tiles are reduced.
- `.planning/` artifacts remain local in this checkout (`commit_docs: false`).
