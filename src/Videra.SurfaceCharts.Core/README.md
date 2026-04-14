# Videra.SurfaceCharts.Core

`Videra.SurfaceCharts.Core` defines the chart-domain contracts for the surface-chart module family.

This package is the place for:
- surface metadata
- viewport and LOD selection
- tile identities and tile sources
- color and probe contracts
- dedicated render-input models for the chart pipeline

The package is intentionally independent from `VideraView`.

## What Belongs Here

Representative contracts include:

- `SurfaceMetadata`
- `SurfaceViewport`
- `SurfaceLodPolicy`
- `SurfaceTileKey`
- `SurfaceTile`
- `ISurfaceTileSource`
- `SurfaceRenderer`

The important contract detail is that `SurfaceTile.Width` / `Height` describe the tile value grid, while `SurfaceTile.Bounds` describe the covered source-space span in the original dataset. Coarse LOD tiles therefore do not assume a 1:1 sample-to-vertex mapping.

## What Does Not Belong Here

- Avalonia controls
- pointer event handling
- demo UI concerns
- offline cache file IO
- `VideraView` lifecycle or viewer semantics

This layer is the reusable boundary that higher-level UI and preprocessing code build on top of.
