# Release Policy

This document defines what Videra publishes, where those packages go, and what stays repository-only.

## Public packages

The public package line is:

- `Videra.Core`
- `Videra.Import.Gltf`
- `Videra.Import.Obj`
- `Videra.Avalonia`
- `Videra.Platform.Windows`
- `Videra.Platform.Linux`
- `Videra.Platform.macOS`
- `Videra.SurfaceCharts.Core`
- `Videra.SurfaceCharts.Rendering`
- `Videra.SurfaceCharts.Processing`
- `Videra.SurfaceCharts.Avalonia`

Public release tags publish these packages to `nuget.org`.

The canonical public viewer stack is `Videra.Avalonia` plus exactly one matching `Videra.Platform.*` package. `Videra.Import.Gltf` and `Videra.Import.Obj` stay explicit ingestion packages on the core path; they are part of the public package line, but they are not a replacement for the Avalonia + matching-platform viewer stack.

The canonical public chart stack is `Videra.SurfaceCharts.Avalonia` plus `Videra.SurfaceCharts.Processing` for the current first-chart path. `Videra.SurfaceCharts.Core` and `Videra.SurfaceCharts.Rendering` are part of the public package line because the current chart control/runtime split depends on them, but they are not the normal first install step for most chart consumers.

Use [Package Matrix](package-matrix.md) for the published-package table and [Hosting Boundary](hosting-boundary.md) for the canonical composition story behind this release line.

## Preview and internal feed

`GitHub Packages` is reserved for `preview` / internal validation flows. It can be used for contributor testing, canary validation, and pre-release evidence gathering, but it is not the default public consumer path.

Do not document `GitHub Packages` as the primary install route for public consumers.

## Repository-only surfaces

The following entries remain repository-only:

- `Videra.Demo`
- `Videra.SurfaceCharts.Demo`
- `Videra.ExtensibilitySample`
- `Videra.InteractionSample`

They ship as source, documentation, and sample assets inside the repository.

SurfaceCharts release truth now has two explicit parts:

- package assets publish `Videra.SurfaceCharts.Core`, `Videra.SurfaceCharts.Rendering`, `Videra.SurfaceCharts.Processing`, and `Videra.SurfaceCharts.Avalonia`
- `Videra.SurfaceCharts.Demo` remains repository-only and keeps the support-summary repro path

## Versioning

- Current public baseline remains `alpha`.
- Tag format is `v<semver>`, for example `v0.2.0-alpha.1`.
- Public tag releases are expected to create consumer-facing notes, attach package assets, and publish the full public package set.

## Asset expectations

Public release runs are expected to produce:

- `.nupkg` assets for the published package set
- `.snupkg` assets for symbols
- generated release notes categorized through `.github/release.yml`

## Support boundary

- Public packages carry the documented viewer-stack or chart-stack support contract.
- Repository-only demos and samples do not silently become installable packages.
- The repository does not use a broad directory reorganization as part of the release policy.
