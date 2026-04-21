# Releasing Videra

This runbook describes how public and preview package publication works.

## Tag format

- Public releases start from a git tag named `v<semver>`.
- Example: `v0.2.0-alpha.1`

## Public release flow

Public package publishing runs through `.github/workflows/publish-public.yml`.

That workflow is expected to:

1. Resolve the version from the tag.
2. Run matching-host native validation for Linux X11, Linux Wayland-session `XWayland`, macOS, and Windows.
3. Run `Consumer Smoke` on the packaged alpha happy path through `scripts/Invoke-ConsumerSmoke.ps1`, with the dedicated consumer-smoke workflow also serving as routine pull-request evidence.
4. Pack the public package set.
5. Validate package metadata and assets through `scripts/Validate-Packages.ps1`.
6. Push `.nupkg` and `.snupkg` assets to `nuget.org`.
7. Create or update the GitHub Release with generated notes and attached package assets.

## Preview flow

Preview and internal validation runs through `.github/workflows/publish-github-packages.yml`.

That workflow is manual (`workflow_dispatch`) and pushes preview artifacts to `GitHub Packages`.

## Release notes

- Release-page categories come from `.github/release.yml`.
- The release surface should communicate breaking changes, features, fixes, docs, and CI/build work.
- Public release assets should make it obvious which package IDs are part of the release.

## Package set

The public package set is limited to:

- `Videra.Core`
- `Videra.Import.Gltf`
- `Videra.Import.Obj`
- `Videra.Avalonia`
- `Videra.Platform.Windows`
- `Videra.Platform.Linux`
- `Videra.Platform.macOS`

`Videra.SurfaceCharts.*` and the sample/demo applications remain repository-only until a later milestone explicitly promotes them.

## Maintainer checklist

- Confirm `CHANGELOG.md` and public docs match the shipped truth.
- Confirm the canonical alpha happy path still matches `Videra.MinimalSample`, `consumer smoke`, and the `Videra.Avalonia` README.
- Confirm `Consumer Smoke` artifacts include the diagnostics snapshot produced by `VideraDiagnosticsSnapshotFormatter`.
- Confirm pull-request `sample-contract-evidence` stayed green for `Videra.ExtensibilitySample` and `Videra.InteractionSample` configuration/runtime contracts.
- Confirm `README.md`, `docs/support-matrix.md`, and `docs/release-policy.md` still say SurfaceCharts stays source-first and that public tags do not publish `Videra.SurfaceCharts.*` package assets.
- Confirm pull-request `sample-contract-evidence` stayed green for `Videra.SurfaceCharts.Demo`, including `Start here: In-memory first chart`, the `Copy support summary` workflow, and the SurfaceCharts runtime evidence step.
- Confirm pull-request `quality-gate-evidence` stayed green so the packaged consumer path, curated Core test surfaces, and `Videra.MinimalSample` still build with warnings treated as errors.
- Confirm public release notes and attached assets do not introduce `Videra.SurfaceCharts.*` package IDs or present `Videra.SurfaceCharts.Demo` as a public package install path.
- Review [Benchmark Gates](benchmark-gates.md) artifacts for trend regressions before promoting the tag when benchmark evidence was explicitly requested; treat them as review evidence, not as proof that a hard numeric threshold passed.
- Confirm release notes categories in `.github/release.yml` still match the current label taxonomy.
- Confirm `NUGET_API_KEY` is configured for the public workflow.
- Confirm preview/internal workflows do not override the public-feed truth.
