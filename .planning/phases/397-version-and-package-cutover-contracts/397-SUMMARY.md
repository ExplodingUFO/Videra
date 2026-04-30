# Phase 397 Summary: Version and Package Cutover Contracts

## Status

Complete.

## What Changed

- Confirmed version/package truth for the v2.58 cutover:
  - version: `0.1.0-alpha.7` from `Directory.Build.props`
  - package contract: `eng/public-api-contract.json`
  - package-size budgets: `eng/package-size-budgets.json`
  - package validation: `scripts/Validate-Packages.ps1`
- Tightened `scripts\Invoke-ConsumerSmoke.ps1` so successful `SupportArtifactPaths` are scenario-specific and only include files/directories that exist.
- Extracted support artifact selection into `scripts\ConsumerSmokeSupportArtifacts.ps1` for direct executable coverage without broadening release automation.
- Added focused repository coverage in `SurfaceChartsConsumerSmokeConfigurationTests` for the scenario-specific metadata contract, including a `pwsh`-executed helper test for `SurfaceCharts` and `ViewerObj`.
- Generated package dry-run evidence under `artifacts/phase397-release-dry-run`.
- Generated SurfaceCharts consumer smoke evidence under `artifacts/phase397-surfacecharts-consumer-smoke`.

## Key Evidence

- `artifacts/phase397-release-dry-run/release-dry-run-summary.json` reports release dry-run pass for `0.1.0-alpha.7`.
- `artifacts/phase397-release-dry-run/packages/.validation/package-size-evaluation.json` records package size validation.
- `artifacts/phase397-surfacecharts-consumer-smoke/consumer-smoke-result.json` reports packaged SurfaceCharts smoke pass.
- The Phase 397 SurfaceCharts smoke `SupportArtifactPaths` list contains only existing files:
  - `diagnostics-snapshot.txt`
  - `surfacecharts-support-summary.txt`
  - `chart-snapshot.png`
  - trace/stdout/stderr/environment logs

## Handoff

Phase 398 can build on the verified package/dry-run contract and should make pass/fail/skipped/manual-gate states explicit and fail-closed. Phase 399 can proceed in parallel after Phase 397 and should document the cleaned support artifact expectations.
