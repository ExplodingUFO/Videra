# Videra.SurfaceCharts.Demo

`Videra.SurfaceCharts.Demo` is the independent demo application for the surface-chart module family.

For the canonical surface/cache-backed chart story, start from `Videra.SurfaceCharts.Avalonia` plus `Videra.SurfaceCharts.Processing`, and use this demo as the repository reference app for the paths it actually exposes. `SurfaceChartView`, `WaterfallChartView`, and `ScatterChartView` ship in `Videra.SurfaceCharts.Avalonia`, and this sample exercises a repo-owned scatter proof path for all three controls.

The shipped efficiency story is the same chart-local path used by the benchmark gate: tighter interactive residency under camera movement and lower probe-path churn.

The sample stays separate from `Videra.Demo` and `VideraView`. It exercises the chart-local renderer seam shipped in `SurfaceChartView`, not a `VideraView` mode. It provides switchable sources and built-in chart interaction:

- `Start here: In-memory first chart`: builds a sample surface matrix at startup and feeds it through `SurfacePyramidBuilder`.
- `Explore next: Cache-backed streaming`: loads manifest metadata from `Assets/sample-surface-cache/sample.surfacecache.json`, then uses lazy tile streaming from `Assets/sample-surface-cache/sample.surfacecache.json.bin` through `SurfaceCacheReader` and `SurfaceCacheTileSource`.
- `Try next: Analytics proof`: exercises an explicit-coordinate `SurfaceChartView` path with independent scalar `ColorField` and pinned-probe workflow proof data.
- `Try next: Waterfall proof`: exercises the thin `WaterfallChartView` proof on the same Avalonia shell.
- `Try next: Scatter proof`: exercises the direct `ScatterChartView` proof path on the same Avalonia shell with repo-owned scatter data.

SurfaceChartView now exposes `ViewState` as the primary chart-view contract while `Viewport` remains a compatibility bridge for existing hosts.
SurfaceChartView now ships built-in `left-drag orbit`, `right-drag pan`, `wheel dolly`, `Ctrl + left-drag` focus zoom, and `Shift + left-click` pinned probe on top of the `ViewState` runtime contract.
The chart enters `Interactive` quality during motion and returns to `Refine` after input settles.
The public interaction diagnostics are `InteractionQuality` + `InteractionQualityChanged` with `Interactive` / `Refine`.
The public overlay configuration seam is `SurfaceChartOverlayOptions` through `OverlayOptions`; overlay state types remain internal.
Hosts own `ISurfaceTileSource`, persisted `ViewState`, color-map selection, and chart-local product UI.
`SurfaceChartView` owns chart-local built-in gestures, tile scheduling/cache, overlay presentation, native-host/render-host orchestration, and `RenderingStatus` projection.
The public rendering truth is `RenderingStatus` + `RenderStatusChanged` with `ActiveBackend`, `IsReady`, `IsFallback`, `FallbackReason`, `UsesNativeSurface`, and `ResidentTileCount`.
The `Videra.SurfaceCharts.*` family is part of the current public package promise. The public chart package line is `Videra.SurfaceCharts.Avalonia` plus `Videra.SurfaceCharts.Processing` for the surface/cache-backed path. `Videra.SurfaceCharts.Avalonia` brings `Videra.SurfaceCharts.Core` and `Videra.SurfaceCharts.Rendering` transitively. This demo remains repository-only.

The committed cache sample uses a tiled manifest+sidecar layout so panning, dolly, and focus changes request different cache tiles instead of materializing every tile value into memory up front.

## Start Here

1. Run the sample and keep the default `Start here: In-memory first chart` source.
2. Confirm the baseline first chart renders, then try `FitToData()`, `ResetCamera()`, orbit, pan, dolly, and focus zoom.
3. Move to `Explore next: Cache-backed streaming` only after the first chart path works and you want to validate lazy tile reads plus the broader demo surfaces.
4. Use `Try next: Analytics proof` to validate explicit-coordinate sampling and independent color scalar flow while keeping `SurfaceChartView` as the proof host.
5. Use `Try next: Waterfall proof` when you want the second control proof on the same Avalonia shell.
6. Use `Try next: Scatter proof` when you want the direct scatter proof on the same Avalonia shell.

## Run

```bash
dotnet run --project samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj
```

## What The Demo Shows Today

- an independent chart application boundary
- a `Start here` in-memory first chart path
- an `Explore next` cache-backed streaming path
- a `Try next` analytics proof with explicit/non-uniform coordinates and independent `ColorField` on `SurfaceChartView`
- a `Try next` waterfall proof path
- a `Try next` scatter proof path
- built-in `left-drag orbit`, `right-drag pan`, `wheel dolly`, and `Ctrl + left-drag` focus zoom on the surface and waterfall proof paths; the scatter proof keeps left-drag orbit and wheel dolly only
- a `View-state contract` panel that projects `ViewState`, `FitToData()`, `ResetCamera()`, and `ZoomTo(...)`
- an `Interaction quality` panel that projects `Interactive` and `Refine`
- overview-first LOD behavior
- lazy cache-backed tile reads through the committed `sample.surfacecache.json` and `.bin` sidecar
- axis/legend overlays rendered by `SurfaceChartView`
- an `Overlay options` panel that shows chart-local `OverlayOptions` for formatter, minor ticks, grid plane, and axis-side behavior
- hover readout and `Shift + left-click` pinned probes on the chart surface
- the shipped `GPU-first` renderer path used by `SurfaceChartView`, with `software fallback` still available when native-host or GPU initialization is unavailable
- a lightweight rendering-path panel driven by `RenderingStatus` / `RenderStatusChanged`

## What The Demo Does Not Show Yet

The current sample is still a focused onboarding surface, not a finished end-user chart workstation:

- no dedicated UI that walks every GPU host / fallback combination or surfaces every `RenderingStatus` field
- on Linux, native GPU hosting still depends on X11 handles; Wayland sessions are `XWayland compatibility` only, not compositor-native Wayland surface embedding

That gap is intentional. The current demo primarily proves the control boundary, built-in interaction contract, renderer path, probe/overlay behavior, and cache/LOD story without trying to be a full workstation shell.
