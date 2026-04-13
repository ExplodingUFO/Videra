# Videra.SurfaceCharts.Processing

`Videra.SurfaceCharts.Processing` contains the offline preprocessing and cache-oriented helpers for the surface-chart family.

The package will stay focused on data preparation and file-backed support, not UI composition.

The surface cache format is versioned and split into two files:

- `*.surfacecache.json`: manifest metadata, tile bounds, and `payloadOffset` / `payloadLength` records
- `*.surfacecache.json.bin`: raw tile values stored as contiguous `float` payloads

`SurfaceCacheReader` validates and indexes the manifest eagerly, while `SurfaceCacheTileSource` loads tile payload bytes lazily on demand so viewport changes only read the levels and tiles the renderer requests.
