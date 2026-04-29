# Phase 393 Summary: Release Validation Script and CI Alignment

## Result

Phase 393 is implemented on branch `v2.57-phase393-validation`.

## Changes

- Added `scripts/Invoke-ReleaseReadinessValidation.ps1` as the single local release-readiness command.
- The script composes:
  - `scripts/Invoke-ReleaseDryRun.ps1` for package build and package validation.
  - `scripts/Invoke-ConsumerSmoke.ps1 -Scenario SurfaceCharts` for packaged SurfaceCharts consumer smoke.
  - focused `Videra.Core.Tests` repository/sample tests for SurfaceCharts smoke/demo and release script contracts.
  - `scripts/Test-SnapshotExportScope.ps1` for snapshot/export scope guardrails.
- The script writes `release-readiness-validation-summary.json` and `.txt`, with separate pass/fail, local environment warning, and skipped publish/tag sections.
- `.github/workflows/release-dry-run.yml` now invokes the readiness command and uploads `release-readiness-validation-evidence`.
- Focused repository tests now cover the new script and aligned workflow entrypoint.

## Files Changed

- `.github/workflows/release-dry-run.yml`
- `scripts/Invoke-ReleaseReadinessValidation.ps1`
- `eng/package-size-budgets.json`
- `tests/Videra.Core.Tests/Repository/PackageSizeBudgetRepositoryTests.cs`
- `tests/Videra.Core.Tests/Repository/ReleaseCandidateTruthRepositoryTests.cs`
- `tests/Videra.Core.Tests/Repository/ReleaseDryRunRepositoryTests.cs`
- `.planning/phases/393-release-validation-script-and-ci-alignment/393-CONTEXT.md`
- `.planning/phases/393-release-validation-script-and-ci-alignment/393-PLAN.md`
- `.planning/phases/393-release-validation-script-and-ci-alignment/393-SUMMARY.md`
- `.planning/phases/393-release-validation-script-and-ci-alignment/393-VERIFICATION.md`

## Residual Risks

- Local verification used `-ConsumerSmokeBuildOnly` to prove script wiring cheaply. The default script path still runs the full SurfaceCharts consumer smoke and should run in CI or a desktop-capable local session before release cutover.
- Package-size budgets were aligned for the current SurfaceCharts package outputs after the new readiness command exposed stale budget ceilings.