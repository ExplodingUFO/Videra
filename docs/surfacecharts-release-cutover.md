# SurfaceCharts Current Consumer Handoff

This page is the current consumer-facing SurfaceCharts handoff for the controlled alpha package line. It summarizes the package install path, release-note evidence, cookbook entry points, migration boundary, support artifacts, and troubleshooting route for consumers who should not need to read internal planning files.

This page is documentation only. Public package publishing, public tags, and GitHub Release publication still require explicit maintainer approval through the documented public release workflow.

## Release Notes

SurfaceCharts is a public alpha package family in the Videra release set. The consumer entry package is `Videra.SurfaceCharts.Avalonia`; add `Videra.SurfaceCharts.Processing` only when the app uses the surface/cache-backed path.

The current handoff notes should tell consumers:

- `VideraChartView` is the shipped Avalonia chart control.
- `Plot.Add.Surface`, `Plot.Add.Waterfall`, `Plot.Add.Scatter`, `Plot.Add.Bar`, and `Plot.Add.Contour` are the current demo cookbook entrypoints exercised by the bounded visible proofs.
- `Plot.Axes`, `Plot.OverlayOptions`, chart-local interaction/profile APIs, linked views, `DataLogger3D`, and PNG-only `Plot.SavePngAsync(...)` / `Plot.CaptureSnapshotAsync(...)` are the supported cookbook surfaces.
- `smoke/Videra.SurfaceCharts.ConsumerSmoke` is the packaged support proof; `Videra.SurfaceCharts.Demo` is repository-only.
- `surfacecharts-support-summary.txt`, `consumer-smoke-result.json`, `diagnostics-snapshot.txt`, `chart-snapshot.png`, trace logs, stdout logs, stderr logs, and environment logs are the first support artifact set for packaged chart issues.

Public release notes are generated after approved publish evidence with:

```powershell
pwsh -File ./scripts/New-PublicReleaseNotes.ps1 -EvidenceSummaryPath artifacts/public-release/public-publish-after-summary.json -OutputPath artifacts/public-release-notes/public-release-notes.md
```

The generated notes must reference `public-publish-after-summary.json`, the package matrix, known alpha limitations, and the package IDs included in the approved publish.

## Package Consumption

Use `nuget.org` as the default public package feed after an approved public release. `GitHub Packages` remains preview/internal validation only.

For Avalonia chart applications:

```bash
dotnet add package Videra.SurfaceCharts.Avalonia
```

Add processing only for the surface/cache-backed path:

```bash
dotnet add package Videra.SurfaceCharts.Processing
```

`Videra.SurfaceCharts.Avalonia` brings `Videra.SurfaceCharts.Core` and `Videra.SurfaceCharts.Rendering` transitively. Install `Videra.SurfaceCharts.Core` directly only when building chart-domain contracts or custom tile sources without the Avalonia shell.

Do not install `Videra.SurfaceCharts.Demo` or `smoke/Videra.SurfaceCharts.ConsumerSmoke` as product dependencies. They are repository-only validation and support entry points.

## Cookbook Entry Points

Start with the root README "Minimal SurfaceCharts cookbook", then use [Videra.SurfaceCharts.Demo](../samples/Videra.SurfaceCharts.Demo/README.md) for copyable recipes:

- `First Chart` for the smallest `VideraChartView` plus `Plot.Add.Surface` setup. See [`first-chart.md`](../samples/Videra.SurfaceCharts.Demo/Recipes/first-chart.md).
- `Surface/cache-backed` for `SurfaceMatrix` -> `SurfacePyramidBuilder` and committed cache asset usage; see [`surface-cache-backed.md`](../samples/Videra.SurfaceCharts.Demo/Recipes/surface-cache-backed.md).
- `Styling` for axes, color maps, precision, and `Plot.OverlayOptions`.
- `Interactions` for built-in commands and probe resolution.
- `Waterfall` for the thin second chart-family proof; see [`waterfall.md`](../samples/Videra.SurfaceCharts.Demo/Recipes/waterfall.md).
- `Live Data` for `Plot.Add.Scatter`, `DataLogger3D`, FIFO windows, and live-view evidence. See [`scatter-and-live-data.md`](../samples/Videra.SurfaceCharts.Demo/Recipes/scatter-and-live-data.md).
- `Linked Axes` for explicit two-chart view linking with a disposable lifetime. See [`axes-and-linked-views.md`](../samples/Videra.SurfaceCharts.Demo/Recipes/axes-and-linked-views.md).
- `Bar` for the bounded grouped-bar proof path in the demo gallery. See [`bar.md`](../samples/Videra.SurfaceCharts.Demo/Recipes/bar.md).
- `Contour` for the bounded radial scalar-field contour proof path in the demo gallery. See [`contour.md`](../samples/Videra.SurfaceCharts.Demo/Recipes/contour.md).
- `Support evidence` for copyable chart-local support summaries; see [`support-evidence.md`](../samples/Videra.SurfaceCharts.Demo/Recipes/support-evidence.md).
- `Export` for PNG-only chart snapshots. See [`png-snapshot.md`](../samples/Videra.SurfaceCharts.Demo/Recipes/png-snapshot.md).

The cookbook follows ScottPlot 5's discoverable recipe ergonomics as inspiration only. It is not a ScottPlot API compatibility, parity, adapter, or migration layer.

## Migration Boundary

Use the Plot-owned chart model:

- Create charts with `VideraChartView`.
- Load chart data through `chart.Plot.Add.Surface(...)`, `chart.Plot.Add.Waterfall(...)`, `chart.Plot.Add.Scatter(...)`, `chart.Plot.Add.Bar(...)`, or `chart.Plot.Add.Contour(...)` on the bounded paths shown by the current demo.
- Configure axes through `chart.Plot.Axes`.
- Configure overlay presentation through `chart.Plot.OverlayOptions`.
- Capture PNG output through `chart.Plot.SavePngAsync(...)` or `chart.Plot.CaptureSnapshotAsync(...)`.
- Keep `ViewState`, color-map selection, source ownership, and chart-local product UI in the host application.

Intentionally absent surfaces stay absent:

- no `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView` public controls
- no direct public `VideraChartView.Source` data-loading API
- no compatibility wrappers for removed alpha APIs
- no hidden scenario/data-path fallback; cache-backed loading failures should report cache-load failure while the previous Plot path remains active
- no PDF/vector export and no image export beyond bounded PNG chart snapshots
- no OpenGL/WebGL/backend expansion and no compositor-native Wayland embedding promise

## Support Handoff

For packaged SurfaceCharts issues, first run or attach evidence from `smoke/Videra.SurfaceCharts.ConsumerSmoke`:

```powershell
pwsh -File ./scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario SurfaceCharts -OutputRoot artifacts/consumer-smoke
```

Attach:

- `consumer-smoke-result.json`
- `diagnostics-snapshot.txt`
- `surfacecharts-support-summary.txt`
- `chart-snapshot.png`
- trace logs
- stdout logs
- stderr logs
- environment logs

For repository-only repros, use `Videra.SurfaceCharts.Demo`. Reproduce `Start here: In-memory first chart` before moving to `Explore next: Cache-backed streaming`, `Try next: Analytics proof`, `Try next: Waterfall proof`, `Try next: Scatter proof`, `Try next: Bar chart proof`, or `Try next: Contour plot proof`, then use `Copy support summary`.

Keep SurfaceCharts support summaries separate from `VideraDiagnosticsSnapshotFormatter` viewer snapshots. The chart summary is support evidence, not benchmark truth, pixel-perfect visual-regression evidence, GPU performance guarantee, backend fallback proof, replay state, PDF/vector export, or a promise that the demo is an installable package.

## Troubleshooting Route

1. Confirm the package install path matches [Package Matrix](package-matrix.md).
2. Confirm `Videra.SurfaceCharts.Avalonia` is installed and `Videra.SurfaceCharts.Processing` is present only when the surface/cache-backed path is used.
3. Run the packaged SurfaceCharts consumer smoke and attach the support artifact set above.
4. If the packaged smoke passes but the app fails, compare host code against the README cookbook and the demo cookbook snippets.
5. If the repository demo fails, start from `Start here: In-memory first chart` and capture the copied support summary before switching source paths.
6. If the issue is platform-specific, include matching-host validation evidence and backend/display environment variables.

The controlled cutover is not a publish approval by itself. Use [Releasing Videra](releasing.md) and [Release Candidate Abort and Cutover Runbook](release-candidate-cutover.md) for the human-approved public publish path.
