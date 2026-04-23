# Releasing Videra

This runbook describes how public and preview package publication works.

Use [Package Matrix](package-matrix.md) as the authoritative public package table and [Hosting Boundary](hosting-boundary.md) when you need to verify that release notes and package assets still describe the same canonical viewer stack.

## Tag format

- Public releases start from a git tag named `v<semver>`.
- Example: `v0.2.0-alpha.1`

## Public release flow

Public package publishing runs through `.github/workflows/publish-public.yml`.

That workflow is expected to:

1. Resolve the version from the tag.
2. Run matching-host native validation for Linux X11, Linux Wayland-session `XWayland`, macOS, and Windows.
3. Run `Consumer Smoke` on the packaged viewer happy path and the packaged SurfaceCharts first-chart happy path through `scripts/Invoke-ConsumerSmoke.ps1`, with the dedicated consumer-smoke workflow and PR `quality-gate-evidence` job both serving as routine pull-request evidence.
4. Pack the public package set.
5. Validate package metadata, package-size budgets, and assets through `scripts/Validate-Packages.ps1`.
6. Push `.nupkg` and `.snupkg` assets to `nuget.org`.
7. Create or update the GitHub Release with generated notes and attached package assets.

## Preview flow

Preview and internal validation runs through `.github/workflows/publish-github-packages.yml`.

That workflow is manual (`workflow_dispatch`) and pushes preview artifacts to `GitHub Packages`.

## Release Dry Run

Release-candidate review uses the `Release Dry Run` workflow at `.github/workflows/release-dry-run.yml` before public tags are cut or feed credentials are involved.

That workflow is expected to:

1. Run `scripts/Invoke-ReleaseDryRun.ps1`.
2. Resolve the dry-run package set from `eng/public-api-contract.json`.
3. Pack each public package with the requested dry-run version.
4. Reuse `scripts/Validate-Packages.ps1` for package set, symbols, README/license/icon/repository metadata, dependency boundaries, and package-size budgets.
5. Upload `release-dry-run-evidence`.
6. Avoid `dotnet nuget push`, `NUGET_API_KEY`, GitHub Packages tokens, and GitHub Release creation.

## Release notes

- Release-page categories come from `.github/release.yml`.
- The release surface should communicate breaking changes, features, fixes, docs, and CI/build work.
- Public release assets should make it obvious which package IDs are part of the release.
- Dry-run evidence should be linked from release-candidate review notes, but it is not a substitute for the tag-triggered public publish workflow.

## Package set

The public package set is:

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

`Videra.SurfaceCharts.Demo` and the other sample/demo applications remain repository-only.

The canonical public viewer stack is `Videra.Avalonia` plus exactly one matching `Videra.Platform.*` package. `Videra.Import.Gltf` and `Videra.Import.Obj` remain explicit ingestion packages on the core path.
The canonical public chart stack is `Videra.SurfaceCharts.Avalonia` plus `Videra.SurfaceCharts.Processing` for the surface/cache-backed path, with `Videra.SurfaceCharts.Core` and `Videra.SurfaceCharts.Rendering` staying visible because they are real shipped package seams.
Every public publish path, including `publish-existing-public-release.yml`, is expected to run packaged viewer consumer smoke, packaged SurfaceCharts consumer smoke, and `Validate-Packages.ps1` before pushing assets. The existing-tag republish workflow is intentionally limited to tags that already carry the current public package set and helper scripts.
`Validate-Packages.ps1` now also enforces the source-controlled byte budgets in `eng/package-size-budgets.json` and emits package-size evaluation artifacts under `PackageRoot/.validation`.

## Maintainer checklist

- Confirm `CHANGELOG.md` and public docs match the shipped truth.
- Confirm the canonical alpha happy path still matches `Videra.MinimalSample`, `consumer smoke`, `smoke/Videra.SurfaceCharts.ConsumerSmoke`, and the package READMEs.
- Confirm `Consumer Smoke` artifacts include the diagnostics snapshot produced by `VideraDiagnosticsSnapshotFormatter`.
- Confirm packaged SurfaceCharts consumer smoke artifacts include `surfacecharts-support-summary.txt`.
- Confirm pull-request `sample-contract-evidence` stayed green for `Videra.ExtensibilitySample` and `Videra.InteractionSample` configuration/runtime contracts.
- Confirm `README.md`, `docs/support-matrix.md`, and `docs/release-policy.md` still describe the same chart truth: public tags publish the `Videra.SurfaceCharts.*` package assets while `Videra.SurfaceCharts.Demo` remains repository-only.
- Confirm `docs/package-matrix.md` and `docs/hosting-boundary.md` still describe the same canonical viewer stack as the release assets: `Videra.Avalonia` + one matching `Videra.Platform.*` package, with `Videra.Import.*` remaining explicit ingestion packages.
- Confirm pull-request `sample-contract-evidence` stayed green for `Videra.SurfaceCharts.Demo`, including `Start here: In-memory first chart`, `Explore next: Cache-backed streaming`, `Try next: Analytics proof`, `Try next: Waterfall proof`, `Try next: Scatter proof`, the `Copy support summary` workflow, and the SurfaceCharts runtime evidence step.
- Confirm pull-request `quality-gate-evidence` stayed green so the Windows packaged viewer consumer smoke path and the Windows packaged SurfaceCharts first-chart consumer smoke path both run with warnings treated as errors, package-size budgets still pass, and the curated Core test surfaces plus `Videra.MinimalSample` remain warning-clean.
- Confirm pull-request `release-dry-run` stayed green and uploaded `release-dry-run-evidence` from the public API contract package set without publishing packages.
- Confirm public release notes and attached assets include the chart package IDs when they are part of the release and do not present `Videra.SurfaceCharts.Demo` as a public package install path.
- Confirm pull-request `Benchmark Gates` stayed green and that the threshold evaluation artifacts did not report committed runtime-budget regressions.
- Confirm release notes categories in `.github/release.yml` still match the current label taxonomy.
- Confirm `NUGET_API_KEY` is configured for the public workflow.
- Confirm preview/internal workflows do not override the public-feed truth.
