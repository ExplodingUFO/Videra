# Phase 223 Verification

## Commands

- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\Invoke-VideraDoctor.ps1 -OutputRoot artifacts/doctor-phase223`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~VideraDoctorRepositoryTests" -p:RestoreIgnoreFailedSources=true`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~VideraDoctorRepositoryTests|FullyQualifiedName~AlphaConsumerIntegrationTests" -p:RestoreIgnoreFailedSources=true`
- `dotnet build Videra.slnx -c Release -p:RestoreIgnoreFailedSources=true`
- `git diff --check`

## Result

- Doctor default run passed and wrote `doctor-summary.txt` / `doctor-report.json`.
- Doctor repository tests passed: 4/4.
- Doctor plus consumer-smoke repository tests passed: 13/13.
- Full solution build passed.
- `git diff --check` passed.

## Residuals

- Full solution build still reports the two pre-existing SurfaceCharts integration-test CS0067 warnings for unused recording host events.
