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

The canonical public chart stack is `Videra.SurfaceCharts.Avalonia` plus `Videra.SurfaceCharts.Processing` for the surface/cache-backed path. `Videra.SurfaceCharts.Core` and `Videra.SurfaceCharts.Rendering` are part of the public package line because the current chart control/runtime split depends on them, but they are not the normal first install step for most chart consumers.
The packaged proof for that chart stack is `smoke/Videra.SurfaceCharts.ConsumerSmoke` on the supported host path, and its support artifact is `surfacecharts-support-summary.txt`.

Use [Package Matrix](package-matrix.md) for the published-package table and [Hosting Boundary](hosting-boundary.md) for the canonical composition story behind this release line.

## Preview and internal feed

`GitHub Packages` is reserved for `preview` / internal validation flows. It can be used for contributor testing, canary validation, and pre-release evidence gathering, but it is not the default public consumer path.

Do not document `GitHub Packages` as the primary install route for public consumers.

## Repository-only surfaces

The following entries remain repository-only:

- `Videra.Demo`
- `Videra.SurfaceCharts.ConsumerSmoke`
- `Videra.SurfaceCharts.Demo`
- `Videra.ExtensibilitySample`
- `Videra.InteractionSample`

They ship as source, documentation, and sample assets inside the repository.

SurfaceCharts release truth now has three explicit parts:

- package assets publish `Videra.SurfaceCharts.Core`, `Videra.SurfaceCharts.Rendering`, `Videra.SurfaceCharts.Processing`, and `Videra.SurfaceCharts.Avalonia`
- `smoke/Videra.SurfaceCharts.ConsumerSmoke` proves the packaged surface/cache-backed path and emits the packaged support-summary artifact
- `Videra.SurfaceCharts.Demo` remains repository-only and keeps the `Start here`, `Explore next`, `Try next: Analytics proof`, `Try next: Waterfall proof`, and `Try next: Scatter proof` support-summary repro paths

The viewer release truth remains imported-asset/runtime only: per-primitive non-Blend material participation, occlusion texture binding/strength, `KHR_texture_transform` offset/scale/rotation plus texture-coordinate override, and mixed Blend/non-Blend guardrails are part of the shipped viewer path, but renderer/shader/backend consumption of that metadata is not a release promise.

## Versioning

- Current public baseline remains `alpha`.
- Tag format is `v<semver>`, for example `v0.2.0-alpha.1`.
- Public tag releases are expected to create consumer-facing notes, attach package assets, and publish the full public package set.

## Asset expectations

Public release runs are expected to produce:

- `.nupkg` assets for the published package set
- `.snupkg` assets for symbols
- generated release notes categorized through `.github/release.yml`

Release-candidate review uses the `Release Dry Run` workflow at `.github/workflows/release-dry-run.yml` before any public tag publication. That workflow runs `scripts/Invoke-ReleaseDryRun.ps1`, performs candidate version/tag simulation through `scripts/Test-ReleaseCandidateVersion.ps1`, packs the package set from `eng/public-api-contract.json`, reuses `scripts/Validate-Packages.ps1`, uploads `release-dry-run-evidence`, and does not push assets to `nuget.org` or GitHub Packages.

## Support boundary

- Public packages carry the documented viewer-stack or chart-stack support contract.
- Repository-only demos and samples do not silently become installable packages.
- The repository does not use a broad directory reorganization as part of the release policy.
