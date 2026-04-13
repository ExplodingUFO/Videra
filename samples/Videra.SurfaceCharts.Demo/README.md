# Videra.SurfaceCharts.Demo

`Videra.SurfaceCharts.Demo` is the independent demo application for the surface-chart module family.

The sample stays separate from `Videra.Demo` and `VideraView`. It provides switchable source and viewport paths:

- `in-memory example`: builds a sample surface matrix at startup and feeds it through `SurfacePyramidBuilder`.
- `cache-backed example`: loads manifest metadata from `Assets/sample-surface-cache/sample.surfacecache.json`, then uses lazy tile streaming from `Assets/sample-surface-cache/sample.surfacecache.json.bin` through `SurfaceCacheReader` and `SurfaceCacheTileSource`.
- `overview` and `zoomed detail`: switches the chart viewport between the full dataset and a stable zoomed-in viewport computed from the active dataset metadata.

The committed cache sample uses a tiled manifest+sidecar layout so switching between overview and detail requests different cache tiles instead of materializing every tile value into memory up front.

Run the sample from the solution like any other Avalonia desktop app.
