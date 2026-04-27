# Videra.SurfaceCharts.Core

`Videra.SurfaceCharts.Core` defines the chart-domain contracts for the surface-chart module family.

`nuget.org` is the default public feed for this package. `GitHub Packages` remains `preview` / internal validation only. The current support level is `alpha`.

This package is the place for:
- surface metadata
- viewport and LOD selection
- tile identities and tile sources
- color and probe contracts
- dedicated render-input models for the chart pipeline

The package is intentionally independent from `VideraView`.

Most consumers should start with `Videra.SurfaceCharts.Avalonia` and add `Videra.SurfaceCharts.Processing` only when they need the surface/cache-backed path. Install `Videra.SurfaceCharts.Core` directly only when you are building custom tile sources or chart-domain contracts without the Avalonia control shell.

That same split keeps the chart-local efficiency story narrow: tighter interactive residency under camera movement and lower probe-path churn stay on the existing chart-local path.

Columnar scatter streaming also stays in this chart-domain layer. `ScatterColumnarSeries` accepts `ReplaceRange(...)` for full replacement and `AppendRange(...)` for streaming batches, validates matching columns and finite coordinates, and can use an optional positive `fifoCapacity` to retain a bounded point window. The high-volume columnar path defaults to `Pickable=false`; opt into picking only when the host needs per-point hit participation.

## What Belongs Here

Representative contracts include:

- `SurfaceMetadata`
- `SurfaceViewport`
- `SurfaceLodPolicy`
- `SurfaceTileKey`
- `SurfaceTile`
- `ISurfaceTileSource`
- `SurfaceRenderer`
- `ScatterColumnarData`
- `ScatterColumnarSeries`

The important contract detail is that `SurfaceTile.Width` / `Height` describe the tile value grid, while `SurfaceTile.Bounds` describe the covered source-space span in the original dataset. Coarse LOD tiles therefore do not assume a 1:1 sample-to-vertex mapping.

For scatter data, retained streaming truth is exposed as retained point count, append/replacement batch count, dropped FIFO point count, last dropped point count, and configured FIFO capacity. These counters are diagnostics and benchmark evidence inputs; they are not a new generic streaming framework.

## Source-First and Advanced Payloads

`SurfaceMatrix` remains the simplest source-first regular-grid entrypoint for the current chart story. Existing hosts can keep the default `new SurfaceMatrix(metadata, values)` path and continue feeding that matrix through `SurfacePyramidBuilder` without learning any new control-layer concepts.

Advanced callers can keep the same chart shell while supplying richer analytics payloads underneath it. The lower-level contracts now also support `SurfaceScalarField`-backed height data, an independent `ColorField`, and a first-class `SurfaceMask` through the `SurfaceMatrix(metadata, heightField, colorField, mask)` and `SurfaceTile(..., heightField, colorField, mask)` overloads.

That split is intentional: the default source-first regular-grid path stays narrow, while advanced callers can opt into independent `ColorField` and first-class `SurfaceMask` semantics without widening `SurfaceChartView` itself.

## What Does Not Belong Here

- Avalonia controls
- pointer event handling
- demo UI concerns
- offline cache file IO
- `VideraView` lifecycle or viewer semantics
- viewer selection, annotation, or camera contracts

This layer is the reusable boundary that higher-level UI and preprocessing code build on top of.
