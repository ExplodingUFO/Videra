# Phase 225 Verification

## Commands

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryReleaseReadinessTests" -p:RestoreIgnoreFailedSources=true`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryReleaseReadinessTests|FullyQualifiedName~ReleaseDryRunRepositoryTests|FullyQualifiedName~VideraDoctorRepositoryTests" -p:RestoreIgnoreFailedSources=true`
- `dotnet build Videra.slnx -c Release -p:RestoreIgnoreFailedSources=true`
- `git diff --check`

## Result

- RepositoryReleaseReadiness tests passed: 23/23.
- Combined readiness/dry-run/Doctor tests passed: 35/35.
- Full solution build passed.
- `git diff --check` passed.

## Residuals

- Full solution build still reports the two pre-existing SurfaceCharts integration-test CS0067 warnings for unused recording host events.
