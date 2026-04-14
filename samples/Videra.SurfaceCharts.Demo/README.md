# Videra.SurfaceCharts.Demo

`Videra.SurfaceCharts.Demo` is the independent demo application for the surface-chart module family.

The sample stays separate from `Videra.Demo` and `VideraView`. It provides switchable source and viewport paths:

- `in-memory example`: builds a sample surface matrix at startup and feeds it through `SurfacePyramidBuilder`.
- `cache-backed example`: loads manifest metadata from `Assets/sample-surface-cache/sample.surfacecache.json`, then uses lazy tile streaming from `Assets/sample-surface-cache/sample.surfacecache.json.bin` through `SurfaceCacheReader` and `SurfaceCacheTileSource`.
- `overview` and `zoomed detail`: switches the chart viewport between the full dataset and a stable zoomed-in viewport computed from the active dataset metadata.

The committed cache sample uses a tiled manifest+sidecar layout so switching between overview and detail requests different cache tiles instead of materializing every tile value into memory up front.

## Run

```bash
dotnet run --project samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj
```

## What The Demo Shows Today

- an independent chart application boundary
- in-memory versus cache-backed source selection
- overview-first LOD behavior
- lazy cache-backed tile reads through the committed `sample.surfacecache.json` and `.bin` sidecar
- host-driven viewport switching between overview and zoomed detail

## What The Demo Does Not Show Yet

This is still an alpha demo. It does not yet provide a full interactive chart UX:

- no finished mouse zoom / pan / orbit workflow
- no finished axis, tick, label, or legend presentation
- no finished end-user hover probe interaction path

That gap is real. The current demo primarily proves the control boundary, render path, and cache/LOD behavior.
