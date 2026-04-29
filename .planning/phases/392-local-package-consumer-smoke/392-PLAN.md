# Phase 392 Plan: Local Package Consumer Smoke

## Goal

A clean consumer can restore, build, run, and produce support artifacts using locally built SurfaceCharts packages and public APIs only.

## Tasks

1. Keep the existing consumer project package-only and lock it with focused tests.
2. Make SurfaceCharts consumer smoke persist a deterministic `chart-snapshot.png` beside the JSON report, diagnostics snapshot, and support summary.
3. Make `scripts/Invoke-ConsumerSmoke.ps1` validate the chart snapshot as a required SurfaceCharts artifact.
4. Fix consumer-smoke-only compile gaps revealed by the package validation path.
5. Verify with focused tests, snapshot scope guardrails, build-only package smoke, and full SurfaceCharts consumer smoke.

## Ownership Boundary

- `smoke/Videra.SurfaceCharts.ConsumerSmoke/Views/MainWindow.axaml.cs`
- `scripts/Invoke-ConsumerSmoke.ps1`
- `tests/Videra.Core.Tests/Samples/SurfaceChartsConsumerSmokeConfigurationTests.cs`

No production `src/` runtime changes are planned for this phase.

## Validation

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsConsumerSmokeConfigurationTests|FullyQualifiedName~VideraDoctorRepositoryTests"`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Test-SnapshotExportScope.ps1`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario SurfaceCharts -BuildOnly -OutputRoot artifacts/phase392-consumer-smoke-build`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario SurfaceCharts -OutputRoot artifacts/phase392-consumer-smoke-run`

