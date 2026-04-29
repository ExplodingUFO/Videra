# SurfaceCharts Release Candidate Handoff

[English](surfacecharts-release-candidate-handoff.md)

This page is the release-candidate handoff for package consumers and support triage. It keeps the public install path, cookbook path, migration notes, and support artifacts in one place so consumers do not need to read internal planning files.

## Package Consumption

Use `nuget.org` as the default public package feed after an approved public release. `GitHub Packages` remains the preview/internal validation feed.

For Avalonia chart applications:

```bash
dotnet add package Videra.SurfaceCharts.Avalonia
```

Add processing only when the application uses surface/cache-backed sources:

```bash
dotnet add package Videra.SurfaceCharts.Processing
```

`Videra.SurfaceCharts.Avalonia` brings `Videra.SurfaceCharts.Core` and `Videra.SurfaceCharts.Rendering` transitively. Install `Videra.SurfaceCharts.Core` directly only when building chart-domain contracts or custom tile sources without the Avalonia shell. `Videra.SurfaceCharts.Demo` and `smoke/Videra.SurfaceCharts.ConsumerSmoke` are repository-only validation/support entries, not package dependencies.

## Cookbook Path

Start with the root README "Minimal SurfaceCharts cookbook", then use [Videra.SurfaceCharts.Demo](../samples/Videra.SurfaceCharts.Demo/README.md) for copyable recipes:

- `First Chart` for the smallest `VideraChartView` + `Plot.Add.Surface` setup.
- `Styling` for axes, color maps, precision, and `Plot.OverlayOptions`.
- `Interactions` for built-in commands and probe resolution.
- `Live Data` for `Plot.Add.Scatter`, `DataLogger3D`, FIFO windows, and live-view evidence.
- `Linked Axes` for explicit two-chart view linking with a disposable lifetime.
- `Export` for PNG-only `Plot.SavePngAsync(...)` output.

The cookbook follows ScottPlot 5's discoverable recipe ergonomics as inspiration only. It is not a ScottPlot API compatibility, parity, adapter, or migration layer. SurfaceCharts remains a Videra 3D chart surface built around `VideraChartView`, `Plot.Add.Surface`, `Plot.Add.Waterfall`, `Plot.Add.Scatter`, `Plot.Axes`, chart-local interaction/profile APIs, linked 3D views, `DataLogger3D`, and PNG-only chart snapshots.

## Migration Notes

The current chart model is Plot-owned:

- `VideraChartView` is the single shipped Avalonia chart control.
- Load chart data through `chart.Plot.Add.Surface(...)`, `chart.Plot.Add.Waterfall(...)`, or `chart.Plot.Add.Scatter(...)`.
- Configure axes through `chart.Plot.Axes`.
- Configure overlay presentation through `chart.Plot.OverlayOptions`.
- Capture chart PNG output through `chart.Plot.SavePngAsync(...)` or `chart.Plot.CaptureSnapshotAsync(...)`.
- Keep `ViewState`, color-map selection, source ownership, and chart-local product UI in the host application.

Removed or intentionally absent surfaces:

- `SurfaceChartView`, `WaterfallChartView`, and `ScatterChartView` are not public entrypoints.
- Direct public `VideraChartView.Source` loading is not part of the current API.
- Compatibility wrappers for removed alpha APIs are not provided.
- Hidden fallback from one chart path to another is not provided; failed cache-backed loading should report cache-load failure while the previous Plot path remains active.

## Non-Goals

The release-candidate SurfaceCharts boundary does not include:

- ScottPlot API compatibility or adapter behavior.
- PDF or vector chart export.
- Image export beyond bounded PNG chart snapshots.
- Generic plotting-engine behavior.
- A `VideraView` mode or viewer selection/annotation/camera semantics.
- Runtime backend expansion, OpenGL/WebGL support, or compositor-native Wayland embedding.
- Demo-as-product behavior; `Videra.SurfaceCharts.Demo` remains repository-only.

## Support Handoff

For packaged SurfaceCharts issues, first attach the artifacts produced by `smoke/Videra.SurfaceCharts.ConsumerSmoke` or by the hosted consumer-smoke workflow:

- `consumer-smoke-result.json`
- `diagnostics-snapshot.txt`
- `surfacecharts-support-summary.txt`
- `chart-snapshot.png`
- trace logs
- stdout logs
- stderr logs
- environment logs

For repository-only repros, use `Videra.SurfaceCharts.Demo`, reproduce on `Start here: In-memory first chart` before moving to cache-backed or analytic paths, then copy the demo support summary. Keep SurfaceCharts support summaries separate from `VideraDiagnosticsSnapshotFormatter` viewer snapshots.

Support summaries and chart snapshots are support evidence. They are not benchmark results, pixel-perfect visual-regression gates, GPU performance guarantees, backend fallback proof, replay files, PDF/vector export, or a promise that the demo is an installable package.

