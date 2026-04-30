# Phase 397 Plan: Version and Package Cutover Contracts

## Goal

Confirm the release-candidate version/package contract and tighten support artifact metadata before gated release automation and docs cutover.

## Tasks

1. Claim `Videra-v258.2` and read the Phase 396 approval packet.
2. Confirm current version, package contract, package budgets, package metadata sources, and release scripts.
3. Fix the narrow SurfaceCharts consumer-smoke support artifact metadata bug identified in Phase 396.
4. Add focused repository coverage for scenario-specific support artifact metadata selection.
5. Run package dry-run and package validation evidence.
6. Run SurfaceCharts packaged consumer smoke and verify every `SupportArtifactPaths` entry exists.
7. Update Phase 397 summary/verification, roadmap/state, Beads export, generated roadmap, and close the phase bead.

## Validation

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter FullyQualifiedName~SurfaceChartsConsumerSmokeConfigurationTests`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Test-SnapshotExportScope.ps1`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Invoke-ReleaseDryRun.ps1 -ExpectedVersion 0.1.0-alpha.7 -Configuration Release -OutputRoot artifacts\phase397-release-dry-run`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario SurfaceCharts -OutputRoot artifacts\phase397-surfacecharts-consumer-smoke`
- PowerShell check that every `SupportArtifactPaths` entry in `artifacts\phase397-surfacecharts-consumer-smoke\consumer-smoke-result.json` exists.
- Reviewer pass on changed script/test files.
