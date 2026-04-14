# Videra.SurfaceCharts.Processing

`Videra.SurfaceCharts.Processing` contains the offline preprocessing and cache-oriented helpers for the surface-chart family.

The package will stay focused on data preparation and file-backed support, not UI composition.

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
