# Videra.SurfaceCharts.Processing

`Videra.SurfaceCharts.Processing` contains the offline preprocessing and cache-oriented helpers for the surface-chart family.

The package will stay focused on data preparation and file-backed support, not UI composition.

`nuget.org` is the default public feed for this package. `GitHub Packages` remains `preview` / internal validation only. The current support level is `alpha`. Add this package when you want the surface/cache-backed chart workflow built on `SurfacePyramidBuilder`.

Current responsibilities:

- building in-memory pyramids from dense surface matrices
- writing versioned cache manifests and payload sidecars
- reading cache metadata eagerly while loading tile payloads lazily on demand

Current non-goals:

- control lifecycle
- camera or pointer interaction
- axis presentation
- real-time streaming UI orchestration

The surface cache format is versioned and split into two files:

- `*.surfacecache.json`: manifest metadata, tile bounds, and `payloadOffset` / `payloadLength` records
- `*.surfacecache.json.bin`: raw tile values stored as contiguous `float` payloads

`SurfaceCacheReader` validates and indexes the manifest eagerly, while `SurfaceCacheTileSource` loads tile payload bytes lazily on demand so viewport changes only read the levels and tiles the renderer requests.

Persistent payload sessions keep one payload stream open for a cache-backed source lifetime instead of reopening the sidecar for every tile request.

`ISurfaceTileBatchSource` adds ordered batch reads without replacing `ISurfaceTileSource`, so schedulers can request groups of tiles through the same payload session when that helps residency and latency.

The live scheduler now consumes those ordered batch reads whenever a source implements `ISurfaceTileBatchSource`; sources that only implement `ISurfaceTileSource` continue through the per-tile fallback path.

Per-tile `statistics` are serialized additively in the manifest. Reduced tiles still render average values by default, while `SurfaceTileStatistics` preserves the covered source-region range, average, sample count, and exact-vs-reduced truth flag.

## Benchmarking and optional native seam

`benchmarks/Videra.SurfaceCharts.Benchmarks/Videra.SurfaceCharts.Benchmarks.csproj` uses `BenchmarkDotNet` to measure viewport selection, cache batch reads, and pyramid/statistics work on the same managed paths used in production.

Any native work remains an `optional native seam`: it is measurement-gated, limited to coarse reduction or cache-processing hotspots, and it must not pull control orchestration across the boundary.

`Videra.SurfaceCharts.Avalonia/Controls/Interaction` and `Videra.SurfaceCharts.Rendering` must remain free of native interop helpers even if a lower-level hotspot eventually earns a native implementation.
