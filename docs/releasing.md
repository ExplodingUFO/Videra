# Releasing Videra

This runbook describes how public and preview package publication works.

Use [Package Matrix](package-matrix.md) as the authoritative public package table and [Hosting Boundary](hosting-boundary.md) when you need to verify that release notes and package assets still describe the same canonical viewer stack.

## Release readiness sequence

Use this sequence for release-candidate review, support triage, and local validation before any publishing path is involved:

1. Capture repository and machine context with `scripts/Invoke-VideraDoctor.ps1`. Attach `artifacts/doctor/doctor-report.json` and `artifacts/doctor/doctor-summary.txt` when the report is about local setup, missing validation inputs, package-feed drift, backend/platform availability, or repository state.
2. Confirm the package contract from `eng/public-api-contract.json`, then run package validation through `scripts/Validate-Packages.ps1` or the read-only `Release Dry Run` workflow. Keep `release-dry-run-evidence` with the candidate review when package metadata, package-size budgets, or release assets are in scope.
3. Run matching-host native validation through `scripts/run-native-validation.ps1` when the issue or release candidate depends on native backend availability.
4. Run `Benchmark Gates` through GitHub Actions or run `scripts/Run-Benchmarks.ps1` locally for the affected suite, then use `scripts/Test-BenchmarkThresholds.ps1` against the emitted benchmark artifacts. Treat `benchmarks/benchmark-contract.json` as the source-controlled benchmark inventory and `benchmarks/benchmark-thresholds.json` as the hard-threshold slice.
5. Run packaged viewer and SurfaceCharts validation through `scripts/Invoke-ConsumerSmoke.ps1`. Attach `artifacts/consumer-smoke/consumer-smoke-result.json`, `artifacts/consumer-smoke/diagnostics-snapshot.txt`, and `artifacts/consumer-smoke/surfacecharts-support-summary.txt` when packaged consumer behavior is relevant.
6. Route issue-specific support artifacts through `docs/alpha-feedback.md`: use `Videra.MinimalSample` for the shortest viewer happy path, `Videra.Demo` for import/backend diagnostics, and `Videra.SurfaceCharts.Demo` or `smoke/Videra.SurfaceCharts.ConsumerSmoke` for chart-specific support summaries.

This sequence does not publish packages, create release tags, push feeds, or replace the human-approved public release workflow.

## Release control model

Release control has three distinct paths:

- Dry run: `.github/workflows/release-dry-run.yml` and `scripts/Invoke-ReleaseDryRun.ps1` validate package and candidate evidence without credentials, tags, GitHub Releases, or package-feed mutation.
- Preview feed: `.github/workflows/publish-github-packages.yml` publishes internal preview packages through manual dispatch only; its feed mutation job is gated by the `preview-packages` environment.
- Public publish: `.github/workflows/publish-public.yml` publishes from manual dispatch only, with explicit `tag`, `version`, and `expected_commit` inputs. `.github/workflows/publish-existing-public-release.yml` republishes an existing tag through the same explicit dispatch truth. Public feed mutation jobs are gated by the `public-release` environment.

Before relying on either publish path, inspect the workflow trigger, release environment, expected branch/tag state, required status checks, and secret names. This check must not expose secret values or mutate GitHub environment, branch, or feed configuration.

## Alpha candidate checklist

Use this checklist for each alpha candidate review. Keep the generated artifacts with the candidate notes; do not treat this checklist as package publication approval.

| Gate | Command or workflow | Required evidence |
| --- | --- | --- |
| Doctor snapshot | `scripts/Invoke-VideraDoctor.ps1` | `artifacts/doctor/doctor-report.json`, `artifacts/doctor/doctor-summary.txt` |
| Release dry run | `.github/workflows/release-dry-run.yml` or `scripts/Invoke-ReleaseDryRun.ps1` | `release-dry-run-evidence`, `release-dry-run-summary.json`, `release-candidate-evidence-index.json`, `release-candidate-evidence-index.txt` |
| Package validation | `scripts/Validate-Packages.ps1` | `package-size-evaluation.json`, `package-size-summary.txt` |
| Benchmark Gates | `.github/workflows/benchmark-gates.yml`, `scripts/Run-Benchmarks.ps1`, `scripts/Test-BenchmarkThresholds.ps1` | viewer and SurfaceCharts benchmark manifests plus threshold evaluation artifacts |
| Native validation | `.github/workflows/native-validation.yml` or `scripts/run-native-validation.ps1` | Windows, Linux X11, Linux XWayland, and macOS native-validation artifacts |
| Packaged consumer smoke | `.github/workflows/consumer-smoke.yml` or `scripts/Invoke-ConsumerSmoke.ps1` | `consumer-smoke-result.json`, `diagnostics-snapshot.txt`, `surfacecharts-support-summary.txt` |
| Public release preflight | `scripts/Invoke-PublicReleasePreflight.ps1` | `public-release-preflight-summary.json`, `public-release-preflight-summary.txt` |

Known non-blockers must be recorded in candidate review notes with evidence links. They should not be described as shipped package changes unless they affect the public package behavior.

## Tag format

- Public releases start from a git tag named `v<semver>`.
- Example: `v0.2.0-alpha.1`

## Public release flow

Public package publishing runs through `.github/workflows/publish-public.yml`.

The workflow is manual (`workflow_dispatch`) and requires explicit `tag`, `version`, and `expected_commit` inputs. The tag must start with `v`, the semver input must match the tag without the `v` prefix, and the checked-out tag commit must match `expected_commit` before any package validation or feed mutation runs.

That workflow is expected to:

1. Resolve the version from the explicit tag/version/expected-commit inputs.
2. Run matching-host native validation for Linux X11, Linux Wayland-session `XWayland`, macOS, and Windows.
3. Run `Consumer Smoke` on the packaged viewer happy path and the packaged SurfaceCharts first-chart happy path through `scripts/Invoke-ConsumerSmoke.ps1`, with the dedicated consumer-smoke workflow and PR `quality-gate-evidence` job both serving as routine pull-request evidence.
4. Pack the public package set.
5. Validate package metadata, package-size budgets, and assets through `scripts/Validate-Packages.ps1`.
6. Write `public-publish-before-summary.json` and `public-publish-before-summary.txt` from the validated package set.
7. Push `.nupkg` and `.snupkg` assets to `nuget.org`.
8. Create or update the GitHub Release with generated notes and attached package assets.
9. Write `public-publish-after-summary.json` and `public-publish-after-summary.txt` after the approved publish path completes.

## Preview flow

Preview and internal validation runs through `.github/workflows/publish-github-packages.yml`.

That workflow is manual (`workflow_dispatch`), uses the same canonical public package contract through `scripts/Validate-Packages.ps1`, and pushes preview artifacts to `GitHub Packages`. It is preview/internal evidence only; `nuget.org` remains the default public install path.

## Final release simulation

Before requesting approval for a real public publish, run the final non-mutating public-release simulation:

```pwsh
pwsh -File ./scripts/Invoke-FinalReleaseSimulation.ps1 -ExpectedVersion <version> -ExpectedCommit <commit>
```

The simulation runs public release preflight, checks public publish decision gates, checks preview-feed boundaries, verifies release docs, and writes `final-release-simulation-summary.json` plus `final-release-simulation-summary.txt`. It does not publish packages, create release tags, push remotes, or mutate public feeds.

## Release Dry Run

Release-candidate review uses the `Release Dry Run` workflow at `.github/workflows/release-dry-run.yml` before public tags are cut or feed credentials are involved.

Abort criteria and the human release cutover boundary are defined in [Release Candidate Abort and Cutover Runbook](release-candidate-cutover.md).

That workflow is expected to:

1. Run `scripts/Invoke-ReleaseDryRun.ps1`.
2. Run `scripts/Test-ReleaseCandidateVersion.ps1` to simulate the corresponding `v*` release tag and validate it against repository package metadata.
3. Resolve the dry-run package set from `eng/public-api-contract.json`.
4. Pack each public package with the requested dry-run version.
5. Reuse `scripts/Validate-Packages.ps1` for package set, symbols, README/license/icon/repository metadata, dependency boundaries, and package-size budgets.
6. Validate `release-dry-run-summary.json` against the simulated tag version and public API contract.
7. Generate `release-candidate-evidence-index.json` and `release-candidate-evidence-index.txt` from `eng/release-candidate-evidence.json`.
8. Upload `release-dry-run-evidence`.
9. Avoid `dotnet nuget push`, `NUGET_API_KEY`, GitHub Packages tokens, and GitHub Release creation.

## Release notes

- Release-page categories come from `.github/release.yml`.
- Generate public release notes from approved publish evidence with `scripts/New-PublicReleaseNotes.ps1`; the output is `public-release-notes.md`.
- The release surface should communicate breaking changes, features, fixes, docs, and CI/build work.
- Public release assets should make it obvious which package IDs are part of the release.
- Alpha candidate notes should state whether Doctor, Release Dry Run, package validation, Benchmark Gates, native validation, and packaged consumer smoke passed, failed, or were not run.
- Public release notes must link `docs/package-matrix.md`, include known alpha limitations, and reference `public-publish-after-summary.json`.
- Known non-blockers should be listed under candidate validation notes, not under package features or fixes.
- Dry-run evidence should be linked from release-candidate review notes, but it is not a substitute for the human-approved public publish workflow.
- Release-candidate review notes should start from `release-candidate-evidence-index.txt`; use the JSON form when automating checklist review.
- Failed candidates must follow the abort steps in [Release Candidate Abort and Cutover Runbook](release-candidate-cutover.md) before another cutover attempt.

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

The canonical public viewer stack is `Videra.Avalonia` plus exactly one matching `Videra.Platform.*` package. `Videra.Import.Gltf` and `Videra.Import.Obj` remain explicit ingestion packages on the core path; on the Avalonia path they only back file loading when consumers install them explicitly and register them through `VideraViewOptions.UseModelImporter(...)`.
The canonical public chart stack is `Videra.SurfaceCharts.Avalonia` plus `Videra.SurfaceCharts.Processing` for the surface/cache-backed path, with `Videra.SurfaceCharts.Core` and `Videra.SurfaceCharts.Rendering` staying visible because they are real shipped package seams.
Every public publish path, including `publish-existing-public-release.yml`, is expected to require explicit `tag`, `version`, and `expected_commit` inputs, run packaged viewer consumer smoke, run packaged SurfaceCharts consumer smoke, and run `Validate-Packages.ps1` before pushing assets. The existing-tag republish workflow is intentionally limited to tags that already carry the current public package set and helper scripts.
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
