# Videra.SurfaceCharts.Demo

`Videra.SurfaceCharts.Demo` is the independent demo application for the surface-chart module family.

The sample stays separate from `Videra.Demo` and `VideraView`. It provides switchable source and viewport paths:

- `in-memory example`: builds a sample surface matrix at startup and feeds it through `SurfacePyramidBuilder`.
- `cache-backed example`: loads a committed seed cache from `Assets/sample-surface-cache/sample.surfacecache.json` and feeds it through `SurfaceCacheReader` and `SurfaceCacheTileSource`.
- `overview` and `zoomed detail`: switches the chart viewport between the full dataset and a stable zoomed-in viewport computed from the active dataset metadata.

Run the sample from the solution like any other Avalonia desktop app.
