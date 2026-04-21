# Release Policy

This document defines what Videra publishes, where those packages go, and what stays repository-only.

## Public packages

The public package line is limited to:

- `Videra.Core`
- `Videra.Import.Gltf`
- `Videra.Import.Obj`
- `Videra.Avalonia`
- `Videra.Platform.Windows`
- `Videra.Platform.Linux`
- `Videra.Platform.macOS`

Public release tags publish these packages to `nuget.org`.

## Preview and internal feed

`GitHub Packages` is reserved for `preview` / internal validation flows. It can be used for contributor testing, canary validation, and pre-release evidence gathering, but it is not the default public consumer path.

Do not document `GitHub Packages` as the primary install route for public consumers.

## Source-only surfaces

The following entries are repository-first and are not part of the public package promise:

- `Videra.SurfaceCharts.Core`
- `Videra.SurfaceCharts.Avalonia`
- `Videra.SurfaceCharts.Processing`
- `Videra.Demo`
- `Videra.SurfaceCharts.Demo`
- `Videra.ExtensibilitySample`
- `Videra.InteractionSample`

They ship as source, documentation, and sample assets inside the repository.

SurfaceCharts release truth stays on the source-first demo/docs/CI/support-summary path, not on a package asset line.

## Versioning

- Current public baseline remains `alpha`.
- Tag format is `v<semver>`, for example `v0.2.0-alpha.1`.
- Public tag releases are expected to create consumer-facing notes, attach package assets, and publish the public package set.

## Asset expectations

Public release runs are expected to produce:

- `.nupkg` assets for the published package set
- `.snupkg` assets for symbols
- generated release notes categorized through `.github/release.yml`

## Support boundary

- Public packages carry the documented viewer-stack support contract.
- Source-only modules do not silently become public packages.
- The repository does not use a broad directory reorganization as part of the release policy.
