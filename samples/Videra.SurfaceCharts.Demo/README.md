# Videra.SurfaceCharts.Demo

`Videra.SurfaceCharts.Demo` is the independent demo application for the surface-chart module family.

The sample stays separate from `Videra.Demo` and `VideraView`. It exercises the chart-local renderer seam shipped in `SurfaceChartView`, not a `VideraView` mode. It provides switchable sources and built-in chart interaction:

- `in-memory example`: builds a sample surface matrix at startup and feeds it through `SurfacePyramidBuilder`.
- `cache-backed example`: loads manifest metadata from `Assets/sample-surface-cache/sample.surfacecache.json`, then uses lazy tile streaming from `Assets/sample-surface-cache/sample.surfacecache.json.bin` through `SurfaceCacheReader` and `SurfaceCacheTileSource`.

SurfaceChartView now exposes `ViewState` as the primary chart-view contract while `Viewport` remains a compatibility bridge for existing hosts.
SurfaceChartView now ships built-in `left-drag orbit`, `right-drag pan`, `wheel dolly`, and `Ctrl + Left drag` focus zoom on top of the `ViewState` runtime contract.
The chart enters `Interactive` quality during motion and returns to `Refine` after input settles.

The committed cache sample uses a tiled manifest+sidecar layout so panning, dolly, and focus changes request different cache tiles instead of materializing every tile value into memory up front.

## Run

```bash
dotnet run --project samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj
```

## What The Demo Shows Today

- an independent chart application boundary
- in-memory versus cache-backed source selection
- built-in `left-drag orbit`, `right-drag pan`, `wheel dolly`, and `Ctrl + Left drag` focus zoom
- a `View-state contract` panel that projects `ViewState`, `Fit to data`, and `Reset camera`
- an `Interaction quality` panel that projects `Interactive` and `Refine`
- overview-first LOD behavior
- lazy cache-backed tile reads through the committed `sample.surfacecache.json` and `.bin` sidecar
- axis/legend overlays rendered by `SurfaceChartView`
- hover readout and `Shift + LeftClick` pinned probes on the chart surface
- the shipped `GPU-first` renderer path used by `SurfaceChartView`, with `software fallback` still available when native-host or GPU initialization is unavailable
- a lightweight rendering-path panel driven by `RenderingStatus` / `RenderStatusChanged`

## What The Demo Does Not Show Yet

The current sample is still a focused onboarding surface, not a finished end-user chart workstation:

- no dedicated UI that walks every GPU host / fallback combination or surfaces every `RenderingStatus` field
- on Linux, native GPU hosting still depends on X11 handles; Wayland sessions are `XWayland compatibility` only, not compositor-native Wayland surface embedding

That gap is intentional. The current demo primarily proves the control boundary, built-in interaction contract, renderer path, probe/overlay behavior, and cache/LOD story without trying to be a full workstation shell.
